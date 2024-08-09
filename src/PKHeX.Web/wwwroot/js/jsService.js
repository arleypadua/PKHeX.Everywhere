window.addEventListener("resize", () => {
    DotNet.invokeMethodAsync("PKHeX.Web", "OnWindowResized", window.innerWidth)
});

screen.orientation.addEventListener("change", (event) => {
  DotNet.invokeMethodAsync("PKHeX.Web", "OnWindowResized", window.innerWidth);
});

window.getWidth = () => window.innerWidth;

window.hasPreferenceForDarkTheme = () => window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches

window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}

window.encryptAes = function (key, data, modeString) {
    const keyHex = CryptoJS.enc.Hex.parse(key);
    const encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Hex.parse(data), keyHex, {
        mode: getMode(modeString),
        padding: CryptoJS.pad.NoPadding
    });
    return encrypted.ciphertext.toString(CryptoJS.enc.Hex);
};

window.decryptAes = function (key, data, modeString) {
    var keyHex = CryptoJS.enc.Hex.parse(key);
    var encryptedHexStr = CryptoJS.enc.Hex.parse(data);
    var encryptedBase64Str = CryptoJS.enc.Base64.stringify(encryptedHexStr);
    var decrypted = CryptoJS.AES.decrypt(encryptedBase64Str, keyHex, {
        mode: getMode(modeString),
        padding: CryptoJS.pad.NoPadding
    });
    return decrypted.toString(CryptoJS.enc.Hex);
};

window.md5Hash = function (data) {
    const parsedHexString =  CryptoJS.enc.Hex.parse(data);
    var hash = CryptoJS.MD5(parsedHexString);
    return hash.toString(CryptoJS.enc.Hex);
}

function getMode(modeString) {
    if (modeString === 'ecb') return CryptoJS.mode.ECB;
    if (modeString === 'cbc') return CryptoJS.mode.CBC;
    if (modeString === 'cfb') return CryptoJS.mode.CFB;
    if (modeString === 'ctr') return CryptoJS.mode.CTR;
    if (modeString === 'ofb') return CryptoJS.mode.OFB;
    
    throw new Error(`AES mode ${modeString} not supported.`);
}
