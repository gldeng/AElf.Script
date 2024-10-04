using AElf.Client;
using AElf.Types;
using Google.Protobuf;

namespace AElf.Scripts;

public sealed class RefBlockInfo
{
    public RefBlockInfo(long height, ByteString prefix)
    {
        Height = height;
        Prefix = prefix;
    }

    public long Height { get; }
    public ByteString Prefix { get; }
}
//
// public interface IRefBlockInfoProvider
// {
//     Task<RefBlockInfo> GetRefBlockInfoAsync();
// }
//
// public class RefBlockInfoProvider : IRefBlockInfoProvider
// {
//     private readonly AElfClient _client;
//
//     public RefBlockInfoProvider(AElfClient client)
//     {
//         _client = client;
//     }
//
//     public async Task<RefBlockInfo> GetRefBlockInfoAsync()
//     {
//         var chain = await _client.GetChainStatusAsync();
//         var height = chain.LastIrreversibleBlockHeight;
//         var prefix = BlockHelper.GetRefBlockPrefix(Hash.LoadFromHex(chain.LastIrreversibleBlockHash));
//         return new RefBlockInfo(height, prefix);
//     }
// }