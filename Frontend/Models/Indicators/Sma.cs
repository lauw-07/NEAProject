using Frontend.Models.Timeseries;
using Syncfusion.Blazor.Charts.Internal;

namespace Frontend.Models.Indicators {
    public class Sma : IndicatorBase {
        private Queue<double> window = new Queue<double>();
        private double sum = 0;
        private double currentMa = double.NaN;

        private int WindowSize { get; set; }

        public Sma(int windowSize) : base() {
            WindowSize = windowSize;
            _name = "Simple Moving Average";
        }

        public override T Update<T>(double value) {
            sum += value;
            window.Enqueue(value);
            if (window.Count > WindowSize) {
                sum -= window.Dequeue();
            }
            currentMa = sum / window.Count;
            return (T)(object)currentMa;
        }

        //extra functionalities if required
        public double GetMa() {
            return currentMa;
        }

        public int GetWindowSize() {
            return WindowSize;
        }

        public void Clear() {
            window.Clear();
            sum = 0;
            currentMa = double.NaN;
        }
    }
}
