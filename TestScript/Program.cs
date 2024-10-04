using AElf.Scripts;
using AElf.Scripts.Predefined;
using TestScript;

Environment.SetEnvironmentVariable(EnvVarNames.AELF_RPC_URL.ToString(), "http://34.136.178.106:8000");
Environment.SetEnvironmentVariable(EnvVarNames.DEPLOYER_PRIVATE_KEY.ToString(),
    "1111111111111111111111111111111111111111111111111111111111111111");

// await new InitScript().RunAsync();
//
// var deploy =
//     new DeployContractTask(
//         "/Users/steven/repo/TomorrowDAOProject/tomorrowDAO-contracts/contracts/merkletree/src/bin/Debug/net8.0/MerkleTreeWithHistory.dll.patched");
// await deploy.RunAsync();

