namespace Frontend.Models.Indicators {
    public class Ewvol : Ewma {
        private bool useMean;
        private double vv = double.NaN; // variance


        public Ewvol(double halfLife, double seedVol, double seedMa) : base(halfLife, seedMa) {
            useMean = !double.IsNaN(seedMa);
            vv = Math.Pow(seedVol, 2);
        }
    }
}
