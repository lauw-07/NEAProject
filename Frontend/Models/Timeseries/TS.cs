using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timeseries {
    public class TS {
        //Time series class
        protected List<long> _timestamps = new List<long>();
        protected List<double> _values = new List<double>();

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

        public TS(long[] timestamps, double[] values) {
            if (timestamps.Length != values.Length)
                return;

            _timestamps.AddRange(timestamps);
            _values.AddRange(values);
        }

        public TS(List<long> timestamps, List<double> values) {
            _timestamps = timestamps;
            _values = values;
        }

        public void Add(long timestamp, double value) {
            _timestamps.Add(timestamp);
            _values.Add(value);
        }

        public void Add(long[] timestamps, double[] values) {
            if (timestamps == null || values == null || timestamps.Length != values.Length) return;
            _timestamps.AddRange(timestamps);
            _values.AddRange(values);
        }


        public void Add(TS other) {
            if (other == null || other.Size() <= 0) return;
            Add(other.GetTimestamps(), other.GetValues());
        }

        public long[] GetTimestamps() { return _timestamps.ToArray(); }

        public long GetTimestamp(int index) { return _timestamps[index]; }

        public double[] GetValues() { return _values.ToArray(); }

        public double GetValue(int index) { return _values[index]; }

        public int Size() { return _timestamps.Count; }
    }
}
