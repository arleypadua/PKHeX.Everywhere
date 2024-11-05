import {decryptAes, encryptAes} from "./crypto/aes.ts";
import {md5Hash} from "./crypto/md5.ts";
import {downloadFileFromStream} from "./files/files.ts";

export function setupWindow() {
    // crypt
    window.encryptAes = encryptAes;
    window.decryptAes = decryptAes;
    window.md5Hash = md5Hash;
    
    // files
    window.downloadFileFromStream = downloadFileFromStream;

    // ui functions
    window.getWidth = () => window.innerWidth;
    window.hasPreferenceForDarkTheme = () => window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches
    
    // event listeners -- integration
    window.addEventListener("resize", async () => {
        await DotNet.invokeMethodAsync("PKHeX.Web", "OnWindowResized", window.innerWidth)
    });

    screen.orientation.addEventListener("change", async (_) => {
        await DotNet.invokeMethodAsync("PKHeX.Web", "OnWindowResized", window.innerWidth);
    });
}