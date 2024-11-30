using ScottPlot;

namespace LatokenBot;

public class ChartBuilder
{
    public static void PlotPriceHistory(List<(DateTime date, decimal price)> prices)
    {
        Plot plt = new();
        List<OHLC> p = OHLCBuilder.GenerateOHLC(prices, TimeSpan.FromDays(1));
        plt.Add.Candlestick(p);
        plt.Axes.DateTimeTicksBottom();

        plt.SavePng("Demo.png", 400, 300);

        Console.WriteLine("Chart saved as 'Demo.png'");
    }
}
