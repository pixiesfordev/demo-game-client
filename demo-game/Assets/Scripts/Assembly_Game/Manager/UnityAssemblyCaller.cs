using Scoz.Func;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace tower.main {
    public static class UnityAssemblyCaller {
        static Assembly asm;
        static readonly Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();

        public static void Init() {
            asm = AppDomain.CurrentDomain
                           .GetAssemblies()
                           .FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
            if (asm == null) {
                WriteLog.LogError("找不到 Assembly-CSharp");
            }
        }

        public static object Invoke(string _typeFullName, string _methodName, bool _cacheMethod, params object[] _args) {
            if (asm == null) {
                Init();
                if (asm == null) return null;
            }

            string key = $"{_typeFullName}.{_methodName}";
            MethodInfo method;

            if (_cacheMethod && methods.TryGetValue(key, out method)) {
                return method.Invoke(null, _args?.Length > 0 ? _args : null);
            }

            var type = asm.GetType(_typeFullName);
            if (type == null) {
                WriteLog.LogError($"找不到類別 {_typeFullName}");
                return null;
            }

            method = type.GetMethod(_methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) {
                WriteLog.LogError($"在 {_typeFullName} 找不到方法 {_methodName}()");
                return null;
            }

            if (_cacheMethod) {
                methods[key] = method;
            }

            return method.Invoke(null, _args?.Length > 0 ? _args : null);
        }
    }
}
