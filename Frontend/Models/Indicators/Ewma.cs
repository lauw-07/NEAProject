using Frontend.Models.Timeseries;
using Syncfusion.Blazor.Charts.Internal;
using System.Diagnostics.Eventing.Reader;

namespace Frontend.Models.Indicators {
    public class Ewma : IndicatorBase {

        //initialise _values with their default _values
        protected double _currentMa = double.NaN;
        protected double _decay = double.NaN;
        protected readonly double _unitDecay = double.NaN;
        public double HalfLife { get; set; }
        public double Seed { get; set; } // the seed is the first value of the timeseries that we are working with
        public Ewma(double halfLife, double seed) : base() {
            HalfLife = halfLife;
            _unitDecay = Math.Pow(2, -1 / halfLife);
            _currentMa = seed;
            _name = "Exponentially Weighted Moving Average";
        }

        public override void Update(double dt, double value) {
            // ensure that the _decay is scaled based on dt
            _decay = Math.Pow(_unitDecay, dt);

            // calculate new _ma and store it in the _currentMa
            _currentMa = Math.Round(_decay * _currentMa + (1 - _decay) * value, 2);
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

        public virtual double GetUpdate(double dt, double value) {
            Update(dt, value);
            return GetCurrentMa();
        }

        public virtual double GetUpdate(TS values) {
            Update(values);
            return GetCurrentMa();
        }

        public double GetCurrentMa() {
            return _currentMa;
        }

        public void SetSeed(double seed) {
            _currentMa = seed;
        }
    }
}
