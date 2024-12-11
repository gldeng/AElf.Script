using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Script;

public static partial class Extension
{
    public static async Task<T> GetConfigurationAsync<T>(this ContextWithSystemContracts ctx, string key)
        where T : IMessage<T>, new()
    {
        var value = await ctx.ConfigurationContractStub.GetConfiguration.CallAsync(new StringValue()
        {
            Value = key
        });
        var output = new T();
        output.MergeFrom(value.Value);
        return output;
    }
}