let pages

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
    pages ??= devServerUrl
        ? fromDevServer(devServerUrl).catch((error) => {
            console.warn(`Vite dev server at ${devServerUrl} is not reachable, using the built React pages.`, error)
            return fromBuild()
        })
        : fromBuild()
    return pages
}

const elements = new Map()

export async function mount(id, element, devServerUrl, name, props, navigator, theme) {
    elements.set(id, element)
    const module = await load(devServerUrl)
    await module.mount(element, name, props, { navigator, theme })
}

export async function unmount(id) {
    const element = elements.get(id)
    elements.delete(id)
    if (element) (await pages)?.unmount(element)
}

export async function setTheme(theme) {
    (await pages)?.setTheme(theme)
}
