using Frontend.Models.Backtest.Breakout;

namespace Frontend.Models.Backtest
{
    public abstract class ExposureManager
    {
        protected double exposure;
        protected double numShares;

        public ExposureManager() { }
        public abstract double GetExposure(double price = double.NaN);
        public abstract double GetNumShares(double price = double.NaN);
        public abstract void Update(StrategyInput input);

        public static ExposureManager GetInstance(Type type, StrategyParams exposureParams)
        {
            if (typeof(FixedValueExposureManager).IsAssignableFrom(type))
            {
                return new FixedValueExposureManager(exposureParams);
            }
            else if (typeof(FixedShareExposureManager).IsAssignableFrom(type))
            {
                return new FixedShareExposureManager(exposureParams);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    // when you want $1 million (a fixed value) of AAPL but you need to divide by price of 1 share
    public class FixedValueExposureManager : ExposureManager
    {
        public FixedValueExposureManager(StrategyParams exposureParams) : base()
        {
            exposure = (double)exposureParams.GetInputs()[BollingerBreakoutStrategyFields.ExposureFixedValue.ToString()];
            numShares = double.NaN;
        }

        public override double GetExposure(double price = double.NaN)
        {
            return exposure;
        }

        // returns num of shares to determine target position
        public override double GetNumShares(double price = double.NaN)
        {
            return exposure / price; // does not necessarily return whole number
        }

        public override void Update(StrategyInput input)
        {
            // do nothing
        }
    }

    // when you want 1000 (a fixed quantity of) shares of AAPL
    public class FixedShareExposureManager : ExposureManager
    {
        public FixedShareExposureManager(StrategyParams exposureParams) : base()
        {
            numShares = (double)exposureParams.GetInputs()[BollingerBreakoutStrategyFields.ExposureFixedShare.ToString()];
            exposure = double.NaN;
        }

        public override double GetExposure(double price = double.NaN)
        {
            return numShares * price;
        }

        public override double GetNumShares(double price = double.NaN)
        {
            return numShares;
        }

        public override void Update(StrategyInput input)
        {
            // do nothing
        }
    }
}
