import { LogService, MatrixClient, MessageEvent, RichReply, UserID } from "matrix-bot-sdk";
import { runHelloCommand } from "./hello";
import * as htmlEscape from "escape-html";
import { runVerifyCommand } from "./verify";
import { runResetPasswordCommand } from "./resetPassword";
import { runAddSpace } from "./addSpace";
import config from "../config";

// The prefix required to trigger the bot. The bot will also respond
// to being pinged directly.
export const COMMAND_PREFIX = "!disco";

// This is where all of our commands will be handled
export default class CommandHandler {

    // Just some variables so we can cache the bot's display name and ID
    // for command matching later.
    private displayName: string;
    private userId: string;
    private localpart: string;

    constructor(private client: MatrixClient) {
    }

    public async start() {
        // Populate the variables above (async)
        await this.prepareProfile();

        // Set up the event handler
        this.client.on("room.message", this.onMessage.bind(this));
        this.client.on('room.invite', this.onInvite.bind(this));

        await this.processExistingInvites();
    }

    private async processExistingInvites() {
        const leftoverFromCrash = await this.client.getJoinedRooms();
        for (const room of leftoverFromCrash) {
            const roomData = await this.client.getAllRoomMembers(room);
            let isDm = 0;
            if (roomData.length === 2 || roomData.length === 1) {
                for (const member of roomData) {
                    LogService.info('processExistingInvites', 'member', member);
                    if (member.content && member.content.is_direct) {
                        isDm++;
                    }
                }
            }
            if (isDm === roomData.length) {
                continue;
            }
            LogService.info('processExistingInvites', 'Leaving room', room);
            try {
                await this.client.leaveRoom(room, 'An unexpected error occurred. Please invite me again!');
            }catch(e) {
                LogService.warn('processExistingInvites', 'Error leaving room', room, e);
            }
        }
    }

    private async prepareProfile() {
        this.userId = await this.client.getUserId();
        this.localpart = new UserID(this.userId).localpart;

        try {
            const profile = await this.client.getUserProfile(this.userId);
            if (profile && profile['displayname']) this.displayName = profile['displayname'];
        } catch (e) {
            // Non-fatal error - we'll just log it and move on.
            LogService.warn("CommandHandler", e);
        }
    }

    private async onInvite(roomId: string, ev: any) {
        LogService.info('InviteHandler', 'raw data',roomId, ev);
        await this.client.joinRoom(roomId);
        // const roomData = await this.client.getRoom(roomId);
        // const spaceData = await this.client.getSpace(roomId);
        const roomData: {
            rooms: {
                room_id: string,
                name: string,
                topic: string,
                canonical_alias: string,
                num_joined_members: number,
                avatar_url: string,
                join_rule: string,
                world_readable: boolean,
                guest_can_join: boolean,
                room_type: string,
            }[],
        } = await this.client.doRequest('GET', `/_matrix/client/v1/rooms/${encodeURIComponent(roomId)}/hierarchy?suggested_only=false&limit=100`);
        // LogService.info('InviteHandler', 'spaceData', spaceData);
        LogService.info('InviteHandler', 'roomData', roomData);

        const spaceInfo = roomData.rooms.find(v => v.room_type === 'm.space' && v.room_id === roomId);
        if (!spaceInfo) {
            // leave and ignore
            LogService.info('InviteHandler', 'not a space, leaving');
            try {
                await this.client.leaveRoom(roomId, 'Not a space');
            }catch(e) {
                LogService.warn('InviteHandler', 'Error leaving room', roomId, e);
            }
            return;
        }

        const powerLevelsEvent: {users: Record<string, number>, events: Record<string, number>, ban: number} = await this.client.getRoomStateEvent(roomId, "m.room.power_levels", "");

        const minimumRoleForAddToSite = powerLevelsEvent.ban || 100;
        const adminUserIds = Object.entries(powerLevelsEvent.users).filter(v => v[1] >= minimumRoleForAddToSite).map(v => v[0]);

        const apiRequest = {
            name: spaceInfo.name,
            description: spaceInfo.topic,
            memberCount: spaceInfo.num_joined_members,
            invite: spaceInfo.canonical_alias,
            avatar: spaceInfo.avatar_url,
            admins: adminUserIds,
        };
        LogService.info('InviteHandler', 'apiRequest', apiRequest);
        const result = await fetch(config.apiBaseUrl + '/api/Bot/AddOrUpdateSpace', {
            method: 'POST',
            body: JSON.stringify(apiRequest),
            headers: {
                'content-type': 'application/json',
                'BotAuthorization': config.apiKey,
            },
        });
        if (result.status !== 200) {
            LogService.warn('InviteHandler', 'Error adding space', roomId, result.status, await result.text());
        }
        await this.client.leaveRoom(roomId, 'Added to site');
    }

    private async onMessage(roomId: string, ev: any) {
        const event = new MessageEvent(ev);
        if (event.isRedacted) return; // Ignore redacted events that come through
        if (event.sender === this.userId) return; // Ignore ourselves
        if (event.messageType !== "m.text") return; // Ignore non-text messages

        // Ensure that the event is a command before going on. We allow people to ping
        // the bot as well as using our COMMAND_PREFIX.
        const prefixes = [COMMAND_PREFIX, `${this.localpart}:`, `${this.displayName}:`, `${this.userId}:`];
        const prefixUsed = prefixes.find(p => event.textBody.startsWith(p));
        if (!prefixUsed) return; // Not a command (as far as we're concerned)

        // Check to see what the arguments were to the command
        const args = event.textBody.substring(prefixUsed.length).trim().split(' ');

        // Try and figure out what command the user ran, defaulting to help
        try {
            LogService.info('CommandHandler', 'handle command:' + args[0]);
            if (args[0] === "hello") {
                return runHelloCommand(roomId, event, args, this.client);
            }else if (args[0] === "verify") {
                // return runVerifyCommand(roomId, event, args, this.client);
            }else if (args[0] === 'resetpassword') {
                return runResetPasswordCommand(roomId, event, args, this.client);
            }else if (args[0] === 'addspace') {
                return runAddSpace(roomId, event, args, this.client);
            }else{
                LogService.error('CommandHandler', 'unknown command: ' + args[0]);
            }
        } catch (e) {
            // Log the error
            LogService.error("CommandHandler", e);

            // Tell the user there was a problem
            const message = "There was an error processing your command";
            return this.client.replyNotice(roomId, ev, message);
        }
    }
}
