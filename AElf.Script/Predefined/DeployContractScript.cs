using System.Net;
using AElf.Types;
using Microsoft.Extensions.Logging;

namespace AElf.Scripts.Predefined;

public class DeployContractScript : Script
{
    public DeployContractScript(string contractPathName)
    {
        ContractPathName = contractPathName;
    }

    public DeployContractScript(byte[] code)
    {
        ContractPathName = "default";
        Code = code;
    }

    public string ContractPathName { get; } = "";
    public byte[]? Code { get; protected set; }
    public Address? DeployedAddress { get; protected set; }

    public override async Task RunAsync()
    {
        if (Code == null || Code.Length == 0)
        {
            if (!File.Exists(ContractPathName))
            {
                throw new Exception("Cannot find code or file.");
            }

            Code = File.ReadAllBytes(ContractPathName);
        }

        DeployedAddress = await this.DeployContractAsync(Code);
        var codeHash = HashHelper.ComputeFrom(Code);
        Logger.LogInformation($"{codeHash} deployed to {DeployedAddress}");
    }
}