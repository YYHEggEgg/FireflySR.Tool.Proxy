using Microsoft.Win32;
using System.Diagnostics;

namespace FireflySR.Tool.Proxy.Guardian;

internal class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length != 1 || !int.TryParse(args[0], out var watchPid))
        {
            Console.WriteLine("Usage: FireflySR.Tool.Proxy.Guardian [watch-pid]");
            Environment.Exit(1);
            return;
        }

        Console.WriteLine("FireflySR.Tool.Proxy.Guardian start.");
        Process proc;
        try
        {
            proc = Process.GetProcessById(watchPid);
            Console.WriteLine($"Guardian find process {proc.ProcessName}:{watchPid}.");
        }
        catch
        {
            DisableSystemProxy();
            Environment.Exit(2);
            return;
        }

        while (!proc.HasExited)
        {
            await Task.Delay(1000);
        }
        DisableSystemProxy();
    }

    private static void DisableSystemProxy()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true);

            key?.SetValue("ProxyEnable", 0);
            Console.WriteLine($"Guardian successfully   disabled System Proxy.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine();
            Console.WriteLine($"Guardian failed to disable System Proxy.");
        }
    }
}
