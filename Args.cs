using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster
{

    enum Mode
    {
        server,
        client
    }
    class Options
    {
        [Option('m', "mode", Required = false, DefaultValue = Mode.client, HelpText = "server or client mode to run this tool.")]
        public Mode mode { get; set; }

        [Option('h', "redisserver", Required = false, HelpText = "Redis Server end point")]
        public string server { get; set; }

        [Option('a', "password", Required = false, HelpText = "Redis Server password")]
        public string password { get; set; }

        [Option('n', "asyncrequests", DefaultValue = 12000, HelpText = "Number of async request load to be maintained")]
        public int asyncRequests { get; set; }

        [Option('c', "connections", DefaultValue = 12000, HelpText = "Number of client connection to create")]
        public int clients { get; set; }

        [Option('d', "datasize", DefaultValue = 1024, HelpText = "Value size in bytes")]
        public int valueSize { get; set; }

        [Option('w', "warmup", DefaultValue = 1, HelpText = "Warm up period before running the tests")]
        public int warmup { get; set; }

        [Option('S', "RPSserver", HelpText = "RPS server endpoint")]
        public string rpsServer { get; set; }

        [Option('P', "RPSserverport", HelpText = "RPS server port")]
        public int rpsServerPort { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText()
            {
                AddDashesToOption = true,
            };

            help.AddPreOptionsLine("Usage: Server mode: Cluster --server ");
            help.AddPreOptionsLine("       cluster ");
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
            return options;
        }
    }
}
