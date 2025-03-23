using Frontend.Models.Backtest.Breakout;
using Frontend.Models.Timeseries;

namespace Frontend.Models.Backtest.Reversion {
    public class BollingerReversionStrategy : BollingerBreakoutStrategy {
        public BollingerReversionStrategy(StrategyParams strategyParams) : base(strategyParams) {

        }

        public override void Update(StrategyInput strategyInput) {
            DateTime timestamp = (DateTime)strategyInput.GetInputs()[BollingerStrategyFields.Timestamp.ToString()];
            double closePx = (double)strategyInput.GetInputs()[BollingerStrategyFields.ClosePrice.ToString()];

            (_upperBound, _lowerBound) = _bollingerBands.Update<(double, double)>(closePx);

            switch (_state) {
                case BollingerForecastStates.Idle:
                    if (timestamp < _firstTimestamp) {
                        _firstTimestamp = timestamp;
                        break;
                    }

                    if (closePx > _upperBound) {
                        _state = BollingerForecastStates.Short;
                        _signal = -1;

                        _targetPosition = _signal * _exposureManager.GetNumShares(closePx);
                    } else if (closePx < _lowerBound) {
                        _state = BollingerForecastStates.Long;
                        _signal = 1;

                        _targetPosition = _signal * _exposureManager.GetNumShares(closePx);
                    } else {
                        break;
                    }
                    break;
                case BollingerForecastStates.Long:
                    if (closePx > GetExitRef(_state, closePx)) {
                        _state = BollingerForecastStates.Idle;
                        _signal = 0;
                        _targetPosition = 0;
                    }
                    break;
                case BollingerForecastStates.Short:
                    if (closePx < GetExitRef(_state, closePx)) {
                        _state = BollingerForecastStates.Idle;
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
