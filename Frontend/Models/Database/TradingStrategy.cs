using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend.Models.Database {
    public class TradingStrategy {
        public int StrategyID { get; set; }
        public string StrategyName { get; set; }
        public string StrategyDescription { get; set; }

        public TradingStrategy(int strategyID, string strategyName, string strategyDescription) {
            StrategyID = strategyID;
            StrategyName = strategyName;
            StrategyDescription = strategyDescription;
        }
    }
}
