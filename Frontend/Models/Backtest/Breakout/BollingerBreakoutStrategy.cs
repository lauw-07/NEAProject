using Frontend.Models.Indicators;
using Frontend.Models.Timeseries;
using System.Runtime.CompilerServices;

namespace Frontend.Models.Backtest.Breakout
{
    public enum BollingerStrategyFields {
        WindowSize,
        Width,
        ClosePrice,
        Timestamp,
        ExposureClass,
        ExposureParams,
        ExposureFixedValue,
        ExposureFixedShare,
        ExitLevelClass
    }

    public enum BollingerForecastStates {
        Idle,
        Long,
        Short
    }

    public class BollingerBreakoutStrategy : StrategyBase {

        // State of _signal + target Pos at a timestamp
        // Last price + Last timestamp

        protected DateTime _firstTimestamp = DateTime.MaxValue;
        protected DateTime? _lastTimestamp = null;
        protected double _lastPrice = double.NaN;
        protected double _signal = 0;
        protected double _targetPosition = 0;

        protected BollingerBands _bollingerBands;
        protected double _upperBound = double.NaN;
        protected double _lowerBound = double.NaN;
        protected int _windowSize = 0;


        protected BollingerForecastStates _state = BollingerForecastStates.Idle;
        protected ExposureManager _exposureManager;
        protected Type _exitManagerClass;
        protected BollingerExitManager _exitManager;

        public BollingerBreakoutStrategy(StrategyParams strategyParams) : base(strategyParams) {
            // assume input dictionary has a key for the closePrices
            _windowSize = (int)strategyParams.GetInputs()[BollingerStrategyFields.WindowSize.ToString()];
            double width = (double)strategyParams.GetInputs()[BollingerStrategyFields.Width.ToString()];
            _bollingerBands = new BollingerBands(_windowSize, width);

            Type exposureManagerClass = (Type)strategyParams.GetInputs()[BollingerStrategyFields.ExposureClass.ToString()];
            StrategyParams exposureManagerParams = (StrategyParams)strategyParams.GetInputs()[BollingerStrategyFields.ExposureParams.ToString()];

            _exposureManager = ExposureManager.GetInstance(exposureManagerClass, exposureManagerParams);

            _exitManagerClass = (Type)strategyParams.GetInputs()[BollingerStrategyFields.ExitLevelClass.ToString()];
            _exitManager = BollingerExitManager.GetInstance(this, _exitManagerClass, _bollingerBands);
        }

        // update on snapshot of TS

        public override void Update(StrategyInput strategyInput) {
            DateTime timestamp = (DateTime)strategyInput.GetInputs()[BollingerStrategyFields.Timestamp.ToString()];
            double closePx = (double)strategyInput.GetInputs()[BollingerStrategyFields.ClosePrice.ToString()];

            (_upperBound, _lowerBound) = _bollingerBands.Update<(double, double)>(closePx);

            switch (_state) {
                case BollingerForecastStates.Idle: // at previous timestamp - no position held
                    if (timestamp < _firstTimestamp) {
                        _firstTimestamp = timestamp;
                        break;
                    }

                    if (closePx > _upperBound) {
                        _state = BollingerForecastStates.Long; // price breaches upper bound
                        _signal = 1; // normalised _signal, 1 = fully long (i.e. max position size) 

                        // based on type of exposure type (i.e. FixedValue or FixedShare), it scales the num shares desired by the _signal strength
                        _targetPosition = _signal * _exposureManager.GetNumShares(closePx);
                    } else if (closePx < _lowerBound) {
                        _state = BollingerForecastStates.Short;
                        _signal = -1;

                        _targetPosition = _signal * _exposureManager.GetNumShares(closePx);
                    } else {
                        break;
                    }
                    break;
                case BollingerForecastStates.Long:
                    if (closePx < GetExitRef(_state, closePx)) {
                        _state = BollingerForecastStates.Idle;
                        _signal = 0;
                        _targetPosition = 0;
                    }
                    break;
                case BollingerForecastStates.Short:
                    if (closePx > GetExitRef(_state, closePx)) {
                        _state = BollingerForecastStates.Idle;
                        _signal = 0;
                        _targetPosition = 0;
                    }
                    break;
                default:
                    break;
            }
        }

        protected double GetExitRef(BollingerForecastStates state, double closePx) {
            double exitLevel = double.NaN;
            if (typeof(BollingerExitWithTrailingStop).IsAssignableFrom(_exitManagerClass)) {
                exitLevel = _exitManager.GetExitLevel(state, closePx);
            } else {
                exitLevel = _exitManager.GetExitLevel(state);
            }
            return exitLevel;
        }

        // allow update on a whole TS
        public override TS Update(List<StrategyInput> strategyInputs) {
            TS targetPositions = new TS();

            foreach (StrategyInput strategyInput in strategyInputs) {
                Update(strategyInput);
                DateTime timestamp = (DateTime)strategyInput.GetInputs()[BollingerStrategyFields.Timestamp.ToString()];
                targetPositions.Add(timestamp, GetTargetPosition());
            }
            return targetPositions;
        }

        public override double GetTargetPosition() {
            return _targetPosition;
        }

        public override double GetSignal() {
            return _signal;
        }

        public int GetWindowSize() {
            return _windowSize;
        }
    }
}
