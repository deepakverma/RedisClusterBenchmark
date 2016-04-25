using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterBenchmark
{
        public interface IClient
        {
            bool CreateClients();
            string[] SetupCluster();
            Task<bool> StringSetAsync(long clientid, string key, string value);
            Task<RedisValue> StringGetAsync(long clientid, string key);
        }
}
