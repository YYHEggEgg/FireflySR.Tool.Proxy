namespace FireflySR.Tool.Proxy
{
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Threading.Tasks;
    using Titanium.Web.Proxy;
    using Titanium.Web.Proxy.EventArguments;
    using Titanium.Web.Proxy.Models;

    internal class ProxyService
    {
        private readonly ProxyConfig _conf;
        private readonly ProxyServer _webProxyServer;
        private readonly string _targetRedirectHost;
        private readonly int _targetRedirectPort;

        public ProxyService(string targetRedirectHost, int targetRedirectPort, ProxyConfig conf)
        {
            _conf = conf;
            _webProxyServer = new ProxyServer();
            _webProxyServer.CertificateManager.EnsureRootCertificate();

            _webProxyServer.BeforeRequest += BeforeRequest;
            _webProxyServer.ServerCertificateValidationCallback += OnCertValidation;

            _targetRedirectHost = targetRedirectHost;
            _targetRedirectPort = targetRedirectPort;

            int port = conf.ProxyBindPort == 0 ? Random.Shared.Next(10000, 60000) : conf.ProxyBindPort;
            SetEndPoint(new ExplicitProxyEndPoint(IPAddress.Any, port, true));
        }

        private void SetEndPoint(ExplicitProxyEndPoint explicitEP)
        {
            explicitEP.BeforeTunnelConnectRequest += BeforeTunnelConnectRequest;

            _webProxyServer.AddEndPoint(explicitEP);
            _webProxyServer.Start();

            _webProxyServer.SetAsSystemHttpProxy(explicitEP);
            _webProxyServer.SetAsSystemHttpsProxy(explicitEP);
        }

        public void Shutdown()
        {
            _webProxyServer.Stop();
            _webProxyServer.Dispose();
        }

        private Task BeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs args)
        {
            string hostname = args.HttpClient.Request.RequestUri.Host;
            Console.WriteLine(hostname);
            args.DecryptSsl = ShouldRedirect(hostname);

            return Task.CompletedTask;
        }

        private Task OnCertValidation(object sender, CertificateValidationEventArgs args)
        {
            if (args.SslPolicyErrors == SslPolicyErrors.None)
                args.IsValid = true;

            return Task.CompletedTask;
        }
        
        private bool ShouldForceRedirect(string path)
        {
            foreach (var keyword in _conf.ForceRedirectOnUrlContains)
            {
                if (path.Contains(keyword)) return true;
            }
            return false;
        }

        private Task BeforeRequest(object sender, SessionEventArgs args)
        {
            string hostname = args.HttpClient.Request.RequestUri.Host;
            if (ShouldRedirect(hostname) || ShouldForceRedirect(args.HttpClient.Request.RequestUri.AbsolutePath))
            {
                string requestUrl = args.HttpClient.Request.Url;
                Uri local = new Uri($"http://{_targetRedirectHost}:{_targetRedirectPort}/");

                string replacedUrl = new UriBuilder(requestUrl)
                {
                    Scheme = local.Scheme,
                    Host = local.Host,
                    Port = local.Port
                }.Uri.ToString();

                Console.WriteLine("Redirecting: " + replacedUrl);
                args.HttpClient.Request.Url = replacedUrl;
            }

            return Task.CompletedTask;
        }

        private bool ShouldRedirect(string hostname)
        {
            if (hostname.Contains(':'))
                hostname = hostname[0..hostname.IndexOf(':')];
            foreach (string domain in _conf.AlwaysIgnoreDomains)
            {
                if (hostname.EndsWith(domain))
                {
                    return false;
                }
            }
            foreach (string domain in _conf.RedirectDomains)
            {
                if (hostname.EndsWith(domain))
                    return true;
            }

            return false;
        }
    }
}
