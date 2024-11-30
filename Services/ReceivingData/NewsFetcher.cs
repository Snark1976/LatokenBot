using NewsAPI.Constants;
using NewsAPI.Models;
using NewsAPI;

namespace LatokenBot;

public class NewsFetcher
{
    public static async Task<List<string>> FetchNewsAsync(string keyword, int days)
    {
        var newsApiClient = new NewsApiClient("3c39315147574cf0aebefea7946f50c3");
        var articlesResponse = await newsApiClient.GetEverythingAsync(new EverythingRequest
        {
            Q = keyword,
            SortBy = SortBys.Popularity,
            Language = Languages.EN,
            From = DateTime.UtcNow.AddDays(-days)
        });

        if (articlesResponse.Status != Statuses.Ok) throw new InvalidOperationException("failed to upload news");

        List<string> news = articlesResponse.Articles
            .Select(ar => $"Title: {ar.Title}, Description: {ar.Description}, Content: {ar.Content}, PublishedAt: {ar.PublishedAt}")
            .ToList();
        return news;
    }
}
