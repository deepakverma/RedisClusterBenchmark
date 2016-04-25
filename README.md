<pre>
  cluster -h *.redis.cache.windows.net  -a password -s 127.0.0.1

  -m, --cbservermode     Run cluster-benchmark in server mode
  -h, --redishostname    Server Hostname
  -a, --redispassword    Redis Server password
  -p, --redisport        Redis Server port. Default 6379
  --nonssl               (Default: False) Use SSL connection
  -c, --clients          (Default: 1) Number of client connection to create
  -d, --datasize         (Default: 1024) Value size in bytes
  -w, --warmup           (Default: 5000) Warm up period before running the
                         tests
  --cbserver             cluster-benchmark server endpoint
  --cbport               (Default: 6400) cluster-benchmark server port
  -k, --keyprefix        (Default: __clusterbenchmark_test) Key prefix of the
                         keys this tool will be creating to perform operations
  -v, --verbose
  --help                 Display this help screen.
</pre>
  <p>
    <strong>Measure Cluster Throughput from a single VM</strong></p>
<p>
   usage: cluster-benchmark.exe -h&nbsp; *.redis.cache.windows.net -a &lt;redis-password&gt;
</p>
<p>
    The tool would automatically detect if it&#39;s a cluster or a non-cluster cache.
    It would evenly distribute keys across shard and measure the cluster throughput </p>
<p>
    <strong>Steps to measure Cluster Throughput by placing load from multiple VMs</strong></p>
<p>
    1. Create a Redis Cache with n shards</p>
<p>
    2. Create multiple IAAS VM&#39;s
    <br />
&nbsp;&nbsp;&nbsp;&nbsp; (Make sure IAAS VM size matches or is higher performant that Redis cache size, for example for a P4 Azure Redis Premium cache create a D4 IAAS VM)</p>
<p>
    3. Enable port 6400 on the IAAS Vm&#39;s <br />
&nbsp;&nbsp;&nbsp; (this is the default port that would be used by the tool to aggregate RPS data, you can use any other port as well)</p>
<p>
    4. On one of the IAAS Vm&#39;s run the tool in server mode<br />
&nbsp;&nbsp;&nbsp; for example on clustlerload.cloudapp.net VM you run<br />
&nbsp;&nbsp; <strong>&nbsp;cluster-benchmark.exe --cbservermode&nbsp;
    <br />
    </strong>
</p>
<p>
    5. One each of the client VM&#39;s run the following command<br />
    <strong>&nbsp;&nbsp; cluster-benchmark.exe -h&nbsp; *.redis.cache.windows.net -a &lt;redis-password&gt; --cbserver clusterload.cloudapp.net<br />
    </strong>&nbsp;&nbsp;&nbsp; --server is set to the IAAs vm name where you are running the tool in the server mode.</p>
<p>
    6. On the node where you were running the tool in server mode, it would output the aggregated RPS</p>
<p>
    &nbsp;</p>
<p>
    
