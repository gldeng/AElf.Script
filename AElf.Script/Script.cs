using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AElf.Scripts;

public abstract class Script : ContextWithSystemContracts
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
    }

    private T GetConfig<T>()
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        return deserializer.Deserialize<T>(ConfigContent);
    }

    public abstract Task RunAsync();
}