using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace Scoz.Func
// ReSharper restore CheckNamespace
{
    public class Comparer {
        /// <summary>
        /// 兩個陣列中有任一相等返回true
        /// </summary>
        public static bool OneOfArrayAEqualToArrayB<T>(T[] _array, params T[] _check) where T : class {
            if (_array == null || _check == null) return false;
            for (var i = 0; i < _array.Length; i++)
            for (var j = 0; j < _check.Length; j++)
                if (ReferenceEquals(_array[i], _check[j]))
                    return true;

            return false;
        }

        /// <summary>
        /// 陣列內容均相等
        /// </summary>
        public static bool AllAreEqual<T>(params T[] _args) {
            if (_args != null && _args.Length > 1) {
                var comparer = EqualityComparer<T>.Default;
                for (var i = 1; i < _args.Length; i++)
                    if (!comparer.Equals(_args[i], _args[i - 1]))
                        return false;
            }
            else {
                WriteLog.LogWarning("傳入要比較的參數為null或數量為0");
            }

            return true;
        }

        /// <summary>
        /// A與另一個陣列內所有內容都相等
        /// </summary>
        public static bool EqualToAll<T>(T _beComparedArg, params T[] _compareArgs) {
            if (_compareArgs != null && _compareArgs.Length > 0) {
                var comparer = EqualityComparer<T>.Default;
                for (var i = 0; i < _compareArgs.Length; i++)
                    if (!comparer.Equals(_beComparedArg, _compareArgs[i]))
                        return false;
            }
            else {
                WriteLog.LogWarning("傳入要比較的參數為null或數量為0");
            }

            return true;
        }

        /// <summary>
        /// A與另一個陣列其中一項內容相等
        /// </summary>
        public static bool EqualToOneOfAll<T>(T _beComparedArg, params T[] _compareArgs) {
            if (_compareArgs != null && _compareArgs.Length > 0) {
                var comparer = EqualityComparer<T>.Default;
                for (var i = 0; i < _compareArgs.Length; i++)
                    if (comparer.Equals(_beComparedArg, _compareArgs[i]))
                        return true;
            }

            return false;
        }
    }
}