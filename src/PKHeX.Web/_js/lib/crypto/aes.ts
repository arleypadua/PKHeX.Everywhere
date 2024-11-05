import CryptoJS from "crypto-js";

export function encryptAes(key: string, data: string, modeString: ModeString) {
    const keyHex = CryptoJS.enc.Hex.parse(key);
    const encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Hex.parse(data), keyHex, {
        mode: getMode(modeString),
        padding: CryptoJS.pad.NoPadding
    });
    return encrypted.ciphertext.toString(CryptoJS.enc.Hex);
}

export function decryptAes(key: string, data: string, modeString: ModeString) {
    var keyHex = CryptoJS.enc.Hex.parse(key);
    var encryptedHexStr = CryptoJS.enc.Hex.parse(data);
    var encryptedBase64Str = CryptoJS.enc.Base64.stringify(encryptedHexStr);
    var decrypted = CryptoJS.AES.decrypt(encryptedBase64Str, keyHex, {
        mode: getMode(modeString),
        padding: CryptoJS.pad.NoPadding
    });
    return decrypted.toString(CryptoJS.enc.Hex);
}

export type ModeString =
    'ecb'
    | 'cbc'
    | 'cfb'
    | 'ctr'
    | 'ofb'

function getMode(modeString: ModeString) {
    if (modeString === 'ecb') return CryptoJS.mode.ECB;
    if (modeString === 'cbc') return CryptoJS.mode.CBC;
    if (modeString === 'cfb') return CryptoJS.mode.CFB;
    if (modeString === 'ctr') return CryptoJS.mode.CTR;
    if (modeString === 'ofb') return CryptoJS.mode.OFB;

    throw new Error(`AES mode ${modeString} not supported.`);
}