using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YahooFinanceApi;

namespace StockPredictorAI.Data
{
    public class StockDataLoader : IStockDataLoader
    {
        private readonly IDataProvider _dataProvider;
        private readonly IStockDataConverter _stockDataConverter;

        public StockDataLoader(IDataProvider dataProvider, IStockDataConverter stockDataConverter)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _stockDataConverter = stockDataConverter ?? throw new ArgumentNullException(nameof(stockDataConverter));
        }
        
        public async Task<List<StockData>> LoadDataAsync(string stockSymbol, DateTime startDate, DateTime endDate)
        {
            try
            {
                var interval = TimeSpan.FromDays(1);
                var historicalData = await _dataProvider.GetHistoricalDataAsync(stockSymbol, startDate, endDate, interval);
                return _stockDataConverter.ConvertToStockDataList(historicalData);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new List<StockData>();
            }
        }
        
        public async Task<List<decimal>> LoadClosingPricesAsync(string stockSymbol, DateTime startDate, DateTime endDate)
        {
            var stockDataList = await LoadDataAsync(stockSymbol, startDate, endDate);
            return stockDataList.Select(data => data.Close).ToList();
        }

        private void LogError(Exception ex)
        {
            Console.WriteLine($"Error fetching stock data: {ex.Message}");
        }
    }

    public interface IStockDataLoader
    {
        Task<List<StockData>> LoadDataAsync(string stockSymbol, DateTime startDate, DateTime endDate);
        Task<List<decimal>> LoadClosingPricesAsync(string stockSymbol, DateTime startDate, DateTime endDate);
    }

    public interface IDataProvider
    {
        Task<IEnumerable<Candle>> GetHistoricalDataAsync(string stockSymbol, DateTime startDate, DateTime endDate, TimeSpan interval);
    }

    public interface IStockDataConverter
    {
        List<StockData> ConvertToStockDataList(IEnumerable<Candle> candles);
    }

    public class StockDataConverter : IStockDataConverter
    {
        public List<StockData> ConvertToStockDataList(IEnumerable<Candle> candles)
        {
            return candles.Select(data => new StockData
            {
                Date = data.DateTime,
                Open = data.Open,
                High = data.High,
                Low = data.Low,
                Close = data.Close,
                Volume = data.Volume
            }).ToList();
        }
    }

    public class StockData
    {
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }
}
