namespace AElf.Script;

public abstract class Script : ContextWithSystemContracts
{
    public abstract Task RunAsync();
}