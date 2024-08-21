const config = {
    webretroUrl: window.location.href.includes("//localhost:")
        ? "http://127.0.0.1:5500/"
        : "https://pkhex-web.github.io/webretro/"
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
    var iframe = webretroEmbed(
        document.getElementById("webretro-container"),
        openOptions.webRetroUrlOverride ?? config.webretroUrl,
        {core: "mgba"});
    
    await waitForIframeReady(iframe)
    
    iframe.contentWindow.postMessage({
        type: 'load_game',
        saveFile: openOptions.saveFile,
        romFile: openOptions.romFile
    }, '*')
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