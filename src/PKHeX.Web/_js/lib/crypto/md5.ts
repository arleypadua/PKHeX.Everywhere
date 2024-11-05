import CryptoJS from "crypto-js";

export function md5Hash(data: string) {
    const parsedHexString =  CryptoJS.enc.Hex.parse(data);
    const hash = CryptoJS.MD5(parsedHexString);
    return hash.toString(CryptoJS.enc.Hex);
}