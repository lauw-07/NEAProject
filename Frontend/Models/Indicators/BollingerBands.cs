namespace Frontend.Models.Indicators {
    public class BollingerBands {
        private readonly Sma sma;
        private double ma = double.NaN;
        private Queue<double> values = new Queue<double>();
        private int _windowSize = 0;
        private double _upperBand = double.NaN;
        private double _lowerBand = double.NaN;

        public BollingerBands(int windowSize) {
            sma = new Sma(windowSize);
            _windowSize = windowSize;
        }

        public (double, double) Update(double value) {
            values.Enqueue(value);
            if (values.Count > _windowSize) {
                values.Dequeue();
            }

            ma = sma.Update(value);

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

                _upperBand = ma + 2 * std;
                _lowerBand = ma - 2 * std;
            }
            return (_upperBand, _lowerBand);
        }
    }
}
