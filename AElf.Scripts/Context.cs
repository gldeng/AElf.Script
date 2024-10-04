using AElf.Client;
using AElf.Cryptography;
using AElf.Cryptography.ECDSA;
using Microsoft.Extensions.Logging;

namespace AElf.Scripts;

public class Context
{
    private static string? _baseUrl;

    public static string? BaseUrl
    {
        get
        {
            if (_baseUrl != null)
            {
                return _baseUrl;
            }

            var urlFromEnvVar = Environment.GetEnvironmentVariable(EnvVarNames.AELF_RPC_URL.ToString());
            if (urlFromEnvVar != null)
            {
                _baseUrl = FormatServiceUrl(urlFromEnvVar);
            }

            return _baseUrl;
        }
    }

    private static AElfClient? _client;

    public static AElfClient? Client
    {
        get
        {
            if (_client != null)
            {
                return _client;
            }

            _client = MaybeCreateClient();

            return _client;
        }
    }

    private static ECKeyPair? _globalSenderKey;

    public static ECKeyPair? GlobalSenderKey
    {
        get
        {
            if (_globalSenderKey != null)
            {
                return _globalSenderKey;
            }

            var value = Environment.GetEnvironmentVariable(EnvVarNames.DEPLOYER_PRIVATE_KEY.ToString());
            if (value != null)
            {
                _globalSenderKey = GetAccountKeyPair(value);
            }

            return _globalSenderKey;
        }
    }

    private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Trace);
    });

    public static ILogger<Context> Logger { get; private set; } = _loggerFactory.CreateLogger<Context>();
    public ECKeyPair? SenderKey { get; set; }

    public static ECKeyPair DefaultKeyPair =>
        GetAccountKeyPair("1111111111111111111111111111111111111111111111111111111111111111");

    private static ECKeyPair GetAccountKeyPair(string priKey)
    {
        return CryptoHelper.FromPrivateKey(ByteArrayHelper.HexStringToByteArray(priKey));
    }

    private static AElfClient? MaybeCreateClient()
    {
        if (BaseUrl == null)
        {
            return null;
        }

        var endpoint = FormatServiceUrl(BaseUrl);
        const int timeout = 120;
        return new AElfClient(endpoint, timeout, "", "");
    }

    private static string FormatServiceUrl(string baseUrl)
    {
        if (baseUrl.Contains("http://") || baseUrl.Contains("https://"))
            return baseUrl;

        return $"http://{baseUrl}";
    }
}