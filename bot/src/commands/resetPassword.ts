import { LogService, MatrixClient, MessageEvent, MessageEventContent } from "matrix-bot-sdk";
import * as htmlEscape from "escape-html";
const TAG = 'resetPassword';
import config from '../config';

if (typeof global.fetch === 'undefined')
    throw new Error('Node version not supported');

export async function runResetPasswordCommand(roomId: string, event: MessageEvent<MessageEventContent>, args: string[], client: MatrixClient) {
    LogService.info(TAG, 'request begin. room=' + roomId + ' args=', args);
    // [verify, token]
    if (args.length !== 2) {
        return client.sendMessage(roomId, {
            body: 'Usage: disco!resetpassword <id>',
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
    await client.setTyping(roomId, true, 5000);
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
    const result = await fetch(config.apiBaseUrl + '/api/Bot/ResetPasswordMatrix', {
        method: 'POST',
        body: JSON.stringify(dataToSend),
        headers: {
            'content-type': 'application/json',
            'BotAuthorization': config.apiKey,
        },
    });
    if (result.status !== 200) {
        let body: string = '';
        try {
            body = await result.text();
        }catch(e) {
            LogService.warn(TAG, 'error reading body for error report',e);
        }
        LogService.warn(TAG, 'invalid token provided by user: token=' + token + ' responseStatus=' + result.status + ' responseText=' + body);
        await client.setTyping(roomId, false);
        return client.sendMessage(roomId, {
            body: 'Invalid token or matrix account.',
            msgtype: "m.notice",
            format: "org.matrix.custom.html",
        });
    }
    const body = await result.json() as {redirectUrl: string};
    // Now send that message as a notice
    await client.setTyping(roomId, false);
    return client.sendMessage(roomId, {
        body: 'Account verified. Visit the following link to reset your password: ' + htmlEscape(body.redirectUrl),
        msgtype: "m.notice",
        format: "org.matrix.custom.html",
    });
}
