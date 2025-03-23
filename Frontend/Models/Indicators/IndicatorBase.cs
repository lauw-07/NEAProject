using Frontend.Models.Timeseries;

namespace Frontend.Models.Indicators {
    public abstract class IndicatorBase {
        protected string _name;

        public IndicatorBase() {
            _name = "";
        }

        public string GetName() {
            return _name;
        }

        public virtual void Update() {
            throw new NotImplementedException();
        }

        public virtual T Update<T>(double value) where T : struct {
            throw new NotImplementedException();
        }

        public virtual void Update(double dt, double value) {
            throw new NotImplementedException();
        }

        public virtual void Update(TS values) {
            throw new NotImplementedException();
        }

        public virtual void Update(double value, List<double> inputs) {
            throw new NotImplementedException();
        }
    }
}
