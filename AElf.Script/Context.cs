using AElf.Client;
using AElf.Cryptography;
using AElf.Cryptography.ECDSA;
using AElf.Types;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

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


    private static string? _chainId;

    public string ChainId
    {
        get
        {
            if (!string.IsNullOrEmpty(_chainId))
            {
                return _chainId;
            }

            AsyncContext.Run(async () =>
            {
                var chainInfo = await Client!.GetChainStatusAsync();
                _chainId = chainInfo.ChainId;
            });
            return _chainId!;
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

    private static Hash _nextSalt = Hash.Empty;

    public static Hash NextSalt
    {
        get
        {
            if (_nextSalt == Hash.Empty)
            {
                var value = Environment.GetEnvironmentVariable(EnvVarNames.DEPLOYE_STARTING_SALT.ToString());
                if (value != null)
                {
                    _nextSalt = Hash.LoadFromHex(value);
                }
                else
                {
                    _nextSalt = Hash.LoadFromHex("0000000000000000000000000000000000000000000000000000000000000001");
                }
            }

            return _nextSalt;
        }
        set { _nextSalt = value; }
    }

    private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Trace);
    });

    public static ILogger<Context> Logger { get; private set; } = _loggerFactory.CreateLogger<Context>();
    public ECKeyPair? DeployerKey { get; set; }

    public static ECKeyPair DefaultKeyPair =>
        GetAccountKeyPair("1111111111111111111111111111111111111111111111111111111111111111");

    private static ECKeyPair GetAccountKeyPair(string privateKey)
    {
        return CryptoHelper.FromPrivateKey(ByteArrayHelper.HexStringToByteArray(privateKey));
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