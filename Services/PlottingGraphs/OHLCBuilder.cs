using ScottPlot;


namespace LatokenBot.Services.PlottingGraphs;

public class OHLCBuilder
{
    public static List<OHLC> GenerateOHLC(List<(DateTime date, decimal price)> priceHistory, TimeSpan timeSpan) => priceHistory
        .GroupBy(
            p => p.date.Ticks / timeSpan.Ticks,
            p => p,
            (key, group) =>
            {
                var orderedGroup = group.OrderBy(p => p.date).ToList();

                return new OHLC(
                    open: (double)orderedGroup.First().price,
                    high: (double)orderedGroup.Max(p => p.price),
                    low: (double)orderedGroup.Min(p => p.price),
                    close: (double)orderedGroup.Last().price,
                    start: orderedGroup.First().date,
                    span: timeSpan
                );
            }
        ).ToList();
}