using System;
using StackExchange.Redis;
using RedisTest.Redis;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RedisTest
{
    class Program
    {
        static ConnectionMultiplexer redis;
        static ConfigurationOptions option = ConfigurationOptions.Parse("47.99.147.160:6379,connectTimeout=2000");
        static void Main(string[] args)
        {


            //CommandSet();
            //RedisTranscation();
            //ConnSentinelRedis();
            TestRedisHelper();
            Console.Read();
        }

        static void TestRedisHelper()
        {
            try
            {
                //var redisHelper = new RedisHelper("127.0.0.1:6379,127.0.0.1:6381,127.0.0.1:6382,Password=123456,connectTimeout=30000");
                //Console.WriteLine("连接字符串："+redisHelper.redis.Configuration.ToString());
                var redisHelper = RedisHelper.Instance;
                Console.WriteLine("设置key值：");
                Console.WriteLine(redisHelper.StringSet("k1", "1"));
                Console.WriteLine("获取k1值：");
                Console.WriteLine(redisHelper.StringGet("k1")[0]);
                Console.WriteLine("扩展k1:v2");
                redisHelper.StringAppend("k1", "v2");
                Console.WriteLine(redisHelper.StringGet("k1")[0]);
                Console.WriteLine("setnx设置k2值");
                Console.WriteLine("设置成功：" + redisHelper.StringSetNx("k2", "v2"));
                Console.WriteLine("setnx设置k2值");
                Console.WriteLine("设置成功：" + redisHelper.StringSetNx("k2", "v2"));
                Console.WriteLine("测试incr、decr");
                Console.WriteLine("incr");
                Console.WriteLine(redisHelper.StringIncryBy("k3"));
                Console.WriteLine("incr");
                Console.WriteLine(redisHelper.StringIncryBy("k3"));
                Console.WriteLine("incr");
                Console.WriteLine(redisHelper.StringIncryBy("k3"));
                Console.WriteLine("incrby3");
                Console.WriteLine(redisHelper.StringIncryBy("k3", 3));
                Console.WriteLine("decr");
                Console.WriteLine(redisHelper.StringDecrBy("k3"));
                Console.WriteLine("decrby2");
                Console.WriteLine(redisHelper.StringDecrBy("k3", 2));
                Console.WriteLine("设置k1有效时间为20s");
                Console.WriteLine("设置成功：" + redisHelper.KeyExpire("k1", TimeSpan.FromSeconds(20)));
                Console.WriteLine("设置k4值为Student...");
                Console.WriteLine("设置成功：" + redisHelper.StringSet("k4", new Student
                {
                    age = 18,
                    name = "沈里林",
                    tall = 180
                }));

                Student stu = redisHelper.StringGet<Student>("k4");
                Console.WriteLine($"获取k4值：姓名：{stu.name},年龄：{stu.age},身高：{stu.tall}");

                Console.WriteLine("----------------------分割线List----------------------");
                Console.WriteLine("插入队列1，2，3");
                Console.WriteLine($"成功插入{redisHelper.Enqueue("easyList", "1", "2", "3")}个");
                Console.WriteLine("插入引用类型数据：stu1、stu2");
                Console.WriteLine($"成功插入{redisHelper.Enqueue("complexList", new Student { name = "sll" }, new Student { name = "slh" })}个");
                Console.WriteLine("获取easyList所有数值:");
                foreach (string str in redisHelper.ListGetRange<string>("easyList"))
                    Console.WriteLine(str);
                Console.WriteLine("获取complexList所有数值:");
                foreach (Student stu1 in redisHelper.ListGetRange<Student>("complexList"))
                    Console.WriteLine(stu1.name);
                Console.WriteLine("easyList出队:");
                Console.WriteLine($"值：{redisHelper.Dequeue<string>("easyList")}成功出队!");
                Console.WriteLine("获取easyList所有数值:");
                foreach (string str in redisHelper.ListGetRange<string>("easyList"))
                    Console.WriteLine(str);
                Console.WriteLine("easyList删除队列值：");
                redisHelper.ListRemove("easyList", "2");
                Console.WriteLine("获取easyList所有数值:");
                foreach (string str in redisHelper.ListGetRange<string>("easyList"))
                    Console.WriteLine(str);
                Console.WriteLine("complexList出队:");
                Console.WriteLine($"值：{redisHelper.Dequeue<Student>("complexList").name}成功出队!");
                Console.WriteLine("complexList所有数值:");
                foreach (Student str in redisHelper.ListGetRange<Student>("complexList"))
                    Console.WriteLine(str.name);
                Console.WriteLine("complexList删除队列值：");
                redisHelper.ListRemove("complexList", new Student { name = "slh" });
                Console.WriteLine("获取complexList所有数值:");
                foreach (Student str in redisHelper.ListGetRange<Student>("complexList"))
                    Console.WriteLine(str);
                Console.WriteLine("设置easyList索引0号位的值：");
                redisHelper.ListSetByIndex("easyList", 0, "10");
                Console.WriteLine("easyList索引0号位的值出队：");
                Console.WriteLine(redisHelper.Dequeue<string>("easyList"));
                Console.WriteLine("easyList索引0号位的值出队：");
                redisHelper.Dequeue<string>("easyList");

                Console.WriteLine("类型转换失败：");
                redisHelper.Enqueue("l1", "v1");
                Console.WriteLine(redisHelper.Dequeue<string>("l1"));

                Console.WriteLine("----------------------分割线hash----------------------");
                Console.WriteLine("设置单个hash值");
                Console.WriteLine($"设置成功？{redisHelper.HashSet("hash1", "name", "沈丽林")}");
                Console.WriteLine("获取hash1 name：");
                Console.WriteLine(redisHelper.HashGet<string>("hash1", "name"));
                Console.WriteLine("设置一个hash对象");
                redisHelper.HashSet("hash2", new Dictionary<string, string> {
                    { "name","sll"},{ "tall","180"},{ "age","18"},
                });
                Console.WriteLine("获取hash2中所有的key");
                redisHelper.HashKeys("hash2").ForEach(item =>
                {
                    Console.WriteLine(item);
                });
                Console.WriteLine("判断hash2中是否存在name字段");
                Console.WriteLine($"存在？{redisHelper.HashExists("hash2", "name")}");
                Console.WriteLine("hash2中age自增");
                Console.WriteLine(redisHelper.HashIncrement("hash2", "age"));
                Console.WriteLine("hash2中age自增10.1");
                Console.WriteLine(redisHelper.HashIncrement("hash2", "age", 10.1));
                Console.WriteLine("获取hash对象：");
                foreach (var item in redisHelper.HashGetAll<string>("hash2"))
                    Console.WriteLine($"{item.Key}:{item.Value}");
                Console.WriteLine("删除hash2中的age\tall字段");
                Console.WriteLine($"成功删除{redisHelper.HashDelete("hash2", "age", "tall")}个");
                Console.WriteLine("设置hash复杂对象：");
                redisHelper.HashSet("hash2", new Dictionary<string, Student> {
                    { "zhangsan",new Student{ age=18,name="张三",tall = 188} },
                    { "lisi",new Student{ age=18,name="李四",tall = 188} },
                    { "wangwu",new Student{age=18,name="王五",tall = 188 } }
                });
                Console.WriteLine("获取hash对象：");
                foreach (var item in redisHelper.HashGetAll<string>("hash2"))
                    Console.WriteLine($"{item.Key}:{item.Value}");

                Console.WriteLine("-------------------------------Set---------------------------------");
                Console.WriteLine($"添加多个元素到计集合中...,成功添加set1:{redisHelper.SetAdd("set1", "1", "2", "3")}个");
                Console.WriteLine($"添加多个元素到计集合中...,成功添加set2:{redisHelper.SetAdd("set2", new Student { name = "sll" }, new Student { name = "sll2" }, new Student { name = "sll" })}个");
                Console.WriteLine("查看集合set1中的所有元素：");
                foreach (var item in redisHelper.SetMembers<string>("set1"))
                    Console.WriteLine(item);
                Console.WriteLine("查看集合set2中的所有元素：");
                foreach (var item in redisHelper.SetMembers<string>("set2"))
                    Console.WriteLine(item);
                Console.WriteLine($"获取集合中元素个数set1:{redisHelper.SetCard("set1")},set2:{redisHelper.SetCard("set2")}");
                Console.WriteLine($"获取集合set1中的随机元素：");
                for (int i = 0; i < 5; i++)
                    Console.WriteLine(redisHelper.SetRandomMembers<string>("set1")[0]);
                Console.WriteLine($"获取集合set2中的随机元素：");
                for (int i = 0; i < 5; i++)
                    Console.WriteLine(redisHelper.SetRandomMembers<string>("set2")[0]);
                Console.WriteLine("删除set1中元素并返回值：" + redisHelper.SetPop<string>("set1")[0]);
                Console.WriteLine($"成功删除set1中{redisHelper.SetRemove("set1", "1", "2", "3")}个元素：");
                Console.WriteLine("插入数据到set3、set4");
                redisHelper.SetAdd("set3", new string[] { "1", "2", "3", "4" });
                redisHelper.SetAdd("set4", new string[] { "3", "4", "5", "6", "7" });
                Console.WriteLine("查看集合set3中的所有元素：");
                foreach (var item in redisHelper.SetMembers<string>("set3"))
                    Console.WriteLine(item);
                Console.WriteLine("查看集合set4中的所有元素：");
                foreach (var item in redisHelper.SetMembers<string>("set4"))
                    Console.WriteLine(item);
                Console.WriteLine($"移动set3数据到set4中:{redisHelper.SetMoveTo("set3", "set4", "1")}");
                Console.WriteLine("查看集合set3中的所有元素：");
                foreach (var item in redisHelper.SetMembers<string>("set3"))
                    Console.WriteLine(item);
                Console.WriteLine("查看集合set4中的所有元素：");
                foreach (var item in redisHelper.SetMembers<string>("set4"))
                    Console.WriteLine(item);
                Console.WriteLine("查看set3、set4的交集");
                foreach (var item in redisHelper.SetCombine<string>(OperatorEnum.Intersect, "set3", "set4"))
                    Console.WriteLine(item);
                Console.WriteLine("查看set3、set4的并集");
                foreach (var item in redisHelper.SetCombine<string>(OperatorEnum.Union, "set3", "set4"))
                    Console.WriteLine(item);
                Console.WriteLine("查看set3、set4的差集");
                foreach (var item in redisHelper.SetCombine<string>(OperatorEnum.Difference, "set3", "set4"))
                    Console.WriteLine(item);

                Console.WriteLine("-----------------------ZSet-----------------------------");
                Console.WriteLine("增加多个有序集合项:");
                redisHelper.SortedSetAdd("zset1", "bantou", 10);
                redisHelper.SortedSetAdd("zset1", "maobi", 20);
                redisHelper.SortedSetAdd("zset1", "datou", 30);
                redisHelper.SortedSetAdd("zset1", "sll", 80);
                Console.WriteLine("查看所有集合数据，升序排序");
                foreach (var item in redisHelper.SortedSetRangeByScore<string>("zset1"))
                    Console.WriteLine(item);
                Console.WriteLine("查看所有集合数据，降序排序");
                foreach (var item in redisHelper.SortedSetRangeByScore<string>("zset1", order: OrderEnum.Descending))
                    Console.WriteLine(item);
                Console.WriteLine("查看集合中20-30分");
                foreach (var item in redisHelper.SortedSetRangeByScore<string>("zset1", 20, 30, order: OrderEnum.Descending))
                    Console.WriteLine(item);
                Console.WriteLine("sll增加10分：" + redisHelper.SortedSetIncrement("zset1", "sll", 10));
                Console.WriteLine("sll扣除20分：" + redisHelper.SortedSetDecrement("zset1", "sll", 20));
                Console.WriteLine("zset1删除:" + redisHelper.SortedSetRemove("zset1", "maobi"));
                Console.WriteLine("查看所有集合数据，升序排序");
                foreach (var item in redisHelper.SortedSetRangeByScore<string>("zset1"))
                    Console.WriteLine(item);
                Console.WriteLine("删除指定范围的数据:");
                redisHelper.SortedSetRemoveByScore("zset1", stop: 70, ex: ExcludeEnum.Stop);
                Console.WriteLine("查看所有集合数据，升序排序");
                foreach (var item in redisHelper.SortedSetRangeByScore<string>("zset1"))
                    Console.WriteLine(item);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error:{e.Message}");
            }
        }

        interface IFly
        {

        }

        class Student
        {
            public string name;
            public int age;
            public long tall;
        }

        /// <summary>
        /// 哨兵模式下连接redis主节点
        /// </summary>
        static void ConnSentinelRedis()
        {
            try
            {
                ConfigurationOptions sentinelOptions = new ConfigurationOptions();
                sentinelOptions.EndPoints.Add("127.0.0.1", 26379);
                sentinelOptions.TieBreaker = "";
                sentinelOptions.CommandMap = CommandMap.Sentinel;
                sentinelOptions.AbortOnConnectFail = true;
                sentinelOptions.AllowAdmin = true;
                // Connect!
                ConnectionMultiplexer sentinelConnection = ConnectionMultiplexer.Connect(sentinelOptions);

                // Get a connection to the master
                ConfigurationOptions redisServiceOptions = new ConfigurationOptions();
                redisServiceOptions.ServiceName = "myredis";   //master名称
                redisServiceOptions.Password = "123456";     //master访问密码
                redisServiceOptions.AbortOnConnectFail = true;
                redisServiceOptions.AllowAdmin = true;
                ConnectionMultiplexer masterConnection = sentinelConnection.GetSentinelMasterConnection(redisServiceOptions);
                Console.WriteLine(masterConnection.Configuration);
                Console.WriteLine("success");

                IDatabase db = masterConnection.GetDatabase();
                db.StringSet("k2", "v2");
                Console.WriteLine(db.StringGet("k2"));

                Console.WriteLine("移除所有key");
                masterConnection.GetServer("127.0.0.1:6382").FlushDatabase();
                Console.WriteLine("移除成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine("fail：" + ex.Message);
            }
        }

        static void CommandSet()
        {
            Console.WriteLine("清空数据库...");
            var server = redis.GetServer("47.99.147.160:6379");
            server.FlushDatabase();//清空数据库

            var db = redis.GetDatabase();
            Console.WriteLine("判断username是否存在..." + db.KeyExists("username"));
            Console.WriteLine("新增键值对...");
            db.StringSet("username", "shenlilin");
            db.StringSet("pwd", "123456");
            Console.WriteLine("系统所有键:");
            Console.WriteLine(server.Keys(pattern: "*"));
        }

        /// <summary>
        /// 事务
        /// </summary>
        static void RedisTranscation()
        {
            string key = "hashkey", field = "hashfield";

            var db = redis.GetDatabase();
            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.HashNotExists(key, field));
            tran.HashSetAsync(key, field, "value");
            var res = tran.Execute();
            if (res)
            {
                //执行事务
            }
            else
            {
                //取消执行
            }
        }
    }
}
