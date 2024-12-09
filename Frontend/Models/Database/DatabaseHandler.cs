using Microsoft.Data.SqlClient;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Frontend.Models.Database {

    /*public enum DatabaseObject {
        Instruments,
        PriceData,
        TradingStrategies,
        StrategyResults
    }*/

    public class DatabaseHandler {
        private string _connectionString;

        public DatabaseHandler(string connectionString) {
            _connectionString = connectionString;
        }

        public async Task<List<Instrument>> GetInstrumentDataAsync(string symbol) {
            List<Instrument> instruments = new List<Instrument>();

            using (SqlConnection connection = new SqlConnection(_connectionString)) {
                await connection.OpenAsync();

                string query = "SELECT * FROM Instruments WHERE InstrumentSymbol = @symbol";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@symbol", symbol);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) {
                        var instrument = new Instrument(
                            reader.GetInt32(reader.GetOrdinal("InstrumentID")),
                            reader.GetString(reader.GetOrdinal("InstrumentName")),
                            reader.GetString(reader.GetOrdinal("InstrumentSymbol")),
                            reader.GetString(reader.GetOrdinal("InstrumentType")),
                            reader.GetString(reader.GetOrdinal("InstrumentCurrency"))
                        );
                        instruments.Add(instrument);
                    }
                }
            }
            return instruments;
        }

        public async Task<List<Instrument>> GetInstrumentDataAsync() {
            List<Instrument> instruments = new List<Instrument>();

            using (SqlConnection connection = new SqlConnection(_connectionString)) {
                await connection.OpenAsync();


                string query = "SELECT * FROM Instruments";
                SqlCommand cmd = new SqlCommand(query, connection);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync())
                    {
                        var instrument = new Instrument(
                            reader.GetInt32(reader.GetOrdinal("InstrumentID")),
                            reader.GetString(reader.GetOrdinal("InstrumentName")),
                            reader.GetString(reader.GetOrdinal("InstrumentSymbol")),
                            reader.GetString(reader.GetOrdinal("InstrumentType")),
                            reader.GetString(reader.GetOrdinal("InstrumentCurrency"))
                        );
                        instruments.Add(instrument);
                    }
                }
            }
            return instruments;
        }

        public async Task AddInstrumentDataAsync(string name, string symbol, string type, string currency) {
            using (SqlConnection connection = new SqlConnection(_connectionString)) {
                await connection.OpenAsync();

                string query = "INSERT INTO Instruments (InstrumentName,InstrumentSymbol,InstrumentType,InstrumentCurrency) VALUES (@name, @symbol, @type, @currency)";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@symbol", symbol);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@currency", currency);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<PriceData>> GetPriceDataAsync(string symbol) {
            List<PriceData> priceDataList = new List<PriceData>();

            using (SqlConnection connection = new SqlConnection(_connectionString)) {
                await connection.OpenAsync();

                string query = $"SELECT p.*, i.InstrumentSymbol FROM PriceData p INNER JOIN Instruments i ON p.InstrumentID = i.InstrumentID WHERE i.InstrumentSymbol = '@symbol';";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@symbol", symbol);

                using (SqlDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        PriceData priceData = new PriceData(
                            reader.GetInt32(reader.GetOrdinal("PriceID")),
                            reader.GetInt32(reader.GetOrdinal("InstrumentID")),
                            reader.GetDateTime(reader.GetOrdinal("PxDate")),
                            reader.GetDouble(reader.GetOrdinal("OpenPx")),
                            reader.GetDouble(reader.GetOrdinal("ClosePx")),
                            reader.GetDouble(reader.GetOrdinal("HighPx")),
                            reader.GetDouble(reader.GetOrdinal("LowPx")),
                            reader.GetInt32(reader.GetOrdinal("Volume"))
                        );

                        priceDataList.Add(priceData);
                    }
                }
            }

            return priceDataList;
        }

        public async Task AddPriceDataAsync(string symbol, string pxDate, double openPx, double closePx, double highPx, double lowPx, double volume) {
            int instrumentID = await GetInstrumentIDAsync(symbol);

            using (SqlConnection connection = new SqlConnection(_connectionString)) {
                await connection.OpenAsync();

                string query = "INSERT INTO PriceData (InstrumentID,PxDate,OpenPx,ClosePx,HighPx,LowPx,Volume) VALUES (@instrumentID,@pxDate,@openPx,@closePx,@highPx,@lowPx,@volume);";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@instrumentID", instrumentID);
                cmd.Parameters.AddWithValue("@pxDate", pxDate);
                cmd.Parameters.AddWithValue("@openPx", openPx);
                cmd.Parameters.AddWithValue("@closePx", closePx);
                cmd.Parameters.AddWithValue("@highPx", highPx);
                cmd.Parameters.AddWithValue("@lowPx", lowPx);
                cmd.Parameters.AddWithValue("@volume", volume);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task<int> GetInstrumentIDAsync(string symbol) {
            int instrumentID = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString)) {
                await connection.OpenAsync();

                string query = "SELECT InstrumentID FROM Instruments WHERE InstrumentSymbol = @symbol";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@symbol", symbol);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) {
                        instrumentID = reader.GetInt32(reader.GetOrdinal("InstrumentID"));
                    }
                }
            }

            return instrumentID;
        }
    }
}
