import {decryptAes, encryptAes} from "./crypto/aes.ts";
import {md5Hash} from "./crypto/md5.ts";
import {downloadFileFromStream} from "./files/files.ts";

declare global {
    interface Window {
        // crypt
        encryptAes: typeof encryptAes;
        decryptAes: typeof decryptAes;
        md5Hash: typeof md5Hash;

        // files
        downloadFileFromStream: typeof downloadFileFromStream;
        
        // ui
        getWidth: () => number;
        hasPreferenceForDarkTheme: () => boolean;
        
        // firebase
        getAuthToken: () => Promise<string>;
    }
}