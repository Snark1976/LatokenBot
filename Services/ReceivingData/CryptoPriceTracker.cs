using LatokenBot.Services.AI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LatokenBot.Services.ReceivingData;

public class CryptoPriceTracker
{
    public static async Task<List<(DateTime date, decimal price)>> GetPriceHistoryAsync(ParsedRequest parsedRequest)
    {
        DateTime today = DateTime.UtcNow.Date;
        long startUnix = new DateTimeOffset(today.AddDays(-parsedRequest.PeriodDays)).ToUnixTimeSeconds();
        long endUnix = new DateTimeOffset(today).ToUnixTimeSeconds();
        HttpClient httpClient = new();
        string url = $"https://api.coingecko.com/api/v3/coins/{parsedRequest.CryptoName}/market_chart/range?vs_currency=usd&from={startUnix}&to={endUnix}";

        HttpResponseMessage response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<MarketChartResponse>(json)!;

        return data.Prices.Select(p => (UnixTimeStampToDateTime((long)p[0]), (decimal)p[1])).ToList();
    }

    private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp).DateTime;
    }

    public record MarketChartResponse(
        [property: JsonPropertyName("prices")] IReadOnlyList<List<double>> Prices,
        [property: JsonPropertyName("market_caps")] IReadOnlyList<List<double>> MarketCaps,
        [property: JsonPropertyName("total_volumes")] IReadOnlyList<List<double>> TotalVolumes
    );
}
