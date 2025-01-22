using Frontend.Models.Timeseries;

namespace Frontend.Models.Indicators {
    public class Sma {
        private Queue<double> window = new Queue<double>();
        private double sum = 0;
        private double currentMa = double.NaN;

        public int WindowSize { get; set; }

        public Sma(int windowSize) {
            WindowSize = windowSize;
        }

        public double update(double value) {
            sum += value;
            window.Enqueue(value);
            if (window.Count > WindowSize) {
                sum -= window.Dequeue();
            }
            currentMa = sum / window.Count;
            return currentMa;
        }

        //extra functionalities if required
        public double GetMa() {
            return currentMa;
        }

        public void Clear() {
            window.Clear();
            sum = 0;
            currentMa = double.NaN;
        }
    }
}
