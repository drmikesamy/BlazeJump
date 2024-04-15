function encodeMessage(message) {
    let enc = new TextEncoder();
    return enc.encode(message);
}

function decodeMessage(message) {
    let dec = new TextDecoder();
    return dec.decode(message);
}

async function uint8ArrayToCryptoKey(uint8Array) {
    const keyArrayBuffer = uint8ArrayToArrayBuffer(uint8Array);
    const key = await crypto.subtle.importKey(
        'raw',
        keyArrayBuffer,
        { name: 'AES-CBC', length: 256 },
        false,
        ['encrypt', 'decrypt']
    );
    return key;
}

function uint8ArrayToArrayBuffer(uint8Array) {
    const buf = new ArrayBuffer(uint8Array.length - 1);
    for (let i = 1, strLen = uint8Array.length; i < strLen; i++) {
        buf[i] = uint8Array[i];
    }
    return buf;
}

function arrayBufferToString(buf) {
    return String.fromCharCode.apply(null, new Uint8Array(buf));
}

window.aesEncrypt = (plainText, sharedPoint, iv) => {
    return new Promise((resolve) => {
        var encryptText = async function () {
            const key = await uint8ArrayToCryptoKey(sharedPoint);
            let encrypted = await window.crypto.subtle.encrypt(
                {
                    name: "AES-CBC",
                    iv: iv,
                },
                key,
                plainText,
            );
            let encryptedUint8Array = arrayBufferToString(encrypted);
            resolve(encryptedUint8Array);
        }
        encryptText();
    });
}
window.aesDecrypt = (cipherText, sharedPointString, ivString) => {
    return new Promise((resolve) => {
        var keyGen = async function () {
            iv = encodeMessage(ivString);
            let decrypted = await window.crypto.subtle.decrypt(
                {
                    name: "AES-CBC",
                    iv
                },
                sharedPointString,
                cipherText
            );
            let decodedMessage = decodeMessage(decrypted);
            resolve(decodedMessage);
        }
        keyGen();
    });
}