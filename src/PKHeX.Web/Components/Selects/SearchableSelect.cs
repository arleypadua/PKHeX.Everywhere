using Microsoft.AspNetCore.Components;

namespace PKHeX.Web.Components.Selects;

// AntDesign's Select hides non-matching options with display:none, which breaks EnableVirtualization:
// the virtualized window keeps spanning the unfiltered list. Filtering the DataSource avoids that.
public abstract class SearchableSelect<TItem> : ComponentBase
{
    private string _searchText = string.Empty;

    protected abstract string SearchTextOf(TItem item);

    protected IEnumerable<TItem> Searched(IEnumerable<TItem> items) => string.IsNullOrEmpty(_searchText)
        ? items
        : items.Where(i => SearchTextOf(i).Contains(_searchText, StringComparison.OrdinalIgnoreCase)).ToList();

    protected void OnSearch(string text)
    {
        _searchText = text ?? string.Empty;
        StateHasChanged();
    }

    protected void OnDropdownVisibleChange(bool visible)
    {
        if (!visible) OnSearch(string.Empty);
    }
}
