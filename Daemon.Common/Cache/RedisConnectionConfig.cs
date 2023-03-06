namespace Daemon.Common.Cache
{
    using System.Collections.Generic;
    public class RedisConnectionConfig
    {
        public string ConnectionStr { get; set; }
        public bool IsSentinel { get; set; }
        public List<string> Sentinel { get; set; }
    }
}
