using Frontend.Models.Database;
using Frontend.Models.Timeseries;

namespace Frontend.Models.Backtest {
    public class PnLHandler {
        public PnLHandler() {

        }

        public TS CalculatePnl(Instrument instrument, List<double> targetPositions, List<DateTime> timestamps, List<double> prices) {
            TS pnlTs = new TS();
            Position position = new Position(instrument);

            if (targetPositions.Count != prices.Count || targetPositions.Count != timestamps.Count || targetPositions.Count <= 1) {
                return new TS();
            }

            position.Add(targetPositions[0], timestamps[0], prices[0]);
            for (int i = 1; i < targetPositions.Count; i++) {
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
