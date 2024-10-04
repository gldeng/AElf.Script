using AElf.Scripts;
using AElf.Types;
using TestScript;

DeployMerkleTreeContractTask.ContractPathName =
    "/Users/steven/repo/TomorrowDAOProject/tomorrowDAO-contracts/contracts/merkletree/src/bin/Debug/net8.0/MerkleTreeWithHistory.dll.patched";
Environment.SetEnvironmentVariable(EnvVarNames.AELF_RPC_URL.ToString(), "http://34.136.178.106:8000");
Environment.SetEnvironmentVariable(EnvVarNames.DEPLOYER_PRIVATE_KEY.ToString(),
    "1111111111111111111111111111111111111111111111111111111111111111");


var deploy = new DeployMerkleTreeContractTask();
await deploy.RunAsync();