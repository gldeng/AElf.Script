using AElf.Types;
using Microsoft.Extensions.Logging;

namespace AElf.Scripts.Predefined;

public class DeployContractScript : Script
{
    public bool SkipIfAlreadyDeployed { get; set; }

    public DeployContractScript(string contractPathName)
    {
        if (!File.Exists(contractPathName))
        {
            throw new Exception($"Not valid path: {contractPathName}.");
        }

        ContractPathName = contractPathName;
        Code = File.ReadAllBytes(ContractPathName);
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
        if (SkipIfAlreadyDeployed)
        {
            var (address, alreadyDeployed) = await CheckAlreadyDeployedAsync();
            if (alreadyDeployed)
            {
                DeployedAddress = address;
                Logger.LogInformation($"Skipping already deployed address {DeployedAddress}.");
                return;
            }
        }

        DeployedAddress = await this.DeployContractAsync(Code);
        var codeHash = HashHelper.ComputeFrom(Code);
        Logger.LogInformation($"{codeHash} deployed to {DeployedAddress}");
    }

    private async Task<(Address, bool)> CheckAlreadyDeployedAsync()
    {
        var address = this.GetAddressBySalt(NextSalt);
        var reg = await Genesis.GetSmartContractRegistrationByAddress.CallAsync(address);
        return (address, reg != null && !reg.Equals(new SmartContractRegistration()));
    }
}