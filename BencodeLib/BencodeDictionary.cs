using System.Collections;
using System.Collections.Generic;

namespace BencodeLib {

    public class BencodeDictionary : IBencodeItem, IDictionary<string, IBencodeItem> {

        public int Count => ((IDictionary<string, IBencodeItem>)_dictionary).Count;
        public bool IsReadOnly => ((IDictionary<string, IBencodeItem>)_dictionary).IsReadOnly;
        public ICollection<string> Keys => ((IDictionary<string, IBencodeItem>)_dictionary).Keys;
        public ICollection<IBencodeItem> Values => ((IDictionary<string, IBencodeItem>)_dictionary).Values;

        private readonly Dictionary<string, IBencodeItem> _dictionary;

        public BencodeDictionary() {
            _dictionary = new Dictionary<string, IBencodeItem>();
        }

        public BencodeDictionary(Dictionary<string, IBencodeItem> dict) {
            _dictionary = dict;
        }

        public IEnumerator<KeyValuePair<string, IBencodeItem>> GetEnumerator() {
            return ((IDictionary<string, IBencodeItem>)_dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, IBencodeItem> item) {
            ((IDictionary<string, IBencodeItem>)_dictionary).Add(item.Key, item.Value);
        }

        // Custom BencodeInt Add
        public void Add(string key, long value) {
            _dictionary.Add(key, new BencodeInteger(value));
        }

        // Custom BencodeByteString Add
        public void Add(string key, string value) {
            _dictionary.Add(key, new BencodeByteString(value));
        }

        // Custom BencodeByteString Add
        public void Add(string key, byte[] value) {
            _dictionary.Add(key, new BencodeByteString(value));
        }

        public void Clear() {
            ((IDictionary<string, IBencodeItem>)_dictionary).Clear();
        }

        public bool Contains(KeyValuePair<string, IBencodeItem> item) {
            return ((IDictionary<string, IBencodeItem>)_dictionary).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, IBencodeItem>[] array, int arrayIndex) {
            ((IDictionary<string, IBencodeItem>)_dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, IBencodeItem> item) {
            return ((IDictionary<string, IBencodeItem>)_dictionary).Remove(item);
        }

        public void Add(string key, IBencodeItem value) {
            ((IDictionary<string, IBencodeItem>)_dictionary).Add(key, value);
        }

        public bool ContainsKey(string key) {
            return ((IDictionary<string, IBencodeItem>)_dictionary).ContainsKey(key);
        }

        public bool Remove(string key) {
            return ((IDictionary<string, IBencodeItem>)_dictionary).Remove(key);
        }

        public bool TryGetValue(string key, out IBencodeItem value) {
            return ((IDictionary<string, IBencodeItem>)_dictionary).TryGetValue(key, out value);
        }

        /// <summary>
        /// Get value assigned with the given key.
        /// <para>Returns null if value was not found.</para>
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Item or null if not found</returns>
        public IBencodeItem Get(string key) {
            return !TryGetValue(key, out IBencodeItem item) ? null : item;
        }

        public T Get<T>(string key) where T : IBencodeItem {
            return (T)Get(key);
        }

        public IBencodeItem this[string key] {
            get => ((IDictionary<string, IBencodeItem>)_dictionary)[key];
            set => ((IDictionary<string, IBencodeItem>)_dictionary)[key] = value;
        }

        public override string ToString() {
            return $"BencodeDictionary, {_dictionary.Count} items";
        }
    }

}
