namespace Frontend.Models.Indicators {
    public class BollingerBands : IndicatorBase {
        private readonly Sma _reference;
        private double _ma = double.NaN;
        private Queue<double> _values = new Queue<double>();
        private double _upperBand = double.NaN;
        private double _lowerBand = double.NaN;
        private double _width = double.NaN;

        public BollingerBands(int windowSize, double width) : base() {
            _reference = new Sma(windowSize);
            _width = width;

            _name = "Bollinger Bands";
        }

        public override T Update<T>(double value) {
            _values.Enqueue(value);
            if (_values.Count > _reference.GetWindowSize()) {
                _values.Dequeue();
            }

            _ma = _reference.Update<double>(value);

            // calculate std
            // variance formula: sum of [ (x - x bar) ^ 2 ] / n
            if (_values.Count > 0) {
                double variance = 0;
                double sum = 0;
                foreach (double val in _values) {
                    sum += Math.Pow(val - _ma, 2);
                }

                variance = sum / _values.Count;

                double std = Math.Sqrt(variance);

                _upperBand = Math.Round(_ma + _width * std, 2);
                _lowerBand = Math.Round(_ma - _width * std, 2);
            }
            return (T)(object)(_upperBand, _lowerBand);
        }

        public double GetReference() {
            return _reference.GetMa();
        }

        public double GetUpperBand() {
            return _upperBand;
        }

        public double GetLowerBand() {
            return _lowerBand;
        }
    }
}
