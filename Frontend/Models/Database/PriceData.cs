using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend.Models.Database {
    public class PriceData {
        public int PriceID { get; set; }
        public int InstrumentID { get; set; }
        public DateTime PxDate { get; set; }
        public double OpenPx { get; set; }
        public double ClosePx { get; set; }
        public double HighPx { get; set; }
        public double LowPx { get; set; }
        public double Volume { get; set; }

        public PriceData(int priceID, int instrumentID, DateTime pxDate, double openPx, double closePx, double highPx, double lowPx, double volume) {
            PriceID = priceID;
            InstrumentID = instrumentID;
            PxDate = pxDate;
            OpenPx = openPx;
            ClosePx = closePx;
            HighPx = highPx;
            LowPx = lowPx;
            Volume = volume;
        }

    }
}
