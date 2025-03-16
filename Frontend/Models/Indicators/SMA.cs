namespace Frontend.Models.Indicators {
    public class Sma : IndicatorBase {
        private Queue<double> _window = new Queue<double>();
        private double _sum = 0;
        private double _currentMa = double.NaN;

        private int WindowSize { get; set; }

        public Sma(int windowSize) : base() {
            WindowSize = windowSize;
            _name = "Simple Moving Average";
        }

        public override T Update<T>(double value) {
            _sum += value;
            _window.Enqueue(value);

            // Maintain the rolling _window by removing first item in queue
            if (_window.Count > WindowSize) {
                _sum -= _window.Dequeue();
            }
            currentMa = sum / window.Count;
            return (T)(object)currentMa;
        }

        //extra functionalities if required
        public double GetMa() {
            return _currentMa;
        }

        public int GetWindowSize() {
            return WindowSize;
        }

        public void Clear() {
            _window.Clear();
            _sum = 0;
            _currentMa = double.NaN;
        }
    }
}
