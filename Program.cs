using Cluster;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RedisMaxServer
{
    class Program
    {
        private int _requests = 0;
        private int _responses = 0;
        private readonly Random _rand = new Random();
        private IDatabase [] dbs;

        static void Main(string[] args)
        {
            Program prog = new Program();
            var options = Options.Parse(args);
            if (options == null) return;

            if (options.mode == Mode.server)
            {
                int listeningport = Int32.Parse(args[1]);
                AsynchronousSocketListener.StartListening(listeningport);
                return;
            }
            prog.CreateClients(options);
            if(!prog.SetupCluster(options))
            {
                Console.Write("Setting up cluster failed");
            }
            prog.DoRequests(options);
            
        }

        void CreateClients(Options options)
        {
            var config = new ConfigurationOptions();
            config.EndPoints.Add(options.server);
            config.Ssl = false;
            config.Password = options.password;
            config.AllowAdmin = true;

            dbs = new IDatabase[options.clients];

            for (int i = 0; i < options.clients; i++)
            {
                var cm = ConnectionMultiplexer.Connect(config);
                var db = cm.GetDatabase();

                dbs[i] = db;
            }
        }
       

        void DoRequests(Options option)
        {
            Console.WriteLine("Warmup for {0}ms...", option.warmup);
            Thread.Sleep(option.warmup);
            var start = DateTime.Now;
            var _responsesAfterWarmup = _responses;

            int lastRequests = 0;
            int lastResponses = 0;
            Socket client = AsynchronousClient.StartClient(option.rpsServer, option.rpsServerPort);
            while (true)
            {
                var elapsed = (DateTime.Now - start).TotalSeconds;
                var requests = _requests;
                var responses = _responses;
                lastRequests = requests;
                lastResponses = responses;
                try
                {
                    AsynchronousClient.Send(client, Math.Round((responses - _responsesAfterWarmup) / elapsed));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }
                Thread.Sleep(1000);
            }
        }

        private bool SetupCluster(Options option)
        {
            byte[] val = new byte[option.valueSize];
            _rand.NextBytes(val);

            if (dbs[0] == null)
            {
                return false;
            }
            
            EndPoint e = dbs[0].Multiplexer.GetEndPoints()[0];
            var serverType = dbs[0].Multiplexer.GetServer(e);
            if (serverType.ServerType == ServerType.Cluster)
            {
                Console.WriteLine("Setting up cluster with test keys, value size {0}", option.valueSize );
                var cn = serverType.ClusterNodes();
                Dictionary<int, string> keys = new Dictionary<int, string>();
                int count = 0;
                int totalmasters = dbs[0].Multiplexer.GetEndPoints().Length / 2;

                do 
                {
                    string keyname = "test" + count;
                    keys[Int32.Parse(cn.GetBySlot(dbs[0].Multiplexer.HashSlot(keyname)).EndPoint.ToString().Split(':')[1])] = keyname;
                    dbs[0].StringSet(keyname, val);
                    count++;
                }while (keys.Count < totalmasters);

                string[] keysarr = keys.Values.ToArray();
                for (int i = 0; i < option.asyncRequests; i++)
                {
                    SendRequest(dbs[i % option.clients], keysarr[i % keysarr.Length]);
                }
            }
            else
            {
                Console.WriteLine("Setting up cache with test keys, value size {0}", option.valueSize);
                dbs[0].StringSet("test", val);
                for (int i = 0; i < option.asyncRequests; i++)
                {
                    SendRequest(dbs[i % option.clients], "test");
                }
            }
            return true;
        }

        void SendRequest(IDatabase db,string val)
        {
            Interlocked.Increment(ref _requests);
            //db.StringGetAsync(_rand.Next(0, _values).ToString()).ContinueWith(t => {
            db.StringGetAsync(val).ContinueWith(t =>
            {
                Interlocked.Increment(ref _responses);
                SendRequest(db,val);
            });
        }
      
    }
}
