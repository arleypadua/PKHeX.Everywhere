const config = {
    initialized: false,
    webretroUrl: window.location.href.includes("//localhost:")
        ? "http://127.0.0.1:5500/"
        : "https://pkhex-web.github.io/webretro/",
    currentGame: {
        saveFile: undefined, // { name: string, bytes: UInt8Array }
    }
}

function webretroEmbed(node, path, queries) {
    var frame = document.createElement("iframe");
    frame.id = "emulator-frame"
    frame.style = "border: none; display: block; width: 100%; height: 100%;";

    // change rom path to absolute if it isn't already
    if (queries.rom) {
        var link = document.createElement("a");
        link.href = (/^(https?:)?\/\//i).test(queries.rom) ? queries.rom : "roms/" + queries.rom;
        queries.rom = link.href;
    }

    frame.src = path + "?" + Object.entries(queries).map(i => i.map(i => i && encodeURIComponent(i))).map(i => i[1] ? i.join("=") : i[0]).join("&");
    node.appendChild(frame);

    return frame;
}

async function open(openOptions) {
    initialize()
    
    config.currentGame.saveFile = openOptions.saveFile
    var iframe = webretroEmbed(
        document.getElementById("webretro-container"),
        openOptions.webRetroUrlOverride ?? config.webretroUrl,
        {core: openOptions.core});
    
    await waitForIframeReady(iframe)
    
    iframe.contentWindow.postMessage(loadGameMessage(openOptions), '*')
}

function initialize() {
    if (config.initialized) return

    window.addEventListener('message', receiveMessage);
    
    config.initialized = true;
}

function unload() {
    window.removeEventListener('message', receiveMessage);
}

function receiveMessage(event) {
    const data = event.data;

    if (typeof data !== 'object') return;
    if (!("type" in data)) return;

    switch (data.type) {
        case 'new_save_available':
            const yes = confirm('A new save has been detected, do you want to override PKHeX.Web currently loaded save?')
            if (yes) {
                DotNet.invokeMethodAsync("PKHeX.Web", "OnLoadRequested", {
                    bytes: data.bytes,
                    fileName: config.currentGame.saveFile?.name
                })
            }

            break;
    }
}

async function waitForIframeReady(iframe) {
    await new Promise((resolve) => {
        const onMessage = (event) => {
            if (event.source === iframe.contentWindow && event.data === 'iframe-ready') {
                window.removeEventListener('message', onMessage);
                resolve();
            }
        };

        window.addEventListener('message', onMessage);
    });
}

function loadGameMessage(openOptions) {
    return {
        type: 'load_game',
        saveFile: openOptions.saveFile,
        romFile: openOptions.romFile,
        showFrameCount: openOptions.showFrameCount,
    }
}