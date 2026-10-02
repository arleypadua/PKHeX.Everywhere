let appModule

async function fromDevServer(url) {
    const { default: refresh } = await import(`${url}/@react-refresh`)
    refresh.injectIntoGlobalHook(window)
    window.$RefreshReg$ = () => {}
    window.$RefreshSig$ = () => (type) => type
    window.__vite_plugin_react_preamble_installed__ = true
    await import(`${url}/@vite/client`)
    return import(`${url}/src/main.tsx`)
}

const fromBuild = () => import(new URL('react/pages.js', document.baseURI).href)

function load(devServerUrl) {
    appModule ??= devServerUrl
        ? fromDevServer(devServerUrl).catch((error) => {
            console.warn(`Vite dev server at ${devServerUrl} is not reachable, using the built React app.`, error)
            return fromBuild()
        })
        : fromBuild()
    return appModule
}

export async function start(devServerUrl, options) {
    (await load(devServerUrl)).start(options)
}

export async function track(devServerUrl, name, params) {
    (await load(devServerUrl)).track(name, params)
}

export async function captureBlazorError(devServerUrl, exception) {
    (await load(devServerUrl)).captureBlazorError(exception)
}
