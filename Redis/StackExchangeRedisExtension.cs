using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedisTest.Redis
{
    static class StackExchangeRedisExtension
    {
        public static RedisValue ToRedisValue<T>(this T t) where T : class
        {
            if (t == null)
                return RedisValue.Null;

            if (t is string)
                return t.ToString();

            var rValue = JsonConvert.SerializeObject(t);

            return rValue;
        }

        public static RedisValue[] ToRedisValues<T>(this T[] t) where T : class
        {
            RedisValue[] rValues = new RedisValue[t.Length];
            for (int i = 0; i < t.Length; i++)
            {
                rValues[i] = t[i].ToRedisValue();
            }

            return rValues;
        }

        public static string ToString(this RedisValue rValue)
        {
            if (rValue == RedisValue.Null)
                return null;

            return rValue;
        }

        public static T ToObject<T>(this RedisValue rValue) where T : class
        {
            if (rValue == RedisValue.Null)
                return null;

            if (typeof(T) == typeof(string))
                return (string)rValue as T;

            var obj = JsonConvert.DeserializeObject<T>(rValue);

            return obj;
        }

        public static HashEntry[] ToHashEntrys<T>(this IDictionary<string, T> dic) where T : class
        {
            List<HashEntry> entrys = new List<HashEntry>();
            foreach (var item in dic)
            {
                HashEntry entry = new HashEntry(item.Key, item.Value.ToRedisValue());
                entrys.Add(entry);
            }
            return entrys.ToArray();
        }

        public static Dictionary<string, T> ToDictionary<T>(this HashEntry[] entries) where T : class
        {
            var dic = new Dictionary<string, T>();
            foreach (var item in entries)
            {
                dic.Add(item.Name, item.Value.ToObject<T>());
            }

            return dic;
        }
    }
}
