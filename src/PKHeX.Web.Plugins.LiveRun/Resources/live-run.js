const webretroUrl = window.location.href.includes("//localhost:")
    ? "http://127.0.0.1:5500/"
    : "https://pkhex-web.github.io/webretro/"

const roms = [
    { versions: ["RBY", "RD", "GN", "BU", "YW"], setting: "Red/Blue/Yellow ROM", extension: "gb" },
    { versions: ["GSC", "GS", "GD", "SI", "C"], setting: "Gold/Silver/Crystal ROM", extension: "gbc" },
    { versions: ["FRLG", "FR", "LG"], setting: "FireRed/LeafGreen ROM", extension: "gba" },
    { versions: ["E"], setting: "Emerald ROM", extension: "gba" },
    { versions: ["RS", "R", "S"], setting: "Ruby/Sapphire ROM", extension: "gba" },
]

export function mount(element, ctx) {
    const container = document.createElement("div")
    container.style.width = "100%"
    container.style.height = "100%"
    element.appendChild(container)

    let save
    let frame
    let unmounted = false
    const receiveMessage = async (event) => {
        if (!frame || event.source !== frame.contentWindow) return

        const data = event.data
        if (typeof data !== "object" || data === null || !("type" in data)) return

        if (data.type === "new_save_available") {
            const yes = confirm("A new save has been detected, do you want to override PKHeX.Web currently loaded save?")
            if (yes) await ctx.loadSave(new Uint8Array(data.bytes), save.fileName)
        }
    }

    open().catch((error) => {
        if (!unmounted) container.textContent = error.message
    })

    async function open() {
        save = await ctx.getSave()
        if (!save) throw new Error("Load a save to play it.")

        const rom = roms.find((r) => r.versions.includes(save.version))
        if (!rom) throw new Error("This version is not supported.")

        const romBytes = await ctx.getSetting(rom.setting)
        if (!romBytes || romBytes.length === 0) throw new Error(`Upload the ${rom.setting} in the plug-in settings first.`)

        const showFrameCount = await ctx.getSetting("Show frame count")
        if (unmounted) return

        window.addEventListener("message", receiveMessage)
        frame = webretroEmbed(container, webretroUrl, { core: "mgba" })
        await waitForIframeReady(frame)
        if (unmounted) return

        frame.contentWindow.postMessage({
            type: "load_game",
            saveFile: { name: save.fileName, bytes: save.bytes },
            romFile: { name: `live-run.${rom.extension}`, bytes: romBytes },
            showFrameCount: showFrameCount === true,
        }, "*")
    }

    return () => {
        unmounted = true
        window.removeEventListener("message", receiveMessage)
        container.remove()
    }
}

function webretroEmbed(node, path, queries) {
    const frame = document.createElement("iframe")
    frame.id = "emulator-frame"
    frame.style = "border: none; display: block; width: 100%; height: 100%;"
    frame.src = path + "?" + new URLSearchParams(queries)
    node.appendChild(frame)

    return frame
}

function waitForIframeReady(frame) {
    return new Promise((resolve) => {
        const onMessage = (event) => {
            if (event.source === frame.contentWindow && event.data === "iframe-ready") {
                window.removeEventListener("message", onMessage)
                resolve()
            }
        }

        window.addEventListener("message", onMessage)
    })
}
