using AElf.Client;
using AElf.Cryptography.ECDSA;
using AElf.CSharp.Core;
using AElf.Types;

namespace AElf.Scripts;

public static partial class Extension
{
    public static T GetInstance<T>(this AElfClient client, Address contractAddress, ECKeyPair senderKey)
        where T : ContractStubBase, new()
    {
        return new T
        {
            __factory = new MethodStubFactory(client, senderKey, contractAddress)
        };
    }

    public static T GetInstance<T>(this Context ctx, string contractAddress)
        where T : ContractStubBase, new()
    {
        return ctx.GetInstance<T>(Address.FromBase58(contractAddress));
    }

    public static T GetInstance<T>(this Context ctx, Address contractAddress)
        where T : ContractStubBase, new()
    {
        Assert(Context.Client != null, "Context is not initialized.");
        var senderKey = ctx.DeloyerKey ?? Context.GlobalSenderKey ?? Context.DefaultKeyPair;
        return new T
        {
            __factory = new MethodStubFactory(Context.Client!, senderKey, contractAddress)
        };
    }
}