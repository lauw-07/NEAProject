using Frontend.Models.Backtest.Breakout;
using Frontend.Models.Database;
using Frontend.Models.Timeseries;
using System.Diagnostics.Metrics;
using Instrument = Frontend.Models.Database.Instrument;

namespace Frontend.Models.Backtest {
    /* What does Backtest manager do???
     * Run a backtesting strategy (i.e. bollinger band breakout or ewma crossover)
     * Using the strategy, create a pnl timeseries
     * This timeseries will then get graphed
     */
    public class BacktestManager {
        private StrategyBase? _strategy;
        private Instrument? _instrument;
        private List<double> _targetPositions = new List<double>();
        private List<DateTime> _timestamps = new List<DateTime>();
        private List<double> _prices = new List<double>();

        private TS pnlTs = new TS();

        public BacktestManager() { }

        public void SetStrategy(Type strategyType, StrategyParams strategyParams, Instrument instrument) {
            if (typeof(BollingerBreakoutStrategy).IsAssignableFrom(strategyType)) {
                // e.g. BollingerBreakoutStrategy#
                _instrument = instrument;
                _strategy = new BollingerBreakoutStrategy(strategyParams);
            }
        }

        public TS RunBacktest(List<StrategyInput> inputs) {
            if (_strategy == null) return new TS();

            TS targetPositions = _strategy.Update(inputs);
            _targetPositions = targetPositions.GetValues();
            _timestamps = targetPositions.GetTimestamps();

            foreach (StrategyInput input in inputs) {
                _prices.Add(input.GetClosePrice());
            }

            /*_targetPositions.Add(_strategy.GetTargetPosition());
            _timestamps.Add(inputs.GetTimestamp());
            _prices.Add(inputs.GetClosePrice());*/


            PnLHandler pnLHandler = new PnLHandler();
            TS pnlTs = pnLHandler.CalculatePnl(_instrument, _targetPositions, _timestamps, _prices);
            return pnlTs;
        }

        public List<double> GetTargetPositions() {
            return _targetPositions;
        }

        public StrategyBase? GetStrategy() {
            return _strategy;
        }

        public Instrument? GetInstrument() {
            return _instrument;
        }
    }
}
