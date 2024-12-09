using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend.Models.Database {
    public class StrategyResult {
        public int ResultID { get; set; }
        public int PNL { get; set; }
        public DateTime ResultDateTime { get; set; }
        public int StrategyID { get; set; }
        public int InstrumentID { get; set; }

        public StrategyResult(int resultID, int pNL, DateTime resultDateTime, int strategyID, int instrumentID) {
            ResultID = resultID;
            PNL = pNL;
            ResultDateTime = resultDateTime;
            StrategyID = strategyID;
            InstrumentID = instrumentID;
        }
    }
}
