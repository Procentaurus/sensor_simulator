using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.KeyStore;
using Nethereum.Hex.HexConvertors;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.NonceServices;
using Nethereum.RPC.TransactionReceipts;
using System.Threading.Tasks;
using Nethereum.RPC.Eth.Transactions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;
using Nethereum.Util;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts;
using Nethereum.Contracts.Extensions;
using System.Numerics;

using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OnboardSystem.Services;



public class BlockchainService
{
    private readonly string blockchainEndpoint;
    private readonly string contractAddress;
    private readonly string mainWalletPrivateKey;
    private readonly int numberOfSensors;
    private readonly List<string> sensorAddresses= new List<string>();
    private readonly Account account;
    private readonly Web3 web3;
    private readonly Task _initializeTask;
    private BigInteger nonce;
    private BigInteger gas;
    private BigInteger gasPrice;
    private static readonly object lockObject = new object();

    [Function("transfer", "bool")]
    public class TransferFunction : FunctionMessage
    {
        [Parameter("address", "_to", 1)]
        public string To { get; set; }

        [Parameter("uint256", "_value", 2)]
        public BigInteger TokenAmount { get; set; }
    }

    public BlockchainService()
    {
        blockchainEndpoint = System.Environment.GetEnvironmentVariable("BLOCKCHAIN_ENDPOINT");
        contractAddress = System.Environment.GetEnvironmentVariable("CONTRACT_ADRESS");
        mainWalletPrivateKey = System.Environment.GetEnvironmentVariable("MAIN_WALLET_PRIVATE_KEY");
        account = new Account(mainWalletPrivateKey);
        web3 = new Web3(account, blockchainEndpoint);
        _initializeTask = InitializeAsync();
        if (int.TryParse(System.Environment.GetEnvironmentVariable("NUMBER_OF_SENSORS"), out int sensorCountResult))
        {
            numberOfSensors = sensorCountResult;
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

        if (int.TryParse(System.Environment.GetEnvironmentVariable("GAS_PRICE"), out int gasPriceResult))
        {
            gasPrice =  new BigInteger(gasPriceResult);

        }
        else
        {
            throw new System.Exception("Cannot process gasPrice env variable");
        }
    }

    private async Task InitializeAsync()
    {
        var  transactionCount = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(account.Address, BlockParameter.CreatePending());
        nonce = transactionCount.Value;
        var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
        var transfer = new TransferFunction()
        {
            To = sensorAddresses[0],
            TokenAmount = new BigInteger(1e18)
        };
        var estimate = await transferHandler.EstimateGasAsync(contractAddress, transfer);
        gas = estimate.Value;
    }

    public async Task RewardSensor(int id)
    {
        await _initializeTask;

        Random random = new Random();
        int milliseconds = random.Next(300);
        Task.Delay(milliseconds).Wait();
        var sensorAddress = sensorAddresses[id - 1];
        BigInteger currentNonce = 0;

        lock (lockObject)
        {
            currentNonce = nonce;
            nonce++;
        }

        try
        {
            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
            var transfer = new TransferFunction()
            {
                To = sensorAddress,
                TokenAmount = new BigInteger(1e18)
            };
            transfer.GasPrice = Nethereum.Web3.Web3.Convert.ToWei(gasPrice, UnitConversion.EthUnit.Gwei);
            transfer.Nonce = currentNonce;
            transfer.Gas = gas;
            var hash = await transferHandler.SendRequestAsync(contractAddress, transfer);
            Console.WriteLine($"MAIN->SENSOR_{id}: {hash}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

}
