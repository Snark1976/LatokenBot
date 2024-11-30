using NewsAPI.Constants;
using NewsAPI.Models;
using NewsAPI;
using LatokenBot.Services.AI;
using Microsoft.Extensions.Options;

namespace LatokenBot.Services.ReceivingData;
public interface INewsFetcher
{
    Task<List<string>> FetchNewsAsync(ParsedRequest parsedRequest);
}
public class NewsFetcher(NewsApiClient newsApiClient) : INewsFetcher
{
    public async Task<List<string>> FetchNewsAsync(ParsedRequest parsedRequest)
    {
        var articlesResponse = await newsApiClient.GetEverythingAsync(new EverythingRequest
        {
            Q = parsedRequest.CryptoName,
            SortBy = SortBys.Popularity,
            Language = Languages.EN,
            From = DateTime.UtcNow.AddDays(-parsedRequest.PeriodDays)
        });

        if (articlesResponse.Status != Statuses.Ok)
            throw new InvalidOperationException(articlesResponse.Error.Message);

        List<string> news = articlesResponse.Articles
            .Select(ar => $"Title: {ar.Title}, Description: {ar.Description}, Content: {ar.Content}, PublishedAt: {ar.PublishedAt}")
            .ToList();

        return news;
    }
}