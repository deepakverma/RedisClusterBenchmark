<p>
    <strong>Measure Cluster Throughput from a single VM</strong></p>
<p>
    cluster-benchmark.exe -h&nbsp; *.redis.cache.windows.net -a &lt;redis-password&gt;
</p>
<p>
    tool would detect if it&#39;s a cluster or a non-cluster cache, helping to measure RPS using the same commandline against a non-cluster cache.</p>
<p>
    <strong>Steps to measure Cluster Throughput by placing load from multiple VMs</strong></p>
<p>
    1. Create a Redis Cache with n shards</p>
<p>
    2. Create multiple IAAS VM&#39;s
    <br />
&nbsp;&nbsp;&nbsp;&nbsp; (Make sure IAAS VM size matches or is higher performant that Redis cache size, for example for a P4 Azure Redis Premium cache create a D4 IAAS VM)</p>
<p>
    3. Enable port 6400 on the IAAS Vm&#39;s (screenshot required)<br />
&nbsp;&nbsp;&nbsp; (this is the default port that would be used by the tool to aggregate RPS data, you can use any other port as well)</p>
<p>
    4. One one of the IAAS Vm&#39;s run the tool in server mode<br />
&nbsp;&nbsp;&nbsp; for example on clustlerload.cloudapp.net VM you run<br />
&nbsp;&nbsp; <strong>&nbsp;cluster-benchmark.exe --servermode&nbsp;
    <br />
    </strong>
</p>
<p>
    5. One each of the VM&#39;s including the one running the server run the following command<br />
    <strong>&nbsp;&nbsp; cluster-benchmark.exe -h&nbsp; *.redis.cache.windows.net -a &lt;redis-password&gt; --server clusterload.cloudapp.net<br />
    </strong>&nbsp;&nbsp;&nbsp; --server is set to the IAAs vm name where you are running the tool in the server mode.</p>
<p>
    6. On the node where you were running the tool in server mode, it would output the aggregated RPS</p>
<p>
    &nbsp;</p>
<p>
    <strong>TO DO: my setup and test results</strong></p>
