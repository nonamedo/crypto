using System;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nonamedo.Crypto.Service.interfaces;
using Nethereum.Web3.Accounts;
using Nethereum.Web3;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;

namespace Nonamedo.Crypto.Service.Ethereum.Services
{
    public class EthereumService : ICryptoService
    {
        private readonly string _node;

        public EthereumService(string node)
        {
            _node = node;
        }

        public async Task<string> WithdrawGasAsync(CryptoAccount from, string toAddress)
        {
            var account = new Account(from.PrivateKey);
            var web3 = new Web3(account, _node);

            var originalGasPrice = await web3.Eth.GasPrice.SendRequestAsync();
            BigInteger gasPrice = new BigInteger(UnitConversion.Convert.FromWei(originalGasPrice, UnitConversion.EthUnit.Wei) * 1.20m);
            var gasPriceInGwei = UnitConversion.Convert.FromWei(gasPrice, UnitConversion.EthUnit.Gwei); // 1 gwei = 1,000,000,000 wei

            var svc = web3.Eth.GetEtherTransferService();
            var amount = await svc.CalculateTotalAmountToTransferWholeBalanceInEtherAsync(account.Address, gasPrice);
            var gas = await svc.EstimateGasAsync(toAddress, amount);

            var transaction = await svc.TransferEtherAndWaitForReceiptAsync(
               toAddress: toAddress,
               etherAmount: amount,
               gasPriceGwei: gasPriceInGwei,
               gas: gas);


            return transaction.TransactionHash;
        }

        public async Task<decimal> CalcGasToWithdrawTokenAsync(CryptoAccount from, string toAddress, string contractAddress, decimal tokenAmount)
        {
            /*var account = new Account(from.PrivateKey);
            var web3 = new Web3(account);

            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
            var transfer = new TransferFunction()
            {
                To = toAddress,
                TokenAmount = new BigInteger(tokenAmount)
            };
            var estimate = await transferHandler.EstimateGasAsync(contractAddress, transfer);*/

            var web3 = new Web3(_node);
            HexBigInteger gasPrice = await web3.Eth.GasPrice.SendRequestAsync();

             BigInteger maxGas = 200_000; // 65_000 for usdt
             decimal gas =  Web3.Convert.FromWei(maxGas * gasPrice.Value);
             return gas;


            /*
            Get Limits for erc20 tokens

            it really depends on the token, and typically varies from 25'000 to 150'000, with very rare cases going over 500'000 gas.
            Recommendation: set it 200'000 if you don't have access to an estimate, it will work 99% of the time. It is a "limit" anyway, you will get back any unspent gas immediately.
            If your wallet proposes an estimate, add roughly 10-20% to it.

            the source: https://ethereum.stackexchange.com/questions/71235/gas-limit-for-erc-20-tokens
            */
            
            // 21000 gas is used to transfer ether
        }

        public async Task<CryptoAccount> GenerateAccountAsync()
        {
            await Task.FromResult(0);
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            var account = new Account(privateKey);

            return new CryptoAccount(
                address: account.Address,
                publicKey: account.PublicKey,
                privateKey: account.PrivateKey);
        }

        public async Task<decimal> GetGasBalanceAsync(CryptoAccount account)
        {
            var web3 = new Web3(_node);
            var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address);

            var etherAmount = Web3.Convert.FromWei(balance.Value);
            return etherAmount;
        }

        public async Task<decimal> GetTokenBalanceAsync(CryptoAccount account, string contractAddress)
        {
            var web3 = new Web3(_node);
            var balanceOfFunctionMessage = new BalanceOfFunction()
            {
                Owner = account.Address,
            };

            var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
            var balance = await balanceHandler.QueryAsync<BigInteger>(contractAddress, balanceOfFunctionMessage);

            var decimalsHandler = web3.Eth.GetContractQueryHandler<DecimalsFunction>();
            var decimals = await decimalsHandler.QueryAsync<int>(contractAddress);
            decimal divider = Convert.ToDecimal(Math.Pow(10, decimals));

            return (decimal)balance / divider;
        }

        public string GetUsdtContract()
        {
            return "0xdac17f958d2ee523a2206206994597c13d831ec7";
        }

        public async Task<bool> IsTrnsactionConfirmedAsync(string txid)
        {
            /*

            When sending the transaction, if the transaction structure is correct and there are no problems with gas fees and the amount of Ethereum present, 
            you will get a response that bears the hash of the transaction, but it is not complete yet

            If you do not get the Transaction Hash, the transaction has not been registered and is considered a failure

            If you get the transaction Hash, you must do the following:

            A - Get Transaction Receipt :
            await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(TransactionHash.Trim());
            var Status =TransactionRecipint.Status.Value
            Status It can be 3 numbers Biginteger [1 = Success| 2 = pending | 3 = fail ]

            B - When you get a successful transaction status you can Get Transaction
            var SelectedTransction = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(TransactionHash.Trim());

            Note that: When sending a transaction that contains parameters and these parameters need to be verified within the smart contract, the Gas Fess value 
            will be deducted even if the transaction is not completed
            This means that you need to verify as much as possible the expected variables and values that you send on the client side first as much as possible


            the sourse: https://github.com/Nethereum/Nethereum/issues/787
            */
            
            var web3 = new Web3(_node);
            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txid); // get block number
            var status = receipt.Status;

            if (status.Value.Equals(1)) // success
            {
                // get current block height
                HexBigInteger latestBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                BigInteger confirmations = latestBlockNumber.Value - receipt.BlockNumber.Value;
                if (confirmations>=7)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<string> SendTransactionAsync(CryptoAccount from, string toAddress, decimal amount)
        {
            var account = new Account(from.PrivateKey);
            var web3 = new Web3(account, _node);

            var originalGasPrice = await web3.Eth.GasPrice.SendRequestAsync();
            BigInteger gasPrice = new BigInteger(UnitConversion.Convert.FromWei(originalGasPrice, UnitConversion.EthUnit.Wei) * 1.20m);
            var gasPriceInGwei = UnitConversion.Convert.FromWei(gasPrice, UnitConversion.EthUnit.Gwei); // 1 gwei = 1,000,000,000 wei
            var transaction = await web3.Eth.GetEtherTransferService()
                .TransferEtherAndWaitForReceiptAsync(toAddress, amount, gasPriceInGwei);

            return transaction.TransactionHash;
        }

        public async Task<string> WithdrawTokenAsync(CryptoAccount from, string toAddress, string contractAddress, decimal tokenAmount)
        {
            var account = new Account(from.PrivateKey);
            var web3 = new Web3(account, _node);

            var decimalsHandler = web3.Eth.GetContractQueryHandler<DecimalsFunction>();
            var decimals = await decimalsHandler.QueryAsync<int>(contractAddress);
            decimal multiplier = Convert.ToDecimal(Math.Pow(10, decimals));

            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
            var transfer = new TransferFunction()
            {
                To = toAddress,
                TokenAmount = new BigInteger(tokenAmount * multiplier)
            };

            var originalGasPrice = await web3.Eth.GasPrice.SendRequestAsync();
            BigInteger gasPrice = new BigInteger(UnitConversion.Convert.FromWei(originalGasPrice, UnitConversion.EthUnit.Wei) * 1.20m);
            transfer.GasPrice = gasPrice;


            var transactionReceipt = await transferHandler.SendRequestAndWaitForReceiptAsync(contractAddress, transfer);

            return transactionReceipt.TransactionHash;
        }
    }
}
