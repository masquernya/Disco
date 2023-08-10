import * as config from "config";

interface IConfig {
    homeserverUrl: string;
    accessToken: string;
    autoJoin: boolean;
    dataPath: string;
    encryption: boolean;
    apiBaseUrl: string;
    apiKey: string;
}

export default <IConfig>config;
