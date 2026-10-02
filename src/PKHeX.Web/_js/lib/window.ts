import {decryptAes, encryptAes} from "./crypto/aes.ts";
import {md5Hash} from "./crypto/md5.ts";

export function setupWindow() {
    // crypt
    window.encryptAes = encryptAes;
    window.decryptAes = decryptAes;
    window.md5Hash = md5Hash;
    
    // ui functions
    window.getWidth = () => window.innerWidth;
    window.clickElement = (element) => {
        if (!(element instanceof HTMLElement)) {
            return;
        }

        if (element instanceof HTMLInputElement && typeof element.showPicker === "function") {
            element.showPicker();
            return;
        }

        element.click();
    };
    
    // event listeners -- integration
    window.addEventListener("resize", async () => {
        await DotNet.invokeMethodAsync("PKHeX.Web", "OnWindowResized", window.innerWidth)
    });

    screen.orientation.addEventListener("change", async (_) => {
        await DotNet.invokeMethodAsync("PKHeX.Web", "OnWindowResized", window.innerWidth);
    });
}