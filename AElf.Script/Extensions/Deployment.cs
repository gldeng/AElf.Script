using System.Collections;
using System.Numerics;
using AElf.Standards.ACS0;
using AElf.Types;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace AElf.Scripts;

public static partial class Extension
{
    public static async Task<Address?> DeployContractAsync(this ContextWithSystemContracts ctx, string filepath)
    {
        Assert(File.Exists(filepath), $"The file doesn't exist: {filepath}");
        var code = File.ReadAllBytes(filepath);
        await ctx.EnsureNextSaltIsNotTakenAsync();
        var tx = (await ctx.Genesis.DeployUserSmartContract.SendAsync(new UserContractDeploymentInput()
        {
            Category = 0,
            Code = ByteString.CopyFrom(code),
            Salt = Context.NextSalt,
            // TODO: running sequence of salt
        })).MustSucceed();
        // TODO: Is it possible to calculate codehash before transaction?
        Context.Logger.LogInformation($"Code hash is {tx.Output.CodeHash}");
        return await ctx.WaitUntilContractIsReleased(tx.Output.CodeHash);
    }

    public static async Task<Address> WaitUntilContractIsReleased(this ContextWithSystemContracts ctx, Hash codeHash)
    {
        // TODO: Will this fail
        var res = await ctx.Genesis.GetSmartContractRegistrationByCodeHash.CallAsync(codeHash);
        return res.ContractAddress;
    }

    public static Address GetAddressBySalt(this Context ctx, Hash salt, Address deployer = null)
    {
        if (deployer == null)
        {
            deployer = Address.FromPublicKey(ctx.DeployerKey!.PublicKey);
        }

        var hash = HashHelper.ConcatAndCompute(HashHelper.ComputeFrom(deployer), salt);
        return Address.FromBytes(hash.ToByteArray());
    }


    public static async Task EnsureNextSaltIsNotTakenAsync(this ContextWithSystemContracts ctx)
    {
        while (true)
        {
            var address = ctx.GetAddressBySalt(Context.NextSalt, Address.FromPublicKey(ctx.DeployerKey!.PublicKey));
            var reg = await ctx.Genesis.GetSmartContractRegistrationByAddress.CallAsync(address);
            if (reg == null || reg.Equals(new SmartContractRegistration()))
            {
                break;
            }

            Context.NextSalt = Context.NextSalt.Add(1);
        }
    }

    public static Hash Add(this Hash value, int addant)
    {
        var bigInt = new BigInteger(value.Value.ToByteArray(), true, true);
        bigInt += addant;

        // Convert bigInt to byte array
        var bytes = bigInt.ToByteArray(true, true);

        if (bytes.Length > 32)
        {
            throw new Exception("Invalid hash value to increase.");
        }
        else if (bytes.Length < 32)
        {
            // Left-pad the byte array to 32 bytes if it's too short
            var paddedBytes = new byte[32];
            Array.Copy(bytes, 0, paddedBytes, 32 - bytes.Length, bytes.Length);
            bytes = paddedBytes;
        }

        return Hash.LoadFromByteArray(bytes);
    }
}