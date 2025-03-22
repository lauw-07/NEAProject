using Frontend.Models.Indicators;
using Frontend.Models.Timeseries;

namespace Frontend.Models.Backtest.Crossover {
    public enum EwmaCrossoverForecastStates {
        Idle,
        Long,
        Short
    }

    public enum EwmaCrossoverStrategyFields {
        SlowHL,
        FastHL,
        ClosePrice,
        Timestamp,
        ExposureClass,
        ExposureParams,
        ExposureFixedValue,
        ExposureFixedShare,
        ExitLevelClass
    }

    public class EwmaCrossoverStrategy : StrategyBase {
        private DateTime _firstTimestamp = DateTime.MaxValue;
        private DateTime _lastTimestamp = default(DateTime);
        private double _lastPrice = double.NaN;
        private double _signal = 0;
        private double _targetPosition = 0;

        private Ewma _ewmaSlow;
        private Ewma _ewmaFast;
        private ExposureManager _exposureManager;
        private EwmaCrossoverForecastStates _state = EwmaCrossoverForecastStates.Idle;

        public EwmaCrossoverStrategy(StrategyParams strategyParams) : base(strategyParams) {
            double slowHL = (double)strategyParams.GetInputs()[EwmaCrossoverStrategyFields.SlowHL.ToString()];
            double fastHL = (double)strategyParams.GetInputs()[EwmaCrossoverStrategyFields.FastHL.ToString()];
            _ewmaSlow = new Ewma(slowHL, double.NaN);
            _ewmaFast = new Ewma(fastHL, double.NaN);

            Type exposureManagerClass = (Type)strategyParams.GetInputs()[EwmaCrossoverStrategyFields.ExposureClass.ToString()];
            StrategyParams exposureManagerParams = (StrategyParams)strategyParams.GetInputs()[EwmaCrossoverStrategyFields.ExposureParams.ToString()];

            _exposureManager = ExposureManager.GetInstance(exposureManagerClass, exposureManagerParams);
        }

        public override void Update(StrategyInput strategyInput) {
            DateTime timestamp = (DateTime)strategyInput.GetInputs()[EwmaCrossoverStrategyFields.Timestamp.ToString()];
            double closePx = (double)strategyInput.GetInputs()[EwmaCrossoverStrategyFields.ClosePrice.ToString()];
            if (timestamp < _firstTimestamp) {
                _firstTimestamp = timestamp;
                _lastTimestamp = timestamp;
                _ewmaFast.SetSeed(closePx);
                _ewmaSlow.SetSeed(closePx);
                return;
            }

            double dt = (timestamp - _lastTimestamp).Days;
            _ewmaSlow.Update(dt, closePx);
            _ewmaFast.Update(dt, closePx);

            double slowMa = _ewmaSlow.GetCurrentMa();
            double fastMa = _ewmaFast.GetCurrentMa();

            _lastTimestamp = timestamp;

            // when fast ewma crosses above slow ewma, bullish trend = go long

            EwmaCrossoverForecastStates targetState;
            if (fastMa > slowMa) {
                targetState = EwmaCrossoverForecastStates.Long;
            } else if (fastMa < slowMa) {
                targetState = EwmaCrossoverForecastStates.Short;
            } else {
                targetState = EwmaCrossoverForecastStates.Idle;
            }

            if (_state != targetState) {
                switch (targetState) {
                    case EwmaCrossoverForecastStates.Long:
                        _signal = 1;
                        break;
                    case EwmaCrossoverForecastStates.Short:
                        _signal = -1;
                        break;
                    default:
                        _signal = 0;
                        break;
                }
                _targetPosition = _signal * _exposureManager.GetNumShares(closePx);
                _state = targetState;
            }
        }

        public override TS Update(List<StrategyInput> strategyInputs) {
            TS targertPositions = new TS();

            foreach (StrategyInput strategyInput in strategyInputs) {
                Update(strategyInput);
                DateTime timestamp = (DateTime)strategyInput.GetInputs()[EwmaCrossoverStrategyFields.Timestamp.ToString()];
                targertPositions.Add(timestamp, GetTargetPosition());
            }
            return targertPositions;
        }
        public override double GetTargetPosition() {
            return _targetPosition;
        }

        public override double GetSignal() {
            return _signal;
        }
    }
}
