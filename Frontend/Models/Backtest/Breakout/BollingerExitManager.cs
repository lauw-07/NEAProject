using Frontend.Models.Backtest.Reversion;
using Frontend.Models.Indicators;

namespace Frontend.Models.Backtest.Breakout {
    public abstract class BollingerExitManager {
        // Choose a default trailing stop percentage of 10%
        private const double StopLossPercentage = 0.1;
        protected BollingerBands bollinger;
        protected Type strategyClass;
        public BollingerExitManager(BollingerBands bollingerBands, Type strategyClass) {
            bollinger = bollingerBands;
            this.strategyClass = strategyClass;
        }

        public abstract double GetExitLevel(BollingerForecastStates state);
        public abstract double GetExitLevel(BollingerForecastStates state, double closePx);
        public static BollingerExitManager GetInstance(StrategyBase strategy, Type exitManagerClass, BollingerBands bollingerBands) {
            if (typeof(BollingerExitAtReference).IsAssignableFrom(exitManagerClass)) {
                return new BollingerExitAtReference(bollingerBands, strategy.GetType());
            } else if (typeof(BollingerExitAtOpposite).IsAssignableFrom(exitManagerClass)) {
                return new BollingerExitAtOpposite(bollingerBands, strategy.GetType());
            } else if (typeof(BollingerExitWithTrailingStop).IsAssignableFrom(exitManagerClass)) {
                return new BollingerExitWithTrailingStop(bollingerBands, StopLossPercentage, strategy.GetType());
            } else {
                throw new NotImplementedException();
            }
        }
    }

    // exit when reaches _reference price (i.e. price of the Moving average at a certain timestamp)
    public class BollingerExitAtReference : BollingerExitManager {
        public BollingerExitAtReference(BollingerBands bollingerBands, Type strategyClass) : base(bollingerBands, strategyClass) {
        }

        public override double GetExitLevel(BollingerForecastStates state) {
            return bollinger.GetReference();
        }

        public override double GetExitLevel(BollingerForecastStates state, double closePx) {
            throw new NotImplementedException();
        }
    }

    // exit when reaches the other bound (i.e. if passed upper bound, exit at lower bound)
    public class BollingerExitAtOpposite : BollingerExitManager {
        public BollingerExitAtOpposite(BollingerBands bollingerBands, Type strategyClass) : base(bollingerBands, strategyClass) {
        }

        public override double GetExitLevel(BollingerForecastStates state) {
            if (typeof(BollingerReversionStrategy).IsAssignableFrom(strategyClass)) {
                switch (state) {
                    case BollingerForecastStates.Long:
                        return bollinger.GetUpperBand();
                    case BollingerForecastStates.Short:
                        return bollinger.GetLowerBand();
                    default:
                        return double.NaN;
                }
            } else if (typeof(BollingerBreakoutStrategy).IsAssignableFrom(strategyClass)) {
                switch (state) {
                    case BollingerForecastStates.Long:
                        return bollinger.GetLowerBand();
                    case BollingerForecastStates.Short:
                        return bollinger.GetUpperBand();
                    default:
                        return double.NaN;
                }
            } else {
                throw new InvalidOperationException($"Unsupported strategy class: {strategyClass}");
            }
        }

        public override double GetExitLevel(BollingerForecastStates state, double closePx) {
            throw new NotImplementedException();
        }
    }

    public class BollingerExitWithTrailingStop : BollingerExitManager {
        private readonly double _stopLossPercentage;
        private double _highestPx = double.MinValue;
        private double _lowestPx = double.MaxValue;

        public BollingerExitWithTrailingStop(BollingerBands bollinger, double stopLossPercentage, Type strategyClass)
            : base(bollinger, strategyClass) {
            _stopLossPercentage = stopLossPercentage;
        }

        public override double GetExitLevel(BollingerForecastStates state) {
            // Do not implement here
            throw new NotImplementedException();
        }

        public override double GetExitLevel(BollingerForecastStates state, double closePx) {
            switch (state) {
                case BollingerForecastStates.Long:
                    // Update the highest price
                    if (closePx > _highestPx) {
                        _highestPx = closePx;
                    }
                    // Calculate the trailing stop as a percentage below the highest price
                    return _highestPx * (1 - _stopLossPercentage);
                case BollingerForecastStates.Short:
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
