using Frontend.Models.Database;

namespace Frontend.Models.Backtest {
    public class Position {
        private double _quantity = 0;
        private double _exposure = 0;
        private DateTime _lastUpdate;
        private Instrument _instrument;
        private double _closePnl = 0;

        public Position(Instrument instrument) {
            _instrument = instrument;
        }

        public Instrument GetInstrument() {
            return _instrument;
        }

        public double GetUnitExposure() {
            return _exposure / _quantity;
        }

        public double GetQuantity() {
            return _quantity;
        }
        public void SetQuantity(double quantity) {
            _quantity = quantity;
        }
        public double GetExposure() {
            return _exposure;
        }
        public void SetExposure(double exposure) {
            _exposure = exposure;
        }
        public void SetLastUpdate(DateTime lastUpdate) {
            _lastUpdate = lastUpdate;
        }
        public DateTime GetLastUpdate() {
            return _lastUpdate;
        }
        public double GetClosePnl() {
            return _closePnl;
        }
        public void SetClosePnl(double closePnl) {
            _closePnl = closePnl;
        }

        public void Add(double delta, DateTime timestamp, double price) {
            PerformUpdate(this, delta, timestamp, price);
        }

        public static void PerformUpdate(Position position, double delta, DateTime timestamp, double price) {
            double quantity = double.NaN;
            double exposure = double.NaN;
            double unitExposure = double.NaN;
            double closePnl = double.NaN;
            bool isClose = false;

            if (double.IsNaN(price)) {
                return;
            }

            quantity = position.GetQuantity() + delta;
            unitExposure = position.GetInstrument().GetUnitExposure(price);

            isClose = Math.Sign(delta) * Math.Sign(position.GetQuantity()) < 0; // if negative, then it is a closing trade
            if (isClose) {
                exposure = position.GetExposure() * (quantity / position.GetQuantity());
                closePnl = position.GetClosePnl() - (unitExposure - position.GetUnitExposure()) * delta;
            } else {
                exposure = position.GetExposure() + delta * unitExposure;
            }

            position.SetQuantity(quantity);
            position.SetLastUpdate(timestamp);
            position.SetExposure(exposure);

            if (isClose) {
                position.SetClosePnl(closePnl);
            }
        }

        public double GetPnl(double price) {
            double openPnl = price * GetQuantity() - GetExposure();
            return Math.Round(GetClosePnl() + openPnl, 2);
        }
    }
}
