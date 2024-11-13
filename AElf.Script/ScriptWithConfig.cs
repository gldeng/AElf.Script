using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AElf.Script;

public abstract class ScriptWithConfig<TConfig> : Script
{
    public string Environment => System.Environment.GetEnvironmentVariable("AELFSCRIPT_ENVIRONMENT") ?? "Development";

    public string ConfigFile => $"config.{Environment}.yaml";

    private string _configContent = "";

    public string ConfigContent
    {
        get
        {
            if (string.IsNullOrEmpty(_configContent))
            {
                if (!File.Exists(ConfigFile))
                {
                    throw new FileNotFoundException($"Configuration file not found: {ConfigFile}");
                }

                _configContent = File.ReadAllText(ConfigFile);
            }

            return _configContent;
        }
        protected set => _configContent = value;
    }

    public T GetConfig<T>()
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new AddressConverter())
            .WithTypeConverter(new HashConverter())
            .IgnoreUnmatchedProperties()
            .Build();

        return deserializer.Deserialize<T>(ConfigContent);
    }

    private TConfig _config = default(TConfig);

    public TConfig Config
    {
        get
        {
            if (Equals(_config, default(TConfig)))
            {
                _config = GetConfig<TConfig>();
            }

            return _config;
        }
    }
}