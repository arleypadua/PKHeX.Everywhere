export function mount(element, ctx) {
    element.textContent = `Hello from ${ctx.plugInId}`
    return () => { element.textContent = '' }
}
