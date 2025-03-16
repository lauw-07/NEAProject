using Frontend.Models.Database;
using Frontend.Models.Timeseries;

namespace Frontend.Models.Backtest {
    public class PnLHandler {
        public PnLHandler() {

        }
        public TS CalculatePnl(Instrument instrument, List<double> targetPositions, List<DateTime> timestamps, List<double> prices) {
            // Hold all the PNLs at each timestamp
            TS pnlTs = new TS();

            // Create instance of a position object which can hold a value for the running PNL
            Position position = new Position(instrument);

            if (targetPositions.Count != prices.Count || targetPositions.Count != timestamps.Count || targetPositions.Count <= 1) {
                return new TS();
            }

            position.Add(targetPositions[0], timestamps[0], prices[0]);
            for (int i = 1; i < targetPositions.Count; i++) {
                // Calculate change in quantity of position
                double delta = targetPositions[i] - position.GetQuantity();
                DateTime timestamp = timestamps[i];
                double price = prices[i];

                position.Add(delta, timestamp, price);
                pnlTs.Add(timestamp, position.GetPnl(price));
            }

            return pnlTs;
        }
    }
}
