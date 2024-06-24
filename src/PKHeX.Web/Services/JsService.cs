using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PKHeX.Web.Services;

public class JsService(IJSRuntime js)
{
    public async ValueTask DownloadFile(Stream stream, string fileName)
    {
        using var streamRef = new DotNetStreamReference(stream);
        await js.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }

    public ValueTask ClickOnAsync(ElementReference? element) => js.InvokeVoidAsync("HTMLElement.prototype.click.call",
        element);
}