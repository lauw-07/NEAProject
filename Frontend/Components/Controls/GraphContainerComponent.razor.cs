﻿using Frontend.Models.Database;
using Frontend.Models.PolygonData;
using Frontend.Models.Timeseries;

namespace Frontend.Components.Controls {
    public partial class GraphContainerComponent {
        private string? Ticker { get; set; }
        private int Multiplier { get; set; }
        private string? Timespan { get; set; }
        private DateTime DateFrom { get; set; }
        private DateTime DateTo { get; set; }

        private PolygonStockPriceData? polygonStockPriceData;
        private string symbol = string.Empty;
        private List<Result>? results;
        private List<TS>? TSLists;



        protected override async Task OnAfterRenderAsync(bool firstRender) {
            Console.WriteLine("OnAfterRenderAsync triggered in GraphContainer component");

            if (firstRender) {
                Console.WriteLine("Fetching intial data from database");
                symbol = await databaseHandler.GetInstrumentByIDAsync(1);
                Console.WriteLine($"Symbol: {symbol}");
                TSLists = await ReadFromDatabase();
                Console.WriteLine(TSLists);

                StateHasChanged();

            }
        }

        private async Task FetchData() {
            await LoadData();
            await SaveToDatabase();
            TSLists = await ReadFromDatabase();
        }

        private async Task LoadData() {
            if (string.IsNullOrEmpty(Ticker) || string.IsNullOrEmpty(Timespan)) {
                return;
            }

            polygonDataLoader.SetParameters(Ticker, Multiplier, Timespan, DateFrom.ToString("yyyy-MM-dd"), DateTo.ToString("yyyy-MM-dd"));

            polygonStockPriceData = await polygonDataLoader.LoadPolygonStockDataAsync();

            symbol = polygonStockPriceData.Ticker;
            results = polygonStockPriceData.Results;
        }

        private async Task SaveToDatabase() {
            if (results == null || results.Count == 0) {
                return;
            }
            foreach (Result result in results) {
                DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds(result.Timestamp);
                string pxDate = offset.Date.ToShortDateString();
                double openPx = result.Open;
                double closePx = result.Close;
                double highPx = result.High;
                double lowPx = result.Low;
                double volume = result.Volume;

                try {
                    await databaseHandler.AddPriceDataAsync(symbol, pxDate, openPx, closePx, highPx, lowPx, volume);
                    await databaseHandler.AddInstrumentDataAsync(symbol);
                } catch (Exception ex) {
                    Console.WriteLine($"An error occurred while adding price data: {ex.Message}\n");
                }
            }
        }


        private async Task<List<TS>> ReadFromDatabase() {
            List<PriceData> priceData = await databaseHandler.GetPriceDataAsync(symbol);
            int initialCapacity = priceData.Count;

            TS openPxTimeseries = new TS(initialCapacity);
            TS closePxTimeseries = new TS(initialCapacity);

            /*
             * Each price data object contains single prices related to a single timestamp
             */

            foreach (PriceData priceDataObj in priceData) {
                double openPx = priceDataObj.OpenPx;
                double closePx = priceDataObj.ClosePx;
                DateTime pxDate = priceDataObj.PxDate;

                openPxTimeseries.Add(pxDate, openPx);
                closePxTimeseries.Add(pxDate, closePx);
            }

            return new List<TS> { openPxTimeseries, closePxTimeseries };
        }

        /* Store this new data into the database (however i haven't validated whether the database already contains this new data loaded)
         * Read all the data relating to the specific Ticker passed in from the input from the database
         * Convert data into a timeseries
         * Create two timeseries for opening and closing price
         * Store this in the data variable and pass this through as a parameter to the graph component
         * 
         * I will also need to update the database with new instruments and new price data
         */
    }
}