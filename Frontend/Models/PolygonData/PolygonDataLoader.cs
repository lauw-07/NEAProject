using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Frontend.Models.PolygonData {
    public class PolygonDataLoader {
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _options = new() {
            PropertyNameCaseInsensitive = true,
        };

        public PolygonDataLoader(string baseUrl, string apiKey) {
            _httpClient = new HttpClient();
            _baseUrl = baseUrl;
            _apiKey = apiKey;
        }

        private string? _ticker;
        private int? _multiplier;
        private string? _timespan;
        private string? _dateFrom;
        private string? _dateTo;

        public void SetParameters(string ticker, int multiplier, string timespan, string dateFrom, string dateTo) {
            _ticker = ticker;
            _multiplier = multiplier;
            _timespan = timespan;
            _dateFrom = dateFrom;
            _dateTo = dateTo;
        }

        public async Task<PolygonStockPriceData> LoadPolygonStockDataAsync() {
            string url = $"{_baseUrl}ticker/{_ticker}/range/{_multiplier}/{_timespan}/{_dateFrom}/{_dateTo}?adjusted=true&sort=asc&apiKey={_apiKey}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            try {
                response.EnsureSuccessStatusCode();
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return new PolygonStockPriceData();
            }

            string responseBody = await response.Content.ReadAsStringAsync();

            try {
                var priceData = JsonSerializer.Deserialize<PolygonStockPriceData>(responseBody, _options);

                if (priceData == null) {
                    await Console.Out.WriteLineAsync("Object is null");
                    return new PolygonStockPriceData();
                }

                await Console.Out.WriteLineAsync("Object is not null");
                return priceData;
            } catch (JsonException ex) {
                await Console.Out.WriteLineAsync($"JSON Serialisation Error: {ex.Message}");
                return new PolygonStockPriceData();
            }

        }
    }
}


/* {"ticker":"AAPL","queryCount":24,"resultsCount":24,"adjusted":true,"_results":[{"v":7.0790813e+07,"vw":131.6292,"o":130.465,"c":130.15,"h":133.41,"l":129.89,"t":1673240400000,"n":645365},{"v":6.3896155e+07,"vw":129.822,"o":130.26,"c":130.73,"h":131.2636,"l":128.12,"t":1673326800000,"n":554940},{"v":6.9458949e+07,"vw":132.3081,"o":131.25,"c":133.49,"h":133.51,"l":130.46,"t":1673413200000,"n":561278},{"v":7.1379648e+07,"vw":133.171,"o":133.88,"c":133.41,"h":134.26,"l":131.44,"t":1673499600000,"n":635331},{"v":5.7809719e+07,"vw":133.6773,"o":132.03,"c":134.76,"h":134.92,"l":131.66,"t":1673586000000,"n":537385},{"v":6.3612627e+07,"vw":135.7587,"o":134.83,"c":135.94,"h":137.29,"l":134.13,"t":1673931600000,"n":595831},{"v":6.96728e+07,"vw":136.3316,"o":136.815,"c":135.21,"h":138.61,"l":135.03,"t":1674018000000,"n":578304},{"v":5.8280413e+07,"vw":134.9653,"o":134.08,"c":135.27,"h":136.25,"l":133.77,"t":1674104400000,"n":491674},{"v":8.0200655e+07,"vw":136.3762,"o":135.28,"c":137.87,"h":138.02,"l":134.22,"t":1674190800000,"n":552230},{"v":8.1760313e+07,"vw":141.2116,"o":138.12,"c":141.11,"h":143.315,"l":137.9,"t":1674450000000,"n":719288},{"v":6.6435142e+07,"vw":142.0507,"o":140.305,"c":142.53,"h":143.16,"l":140.3,"t":1674536400000,"n":498679},{"v":6.5799349e+07,"vw":140.7526,"o":140.89,"c":141.86,"h":142.43,"l":138.81,"t":1674622800000,"n":536505},{"v":5.4105068e+07,"vw":143.3429,"o":143.17,"c":143.96,"h":144.25,"l":141.9,"t":1674709200000,"n":472135},{"v":7.0547743e+07,"vw":145.8365,"o":143.155,"c":145.93,"h":147.23,"l":143.08,"t":1674795600000,"n":560022},{"v":6.4015274e+07,"vw":143.6524,"o":144.955,"c":143,"h":145.55,"l":142.85,"t":1675054800000,"n":551111},{"v":6.5874459e+07,"vw":143.6473,"o":142.7,"c":144.29,"h":144.34,"l":142.28,"t":1675141200000,"n":468170},{"v":7.7663426e+07,"vw":143.8723,"o":143.97,"c":145.43,"h":146.61,"l":141.32,"t":1675227600000,"n":693374},{"v":1.1833898e+08,"vw":149.3764,"o":148.9,"c":150.82,"h":151.18,"l":148.17,"t":1675314000000,"n":996203},{"v":1.54338835e+08,"vw":154.2437,"o":148.03,"c":154.5,"h":157.38,"l":147.83,"t":1675400400000,"n":1141350},{"v":6.9771906e+07,"vw":152.0939,"o":152.575,"c":151.73,"h":153.1,"l":150.78,"t":1675659600000,"n":583517},{"v":8.3322551e+07,"vw":153.4202,"o":150.64,"c":154.65,"h":155.23,"l":150.64,"t":1675746000000,"n":661767},{"v":6.3620079e+07,"vw":152.3636,"o":153.88,"c":151.92,"h":154.58,"l":151.168,"t":1675832400000,"n":524140},{"v":5.5994243e+07,"vw":152.2769,"o":153.775,"c":150.87,"h":154.33,"l":150.42,"t":1675918800000,"n":471973},{"v":5.7388108e+07,"vw":150.4054,"o":149.46,"c":151.01,"h":151.3401,"l":149.22,"t":1676005200000,"n":443405}],"status":"OK","request_id":"113af1a18af9155b6d9a14c78fe5eb42","count":24}
 */