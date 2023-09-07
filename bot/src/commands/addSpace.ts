import { LogService, MatrixClient, MessageEvent, MessageEventContent } from "matrix-bot-sdk";
const TAG = 'verifyCommand';

export async function runAddSpace(roomId: string, event: MessageEvent<MessageEventContent>, args: string[], client: MatrixClient) {
    LogService.info(TAG, "raw args:",args);
    // [verify, token]
    if (args.length < 1) {
        return client.sendMessage(roomId, {
            body: 'Usage: !disco addspace <id>',
            msgtype: "m.notice",
            format: "org.matrix.custom.html",
        });
    }
    const [action, space] = args;
    if (typeof space !== 'string' || space.length < 4 || space.length > 128 || !space.startsWith('#') || space.indexOf(':') === -1) {
        return client.sendMessage(roomId, {
            body: 'Invalid space id.',
            msgtype: "m.notice",
            format: "org.matrix.custom.html",
        });
    }
    LogService.info(TAG, 'args: ', args);
    const sender = event.sender;
    LogService.info(TAG, 'sender: ' + sender);
    try {
        const roomInfo = await client.getSpace(space);
        LogService.info(TAG, 'full space info:', roomInfo);
    }catch(e) {
        LogService.error(TAG, 'error getting room details', e);
    }
    try {
        const members = await client.getJoinedRoomMembers(space);
        LogService.info(TAG, 'members: ', members);
    }catch(e) {
        LogService.error(TAG, 'error getting members', e);
    }

    // Now send that message as a notice
    return client.sendMessage(roomId, {
        body: 'Test',
        msgtype: "m.notice",
        format: "org.matrix.custom.html",
    });
}
