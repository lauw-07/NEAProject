using Frontend.Models.Timeseries;
using System.Diagnostics.Eventing.Reader;

namespace Frontend.Models.Indicators {
    public class Ewma {

        //initialise values with their default values
        protected double currentMa = double.NaN;
        protected double decay = double.NaN;
        protected readonly double unitDecay = double.NaN;

        public double HalfLife { get; set; }
        public double Seed { get; set; } // the seed is the first value of the timeseries that we are working with
        public Ewma(double halfLife, double seed) {
            HalfLife = halfLife;
            unitDecay = Math.Pow(2, -1 / halfLife);
            currentMa = seed;
        }

        public virtual void Update(double dt, double value) {
            // ensure that the decay is scaled based on dt
            decay = Math.Pow(unitDecay, dt);

            // calculate new ma and store it in the currentMa
            currentMa = decay * currentMa + (1 - decay) * value;
        }

        public virtual void Update(TS values) {
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
            return currentMa;
        }
    }
}
