using Microsoft.AspNetCore.Components;

namespace PKHeX.Web.Services;

public partial class NewsService
{
    public List<News> AllNews => _news.OrderByDescending(n => n.Date).ToList();
    public DateOnly LatestNewsDate => _news.MaxBy(n => n.Date)!.Date;
    
    public record News(
        DateOnly Date,
        RenderFragment Fragment);
}