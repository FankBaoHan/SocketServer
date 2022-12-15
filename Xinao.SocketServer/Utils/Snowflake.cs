using System;
using System.Collections.Generic;
using System.Text;

namespace Xinao.SocketServer.Utils
{
    #region 

    /*

    /// <summary>
    /// Snowflake算法
    /// </summary>
    public class Snowflake
    {
        private static long machineId;//机器ID
        private static long datacenterId = 0L;//数据ID
        private static long sequence = 0L;//计数从零开始

        private static long twepoch = 687888001020L; //唯一时间随机量

        private static long machineIdBits = 5L; //机器码字节数
        private static long datacenterIdBits = 5L;//数据字节数
        public static long maxMachineId = -1L ^ -1L << (int)machineIdBits; //最大机器ID
        private static long maxDatacenterId = -1L ^ (-1L << (int)datacenterIdBits);//最大数据ID

        private static long sequenceBits = 12L; //计数器字节数，12个字节用来保存计数码        
        private static long machineIdShift = sequenceBits; //机器码数据左移位数，就是后面计数器占用的位数
        private static long datacenterIdShift = sequenceBits + machineIdBits;
        private static long timestampLeftShift = sequenceBits + machineIdBits + datacenterIdBits; //时间戳左移动位数就是机器码+计数器总字节数+数据字节数
        public static long sequenceMask = -1L ^ -1L << (int)sequenceBits; //一微秒内可以产生计数，如果达到该值则等到下一微秒在进行生成
        private static long lastTimestamp = -1L;//最后时间戳

        private static object syncRoot = new object();//加锁对象

        static Snowflake snowflake;

        public static Snowflake Instance()
        {
            if (snowflake == null) snowflake = new Snowflake();

            return snowflake;
        }

        public Snowflake()
        {
            Snowflakes(0L, -1);
        }

        public Snowflake(long machineId)
        {
            Snowflakes(machineId, -1);
        }

        public Snowflake(long machineId, long datacenterId)
        {
            Snowflakes(machineId, datacenterId);
        }

        private void Snowflakes(long machineId, long datacenterId)
        {
            if (machineId >= 0)
            {
                if (machineId > maxMachineId) throw new Exception("机器码ID非法");

                Snowflake.machineId = machineId;
            }

            if (datacenterId >= 0)
            {
                if (datacenterId > maxDatacenterId) throw new Exception("数据中心ID非法");

                Snowflake.datacenterId = datacenterId;
            }
        }

        /// <summary>
        /// 生成当前时间戳
        /// </summary>
        /// <returns>毫秒</returns>
        private static long GetTimestamp()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// 获取下一微秒时间戳
        /// </summary>
        /// <param name="lastTimestamp"></param>
        /// <returns></returns>
        private static long GetNextTimestamp(long lastTimestamp)
        {
            long timestamp = GetTimestamp();
            if (timestamp <= lastTimestamp) timestamp = GetTimestamp();

            return timestamp;
        }

        /// <summary>
        /// 获取长整形的ID
        /// </summary>
        /// <returns></returns>
        public long GetId()
        {
            lock (syncRoot)
            {
                long timestamp = GetTimestamp();
                if (Snowflake.lastTimestamp == timestamp)
                {
                    //同一微秒中生成ID
                    sequence = (sequence + 1) & sequenceMask; //用&运算计算该微秒内产生的计数是否已经到达上限

                    //一微秒内产生的ID计数已达上限，等待下一微秒
                    if (sequence == 0) timestamp = GetNextTimestamp(Snowflake.lastTimestamp);

                }
                else sequence = 0L;  //不同微秒生成ID

                if (timestamp < lastTimestamp) throw new Exception("时间戳比上一次生成ID时时间戳还小，故异常");

                Snowflake.lastTimestamp = timestamp; //把当前时间戳保存为最后生成ID的时间戳
                long id = ((timestamp - twepoch) << (int)timestampLeftShift)
                    | (datacenterId << (int)datacenterIdShift)
                    | (machineId << (int)machineIdShift)
                    | sequence;

                return id;
            }
        }

        /// <summary>
        /// 获取字符串型ID
        /// </summary>
        /// <returns></returns>
        public string GetUId() => this.GetId().ToString();

    }

    */

    #endregion

    public class Snowflake
    {
        private static IdWorker worker = new IdWorker(1, 1);

        /// <summary>
        /// 获取long型ID
        /// </summary>
        /// <returns></returns>
        public static long GetId() => worker.NextId();

        /// <summary>
        /// 获取字符串型ID
        /// </summary>
        /// <returns></returns>
        public static string GetUId() => worker.NextId().ToString();

    }

    /// <summary>
    /// 分布式主键生成
    /// https://github.com/stulzq/snowflake-net
    /// </summary>
    public class IdWorker
    {
        #region 内部变量

        public const long Twepoch = 687888001020L; // 基准时间 (默认为1288834974657L，为了和当前系统兼容使用687888001020L)

        const int WorkerIdBits = 5; // 机器标识位数

        const int DatacenterIdBits = 5; // 数据标志位数

        const int SequenceBits = 12;// 序列号识位数

        const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits); // 机器ID最大值

        const long MaxDatacenterId = -1L ^ (-1L << DatacenterIdBits); // 数据标志ID最大值

        private const long SequenceMask = -1L ^ (-1L << SequenceBits); // 序列号ID最大值

        private const int WorkerIdShift = SequenceBits; // 机器ID偏左移12位

        private const int DatacenterIdShift = SequenceBits + WorkerIdBits; // 数据ID偏左移17位

        public const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits; // 时间毫秒左移22位

        private long _sequence = 0L;

        private long _lastTimestamp = -1L;

        public long WorkerId { get; protected set; }

        public long DatacenterId { get; protected set; }

        public long Sequence
        {
            get { return _sequence; }
            internal set { _sequence = value; }
        }

        #endregion 内部变量

        public IdWorker(long workerId, long datacenterId, long sequence = 0L)
        {
            // 如果超出范围就抛出异常
            if (workerId > MaxWorkerId || workerId < 0)
            {
                throw new ArgumentException(string.Format("worker Id 必须大于0，且不能大于MaxWorkerId： {0}", MaxWorkerId));
            }

            if (datacenterId > MaxDatacenterId || datacenterId < 0)
            {
                throw new ArgumentException(string.Format("region Id 必须大于0，且不能大于MaxWorkerId： {0}", MaxDatacenterId));
            }

            //先检验再赋值
            WorkerId = workerId;
            DatacenterId = datacenterId;
            _sequence = sequence;
        }

        readonly object _lock = new object();

        public virtual long NextId()
        {
            lock (_lock)
            {
                var timestamp = TimeGen();
                if (timestamp < _lastTimestamp)
                {
                    throw new Exception(string.Format("时间戳必须大于上一次生成ID的时间戳.  拒绝为{0}毫秒生成id", _lastTimestamp - timestamp));
                }

                //如果上次生成时间和当前时间相同,在同一毫秒内
                if (_lastTimestamp == timestamp)
                {
                    //sequence自增，和sequenceMask相与一下，去掉高位
                    _sequence = (_sequence + 1) & SequenceMask;
                    //判断是否溢出,也就是每毫秒内超过1024，当为1024时，与sequenceMask相与，sequence就等于0
                    if (_sequence == 0)
                    {
                        //等待到下一毫秒
                        timestamp = TilNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    //如果和上次生成时间不同,重置sequence，就是下一毫秒开始，sequence计数重新从0开始累加,
                    //为了保证尾数随机性更大一些,最后一位可以设置一个随机数
                    _sequence = 0;//new Random().Next(10);
                }

                _lastTimestamp = timestamp;
                return ((timestamp - Twepoch) << TimestampLeftShift) | (DatacenterId << DatacenterIdShift) | (WorkerId << WorkerIdShift) | _sequence;
            }
        }

        /// <summary>
        /// 防止产生的时间比之前的时间还要小（由于NTP回拨等问题）,保持增量的趋势.
        /// </summary>
        /// <param name="lastTimestamp"></param>
        /// <returns></returns>
        protected virtual long TilNextMillis(long lastTimestamp)
        {
            var timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
            }
            return timestamp;
        }

        /// <summary>
        /// 获取当前的时间戳
        /// </summary>
        /// <returns></returns>
        protected virtual long TimeGen()
        {
            return TimeExtensions.CurrentTimeMillis();
        }

        /*********************************************************************************/

        private static class TimeExtensions
        {
            public static Func<long> currentTimeFunc = InternalCurrentTimeMillis;

            public static long CurrentTimeMillis()
            {
                return currentTimeFunc();
            }

            public static IDisposable StubCurrentTime(Func<long> func)
            {
                currentTimeFunc = func;
                return new DisposableAction(() =>
                {
                    currentTimeFunc = InternalCurrentTimeMillis;
                });
            }

            public static IDisposable StubCurrentTime(long millis)
            {
                currentTimeFunc = () => millis;
                return new DisposableAction(() =>
                {
                    currentTimeFunc = InternalCurrentTimeMillis;
                });
            }

            private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            private static long InternalCurrentTimeMillis()
            {
                return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
            }
        }

        private class DisposableAction : IDisposable
        {
            readonly Action _action;

            public DisposableAction(Action action)
            {
                if (action == null)
                    throw new ArgumentNullException("action");
                _action = action;
            }

            public void Dispose()
            {
                _action();
            }
        }

    }

}
