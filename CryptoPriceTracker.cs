using System.Text.Json;
using System.Text.Json.Serialization;

namespace LatokenBot;

public class CryptoPriceTracker
{
    public static async Task<List<(DateTime date, decimal price)>> GetPriceHistoryAsync(
        string cryptoId, string currency, DateTime startDate, DateTime endDate)
    {
        long startUnix = new DateTimeOffset(startDate).ToUnixTimeSeconds();
        long endUnix = new DateTimeOffset(endDate).ToUnixTimeSeconds();
        HttpClient httpClient = new();
        string url = $"https://api.coingecko.com/api/v3/coins/{cryptoId}/market_chart/range?vs_currency={currency}&from={startUnix}&to={endUnix}";

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
