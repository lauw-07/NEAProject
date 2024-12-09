using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Frontend.Models.PolygonData;
using Frontend.Models.Database;

namespace Frontend.Models {
    public class DataHandler {
        PolygonDataLoader DataLoader { get; set; }
        DatabaseHandler DatabaseHandler { get; set; }
        public DataHandler(PolygonDataLoader dataLoader, DatabaseHandler databaseHandler) {
            DataLoader = dataLoader;
            DatabaseHandler = databaseHandler;
        }

        //All objects and data

        

        //All Functions

        public async Task<PolygonStockPriceData> LoadDataAsync() {
            var config = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();

            string baseUrl = config["BaseUrl"] ?? string.Empty;
            string apiKey = config["ApiKey"] ?? string.Empty;

            PolygonStockPriceData polygonPriceData = await DataLoader.LoadPolygonStockDataAsync();

            return polygonPriceData;
        }

        public async Task SavePriceDataToDatabase(PolygonStockPriceData stockPriceData) {
            if (stockPriceData != null) {
                string symbol = stockPriceData.Ticker;

                List<Result> results = stockPriceData.Results;
                int count = 0;
                //Temporary values, these are all in the results
                foreach (Result result in results) {
                    DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds(result.Timestamp);
                    string pxDate = offset.Date.ToShortDateString();
                    double openPx = result.Open;
                    double closePx = result.Close;
                    double highPx = result.High;
                    double lowPx = result.Low;
                    double volume = result.Volume;

                    try {
                        await DatabaseHandler.AddPriceDataAsync(symbol, pxDate, openPx, closePx, highPx, lowPx, volume);
                        count++;
                    } catch (Exception ex) {
                        Console.WriteLine($"An error occurred while adding price data: {ex.Message}\n");
                    }
                }
                Console.WriteLine($"{count} entries saved to database");
            }
        }

        public async Task<List<Instrument>> FetchInstrumentsAsync() {
            return await DatabaseHandler.GetInstrumentDataAsync();
        }

        public async Task<List<Instrument>> FetchInstrumentAsync(string symbol) {
            return await DatabaseHandler.GetInstrumentDataAsync(symbol);
        }

        public async Task<List<PriceData>> FetchPriceDataAsync(string symbol) {
            return await DatabaseHandler.GetPriceDataAsync(symbol);
        }
    }
}
