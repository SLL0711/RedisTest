using System;
using System.Collections.Generic;
using System.Text;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace RedisTest.Redis
{
    public class RedisHelper : IDisposable
    {
        private ConnectionMultiplexer _redis = null;
        private IDatabase db = null;
        private static object lock_obj = new object();
        private static RedisHelper _instance;
        //private static string _redisConfigString;

        private static string RedisConfigString
        {
            get
            {
                //_redisConfigString = GetConfigStr();
                return GetConfigStr();
            }
        }

        //public ConnectionMultiplexer redis { get { return _redis; } }

        /// <summary>
        /// 实现多路复用器的单例模式，避免重复连接
        /// </summary>
        /// <param name="connStr"></param>
        private RedisHelper(string connStr)
        {
            _redis = ConnectionMultiplexer.Connect(connStr);
            db = _redis.GetDatabase();

            #region 废弃

            //if (_redis == null)
            //{
            //    lock (lock_obj)
            //    {
            //        if (_redis == null)
            //        {
            //            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //            sw.Start();
            //            _redis = ConnectionMultiplexer.Connect(connStr);
            //            sw.Stop();
            //            Console.WriteLine("连接耗时：" + sw.ElapsedMilliseconds);
            //        }
            //    }
            //}

            #endregion
        }

        public static RedisHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (lock_obj)
                    {
                        if (_instance == null)
                        {
                            _instance = new RedisHelper(RedisConfigString);
                        }
                    }
                }

                return _instance;
            }
        }

        private static string GetConfigStr()
        {
            //TODO:.json文件获取配置
            return "127.0.0.1:6379,127.0.0.1:6381,127.0.0.1:6382,Password=123456,connectTimeout=30000";
        }

        public void SelectDB(int index)
        {
            this.db = _redis.GetDatabase(Math.Abs(index));
        }

        #region String

        /// <summary>
        /// 获取字符串集合
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<string> StringGet(params string[] keys)
        {
            List<string> l = new List<string>();
            foreach (string k in keys)
            {
                l.Add(db.StringGet(k));
            }
            return l;
        }

        /// <summary>
        /// 获取某个key并反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T StringGet<T>(string key) where T : class
        {
            RedisValue rValue = db.StringGet(key);
            return rValue.ToObject<T>();
        }

        /// <summary>
        /// 设置String
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timespan"></param>
        /// <param name="when"></param>
        /// <returns></returns>

        public bool StringSet<T>(string key, T value, TimeSpan? timespan = null, When when = When.Always) where T : class
        {
            return db.StringSet(key, value.ToRedisValue<T>(), timespan, when);
        }

        /// <summary>
        /// String setnx 不存在时设置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timespan"></param>
        /// <returns></returns>

        public bool StringSetNx<T>(string key, T value, TimeSpan? timespan = null) where T : class
        {
            return db.StringSet(key, value.ToRedisValue<T>(), timespan, When.NotExists);
        }

        /// <summary>
        /// string追加字符
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>

        public long StringAppend<T>(string key, T value) where T : class
        {
            return db.StringAppend(key, value.ToRedisValue<T>());
        }

        /// <summary>
        /// 设置key过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timespan"></param>
        /// <returns></returns>

        public bool KeyExpire(string key, TimeSpan timespan)
        {
            return db.KeyExpire(key, timespan);
        }

        /// <summary>
        /// 设置自增i
        /// </summary>
        /// <param name="key"></param>
        /// <param name="i"></param>
        /// <returns></returns>

        public double StringIncryBy(string key, double i = 1)
        {
            return db.StringIncrement(key, (double)i);
        }

        /// <summary>
        /// 设置自减i
        /// </summary>
        /// <param name="key"></param>
        /// <param name="i"></param>
        /// <returns></returns>

        public double StringDecrBy(string key, double i = 1)
        {
            return db.StringDecrement(key, (double)i);
        }

        /// <summary>
        /// 截取key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>

        public string StringGetRange(string key, long begin = 0, long end = -1)
        {
            return db.StringGetRange(key, begin, end);
        }

        /// <summary>
        /// 设置string中的部分
        /// </summary>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <param name="value"></param>
        /// <returns></returns>

        public string StringSetRange(string key, long offset, string value)
        {
            return db.StringSetRange(key, offset, value);
        }

        #endregion

        #region List

        /// <summary>
        /// 入队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>

        public long Enqueue<T>(string key, params T[] value) where T : class
        {
            RedisValue[] rvalues = value.ToRedisValues();
            return db.ListRightPush(key, rvalues);
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>

        public T Dequeue<T>(string key) where T : class
        {
            RedisValue rValue = db.ListLeftPop(key);
            return rValue.ToObject<T>();
        }

        /// <summary>
        /// 获取List中范围数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>

        public T[] ListGetRange<T>(string key, long start = 0, long stop = -1) where T : class
        {
            RedisValue[] rValues = db.ListRange(key, start, stop);

            T[] t = new T[rValues.Length];
            for (int i = 0; i < rValues.Length; i++)
            {
                t[i] = rValues[i].ToObject<T>();
            }
            return t;
        }

        /// <summary>
        /// 移除List中的值 count=0所有匹配的值 count=1从左往右一个  count=-1 从右往左一个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns></returns>

        public long ListRemove<T>(string key, T value, long count = 0) where T : class
        {
            return db.ListRemove(key, value.ToRedisValue(), count);
        }

        /// <summary>
        /// 设置List集合中索引位置index的值并序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void ListSetByIndex<T>(string key, long index, T value) where T : class
        {
            db.ListSetByIndex(key, index, value.ToRedisValue());
        }

        #endregion

        #region Hash

        /// <summary>
        /// 设置单个字段对应值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool HashSet<T>(string key, string field, T value) where T : class
        {
            return db.HashSet(key, field, value.ToRedisValue());
        }

        /// <summary>
        ///设置多个hash键值对
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dic"></param>
        public void HashSet<T>(string key, IDictionary<string, T> dic) where T : class
        {
            db.HashSet(key, dic.ToHashEntrys());
        }

        /// <summary>
        /// 获取hash中某个key对应的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public T HashGet<T>(string key, string fields) where T : class
        {
            RedisValue rValue = db.HashGet(key, fields);
            return rValue.ToObject<T>();
        }

        /// <summary>
        /// 获取hash对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public Dictionary<string, T> HashGetAll<T>(string key) where T : class
        {
            HashEntry[] entries = db.HashGetAll(key);
            return entries.ToDictionary<T>();
        }

        /// <summary>
        /// 获取hash中所有key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string> HashKeys(string key)
        {
            RedisValue[] rValues = db.HashKeys(key);
            List<string> list = new List<string>();
            foreach (var item in rValues)
            {
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// 判断hash中是否存在某个field
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public bool HashExists(string key, string field)
        {
            return db.HashExists(key, field);
        }

        /// <summary>
        /// 删除一个\多个hash字段
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public long HashDelete(string key, params string[] field)
        {
            RedisValue[] rValues = field.ToRedisValues();
            return db.HashDelete(key, rValues);
        }

        /// <summary>
        /// hash字段自增
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public double HashIncrement(string key, string field, double value = 1)
        {
            return db.HashIncrement(key, field, value);
        }

        #endregion

        #region Set

        /// <summary>
        /// 添加一个或多个元素到集合中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        public long SetAdd<T>(string key, params T[] members) where T : class
        {
            RedisValue[] rValues = new RedisValue[members.Length];
            for (int i = 0; i < members.Length; i++)
            {
                rValues[i] = members[i].ToRedisValue();
            }

            return db.SetAdd(key, rValues);
        }

        /// <summary>
        /// 查看集合中的所有元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> SetMembers<T>(string key) where T : class
        {
            RedisValue[] rValues = db.SetMembers(key);
            List<T> t = new List<T>();
            for (int i = 0; i < rValues.Length; i++)
            {
                t.Add(rValues[i].ToObject<T>());
            }
            return t;
        }

        /// <summary>
        /// 获取集合中的元素数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SetCard(string key)
        {
            return db.SetLength(key);
        }

        /// <summary>
        /// 从集合中删除一个或多个元素
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public long SetRemove<TValue>(string key, params TValue[] t) where TValue : class
        {
            return db.SetRemove(key, t.ToRedisValues());
        }

        /// <summary>
        /// 删除并获取一个集合中的随机元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<T> SetPop<T>(string key, long count = 1) where T : class
        {
            RedisValue[] rValues = db.SetPop(key, count);
            List<T> list = new List<T>();
            for (int i = 0; i < rValues.Length; i++)
            {
                list.Add(rValues[i].ToObject<T>());
            }

            return list;
        }

        /// <summary>
        /// 获取集合中的随机元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<T> SetRandomMembers<T>(string key, long count = 1) where T : class
        {
            RedisValue[] rValues = db.SetRandomMembers(key, count);
            List<T> list = new List<T>();
            for (int i = 0; i < rValues.Length; i++)
            {
                list.Add(rValues[i].ToObject<T>());
            }

            return list;
        }

        /// <summary>
        /// 移动集合里一个元素到另一个集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceKey"></param>
        /// <param name="desKey"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public bool SetMoveTo<T>(string sourceKey, string desKey, T member) where T : class
        {
            return db.SetMove(sourceKey, desKey, member.ToRedisValue());
        }

        /// <summary>
        /// 获取两个集合的并集、交集、差集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ope"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<T> SetCombine<T>(OperatorEnum ope, params string[] keys) where T : class
        {
            RedisKey[] rKeys = new RedisKey[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                rKeys[i] = keys[i];
            }

            RedisValue[] rValues = db.SetCombine((SetOperation)ope.GetHashCode(), rKeys);
            List<T> list = new List<T>();
            for (int i = 0; i < rValues.Length; i++)
            {
                list.Add(rValues[i].ToObject<T>());
            }

            return list;
        }

        #endregion

        #region SortedSet

        /// <summary>
        /// 增加一个有序集合项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public bool SortedSetAdd(string key, string member, double score)
        {
            return db.SortedSetAdd(key, member, score);
        }

        /// <summary>
        /// 删除有序集合项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public long SortedSetRemove<T>(string key, params T[] t) where T : class
        {
            RedisValue[] rValues = new RedisValue[t.Length];
            for (int i = 0; i < t.Length; i++)
            {
                rValues[i] = t[i].ToRedisValue();
            }

            return db.SortedSetRemove(key, rValues);
        }

        /// <summary>
        /// 删除指定范围内的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public long SortedSetRemoveByScore(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, ExcludeEnum ex = ExcludeEnum.Both)
        {
            return db.SortedSetRemoveRangeByScore(key, start, stop, (Exclude)ex);
        }

        /// <summary>
        /// 根据score范围获取集合项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="ex"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public List<T> SortedSetRangeByScore<T>(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
            ExcludeEnum ex = ExcludeEnum.None, OrderEnum order = OrderEnum.Ascending) where T : class
        {
            RedisValue[] rValues = db.SortedSetRangeByScore(key, start, stop, (Exclude)ex, (Order)order);
            List<T> list = new List<T>();
            for (int i = 0; i < rValues.Length; i++)
            {
                list.Add(rValues[i].ToObject<T>());
            }
            return list;
        }

        /// <summary>
        /// 指定元素增加分数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public double SortedSetIncrement<T>(string key, T value, double score) where T : class
        {
            return db.SortedSetIncrement(key, value.ToRedisValue(), score);
        }

        /// <summary>
        /// 指定元素扣除分数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public double SortedSetDecrement<T>(string key, T value, double score) where T : class
        {
            return db.SortedSetDecrement(key, value.ToRedisValue(), score);
        }

        #endregion

        //TODO
        //Keys相关
        //Server相关
        //分布式锁

        public void Dispose()
        {

        }
    }
}
