using System.Globalization;
using NasdaqTrader.Bot.Core;

namespace NasdaqTraderSystem.Core;

public class StockLoader
{
    private string[] _selectedCsvs;
    private string _dataFolder;

    public StockLoader(string dataFolder, int amountOfStocks)
    {
        var random = new Random();
        string[] csvFiles = Directory.GetFiles(dataFolder, "*.csv");

        _selectedCsvs = new string[amountOfStocks];
        for (int i = 0; i < amountOfStocks; i++)
        {
            int index = random.Next(0, csvFiles.Length);
            string selectedCsv = csvFiles[index];
            while (string.IsNullOrWhiteSpace(selectedCsv))
            {
                index = random.Next(0, csvFiles.Length);
                selectedCsv = csvFiles[index];
            }

            csvFiles[index] = "";
            _selectedCsvs[i] = selectedCsv;
        }

        _dataFolder = dataFolder;
    }

    public List<IStockListing> GetListings(DateOnly from, DateOnly to)
    {
        List<StockListing> listings = new List<StockListing>();

        //File.ReadAllText(_dataFolder + "\\Tickers.txt").Split(Environment.NewLine).sele;

        foreach (var csvFileName in _selectedCsvs)
        {
            var stockListing = new StockListing()
            {
                Ticker = Path.GetFileNameWithoutExtension(csvFileName)
            };
            string[][] csvContent = File.ReadAllText(csvFileName).Split(Environment.NewLine)
                .Select(c => c.Split(',')).ToArray();

            List<PricePoint> pricePoints = new List<PricePoint>();
            for (int i = 3; i < csvContent.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(csvContent[i][0]))
                {
                    continue;
                }
                DateOnly date = DateOnly.ParseExact(csvContent[i][0], "yyyy-MM-dd");
                if (date >= from && date <= to)
                {
                    pricePoints.Add(new PricePoint()
                    {
                        Date = date,
                        Price = decimal.Parse(csvContent[i][1],CultureInfo.InvariantCulture)
                    });
                }
            }

            stockListing.PricePoints = pricePoints.OfType<IPricePoint>().ToArray();
            listings.Add(stockListing);
        }

        string[][] tickerInfo = File.ReadAllText(Path.Combine(_dataFolder, "Tickers.txt")).Split(Environment.NewLine)
            .Select(c => c.Split('|')).ToArray();
        foreach (var info in tickerInfo)
        {
            var listing = listings.FirstOrDefault(c => c.Ticker == info[1]);
            if (listing != null)
            {
                listing.Name = info[2];
            }
        }

        tickerInfo = null;
        
        return listings.OfType<IStockListing>().ToList();
    }

    public void Dispose()
    {
        _selectedCsvs = null;
        _dataFolder = null;
    }
}