using System;
using System.Collections.Generic;
using System.Text;

namespace RedisTest.Redis
{
    /// <summary>
    /// Set无序集合
    /// </summary>
    public enum OperatorEnum
    {
        /// <summary>
        /// 差集
        /// </summary>
        Difference = 2,
        /// <summary>
        /// 交集
        /// </summary>
        Intersect = 1,
        /// <summary>
        /// 并集
        /// </summary>
        Union = 0
    }

    /// <summary>
    /// 有序集合左闭右开枚举
    /// </summary>
    public enum ExcludeEnum
    {
        None = 0,
        Start = 1,
        Stop = 2,
        Both = 3
    }

    /// <summary>
    /// 有序集合排序枚举
    /// </summary>
    public enum OrderEnum
    {
        Ascending = 0,
        Descending = 1
    }
}
