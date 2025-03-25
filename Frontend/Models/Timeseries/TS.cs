using Frontend.Models.Indicators;

namespace Frontend.Models.Timeseries {
    public class TS {
        //Time series class
        protected List<DateTime> _timestamps = new List<DateTime>();
        protected List<double> _values = new List<double>();
        private string _indicator = "";

        //Constructor Overloading
        public TS() { }

        public TS(int initialCapacity) {
            _timestamps.Capacity = initialCapacity;
            _values.Capacity = initialCapacity;
        }

        public TS(TS other) {
            if (other == null) return;

            _timestamps.Capacity = other.Size();
            _values.Capacity = other.Size();

            _timestamps.AddRange(other._timestamps);
            _values.AddRange(other._values);
        }

        public TS(DateTime[] timestamps, double[] values) {
            if (timestamps.Length != values.Length)
                return;

            _timestamps.AddRange(timestamps);
            _values.AddRange(values);
        }

        public TS(List<DateTime> timestamps, List<double> values) {
            if (timestamps.Count != values.Count)
                return;

            _timestamps = new List<DateTime>(timestamps);
            _values = new List<double>(values);
        }

        public void Add(DateTime timestamp, double value) {
            _timestamps.Add(timestamp);
            _values.Add(value);
        }

        public void Add(List<DateTime> timestamps, List<double> values) {
            if (timestamps == null || values == null || timestamps.Count != values.Count) return;

            _timestamps.AddRange(timestamps);
            _values.AddRange(values);
        }


        public void Add(TS other) {
            if (other == null || other.Size() <= 0) return;
            Add(other.GetTimestamps(), other.GetValues());
        }

        public List<DateTime> GetTimestamps() { return _timestamps; }

        public DateTime GetTimestamp(int index) { return _timestamps[index]; }

        public List<double> GetValues() { return _values; }

        public double GetValue(int index) { return _values[index]; }

        public int Size() { return _timestamps.Count; }

        public double GetLastValue() { return _values[_values.Count - 1]; }

        public DateTime GetLastTime() { return _timestamps[_timestamps.Count - 1]; }

        public void SetIndicator(string indicator) { _indicator = indicator; }
        public string GetIndicator() { return _indicator; }
        public override bool Equals(object? obj) {
            if (obj is TS other) {
                return _indicator == other._indicator &&
                       _timestamps.SequenceEqual(other._timestamps) &&
                       _values.SequenceEqual(other._values);
            }
            return false;
        }

        public bool IsEmpty() { return (_timestamps.Count == 0 || _values.Count == 0); }

        public void Clear() {
            _timestamps.Clear();
            _values.Clear();
        }

        public TS CopyTs() { return new TS(this); }

        public TS Sma(int n) {
            if (_values.Count == 0) return new TS();

            Sma sma = new Sma(n);
            TS smaTs = CopyTs();

            for (int i = 0; i < _timestamps.Count; i++) {
                smaTs._values[i] = sma.Update<double>(_values[i]);
            }
            return smaTs;
        }

        public TS Ewma(double halfLife) {
            if (_values.Count == 0) return new TS();

            Ewma ewma = new Ewma(halfLife, _values[0]);
            TS ewmaTs = CopyTs();

            for (int i = 1; i < _timestamps.Count; i++) {
                // Assuming that price data is daily
                double dt = (_timestamps[i] - _timestamps[i - 1]).Days;
                ewmaTs._values[i] = ewma.GetUpdate(dt, _values[i]);
            }
            return ewmaTs;
        }

        public TS Ewvol(double halfLife) {
            return Ewvol(halfLife, double.NaN, true);
        }

        // seedVol is used as the initial value to initialise the calculations
        public TS Ewvol(double halfLife, double seedVol) {
            return Ewvol(halfLife, seedVol, true);
        }

        public TS Ewvol(double halfLife, double seedVol, bool useMean) {
            if (_values.Count == 0) return new TS();

            Ewvol ewvol = new Ewvol(halfLife, double.IsNaN(seedVol) ? _values[0] : seedVol, useMean ? _values[0] : double.NaN);
            TS ewvolTs = CopyTs();

            double initialSeed = double.IsNaN(seedVol) ? (useMean ? 0 : _values[0]) : seedVol;

            for (int i = 1; i < _timestamps.Count; i++) {
                // Assuming that price data is daily
                double dt = (_timestamps[i] - _timestamps[i - 1]).Days;
                ewvolTs._values[i] = ewvol.GetUpdate(dt, _values[i]);
            }
            return ewvolTs;
        }

        public (TS, TS) BollingerBands(int windowSize, double width) {
            if (_values.Count == 0) return (new TS(), new TS());

            BollingerBands bollingerBands = new BollingerBands(windowSize, width);
            TS upperBandTs = CopyTs();
            TS lowerBandTs = CopyTs();

            for (int i = 0; i < _timestamps.Count; i++) {
                (double, double) bounds = bollingerBands.Update<(double, double)>(_values[i]);
                upperBandTs._values[i] = bounds.Item1;
                lowerBandTs._values[i] = bounds.Item2;
            }
            return (upperBandTs, lowerBandTs);
        }

        public TS LinearRegression(List<TS> predictors) {
            foreach (TS ts in predictors) {
                if (!ts.GetTimestamps().SequenceEqual(_timestamps)) {
                    return new TS();
                }
            }
            Regression regression = new Regression();
            TS regressionTs = new TS();

            for (int i = 0; i < _timestamps.Count; i++) {
                List<double> predictorValues = new List<double>();
                foreach (TS ts in predictors) {
                    predictorValues.Add(ts.GetValue(i));
                }

                regression.Update(_values[i], predictorValues);
                double prediction = regression.GetCurrentPrediction();
                if (!double.IsNaN(prediction)) {
                    regressionTs.Add(_timestamps[i], prediction);
                }
            }
            return regressionTs;
        }

        public override int GetHashCode() {
            return HashCode.Combine(GetTimestamps(), GetValues());
        }
    }
}
