using StockAnalyzerProgram;

public class Program
{
    public static void Main(string[] args)
    {
        string apiKey = "";
        var analyzer = new StockAnalyzer(apiKey);

        analyzer.PerformAnalysis("NVDA");
    }
}