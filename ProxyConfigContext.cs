using System.Text.Json.Serialization;
using System.Text.Json;

namespace FireflySR.Tool.Proxy;

#if NET8_0_OR_GREATER
[JsonSourceGenerationOptions(
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip
)]
[JsonSerializable(typeof(ProxyConfig))]
internal partial class ProxyConfigContext : JsonSerializerContext
{
}
#endif