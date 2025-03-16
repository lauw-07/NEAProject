using Frontend.Models.Timeseries;

namespace Frontend.Models.Indicators {
    public class Ewvol : Ewma {
        private bool _useMean;
        protected double _vv = double.NaN; // exponential weighted variance

        public Ewvol(double halfLife, double seedVol, double seedMa) : base(halfLife, seedMa) {
            _useMean = !double.IsNaN(seedMa);
            _vv = Math.Pow(seedVol, 2);
            _name = "Exponential Weighted Volatility";
        }

        public override void Update(double dt, double value) {
            _decay = Math.Pow(_unitDecay, dt);
            if (_useMean) {
                // Use ewma formula but adapted for volatility
                _currentMa = _decay * _currentMa + (1 - _decay) * value;
                _vv = _decay * _vv + (1 - _decay) * Math.Pow(value - _currentMa, 2);
            } else {
                _vv = _decay * _vv + (1 - _decay) * Math.Pow(value, 2);
            }
        }

        public override void Update(TS values) {
            if (values == null || values.Size() == 0) return;

            DateTime prevTime;
            DateTime currentTime = values.GetTimestamp(0);
            for (int t = 1; t < values.Size(); t++) {
                prevTime = currentTime;
                currentTime = values.GetTimestamp(t);
                Update((currentTime - prevTime).Days, values.GetValue(t));
            }
        }

        public override double GetUpdate(double dt, double value) {
            Update(dt, value);
            return GetVol();
        }

        public override double GetUpdate(TS values) {
            Update(values);
            return GetVol();
        }

        public double GetVol() {
            return Math.Round(Math.Sqrt(_vv), 2);
        }
    }
}
