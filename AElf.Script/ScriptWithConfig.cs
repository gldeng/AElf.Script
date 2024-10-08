namespace AElf.Scripts;

public abstract class ScriptWithConfig<TConfig> : Script
{
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