using AElf.Cryptography.ECDSA;
using AElf.Types;

namespace AElf.Scripts;

public static partial class Extension
{
    public static Address DeriveVirtualAddress(this Address contractAddress, Hash virtualAddress)
    {
        return Address.FromPublicKey(
            contractAddress.Value.Concat(
                virtualAddress.Value.ToByteArray().ComputeHash()
            ).ToArray()
        );
    }

    public static Address GetAddress(this ECKeyPair keyPair)
    {
        return Address.FromPublicKey(keyPair.PublicKey);
    }
}