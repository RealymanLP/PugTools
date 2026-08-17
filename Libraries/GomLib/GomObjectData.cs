using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GomLib
{
    public class GomObjectData// : System.Dynamic.DynamicObject
    {
        private readonly Dictionary<string, object> data = new Dictionary<string, object>();

        public IDictionary<string, object> Dictionary { get { return data; } }

        public bool ContainsKey(string key)
        {
            return data.ContainsKey(key);
        }

        //public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        //{
        //    string key = binder.Name;
        //    if (!data.TryGetValue(key, out result))
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        //public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value)
        //{
        //    string key = binder.Name;

        //    data[key] = value;

        //    return true;
        //}

        public T Get<T>(string key)
        {
            return (T)data[key];
        }

        private static bool TryConvertValue<T>(object value, out T result)
        {
            result = default;
            if (value == null) return true;
            if (value is T typed) { result = typed; return true; }

            try
            {
                Type target = typeof(T);
                if (target == typeof(string))
                {
                    result = (T)(object)value.ToString();
                    return true;
                }

                if (target.IsEnum)
                {
                    result = (T)Enum.ToObject(target, value);
                    return true;
                }

                if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(target))
                {
                    result = (T)Convert.ChangeType(value, target);
                    return true;
                }
            }
            catch
            {
                // Keep the old safe ValueOrDefault semantics: return the supplied default.
            }
            return false;
        }

        public T ValueOrDefault<T>(string key, T defaultValue)
        {
            if (!ContainsKey(key)) return defaultValue;
            return TryConvertValue(data[key], out T value) ? value : defaultValue;
        }

        public T ValueOrDefault<T>(string key)
        {
            if (!ContainsKey(key)) return default;
            return TryConvertValue(data[key], out T value) ? value : default;
        }
    }
}
