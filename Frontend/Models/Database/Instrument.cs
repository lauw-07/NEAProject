using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend.Models.Database {
    public class Instrument {
        public int InstrumentID { get; set; }
        public string InstrumentName { get; set; }
        public string InstrumentSymbol { get; set; }
        public string InstrumentType { get; set; }
        public string InstrumentCurrency { get; set; }

        public Instrument(int instrumentID, string instrumentName, string instrumentSymbol, string instrumentType, string instrumentCurrency) {
            InstrumentID = instrumentID;
            InstrumentName = instrumentName;
            InstrumentSymbol = instrumentSymbol;
            InstrumentType = instrumentType;
            InstrumentCurrency = instrumentCurrency;
        }

        public double GetUnitExposure(double price) {
            switch (InstrumentType) {
                case "Stock":
                    return 1 * price;
                default:
                    return double.NaN;
            }
        }
    }
}
