window.blazeJump.aesEncrypt = (sharedX, message) => {
    return new Promise((resolve, reject) => {
        var encryptMessage = async function () {
            let enc = new TextEncoder();
            let encoded = enc.encode(message);
            let iv = window.crypto.getRandomValues(new Uint8Array(16));
            let encrypted = window.crypto.subtle.encrypt(
                {
                    name: "AES-CBC",
                    iv: iv,
                },
                Buffer.from(sharedX),
                encoded,
            );
            resolve(encrypted);
        }
        encryptMessage();
    });
}