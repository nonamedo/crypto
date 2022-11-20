using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nonamedo.Crypto.Service.interfaces;

namespace Nonamedo.Crypto.Service.Tron
{
    public class TronService: ICryptoService
    {
        private readonly string _fullNode;
        private readonly string _solidityNode;
        private readonly HttpClient _client;

        public TronService(string fullNode, string solidityNode, HttpClient client)
        {
            this._fullNode = fullNode.TrimEnd('/');
            this._solidityNode = solidityNode.TrimEnd('/');
            this._client = client;
        }

        public async Task<string> SendTransactionAsync(CryptoAccount from, string toAddress, decimal amount)
        {
            var rawTransaction = await CreateTransactionAsync(
                toAddress: toAddress,
                ownerAddress: from.Address,
                amount: Convert.ToInt64(amount * 1_000_000L));

            if (rawTransaction == null)
                return null;

            if (rawTransaction.TxID == null)
                return null;

            TronTransaction signedTransaction = await AddTransactionSignAsync(rawTransaction, from.PrivateKey);
            BroadcastTransactionResult result = await BroadcastTransactionAsync(signedTransaction);

            if (result.Result)
            {
                return result.Txid;
            }
            
            return null;
        }

        public async Task<bool> IsTrnsactionConfirmedAsync(string txid)
        {
            var body = new GettTansactionByIdRequest{
                Value = txid
            };

            TronTransaction transaction = await PostAsync<TronTransaction>(
                baseUrl: this._solidityNode, 
                resourse: "walletsolidity/gettransactionbyid", 
                body:body);

             
            if (transaction.Ret!=null && transaction.Ret.Length > 0)
            {
                var contractRet = transaction.Ret[0].ContractRet;
                if (contractRet == "SUCCESS")
                {
                    return true;
                }
                else
                {
                     throw new Exception($"Transaction status is '{contractRet}'");
                }
            }
            else
            {
                return false;
            }
        }

        

        async Task<TronTransaction> CreateTransactionAsync(string toAddress, string ownerAddress, long amount)
        {
            var body = new CreateTransactionRequest{
                OwnerAddress = Helper.ToHex(ownerAddress),
                ToAddress = Helper.ToHex(toAddress),
                Amount = amount
            };
             
            var result = await PostAsync<TronTransaction>(baseUrl: this._fullNode, resourse: "wallet/createtransaction", body: body);
            return result;
        }
        
        public async Task<decimal> CalcGasAmountToSendAsync(string contractAddress)
        {
            var fee_limit = await GetFeeLimitAsync(contractAddress);
            return fee_limit / 1_000_000L;
        }

        public async Task <decimal> CalcGasAmountToWithdrawAsync(decimal balance)
        {
            await Task.FromResult(0);
            return balance;
        }

        public async Task<string> TransferTokenAsync(CryptoAccount from, string contractAddress, string toAddress, decimal amount)
        {
            /*
            1 TRX = 1,000,000 SUN.
            1 Energy = 280 SUN

            fee_limit is the total SUN value that contact call can consume. 
            For example, 
            if a contract consumes 10 energy, then you need to set feeLimit to at least 2,800 SUN because the price of 1 enerege is 280 SUN
            
            setting fee_limit Prevents the contract call transaction from consuming too much energy.
            */


            long feeLimit = await GetFeeLimitAsync(contractAddress);
            long amountInSun = Convert.ToInt64(amount * 1_000_000m);
            string amountInHex = amountInSun.ToString("x");

            var str = new StringBuilder();
            str.Append(Helper.ToParameter(Helper.ToHex(toAddress)));
            str.Append(Helper.ToParameter(amountInHex));
            var parameter = str.ToString();

            var contractTransaction =  await TriggerSmartContractAsync(
                contractAddress: Helper.ToHex(contractAddress),
                ownerAddress: Helper.ToHex(from.Address),
                parameter: parameter,
                functionSelector: "transfer(address,uint256)",
                feeLimit: feeLimit);

            if (!contractTransaction.Result.Result)
                return null;

            TronTransaction signedTransaction = await AddTransactionSignAsync(contractTransaction.Transaction, from.PrivateKey);
            BroadcastTransactionResult result = await BroadcastTransactionAsync(signedTransaction);

            if (!result.Result)
                return null;

            return result.Txid;
        }

        string GetTxid(string rawDataHex)
        {
            var bytes = rawDataHex.HexToByteArray();
            using (SHA256 mySHA256 = SHA256.Create())
            {
                var hash = mySHA256.ComputeHash(bytes);   
                return hash.ToHex();
            }
        }

        string GenerateSignature(string txid, string privateKey)
        {
            var ecKey = new ECKey(privateKey.HexToByteArray(), true);
            var bytes = txid.HexToByteArray();
            var sign = ecKey.Sign(bytes).ToByteArray();  
            var signature = sign.ToHexCompact();
            return signature;
        }


        async Task<TronTransaction> AddTransactionSignAsync(TronTransaction rawTransaction, string privateKey)
        {
            await Task.FromResult(0);

            var ecKey = new ECKey(privateKey.HexToByteArray(), true);

            var json = JsonConvert.SerializeObject(rawTransaction);
            var transactionSigned = JsonConvert.DeserializeObject<TronTransaction>(json);
            
            var signature = GenerateSignature(transactionSigned.TxID, privateKey);
            
            transactionSigned.Signature = new string[] { signature };
            return transactionSigned;
        }

        async Task<BroadcastTransactionResult> BroadcastTransactionAsync(TronTransaction signedTransaction)
        {
            var result = await PostAsync<BroadcastTransactionResult>(baseUrl: this._fullNode, resourse: "wallet/broadcasttransaction", body: signedTransaction);
            return result;
        }
        
        public string GetUsdtContract()
        {
            return "TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t";
        }

        public async Task<CryptoAccount> GenerateAccountAsync()
        {
            await Task.FromResult(0);

            var keyPair = TronECKey.GenerateKey(TronNetwork.MainNet);
            string publicKey = Helper.ByteArrayToString(keyPair.GetPubKey());
            
            return new CryptoAccount(
                address: keyPair.GetPublicAddress(),
                publicKey: publicKey,
                privateKey: keyPair.GetPrivateKey());
        }

        public async Task<decimal> GetTokenBalanceAsync(CryptoAccount account, string contractAddress)
        {
            var contractTransaction =  await TriggerSmartContractAsync(
                contractAddress: Helper.ToHex(contractAddress),
                ownerAddress: Helper.ToHex(account.Address),
                parameter: Helper.ToParameter(Helper.ToHex(account.Address)),
                functionSelector: "balanceOf(address)",
                feeLimit: 0);


            if (contractTransaction.ConstantResult!=null && contractTransaction.ConstantResult.Length >0)
            {
                var first= contractTransaction.ConstantResult[0];

                string hex = first.TrimStart('0');

                if (!string.IsNullOrWhiteSpace(hex))
                {
                    var bytes = hex.HexToByteArray();
                    decimal total = Convert.ToInt64(hex,16);
                    //var number= BitConverter.ToInt64(bytes);
                    return total / 1_000_000m;
                }
            }

            return 0;
        }

        public async Task<decimal> GetGasBalanceAsync(CryptoAccount account)
        {
            var hex = Helper.ToHex(account.Address);
            var body = new GetAccountRequest{
                Address = hex
            };

            var result = await PostAsync<TronAccount>(baseUrl: this._fullNode, resourse: "wallet/getaccount", body: body);
            var balance = result.Balance / 1_000_000m;
            return balance;
        }


     

        async Task<T> PostAsync<T>(string baseUrl, string resourse, object body)
        {
            Uri uri = new Uri($"{baseUrl}/{resourse}");

            var json = JsonConvert.SerializeObject(body);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(uri, data);
            var resultStr = await response.Content.ReadAsStringAsync();
            
            var result = JsonConvert.DeserializeObject<T>(resultStr);
            return result;
        }

        async Task<T> GetAsync<T>(string baseUrl, string resourse)
        {
            Uri uri = new Uri($"{baseUrl}/{resourse}");

            var response = await _client.GetAsync(uri);
            var resultStr = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<T>(resultStr);
            return result;
        }

        async Task<ContractTransaction> TriggerSmartContractAsync(string contractAddress, string ownerAddress, 
            string parameter, string functionSelector, long feeLimit)
        {
            var body =  new TriggerSmartContractRequest{
                ContractAddress= contractAddress,
                OwnerAddress = ownerAddress,
                Parameter = parameter,
                FunctionSelector = functionSelector,
                FeeLimit = feeLimit
            };

            var result = await PostAsync<ContractTransaction>(baseUrl: this._fullNode, resourse: "wallet/triggersmartcontract", body: body);
            return result;
        }

       
        async Task<long> GetFeeLimitAsync(string contractAddress) 
        {
            /*
            - if the receiver address did not receive a transaction on the specific trc20 contract (say USDT-TRC20), 
            energy consumption would be exactly 29,631.

            - if the receiver address already received a transaction on the specific trc20 contract, 
            energy consumption would be exactly 14,631.

            --------------------------------------------------------------------------
            Currently 1 energy = 280 sun ,therefore max fee limit is 29,631 * 280 = 8,296,680
           
            */

            await Task.FromResult(0);
            return 10 * 1_000_000L; // it means the contract can consume up to 5_000_000 / 280 SUN =  1,000 energy 
        }
       
       
    
 

       

       
    }
}