const pages = new Map()

export function mount(id, element, source, plugInId, theme, host) {
    const page = { theme }
    page.mounted = start(page, element, source, plugInId, host)
    pages.set(id, page)
    return page.mounted.then(() => undefined)
}

async function start(page, element, source, plugInId, host) {
    const url = URL.createObjectURL(new Blob([source], { type: 'text/javascript' }))
    let module
    try {
        module = await import(url)
    } finally {
        URL.revokeObjectURL(url)
    }

    const ctx = {
        plugInId,
        get theme() { return page.theme },
        getSave: () => host.invokeMethodAsync('GetSave'),
        getSetting: (key) => host.invokeMethodAsync('GetSetting', key),
        loadSave: (bytes, fileName) => host.invokeMethodAsync('LoadSave', bytes, fileName),
        navigate: (url) => void host.invokeMethodAsync('Navigate', url),
    }

    return await module.mount(element, ctx)
}

export async function unmount(id) {
    const page = pages.get(id)
    pages.delete(id)
    const unmountPage = await page?.mounted.catch(() => undefined)
    if (typeof unmountPage === 'function') unmountPage()
}

export function setTheme(id, theme) {
    const page = pages.get(id)
    if (page) page.theme = theme
}
