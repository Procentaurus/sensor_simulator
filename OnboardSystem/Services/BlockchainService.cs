using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OnboardSystem.Services;

[Function("transfer", "bool")]
public class TransferFunction : FunctionMessage
{
    [Parameter("address", "_to", 1)]
    public string To { get; set; }

    [Parameter("uint256", "_value", 2)]
    public BigInteger TokenAmount { get; set; }
}

public class BlockchainService
{
    private readonly string blockchainEndpoint;
    private readonly string contractAddress;
    private readonly string mainWalletPrivateKey;
    private readonly int numberOfSensors;
    private readonly List<string> sensorAddresses= new List<string>();
    private readonly Account account;
    private readonly Web3 web3;

    public BlockchainService()
    {
        blockchainEndpoint = System.Environment.GetEnvironmentVariable("BLOCKCHAIN_ENDPOINT");
        contractAddress = System.Environment.GetEnvironmentVariable("CONTRACT_ADRESS");
        mainWalletPrivateKey = System.Environment.GetEnvironmentVariable("MAIN_WALLET_PRIVATE_KEY");
        account = new Account(mainWalletPrivateKey);
        web3 = new Web3(account, blockchainEndpoint);

        if (int.TryParse(System.Environment.GetEnvironmentVariable("NUMBER_OF_SENSORS"), out int result))
        {
            numberOfSensors = result;
            for (int i = 1; i <= numberOfSensors; i++)
            {
                string envVarName = $"SENSOR_{i}_PUBLIC_KEY";
                sensorAddresses.Add(System.Environment.GetEnvironmentVariable(envVarName));
            }
        }
        else
        {
            throw new System.Exception("Cannot process number of sensors env variable");
        }
    }

    public async Task RewardSensor(int id)
    {
        var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
        var sensorAddress = sensorAddresses[id - 1];
        var transfer = new TransferFunction()
        {
            To = sensorAddress,
            TokenAmount = (BigInteger)1e18
        };
        var hash = await transferHandler.SendRequestAsync(contractAddress, transfer);
        Console.WriteLine($"{hash}");
    }

}
