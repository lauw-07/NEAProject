using Frontend.Models.Backtest.Breakout;
using Frontend.Models.Timeseries;

namespace Frontend.Models.Backtest.Reversion {
    public class ReversionStrategy : BollingerBreakoutStrategy {
        public ReversionStrategy(StrategyParams strategyParams) : base(strategyParams) {

        }

        public override void Update(StrategyInput strategyInput) {
            DateTime timestamp = (DateTime)strategyInput.GetInputs()[BollingerBreakoutStrategyFields.Timestamp.ToString()];
            double closePx = (double)strategyInput.GetInputs()[BollingerBreakoutStrategyFields.ClosePrice.ToString()];

            (_upperBound, _lowerBound) = _bollingerBands.Update<(double, double)>(closePx);

            switch (_state) {
                case BollingerBreakoutForecastStates.Idle:
                    if (timestamp < _firstTimestamp) {
                        _firstTimestamp = timestamp;
                        break;
                    }

                    if (closePx > _upperBound) {
                        _state = BollingerBreakoutForecastStates.Short;
                        _signal = -1;

                        _targetPosition = _signal * _exposureManager.GetNumShares(closePx);
                    } else if (closePx < _lowerBound) {
                        _state = BollingerBreakoutForecastStates.Long;
                        _signal = 1;

                        _targetPosition = _signal * _exposureManager.GetNumShares(closePx);
                    } else {
                        break;
                    }
                    break;
                case BollingerBreakoutForecastStates.Long:
                    if (closePx > GetExitRef(_state, closePx)) {
                        _state = BollingerBreakoutForecastStates.Idle;
                        _signal = 0;
                        _targetPosition = 0;
                    }
                    break;
                case BollingerBreakoutForecastStates.Short:
                    if (closePx < GetExitRef(_state, closePx)) {
                        _state = BollingerBreakoutForecastStates.Idle;
                        _signal = 0;
                        _targetPosition = 0;
                    }
                    break;
                default:
                    break;
            }
        }

        public override TS Update(List<StrategyInput> strategyInputs) {
            return base.Update(strategyInputs);
        }
    }
}
