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
            Task<StackExchange.Redis.RedisValue> StringGetAsync(long clientid, string key);
        }
}
