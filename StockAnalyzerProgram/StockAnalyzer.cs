using Newtonsoft.Json.Linq;

namespace StockAnalyzerProgram
{
    public class StockAnalyzer
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public StockAnalyzer(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
        }

        public async Task<(string stockName, decimal currentPrice)> GetStockInfo(string symbol)
        {
            string url = $"https://www.alphavantage.co/query?function=OVERVIEW&symbol={symbol}&apikey={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);
            var json = JObject.Parse(response);

            if (json.ContainsKey("Information"))
            {
                Console.WriteLine("API Rate Limit Exceeded: " + json["Information"].ToString());
                throw new Exception("API Rate Limit Exceeded");
            }

            if (json["Name"] == null)
            {
                throw new Exception("Stock name not found in response.");
            }
            string stockName = json["Name"].ToString();

            url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={_apiKey}";
            response = await _httpClient.GetStringAsync(url);
            json = JObject.Parse(response);

            if (json.ContainsKey("Information"))
            {
                Console.WriteLine("API Rate Limit Exceeded: " + json["Information"].ToString());
                throw new Exception("API Rate Limit Exceeded");
            }

            if (json["Time Series (Daily)"] == null)
            {
                throw new Exception("Time series data not found in response.");
            }

            var latestData = json["Time Series (Daily)"].First.First;
            if (latestData["4. close"] == null)
            {
                throw new Exception("Closing price not found in response.");
            }

            decimal currentPrice = decimal.Parse(latestData["4. close"].ToString());

            return (stockName, currentPrice);
        }

        public decimal CalculateMovingAverage(List<decimal> prices, int period)
        {
            if (prices.Count < period) throw new ArgumentException("Not enough data points.");
            decimal sum = 0;
            for (int i = prices.Count - period; i < prices.Count; i++)
            {
                sum += prices[i];
            }

            return sum / period;
        }

        public void PerformAnalysis(string symbol)
        {
            try
            {
                var stockInfo = GetStockInfo(symbol).Result;
                var prices = GetHistoricalPrices(symbol).Result;
                decimal currentPrice = stockInfo.currentPrice;
                decimal movingAverage20 = CalculateMovingAverage(prices, 20);
                decimal movingAverage50 = CalculateMovingAverage(prices, 50);
                decimal movingAverage200 = CalculateMovingAverage(prices, 200);

                Console.WriteLine($"Stock: {stockInfo.stockName} ({symbol})");
                Console.WriteLine($"Current Price: {currentPrice}");
                Console.WriteLine($"20-Day Moving Average: {movingAverage20}");
                Console.WriteLine($"50-Day Moving Average: {movingAverage50}");
                Console.WriteLine($"200-Day Moving Average: {movingAverage200}");

                if (currentPrice > movingAverage20)
                {
                    Console.WriteLine("Recommendation: Buy");
                }
                else
                {
                    Console.WriteLine("Recommendation: Sell");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private async Task<List<decimal>> GetHistoricalPrices(string symbol)
        {
            string url =
                $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);
            var json = JObject.Parse(response);

            if (json.ContainsKey("Information"))
            {
                Console.WriteLine("API Rate Limit Exceeded: " + json["Information"].ToString());
                throw new Exception("API Rate Limit Exceeded");
            }

            if (json["Time Series (Daily)"] == null)
            {
                throw new Exception("Time series data not found in response.");
            }

            var prices = new List<decimal>();
            foreach (var item in json["Time Series (Daily)"])
            {
                if (item.First["4. close"] != null)
                {
                    prices.Add(decimal.Parse(item.First["4. close"].ToString()));
                }
            }

            return prices;
        }
    }
}