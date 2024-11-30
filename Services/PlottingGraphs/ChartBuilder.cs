using ScottPlot;
using Telegram.Bot.Types;

namespace LatokenBot.Services.PlottingGraphs;

public interface IChartBuilder
{
    InputFileStream PlotPriceHistory(List<(DateTime date, decimal price)> prices, string cryptoName);
}

public class ChartBuilder : IChartBuilder
{
    public InputFileStream PlotPriceHistory(List<(DateTime date, decimal price)> prices, string cryptoName)
    {
        DateTime from = prices.MinBy(price => price.date).date;
        DateTime to = prices.MaxBy(price => price.date).date;
        Plot plt = new();

        List<OHLC> data = OHLCBuilder.GenerateOHLC(prices, TimeSpan.FromDays(1));
        plt.Add.Candlestick(data);

        plt.Axes.DateTimeTicksBottom();

        plt.Title($"Price History for {cryptoName} ({from:yyyy.MM.dd} - {to:yyyy.MM.dd})");
        plt.YLabel("Price (USD)");
        plt.XLabel("Date");

        byte[] imageBytes = plt.GetImageBytes(width: 800, height: 300, format: ImageFormat.Png);
        var stream = new MemoryStream(imageBytes);
        stream.Seek(0, SeekOrigin.Begin);
        return new(stream);
    }
}
