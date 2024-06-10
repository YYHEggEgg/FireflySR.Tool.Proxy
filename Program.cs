using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace FireflySR.Tool.Proxy
{
    internal static class Program
    {
        private const string Title = "FreeSR Proxy (Alter)";
        private const string ConfigPath = "config.json";
        private const string ConfigTemplatePath = "config.tmpl.json";
        private const string GuardianPath = "tool/FireflySR.Tool.Proxy.Guardian.exe";

        private static ProxyService s_proxyService = null!;
        private static bool s_clearupd = false;
        
        private static void Main(string[] args)
        {
            Console.Title = Title;
            Console.WriteLine($"Firefly.Tool.Proxy - Credits for original FreeSR Proxy");
            _ = Task.Run(WatchGuardianAsync);
            CheckProxy();
            InitConfig();

            var conf = JsonSerializer.Deserialize(File.ReadAllText(ConfigPath), ProxyConfigContext.Default.ProxyConfig) ?? throw new FileLoadException("Please correctly configure config.json.");
            s_proxyService = new ProxyService(conf.DestinationHost, conf.DestinationPort, conf);
            Console.WriteLine($"Proxy now running");
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            Console.CancelKeyPress += OnProcessExit;

            Thread.Sleep(-1);
        }

        private static async Task WatchGuardianAsync()
        {
            var proc = StartGuardian();
            if (proc == null)
            {
                Console.WriteLine("Guardian start failed. Your proxy settings may not be able to recover after closing.");
                return;
            }

            // Notice that on some PTY, closing it may lead
            // to Guardian be killed, not the Proxy itself.
            // Therefore, Proxy should also watch Guardian
            // and exit when Guardian dies.
            while (!proc.HasExited)
            {
                await Task.Delay(1000);
            }
            Console.WriteLine("! Guardian exit");
            OnProcessExit(null, null);
            Environment.Exit(0);
        }

        private static Process? StartGuardian()
        {
            if (!OperatingSystem.IsWindows()) return null;

            try
            {
                return Process.Start(new ProcessStartInfo(GuardianPath, $"{Environment.ProcessId}")
                {
                    UseShellExecute = false,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine();
                return null;
            }
        }

        private static void InitConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                File.Copy(ConfigTemplatePath, ConfigPath);
            }
        }

        private static void OnProcessExit(object? sender, EventArgs? args)
        {
            if (s_clearupd) return;
            s_proxyService?.Shutdown();
            s_clearupd = true;
        }

        public static void CheckProxy()
        {
            try
            {
                string? ProxyInfo = GetProxyInfo();
                if (ProxyInfo != null)
                {
                    Console.WriteLine("well... It seems you are using other proxy software(such as Clash,V2RayN,Fiddler,etc)");
                    Console.WriteLine($"You system proxy: {ProxyInfo}");
                    Console.WriteLine("You have to close all other proxy software to make sure FireflySR.Tool.Proxy can work well.");
                    Console.WriteLine("Press any key to continue if you closed other proxy software, or you think you are not using other proxy.");
                    Console.ReadKey();
                }
            }
            catch (NullReferenceException)
            {

            }
        }

        public static string? GetProxyInfo()
        {
            try
            {
                IWebProxy proxy = WebRequest.GetSystemWebProxy();
                Uri? proxyUri = proxy.GetProxy(new Uri("https://www.example.com"));
                if (proxyUri == null) return null;

                string proxyIP = proxyUri.Host;
                int proxyPort = proxyUri.Port;
                string info = proxyIP + ":" + proxyPort;
                return info;
            }
            catch
            {
                return null;
            }
        }
    }
}
