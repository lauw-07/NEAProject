using Frontend.Models.Timeseries;

namespace Frontend.Models.Backtest
{
    public enum BaseStrategyFields
    {
        ClosePrice,
        Timestamp
    }

    public abstract class StrategyBase
    {
        protected StrategyParams StrategyParams { get; set; }

        public StrategyBase(StrategyParams strategyParams)
        {
            StrategyParams = strategyParams;
        }

        public abstract void Update(StrategyInput strategyInput);

        public abstract TS Update(List<StrategyInput> strategyInputs);

        public abstract double GetTargetPosition();

        public abstract double GetSignal();
    }
}
