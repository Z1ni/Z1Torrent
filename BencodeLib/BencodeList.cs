using System.Collections;
using System.Collections.Generic;

namespace BencodeLib {

    public class BencodeList : IBencodeItem, IList<IBencodeItem> {

        public int Count => _items.Count;
        public bool IsReadOnly => ((IList<IBencodeItem>)_items).IsReadOnly;

        private List<IBencodeItem> _items;

        public BencodeList() {
            _items = new List<IBencodeItem>();
        }

        public BencodeList(List<IBencodeItem> items) {
            _items = items;
        }

        public IEnumerator<IBencodeItem> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(IBencodeItem item) {
            _items.Add(item);
        }

        public void Clear() {
            _items.Clear();
        }

        public bool Contains(IBencodeItem item) {
            return _items.Contains(item);
        }

        public void CopyTo(IBencodeItem[] array, int arrayIndex) {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(IBencodeItem item) {
            return _items.Remove(item);
        }

        public int IndexOf(IBencodeItem item) {
            return _items.IndexOf(item);
        }

        public void Insert(int index, IBencodeItem item) {
            _items.Insert(index, item);
        }

        public void RemoveAt(int index) {
            _items.RemoveAt(index);
        }

        public IBencodeItem this[int index] {
            get => _items[index];
            set => _items[index] = value;
        }

        public override string ToString() {
            return $"BencodeList, {_items.Count} items";
        }
    }

}
