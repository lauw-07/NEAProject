using Frontend.Models.Timeseries;
using MathNet.Numerics.LinearAlgebra;

namespace Frontend.Models.Indicators {
    public class Regression : IndicatorBase {
        private List<List<double>> _predictorList = new();
        private List<double> _targetList = new();
        private double _currentPrediction = double.NaN;

        public Regression() { }

        public override void Update(double dt, double targetValue, List<double> predictorValues) {
            _predictorList.Add(predictorValues);
            _targetList.Add(targetValue);

            if (_predictorList.Count > 1) {
                //Matrix<double> predictors = Matrix<double>.Build.DenseOfRowArrays(_predictorList);
                //Vector<double> target = Vector<double>.Build.Dense(_targetList.ToArray());

                int numRows = _predictorList.Count;
                int numCols = _predictorList[0].Count;

                Matrix<double> x = Matrix<double>.Build.Dense(numRows, numCols, (i, j) => _predictorList[i][j]);
                Vector<double> y = Vector<double>.Build.Dense(_targetList.ToArray());

                Vector<double> intercepts = Vector<double>.Build.Dense(numRows, 1.0);
                x = x.InsertColumn(0, intercepts);

                Matrix<double> xTranspose = x.Transpose();
                Vector<double> beta = (xTranspose * x).Inverse() * xTranspose * y;

                Vector<double> xLast = x.Row(x.RowCount - 1);
                _currentPrediction = beta.DotProduct(xLast);
            }
        }

        public double GetCurrentPrediction() {
            return _currentPrediction;
        }
    }
}
