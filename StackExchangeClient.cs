using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClusterBenchmark
{
    class StackExchangeClient : ClusterBenchmark.IClient
    {
        private IDatabase[] dbs;
        public ConfigurationOptions ConfigOptions { get; private set; }
        public Options Options { get; private set; }

        public StackExchangeClient(Options options)
        {
            dbs = new IDatabase[options.clients];
            ConfigOptions = InitConfigurationOptions(options);
            Options = options;
        }

        private ConfigurationOptions InitConfigurationOptions(Options options)
        {
            var config = new ConfigurationOptions();
            config.EndPoints.Add(options.redisserver, options.redisport);
            config.Ssl = options.ssl;
            config.Password = options.password;
            config.AllowAdmin = true;
            return config;
        }

        public bool CreateClients()
        {
            for (int i = 0; i < Options.clients; i++)
            {
                try
                {
                    var cm = ConnectionMultiplexer.Connect(ConfigOptions);
                    var db = cm.GetDatabase();
                    dbs[i] = db;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + e.StackTrace);
                    return false;
                }
            }
            return true;
        }

        public Task<RedisValue> StringGetAsync(long clientid, string key)
        {
            return dbs[clientid].StringGetAsync(key);
        }

        public string[] SetupCluster()
        {
            string[] keysarr = null;
            Random rand = new Random();
            byte[] val = new byte[Options.valueSize];
            rand.NextBytes(val);
            using (var cm = ConnectionMultiplexer.Connect(ConfigOptions))
            {
                var db = cm.GetDatabase();
                EndPoint e = db.Multiplexer.GetEndPoints()[0];
                var serverType = db.Multiplexer.GetServer(e);
                if (serverType.ServerType == ServerType.Cluster)
                {
                    Console.WriteLine("Setting up cluster with keyname prefix {0}, value size {1}", Options.keyPrefix, Options.valueSize);
                    var cn = serverType.ClusterNodes();
                    Dictionary<int, string> keys = new Dictionary<int, string>();
                    int count = 0;
                    int totalmasters = db.Multiplexer.GetEndPoints().Length / 2;

                    do
                    {
                        string keyname = Options.keyPrefix + count;
                        keys[Int32.Parse(cn.GetBySlot(db.Multiplexer.HashSlot(keyname)).EndPoint.ToString().Split(':')[1])] = keyname;
                        db.StringSet(keyname, val);
                        count++;
                    } while (keys.Count < totalmasters);

                    keysarr = keys.Values.ToArray();

                }
                else
                {
                    Console.WriteLine("Setting up cache with test keys, value size {0}", Options.valueSize);
                    db.StringSet(Options.keyPrefix, val);
                    keysarr = new string[] { Options.keyPrefix };
                }
            }
            return keysarr;
        }
    }
}
