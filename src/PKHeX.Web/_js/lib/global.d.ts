import {decryptAes, encryptAes} from "./crypto/aes.ts";
import {md5Hash} from "./crypto/md5.ts";

declare global {
    interface Window {
        // crypt
        encryptAes: typeof encryptAes;
        decryptAes: typeof decryptAes;
        md5Hash: typeof md5Hash;

        // ui
        getWidth: () => number;
        clickElement: (element: HTMLElement | null | undefined) => void;
    }
}