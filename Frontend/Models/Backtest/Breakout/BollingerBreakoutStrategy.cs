using Frontend.Models.Indicators;
using Frontend.Models.Timeseries;

namespace Frontend.Models.Backtest.Breakout {
    public enum BollingerBreakoutStrategyFields {
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

    public enum BollingerBreakoutForecastStates {
        Idle,
        Long,
        Short
    }

    public class BollingerBreakoutStrategy : StrategyBase {

        // State of _signal + target Pos at a timestamp
        // Last price + Last timestamp

        private DateTime _firstTimestamp = DateTime.MaxValue;
        private DateTime? _previousTimestamp = null;
        private double _lastPrice = double.NaN;
        private double _signal = 0;
        private double _targetPosition = 0;

        private BollingerBands _bollingerBands;
        private double _upperBound = double.NaN;
        private double _lowerBound = double.NaN;
        private int _windowSize = 0;
        

        private BollingerBreakoutForecastStates _state = BollingerBreakoutForecastStates.Idle;
        private ExposureManager _exposureManager;

        private BollingerExitManager _exitManager;

        public BollingerBreakoutStrategy(StrategyParams strategyParams) : base(strategyParams) {
            // assume input dictionary has a key for the closePrices
            _windowSize = (int)strategyParams.GetInputs()[BollingerBreakoutStrategyFields.WindowSize.ToString()];
            double width = (double)strategyParams.GetInputs()[BollingerBreakoutStrategyFields.Width.ToString()];
            _bollingerBands = new BollingerBands(_windowSize, width);

            Type exposureManagerClass = (Type)strategyParams.GetInputs()[BollingerBreakoutStrategyFields.ExposureClass.ToString()];
            StrategyParams exposureManagerParams = (StrategyParams)strategyParams.GetInputs()[BollingerBreakoutStrategyFields.ExposureParams.ToString()];

            _exposureManager = ExposureManager.GetInstance(exposureManagerClass, exposureManagerParams);

            Type exitManagerClass = (Type)strategyParams.GetInputs()[BollingerBreakoutStrategyFields.ExitLevelClass.ToString()];
            _exitManager = BollingerExitManager.GetInstance(exitManagerClass, _bollingerBands);
        }

        // update on snapshot of TS

        public override void Update(StrategyInput strategyInput) {
            DateTime timestamp = (DateTime)strategyInput.GetInputs()[BollingerBreakoutStrategyFields.Timestamp.ToString()];
            double closePx = (double)strategyInput.GetInputs()[BollingerBreakoutStrategyFields.ClosePrice.ToString()];

            (_upperBound, _lowerBound) = _bollingerBands.Update<(double, double)>(closePx);

            switch (_state) {
                case BollingerBreakoutForecastStates.Idle: // at previous timestamp - no position held
                    if (timestamp < _firstTimestamp) {
                        _firstTimestamp = timestamp;
                        break;
                    }

                    if (closePx >= _upperBound) {
                        _state = BollingerBreakoutForecastStates.Long; // price breaches upper bound
                        _signal = 1; // normalised _signal, 1 = fully long (i.e. max position size) 

                        // based on type of exposure type (i.e. FixedValue or FixedShare), it scales the num of shares desired by the _signal strength
                        _targetPosition = _signal * _exposureManager.GetNumShares(closePx);
                    } else if (closePx <= _lowerBound) {
                        _state = BollingerBreakoutForecastStates.Short;
                        _signal = -1;

                        _targetPosition = _signal * _exposureManager.GetNumShares(closePx);
                    } else {
                        break;
                    }
                    break;
                case BollingerBreakoutForecastStates.Long:
                    if (closePx <= _exitManager.GetExitLevel(_state)) {
                        _state = BollingerBreakoutForecastStates.Idle;
                        _signal = 0;
                        _targetPosition = 0;
                    }
                    break;
                case BollingerBreakoutForecastStates.Short:
                    if (closePx >= _exitManager.GetExitLevel(_state)) {
                        _state = BollingerBreakoutForecastStates.Idle;
                        _signal = 0;
                        _targetPosition = 0;
                    }
                    break;
                default:
                    break;
            }
        }

        // allow update on a whole TS
        public override TS Update(List<StrategyInput> strategyInputs) {
            TS targetPositions = new TS();

            foreach (StrategyInput strategyInput in strategyInputs) {
                Update(strategyInput);
                DateTime timestamp = (DateTime)strategyInput.GetInputs()[BollingerBreakoutStrategyFields.Timestamp.ToString()];
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

        public override int GetWindowSize() {
            return _windowSize;
        }

    }

    public abstract class BollingerExitManager {
        protected BollingerBands bollinger;
        public BollingerExitManager(BollingerBands bollingerBands) {
            bollinger = bollingerBands;
        }

        public abstract double GetExitLevel(BollingerBreakoutForecastStates state);
        public static BollingerExitManager GetInstance(Type type, BollingerBands bollingerBands) {
            if (typeof(BollingerExitAtReference).IsAssignableFrom(type)) {
                return new BollingerExitAtReference(bollingerBands);
            } else if (typeof(BollingerExitAtOpposite).IsAssignableFrom(type)) {
                return new BollingerExitAtOpposite(bollingerBands);
            } else {
                throw new NotImplementedException();
            }
        }
    }

    // exit when reaches reference price (i.e. price of the Moving average at a certain timestamp)
    public class BollingerExitAtReference : BollingerExitManager {
        public BollingerExitAtReference(BollingerBands bollingerBands) : base(bollingerBands) {
        }

        public override double GetExitLevel(BollingerBreakoutForecastStates state) {
            return bollinger.GetReference();
        }
    }

    // exit when reaches the other bound (i.e. if passed upper bound, exit at lower bound)
    public class BollingerExitAtOpposite : BollingerExitManager {
        public BollingerExitAtOpposite(BollingerBands bollingerBands) : base(bollingerBands) {
        }

        public override double GetExitLevel(BollingerBreakoutForecastStates state) {
            switch (state) {
                case BollingerBreakoutForecastStates.Long:
                    return bollinger.GetLowerBand();
                case BollingerBreakoutForecastStates.Short:
                    return bollinger.GetUpperBand();
                default:
                    return double.NaN;
            }
        }
    }
}
