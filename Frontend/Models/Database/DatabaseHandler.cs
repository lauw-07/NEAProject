using Microsoft.Data.SqlClient;

namespace Frontend.Models.Database {
    public class DatabaseHandler {
        private string _connectionString;

        public DatabaseHandler(string connectionString) {
            _connectionString = connectionString;
        }
        public async Task<Instrument> GetInstrumentDataAsync(string symbol) {
            Instrument? instrument = null;
            using (SqlConnection connection = new SqlConnection(_connectionString)) {
                await connection.OpenAsync();

                string query = "SELECT * FROM Instruments WHERE InstrumentSymbol = @symbol";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@symbol", symbol);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) {
                        instrument = new Instrument(
                            reader.GetInt32(reader.GetOrdinal("InstrumentID")),
                            reader.GetString(reader.GetOrdinal("InstrumentName")),
                            reader.GetString(reader.GetOrdinal("InstrumentSymbol")),
                            reader.GetString(reader.GetOrdinal("InstrumentType")),
                            reader.GetString(reader.GetOrdinal("InstrumentCurrency"))
                        );
                    }
                }
            }
            return instrument;
        }

        public async Task<string> GetInstrumentByIDAsync(int id) {
            string symbol = "";
            using (SqlConnection connection = new SqlConnection(_connectionString)) {
                await connection.OpenAsync();

                string query = "SELECT InstrumentSymbol FROM Instruments WHERE InstrumentID = @id";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) {
                        symbol = reader.GetString(0);
                    }
                }
            }
            return symbol;
        }

        public async Task<string> GetInstrumentByNameAsync(string? name) {
            if (name == null) {
                //Just a default market because I know that my database has some data for this market already
                return "AAPL";
            }
            string symbol = "";
            using (SqlConnection connection = new SqlConnection(_connectionString)) {
                await connection.OpenAsync();

                string query = "SELECT InstrumentSymbol FROM Instruments WHERE InstrumentName = @name";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", name);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) {
                        symbol = reader.GetString(0);
                    }
                }
            }
            return symbol;
        }

        public async Task<List<Instrument>> GetInstrumentDataByTypeAsync(string instrumentType) {
            List<Instrument> instruments = new List<Instrument>();

            using (SqlConnection connection = new SqlConnection(_connectionString)) {
                await connection.OpenAsync();

                string query = "SELECT * FROM Instruments WHERE InstrumentType = @instrumentType";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@instrumentType", instrumentType);

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

        public async Task AddInstrumentDataAsync(string symbol, string name = "", string type = "", string currency = "") {
            using (SqlConnection connection = new SqlConnection(_connectionString)) {
                await connection.OpenAsync();

                //MUST CHECK THAT DATABASE DOES NOT ALREADY CONTAIN DATA BEING INSERTED (NOT TOO HARD THOUGH)
                string query = "IF NOT EXISTS(SELECT 1 FROM Instruments WHERE InstrumentSymbol = @symbol) BEGIN INSERT INTO Instruments(InstrumentName, InstrumentSymbol, InstrumentType, InstrumentCurrency) VALUES(@name, @symbol, @type, @currency) END";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", (name == "") ? "NULL" : name);
                cmd.Parameters.AddWithValue("@symbol", symbol);
                cmd.Parameters.AddWithValue("@type", (type == "") ? "NULL" : type);
                cmd.Parameters.AddWithValue("@currency", (currency == "") ? "NULL" : currency);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<PriceData>> GetPriceDataAsync(string symbol) {
            List<PriceData> priceDataList = new List<PriceData>();

            using (SqlConnection connection = new SqlConnection(_connectionString)) {
                await connection.OpenAsync();

                string query = $"SELECT p.* FROM PriceData p INNER JOIN Instruments i ON p.InstrumentID = i.InstrumentID WHERE i.InstrumentSymbol = @symbol ORDER BY p.PxDate ASC";
                //price data is fetched from oldest prices to newest prices
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
                            reader.GetDouble(reader.GetOrdinal("Volume"))
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

                string query = "INSERT INTO PriceData (InstrumentID, PxDate, OpenPx, ClosePx, HighPx, LowPx, Volume) \r\nSELECT @instrumentID, @pxDate, @openPx, @closePx, @highPx, @lowPx, @volume\r\nWHERE NOT EXISTS (\r\n\tSELECT 1\r\n\tFROM PriceData\r\n\tWHERE InstrumentID = @instrumentID AND PxDate = @pxDate\r\n);";

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