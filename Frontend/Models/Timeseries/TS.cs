using Frontend.Models.Indicators;
using Microsoft.AspNetCore.Server.HttpSys;
using Newtonsoft.Json.Linq;
using Syncfusion.Blazor.Charts.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend.Models.Timeseries {
    public class TS {
        //Time series class
        protected List<DateTime> _timestamps = new List<DateTime>();
        protected List<double> _values = new List<double>();
        private string _indicator = string.Empty;
        
        //Constructor Overloading
        public TS() { }

        public TS(int initialCapacity) {
            _timestamps.Capacity = initialCapacity;
            _values.Capacity = initialCapacity;
        }

        public TS(TS other) {
            if (other == null) return;

            _timestamps.Capacity = other.Size();
            _values.Capacity = other.Size();

            _timestamps.AddRange(other._timestamps);
            _values.AddRange(other._values);
        }

        public TS(DateTime[] timestamps, double[] values) {
            if (timestamps.Length != values.Length)
                return;

            _timestamps.AddRange(timestamps);
            _values.AddRange(values);
        }

        public TS(List<DateTime> timestamps, List<double> values) {
            _timestamps = timestamps;
            _values = values;
        }

        public void Add(DateTime timestamp, double value) {
            _timestamps.Add(timestamp);
            _values.Add(value);
        }

        public void Add(List<DateTime> timestamps, List<double> values) {
            if (timestamps == null || values == null || timestamps.Count != values.Count) return;

            _timestamps.AddRange(timestamps);
            _values.AddRange(values);
        }


        public void Add(TS other) {
            if (other == null || other.Size() <= 0) return;
            Add(other.GetTimestamps(), other.GetValues());
        }

        public List<DateTime> GetTimestamps() { return _timestamps; }

        public DateTime GetTimestamp(int index) { return _timestamps[index]; }

        public List<double> GetValues() { return _values; }

        public double GetValue(int index) { return _values[index]; }

        public int Size() { return _timestamps.Count; }

        public double GetLastValue() {
            return _values[_values.Count - 1];
        }

        public DateTime GetLastTime() {
            return _timestamps[_timestamps.Count - 1];
        }

        public override bool Equals(object? obj) {
            if (obj is TS other) {
                return _indicator == other._indicator &&
                       _timestamps.SequenceEqual(other._timestamps) &&
                       _values.SequenceEqual(other._values);
            }
            return false;
        }

        public bool IsEmpty() {
            return (_timestamps.Count == 0 || _values.Count == 0);
        }

        public TS CopyTs() {
            TS newTs = new TS(_timestamps, _values);
            return newTs;
        }

        public TS Sma(int n) {
            if (_values.Count == 0) return new TS();

            Sma sma = new Sma(n);
            TS smaTs = CopyTs();

            for (int i = 1; i < _timestamps.Count; i++) {
                smaTs._values[i] = sma.update(smaTs._values[i]);
            }
            _indicator = "sma";
            return smaTs;
        }

        public TS Ewma(double halfLife) {
            if (_values.Count == 0) return new TS();

            Ewma ewma = new Ewma(halfLife, _values[0]);
            TS ewmaTs = CopyTs();

            for (int i = 1; i < _timestamps.Count; i++) {
                // Assuming that price data is daily
                double dt = (_timestamps[i] - _timestamps[i - 1]).Days;
                ewmaTs._values[i] = ewma.GetUpdate(dt, _values[i]);
            }
            _indicator = "ewma";
            return ewmaTs;
        }

        /*public bool RemoveIndicator(string indicator) {
            return _currentIndicators.Remove(indicator);
        }*/

        public (TS, TS) BollingerBands() {
            if (_values.Count == 0) return (new TS(), new TS());

            BollingerBands bollingerBands = new BollingerBands(20);
            TS upperBandTs = CopyTs();
            TS lowerBandTs = CopyTs();

            for (int i = 1; i < _timestamps.Count; i++) {
                (double, double) bounds = bollingerBands.Update(_values[i]);
                upperBandTs._values[i] = bounds.Item1;
                lowerBandTs._values[i] = bounds.Item2;
            }
            return (upperBandTs, lowerBandTs);
        }

        public override int GetHashCode() {
            return HashCode.Combine(GetTimestamps(), GetValues());
        }
    }
}
