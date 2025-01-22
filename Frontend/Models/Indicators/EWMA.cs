using Frontend.Models.Timeseries;

namespace Frontend.Models.Indicators {
    public class Ewma {

        //initialise values with their default values
        private double currentMa = double.NaN;
        private double decay = double.NaN;
        private readonly double unitDecay = double.NaN;

        public double HalfLife { get; set; }
        public double Seed { get; set; } // the seed is the first value of the timeseries that we are working with
        public Ewma(double halfLife, double seed) {
            HalfLife = halfLife;
            unitDecay = Math.Pow(2, -1 / halfLife);
            currentMa = seed;
        }

        private void update(double dt, double value) {
            // ensure that the decay is scaled based on dt
            decay = Math.Pow(unitDecay, dt);

            // calculate new ma and store it in the currentMa
            currentMa = decay * currentMa + (1 - decay) * value;
        }

        public double GetUpdate(double dt, double value) {
            update(dt, value);
            return currentMa;
        }

        public double GeCurrentMa() {
            return currentMa;
        }
    }
}
