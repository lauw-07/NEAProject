using Frontend.Models.Indicators;

namespace Frontend.Models.Backtest.Breakout {
    public abstract class BollingerExitManager {
        // Choose a default trailing stop percentage of 10%
        private const double StopLossPercentage = 0.1;
        protected BollingerBands bollinger;
        public BollingerExitManager(BollingerBands bollingerBands) {
            bollinger = bollingerBands;
        }

        public abstract double GetExitLevel(BollingerBreakoutForecastStates state);
        public abstract double GetExitLevel(BollingerBreakoutForecastStates state, double closePx);
        public static BollingerExitManager GetInstance(Type type, BollingerBands bollingerBands) {
            if (typeof(BollingerExitAtReference).IsAssignableFrom(type)) {
                return new BollingerExitAtReference(bollingerBands);
            } else if (typeof(BollingerExitAtOpposite).IsAssignableFrom(type)) {
                return new BollingerExitAtOpposite(bollingerBands);
            } else if (typeof(BollingerExitWithTrailingStop).IsAssignableFrom(type)) {
                return new BollingerExitWithTrailingStop(bollingerBands, StopLossPercentage);
            } else {
                throw new NotImplementedException();
            }
        }
    }

    // exit when reaches _reference price (i.e. price of the Moving average at a certain timestamp)
    public class BollingerExitAtReference : BollingerExitManager {
        public BollingerExitAtReference(BollingerBands bollingerBands) : base(bollingerBands) {
        }

        public override double GetExitLevel(BollingerBreakoutForecastStates state) {
            return bollinger.GetReference();
        }

        public override double GetExitLevel(BollingerBreakoutForecastStates state, double closePx) {
            throw new NotImplementedException();
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

        public override double GetExitLevel(BollingerBreakoutForecastStates state, double closePx) {
            throw new NotImplementedException();
        }
    }

    public class BollingerExitWithTrailingStop : BollingerExitManager {
        private readonly double _stopLossPercentage;
        private double _highestPx = double.MinValue;
        private double _lowestPx = double.MaxValue;

        public BollingerExitWithTrailingStop(BollingerBands bollinger, double stopLossPercentage)
            : base(bollinger) {
            _stopLossPercentage = stopLossPercentage;
        }

        public override double GetExitLevel(BollingerBreakoutForecastStates state) {
            // Do not implement here
            throw new NotImplementedException();
        }

        public override double GetExitLevel(BollingerBreakoutForecastStates state, double closePx) {
            switch (state) {
                case BollingerBreakoutForecastStates.Long:
                    // Update the highest price
                    if (closePx > _highestPx) {
                        _highestPx = closePx;
                    }
                    // Calculate the trailing stop as a percentage below the highest price
                    return _highestPx * (1 - _stopLossPercentage);
                case BollingerBreakoutForecastStates.Short:
                    // Update the lowest price
                    if (closePx < _lowestPx) {
                        _lowestPx = closePx;
                    }
                    // Calculate the trailing stop as a percentage above the lowest price
                    return _lowestPx * (1 + _stopLossPercentage);
                default:
                    // Do nothing
                    return closePx;
            }
        }
    }
}
