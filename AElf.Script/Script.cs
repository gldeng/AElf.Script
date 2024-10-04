namespace AElf.Scripts;

public abstract class Script : ContextWithSystemContracts
{
    public abstract Task RunAsync();
}