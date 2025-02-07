using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend.Models.Backtest.Breakout {
    public class StrategyInput {
        private Dictionary<string, object> _inputs = new Dictionary<string, object>();

        public StrategyInput() { }

        public Dictionary<string, object> GetInputs() {
            return _inputs;
        }

        public void AddInput(string key, object value) {
            _inputs.Add(key, value);
        }

        public void AddInputs(Dictionary<string, object> inputs) {
            foreach (KeyValuePair<string, object> pair in inputs) {
                if (!_inputs.ContainsKey(pair.Key))
                    _inputs.Add(pair.Key, pair.Value);
            }
        }

        public double GetClosePrice() {
            return (double)_inputs[BaseStrategyFields.ClosePrice.ToString()];
        }

        public DateTime GetTimestamp() {
            return (DateTime)_inputs[BaseStrategyFields.Timestamp.ToString()];
        }
    }
}
