import { LogService } from "matrix-bot-sdk";
import config from "../config";

export default class DiscoApi {
    public async uploadSpace(name: string, description: string, members: number, invite: string, avatar: string, is18Plus: boolean, admins?: string[]) {
        const apiRequest = {
            name: name,
            description: description,
            memberCount: members,
            invite: invite,
            avatar: avatar,
            admins: admins || [],
            is18Plus: is18Plus,
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
            LogService.warn('InviteHandler', 'Error adding space', invite, result.status, await result.text());
        }else{
            LogService.info('InviteHandler', 'Added space', invite, await result.text());
        }
    }
}