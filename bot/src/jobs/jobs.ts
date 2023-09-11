import { LogService, MatrixClient } from "matrix-bot-sdk";
import DiscoApi from "../api/api";
const delay = (n) => new Promise((resolve) => setTimeout(resolve, n));
interface IRoom {
    room_id: string;
    name: string;
    topic: string;
    canonical_alias: string;
    num_joined_members: number;
    avatar_url: string;
    world_readable: boolean;
    join_rule: string;
    room_type: string;
}

interface IRoomResponse {
    next_batch: string;
    chunk: IRoom[];
}

export default class Jobs {
    constructor(private client: MatrixClient, private discoApi: DiscoApi) {

    }

    public async start() {
        await this.scrapeSpaces();
    }
    
    private async scrapeSpaces() {

        LogService.info('scrapeSpaces', 'starting');
        const domains = [
            'matrix.org',
        ];
        for (const domain of domains) {
            LogService.info('scrapeSpaces', 'scraping', domain);
            await this.scrapeAllSpaces(domain, async (room) => {
                let allText = ((room.name || '') + (room.topic || '') + (room.canonical_alias || '')).toLocaleLowerCase();
                const isProbablyNsfw = allText.includes('nsfw') || allText.includes('18+') || allText.includes('porn') || allText.includes('hentai') || allText.includes('yiff') || allText.includes('lewd') || allText.includes('sex');
                await this.discoApi.uploadSpace(room.name, room.topic, room.num_joined_members, room.canonical_alias, room.avatar_url, isProbablyNsfw, undefined);
            });
        }
    }

    private async scrapeAllSpaces(domain: string, handler: (room: IRoom) => Promise<void>) {
        let batch: string|undefined;
        while (true) {
            const url = `/_matrix/client/v3/publicRooms`;
            const query = `server=${encodeURIComponent(domain)}&limit=50`;
            const data = {
                include_all_networks: true,
                since: batch,
                "filter":{
                    "generic_search_term":"",
                    "room_types":["m.space"]
                }
            };
            const result = await this.client.doRequest('POST', url, query, data, 2*60*1000, false, 'application/json', undefined);
            const rooms: IRoomResponse = result;
            if (!rooms || !rooms.chunk || rooms.chunk.length === 0) {
                break;
            }
            for (const room of rooms.chunk) {
                if (room && room.room_type === 'm.space' && room.world_readable && room.join_rule === 'public') {
                    await handler(room);
                }
            }
            batch = rooms.next_batch;
            if (!batch)
                return;
        }
    }
}