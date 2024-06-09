# FireflySR.Tool.Proxy

A simple Proxy for playing Private Servers.

## Usage

Go to [releases page](https://git.xeondev.com/YYHEggEgg/FireflySR.Tool.Proxy/releases) for the latest pre-built binary. It don't require any .NET versions.

## Update

### v2.0.0

- Fixed the issue whereby the proxy has been closed by the user while the System Proxy Settings is still enabled, which may cause the user "can't access the network".  
  Notice you may still need to change the setting if you meet a PC crash (or just start the Proxy software again and close it).
- Updated the building .NET requirement to `8.0`; however, `PublishAot` is enabled, so the generated binaries won't require any .NET versions. You can generate AOT build by the following command:

  ```sh
  dotnet publish FireflySR.Tool.Proxy.csproj -r win-x64
  ```

  Notice a [Guardian](Guardian/Guardian.csproj) is introduced **only on Windows** and will be copied to the output directory only when the build is triggered on Windows.
- Updated the `BlockUrls` list in `config.tmpl.json`.

## Configuration

Simple: see [config.json](config.json). You can also see full options in [ProxyConfig.cs](ProxyConfig.cs).

## Credits

- [FreeSR](https://git.xeondev.com/Moux23333/FreeSR) & `FreeSR.Tool.Proxy`
- Rebooted `Titanium.Web.Proxy` [Unobtanium.Web.Proxy](https://github.com/svrooij/titanium-web-proxy.git)
