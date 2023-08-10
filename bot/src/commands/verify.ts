import { LogService, MatrixClient, MessageEvent, MessageEventContent } from "matrix-bot-sdk";
const TAG = 'verifyCommand';

export async function runVerifyCommand(roomId: string, event: MessageEvent<MessageEventContent>, args: string[], client: MatrixClient) {
    // [verify, token]
    if (args.length !== 2) {
        return client.sendMessage(roomId, {
            body: 'Usage: disco!verify <id>',
            msgtype: "m.notice",
            format: "org.matrix.custom.html",
        });
    }
    const [action, token] = args;
    if (typeof token !== 'string' || token.length < 4 || token.length > 128) {
        return client.sendMessage(roomId, {
            body: 'Invalid verification token.',
            msgtype: "m.notice",
            format: "org.matrix.custom.html",
        });
    }
    LogService.info(TAG, 'args: ', args);
    const sender = event.sender;
    LogService.info(TAG, 'sender: ' + sender);
    // get profile for avatar
    const profile = await client.getUserProfile(sender);
    LogService.info(TAG, 'full data for profile:' + JSON.stringify(profile));
    let imageUrl: string|undefined;
    if (profile && typeof profile === 'object' && typeof profile.avatar_url === 'string') {
        imageUrl = profile.avatar_url;
    }
    const dataToSend = {
        token: token,
        userId: sender,
        imageUrl: imageUrl || null,
    }
    LogService.info(TAG, 'post to api:' + JSON.stringify(dataToSend));
    // Now send that message as a notice
    return client.sendMessage(roomId, {
        body: 'Test',
        msgtype: "m.notice",
        format: "org.matrix.custom.html",
    });
}
