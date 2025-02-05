using Syncfusion.Blazor.Charts.Internal;

namespace Frontend.Models.Indicators {
    public class BollingerBands : IndicatorBase {
        private readonly Sma reference;
        private double ma = double.NaN;
        private Queue<double> values = new Queue<double>();
        private double _upperBand = double.NaN;
        private double _lowerBand = double.NaN;

        private double width;

        public BollingerBands(int windowSize, double width) : base() {
            reference = new Sma(windowSize);
            this.width = width;

            _name = "Bollinger Bands";
        }

        public override T Update<T>(double value) {
            values.Enqueue(value);
            if (values.Count > reference.GetWindowSize()) {
                values.Dequeue();
            }

            ma = reference.Update<double>(value);

            // calculate std
            // variance formula: sum of [ (x - x bar) ^ 2 ] / n
            if (values.Count > 0) {
                double variance = 0;
                double sum = 0;
                foreach (double val in values) {
                    sum += Math.Pow(val - ma, 2);
                }

                variance = sum / values.Count;

                double std = Math.Sqrt(variance);

                _upperBand = ma + width * std;
                _lowerBand = ma - width * std;
            }
            return (T)(object)(_upperBand, _lowerBand);
        }

        public double GetReference() {
            return reference.GetMa();
        }

        public double GetUpperBand() {
            return _upperBand;
        }

        public double GetLowerBand() {
            return _lowerBand;
        }
    }
}
