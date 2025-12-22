using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scoz.Func {
    public static class CollectionManager {
        /// <summary>
        /// 移除陣列中指定索引的元素並重新調整陣列大小
        /// </summary>
        public static void RemoveAt<T>(ref T[] _arr, int _index) {
            if (_arr == null || _index < 0 || _index >= _arr.Length) return;
            var newLength = _arr.Length - 1;
            if (_index < newLength) System.Array.Copy(_arr, _index + 1, _arr, _index, newLength - _index);
            System.Array.Resize(ref _arr, newLength);
        }

        /// <summary>
        /// 將來源集合中的所有元素新增至目標集合中
        /// </summary>
        public static void AddRange<T>(this ICollection<T> _target, IEnumerable<T> _source) {
            if (_target == null)
                throw new System.ArgumentNullException(nameof(_target));
            if (_source == null)
                throw new System.ArgumentNullException(nameof(_source));

            if (_target is List<T> list)
                list.AddRange(_source);
            else
                foreach (var element in _source)
                    _target.Add(element);
        }

        /// <summary>
        /// 根據指定偏移量重新排列清單並回傳新清單
        /// </summary>
        public static List<T> GetReArrangeListWithOffset<T>(List<T> _list, int _offset) {
            if (_list == null || _list.Count == 0) return _list;
            var count = _list.Count;
            _offset = (_offset % count + count) % count;
            if (_offset == 0)
                return new List<T>(_list);

            var result = new List<T>(count);
            for (var i = 0; i < count; i++) {
                var index = (i + _offset) % count;
                result.Add(_list[index]);
            }

            return result;
        }

        /// <summary>
        /// 從清單中隨機取得一個元素
        /// </summary>
        public static T GetRandomTFromList<T>(List<T> _list) {
            if (_list == null || _list.Count <= 0)
                return default;
            var rand = Random.Range(0, _list.Count);
            return _list[rand];
        }

        /// <summary>
        /// 從清單中隨機取得指定數量的元素（允許重複）
        /// </summary>
        public static List<T> GetRepeatTListFromList<T>(List<T> _list, int _count) {
            if (_list == null || _list.Count <= 0 || _count <= 0)
                return new List<T>();

            var result = new List<T>(_count);
            for (var i = 0; i < _count; i++) {
                var index = Random.Range(0, _list.Count);
                result.Add(_list[index]);
            }

            return result;
        }

        /// <summary>
        /// 從清單中隨機取得指定數量的元素（不允許重複，且不會修改原始清單）
        /// </summary>
        public static List<T> GetNoRepeatTListFromList<T>(List<T> _list, int _count) {
            if (_list == null || _list.Count <= 0 || _count > _list.Count || _count < 0)
                return new List<T>();
            if (_count == _list.Count)
                return new List<T>(_list);

            var temp = new List<T>(_list);
            var result = new List<T>(_count);
            for (var i = 0; i < _count; i++) {
                var index = Random.Range(0, temp.Count);
                result.Add(temp[index]);
                temp[index] = temp[temp.Count - 1];
                temp.RemoveAt(temp.Count - 1);
            }

            return result;
        }

        /// <summary>
        /// 從陣列中隨機取得一個元素
        /// </summary>
        public static T GetRandomTFromArray<T>(T[] _array) {
            if (_array == null || _array.Length <= 0) return default;
            var rand = Random.Range(0, _array.Length);
            return _array[rand];
        }

        /// <summary>
        /// 將元素插入到清單中的隨機位置
        /// </summary>
        public static List<T> AddTIntoTListAtRandomIndex<T>(List<T> _list, T _t) {
            if (_list == null) return _list;
            var index = Random.Range(0, _list.Count + 1);
            _list.Insert(index, _t);
            return _list;
        }

        /// <summary>
        /// 將來源清單中的每個元素分別插入到目標清單中的隨機位置
        /// </summary>
        public static List<T> AddTListIntoTListAtRandomIndex<T>(List<T> _list, List<T> _addTList) {
            if (_list == null || _addTList == null) return _list;
            for (var i = 0; i < _addTList.Count; i++) {
                var index = Random.Range(0, _list.Count + 1);
                _list.Insert(index, _addTList[i]);
            }

            return _list;
        }

        /// <summary>
        /// 將清單中指定索引的元素移動到最後一位
        /// </summary>
        public static void MoveItemToLast<T>(List<T> _list, int _index) {
            if (_list == null || _index < 0 || _index >= _list.Count) return;
            var item = _list[_index];
            _list.RemoveAt(_index);
            _list.Add(item);
        }

        /// <summary>
        /// 將清單依據指定長度切分成多個子清單
        /// </summary>
        /// <example>傳入 [1..22] 與 10，回傳 [[1..10], [11..20], [21, 22]]</example>
        public static List<List<T>> SplitList<T>(List<T> _list, int _splitLength) {
            if (_list == null || _splitLength <= 0) return new List<List<T>>();
            var count = _list.Count;
            var newList = new List<List<T>>((count + _splitLength - 1) / _splitLength);
            for (var i = 0; i < count; i += _splitLength) {
                var length = System.Math.Min(_splitLength, count - i);
                newList.Add(_list.GetRange(i, length));
            }

            return newList;
        }
    }
}