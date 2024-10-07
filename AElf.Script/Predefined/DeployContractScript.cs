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
    }

    public string ContractPathName { get; } = "";
    public byte[]? Code { get; private set; }
    public Address? DeployedAddress { get; private set; }

    public override async Task RunAsync()
    {
        if (Code == null || Code.Length == 0)
        {
            if (File.Exists(ContractPathName))
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