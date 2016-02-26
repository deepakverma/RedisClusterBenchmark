using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster
{

    public class Options
    {
        [Option('m',"servermode",HelpText = "Run in server mode")]
        public bool servermode { get; set; }

        [Option('h', "redisserver", Required = false, HelpText = "Redis Server end point")]
        public string redisserver { get; set; }

        [Option('p', "redisport", HelpText = "Redis Server port. Default 6379")]
        public int redisport { get; set; }

        [Option("ssl", HelpText = "Use SSL connection")]
        public bool ssl { get; set; }

        [Option('a', "redispassword", Required = false, HelpText = "Redis Server password")]
        public string password { get; set; }

        [Option('n', "asyncrequests", DefaultValue = 12000, HelpText = "Number of async request load to be maintained")]
        public int asyncRequests { get; set; }

        [Option('c', "connections", DefaultValue = 2, HelpText = "Number of client connection to create")]
        public int clients { get; set; }

        [Option('d', "datasize", DefaultValue = 1024, HelpText = "Value size in bytes")]
        public int valueSize { get; set; }

        [Option('w', "warmup", DefaultValue = 5000, HelpText = "Warm up period before running the tests")]
        public int warmup { get; set; }

        [Option("server", HelpText = "RPS server endpoint")]
        public string RPSServer { get; set; }

        [Option("port", DefaultValue=6400, HelpText = "RPS server port")]
        public int RPSport { get; set; }

        [Option('k', "keyprefix", DefaultValue = "__clusterbenchmark_test", HelpText = "Key prefix of the keys this tool will be creating to perform operations")]
        public string keyPrefix { get; set; }

        [Option('v', "verbose")]
        public bool verbose { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText()
            {
                AddDashesToOption = true,
            };

            help.AddPreOptionsLine("Usage: Server mode: Cluster --server ");
            help.AddPreOptionsLine("cluster -h dvperf4shard.redis.cache.windows.net  -a password -s 127.0.0.1");
            help.AddOptions(this);
            return help;
        }


        public static Options Parse(string[] args)
        {
            Options options = new Options();

            if (!Parser.Default.ParseArguments(args, options))
            {
                return null;
            }
            if(!options.servermode && options.redisserver == null)
            {
                Console.WriteLine(options.GetUsage());
                return null;
            }
            if (options.ssl == true && options.redisport == 0)
            {
                options.redisport = 6380;
            }
            if (options.ssl == false && options.redisport == 0)
            {
                options.redisport = 6379;
            }

            return options;
        }
    }
}
