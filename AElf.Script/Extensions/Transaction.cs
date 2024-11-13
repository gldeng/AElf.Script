using AElf.Client;
using AElf.Script;
using AElf.Types;

namespace AElf.Script;

public static partial class Extension
{
    public static async Task<RefBlockInfo> GetRefBlockInfoAsync(this AElfClient client)
    {
        var chain = await client.GetChainStatusAsync();
        var height = chain.LastIrreversibleBlockHeight;
        var prefix = BlockHelper.GetRefBlockPrefix(Hash.LoadFromHex(chain.LastIrreversibleBlockHash));
        return new RefBlockInfo(height, prefix);
    }
}