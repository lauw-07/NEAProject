using Frontend.Models.Timeseries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend.Models.Backtest.Breakout {
    public enum BaseStrategyFields {
        ClosePrice,
        Timestamp
    }

    public abstract class StrategyBase {
        //protected StrategyType type;
        protected StrategyParams StrategyParams { get; set; }

        public StrategyBase(StrategyParams strategyParams) {
            StrategyParams = strategyParams;
        }

        public abstract void Update(StrategyInput strategyInput);

        public abstract TS Update(List<StrategyInput> strategyInputs);

        public abstract double GetTargetPosition();

        public abstract double GetSignal();

        public abstract int GetWindowSize();
    }
}
