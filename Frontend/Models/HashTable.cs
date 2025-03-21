namespace Frontend.Models {
    public interface IHashtable<TKey, TValue> {
        void Add(TKey key, TValue value);
        TValue this[TKey key] { get; set; }
    }

    public class HashTable<TKey, TValue> : IHashtable<TKey, TValue>
        where TKey : notnull {
        // Represents the entire hashtable as an array of linked lists
        private LinkedList<KeyValuePair<TKey, TValue>>[] _array;
        // Threshold load factor to be reached before rehashing
        private readonly double loadFactorThreshold;
        private int size;
        private int entries;
        private double currentLoadFactor;

        public HashTable() {
            // Choosing 0.72 because it is a common value for load factor threshold
            // Starting with size = 11 since it is a somewhat small prime number
            loadFactorThreshold = 0.72;
            size = 11; 
            entries = 0;
            _array = new LinkedList<KeyValuePair<TKey, TValue>>[size];
        }

        private bool CheckPrime(int num, int checkDigit = 2) {
            // Base cases
            if (num < 2) return false;
            if (num % checkDigit == 0) return false;
            if (checkDigit > Math.Sqrt(num)) return true;

            return CheckPrime(num, checkDigit + 1);
        }

        private int FindNextPrime(int num) {
            // Base case
            if (CheckPrime(num)) return num;

            return FindNextPrime(num + 1);
        }

        private void UpdateLoadFactor() {
            // Convert to double to avoid integer division
            currentLoadFactor = (double)entries / size;
        }

        private int GetHashCode(TKey key) {
            return Math.Abs(key.GetHashCode() % size);
        }

        // Helper function to ensure that a certain row in the hashtable exists
        private void EnsureRowExists(int pos) {
            if (_array[pos] == null) {
                _array[pos] = new LinkedList<KeyValuePair<TKey, TValue>>();
            }
        }

        private void Rehash() {
            // Double size and find next prime to make hashtable larger
            size = FindNextPrime(size * 2);
            LinkedList<KeyValuePair<TKey, TValue>>[] newArray = new LinkedList<KeyValuePair<TKey, TValue>>[size];

            // Add back each key-value pair into the new array
            foreach (LinkedList<KeyValuePair<TKey, TValue>> row in _array) {
                if (row != null) {
                    foreach (KeyValuePair<TKey, TValue> kvp in row) {
                        // Calculate a new position for the key-value pair
                        int newPos = GetHashCode(kvp.Key);

                        // Check if there will be any collisions as the new position. If none,
                        // create a new linked list. If there is, just add to end of existing linked list
                        if (newArray[newPos] == null) {
                            newArray[newPos] = new LinkedList<KeyValuePair<TKey, TValue>>();
                        }
                        newArray[newPos].AddLast(kvp);
                    }
                }
            }
            _array = newArray;
        }

        public void Add(TKey key, TValue val) {
            // Check that the load factor is not exceeded and that the hashtable can be added to
            UpdateLoadFactor();
            if (currentLoadFactor > loadFactorThreshold) {
                Rehash();
            }

            int pos = GetHashCode(key);
            EnsureRowExists(pos);

            // Check for if key already exists
            foreach (KeyValuePair<TKey, TValue> kvp in _array[pos]) {
                if (kvp.Key.Equals(key)) {
                    throw new ArgumentException("An element with the same key already exists.");
                }
            }

            // Add the new key-value pair and update entries count
            _array[pos].AddLast(new KeyValuePair<TKey, TValue>(key, val));
            entries++;
        }

        public bool Remove(TKey key) {
            int pos = GetHashCode(key);
            if (_array[pos] == null) {
                return false;
            }

            // Loop through linked list to find the desired key since collisions might have occured
            // meaning that multiple different kvps hold the same position in the hashtable
            LinkedListNode<KeyValuePair<TKey, TValue>>? head = _array[pos].First;
            while (head != null) {
                if (head.Value.Key.Equals(key)) {
                    _array[pos].Remove(head);
                    entries--;
                    return true;
                }
                head = head.Next;
            }
            // Otherwise key was not found so nothing was removed
            return false;
        }

        // Getter and setter for the hashtable
        public TValue this[TKey key] {
            get {
                // Get desired row of key
                int pos = GetHashCode(key);
                if (_array[pos] == null) {
                    throw new KeyNotFoundException("Key not found.");
                }

                // Loop through linked list to find the desired key
                foreach (KeyValuePair <TKey, TValue> kvp in _array[pos]) {
                    if (kvp.Key.Equals(key)) {
                        return kvp.Value;
                    }
                }

                // If key is not found, throw an exception
                throw new KeyNotFoundException("Key not found.");
            }
            set {
                // Get desired row of key and ensure that the row exists
                int pos = GetHashCode(key);
                EnsureRowExists(pos);


                // Must loop through manually since a foreach loop does not allow for updating values
                LinkedListNode<KeyValuePair<TKey, TValue>>? head = _array[pos].First;

                bool found = false;
                while (head != null) {
                    if (head.Value.Key.Equals(key)) {
                        // Replace the value if the key exists.
                        KeyValuePair<TKey, TValue> newNode = new KeyValuePair<TKey, TValue>(key, value);
                        head.Value = newNode;
                        found = true;
                        break;
                    }
                    // Otherwise key has not been found so no update made and move to next node
                    head = head.Next;
                }

                if (!found) {
                    // If the key does not exist, add the new key-value pair.
                    _array[pos].AddLast(new KeyValuePair<TKey, TValue>(key, value));
                    entries++;
                    UpdateLoadFactor();
                    if (currentLoadFactor > loadFactorThreshold) {
                        Rehash();
                    }
                }
            }
        }
    }
}
