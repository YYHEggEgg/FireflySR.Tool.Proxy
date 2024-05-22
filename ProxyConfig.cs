namespace FireflySR.Tool.Proxy;

public class ProxyConfig
{
    public List<string> RedirectDomains { get; set; } = [];
    public List<string> AlwaysIgnoreDomains { get; set; } = [];
    public List<string> ForceRedirectOnUrlContains { get; set; } = [];
    public HashSet<string> BlockUrls { get; set; } = [];
    public required string DestinationHost { get; set; }
    public required int DestinationPort { get; set; }
    public int ProxyBindPort { get; set; }
}
