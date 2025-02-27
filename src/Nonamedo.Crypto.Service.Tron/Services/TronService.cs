using System;
using System.Text;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nonamedo.Crypto.Service.interfaces;
using System.Text.RegularExpressions;
using Polly;
using System.Net.Sockets;

namespace Nonamedo.Crypto.Service.Tron
{
    public class TronService: ICryptoService
    {
        private readonly string _fullNode;
        private readonly string _solidityNode;
        private readonly HttpClient _client;


        private void CheckAddressFormat(string address)
        {
            if (!Regex.IsMatch(address, "^T[A-Za-z1-9]{33}$"))
                throw new ArgumentException("Invalid address format");
        }

        public TronService(string fullNode, string solidityNode, HttpClient client)
        {
            this._fullNode = fullNode.TrimEnd('/');
            this._solidityNode = solidityNode.TrimEnd('/');
            this._client = client;
        }

        public async Task<string> SendTransactionAsync(CryptoAccount from, string toAddress, decimal amount)
        {
            CheckAddressFormat(toAddress);

            // todo: get account info
            
            var rawTransaction = await CreateTransactionAsync(
                toAddress: toAddress,
                ownerAddress: from.Address,
                amount: Convert.ToInt64(amount * 1_000_000L),
                permissionId: from.PermissionId);

            if (rawTransaction == null)
                return null;

            if (rawTransaction.TxID == null)
                return null;
            
            //rawTransaction.PermissionId = from.PermissionId;

            TronTransaction signedTransaction = await AddTransactionSignAsync(rawTransaction, from.PrivateKey);
            BroadcastTransactionResult result = await BroadcastTransactionAsync(signedTransaction);

            if (result.Result)
            {
                return result.Txid;
            }
            
            return null;
        }

        // public async Task GetAccountAsync(string address)
        // {
        //     var body = new GetAccountRequest{
        //        Address = address
        //     };
        //      
        //     var result = await PostAsync<TronAccount>(baseUrl: this._fullNode, resourse: "wallet/getaccount", body: body);
        //     return result;
        // }

        public Task<string> SendTransactionAsync(CryptoAccount from, string toAddress, decimal amount, int? permissionId)
        {
            throw new NotImplementedException();
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

        

        // async Task<TronTransaction> CreateTransactionAsync(string toAddress, string ownerAddress, long amount)
        // {
        //     CheckAddressFormat(toAddress);
        //
        //     var body = new CreateTransactionRequest{
        //         OwnerAddress = Helper.ToHex(ownerAddress),
        //         ToAddress = Helper.ToHex(toAddress),
        //         Amount = amount
        //     };
        //      
        //     var result = await PostAsync<TronTransaction>(baseUrl: this._fullNode, resourse: "wallet/createtransaction", body: body);
        //     return result;
        // }
        //
        async Task<TronTransaction> CreateTransactionAsync(string toAddress, string ownerAddress, long amount, int? permissionId)
        {
            CheckAddressFormat(toAddress);

            var body = new CreateTransactionRequest{
                OwnerAddress = Helper.ToHex(ownerAddress),
                ToAddress = Helper.ToHex(toAddress),
                Amount = amount,
                PermissionId = permissionId
            };
             
            var result = await PostAsync<TronTransaction>(baseUrl: this._fullNode, resourse: "wallet/createtransaction", body: body);
            return result;
        }
        
        public async Task<decimal> CalcGasToWithdrawTokenAsync(CryptoAccount from, string toAddress, string contractAddress, decimal tokenAmount)
        {
            CheckAddressFormat(toAddress);

            var fee_limit = await GetFeeLimitAsync(contractAddress);




            // 31,895 Energy （13.3959 TRX burned for Energy）
            // Basic Energy consumption is 14,650 and extra Energy consumption is 17,245


            return fee_limit / 1_000_000L;
        }

       

        public async Task<string> WithdrawGasAsync(CryptoAccount from, string toAddress)
        {
            CheckAddressFormat(toAddress);

            var balance = await GetGasBalanceAsync(from);

            // 268 bp
            // the unit price is = 1_000 sun, so 268 * 1_000 = 268_
            var amount = balance - 0.1m; // 600 bp - 345 (to withdraw USDT) = 255 is not enough


            string txid = "0000000000000000000000000000000000000000000000000000000000000000";
            if (amount > 0)
            {
                txid = await SendTransactionAsync(from, toAddress, amount);
            }
            return txid;
        }

        public async Task<string> WithdrawTokenAsync(CryptoAccount from, string toAddress, string contractAddress, decimal tokenAmount)
        {
            CheckAddressFormat(toAddress);

            /*
            1 TRX = 1,000,000 SUN.
            1 Energy = 420 SUN

            fee_limit is the total SUN value that contact call can consume. 
            For example, 
            if a contract consumes 10 energy, then you need to set feeLimit to at least 2,800 SUN because the price of 1 enerege is 280 SUN
            
            setting fee_limit Prevents the contract call transaction from consuming too much energy.
            */

            

            


            long feeLimit = await GetFeeLimitAsync(contractAddress);
            long amountInSun = Convert.ToInt64(tokenAmount * 1_000_000m);
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

         async Task<ContractInfo> GetContractInfoAsync(string contractAddress)
        {
            var hex = Helper.ToHex(contractAddress);
            var body = new GetContractInfoRequest{
                Value = hex
            };

            var result = await PostAsync<ContractInfo>(baseUrl: this._fullNode, resourse: "wallet/getcontractinfo", body: body);

            return result;
        }




        async Task<T> PostAsync<T>(string baseUrl, string resourse, object body)
        {
            Uri uri = new Uri($"{baseUrl}/{resourse}");

            var json = JsonConvert.SerializeObject(body);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var policy = Policy
                .HandleInner<HttpRequestException>()
                .WaitAndRetry(
                    5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, context) =>
                    {
                        // Add logic to be executed before each retry, such as logging

                    }
                );

            HttpResponseMessage response = policy
                .Execute<HttpResponseMessage>(() => _client.PostAsync(uri, data).Result);

            var resultStr = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<T>(resultStr);

            return result;
        }

        async Task<T> GetAsync<T>(string baseUrl, string resourse)
        {
            Uri uri = new Uri($"{baseUrl}/{resourse}");

            var policy = Policy
                .HandleInner<HttpRequestException>()
                .WaitAndRetry(
                    5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, context) =>
                    {
                        // Add logic to be executed before each retry, such as logging

                    }
                );

            HttpResponseMessage response = policy
                .Execute<HttpResponseMessage>(() => _client.GetAsync(uri).Result);

            string resultStr = await response.Content.ReadAsStringAsync();
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

        async Task<ContractTransaction> TriggerConstantContract(string ownerAddress, string contractAddress, 
            string functionSelector, string parameter, string data = null, long? callValue= null, long? callTokenValue = null,
            long? tokenId = null, bool visible = true)
        {
            var body =  new TriggerConstantContractRequest{
                OwnerAddress = ownerAddress,
                ContractAddress= contractAddress,
                FunctionSelector = functionSelector,
                Parameter = parameter,
                Data = data,
                CallValue = callValue,
                CallTokenValue = callTokenValue,
                TokenId = tokenId,
                Visible = visible
            };

            var result = await PostAsync<ContractTransaction>(baseUrl: this._fullNode, resourse: "wallet/triggerconstantcontract", body: body);
            return result;
        }

       
        async Task<long> GetFeeLimitAsync(string contractAddress) 
        {
            /*
            - if the receiver address did not receive a transaction on the specific trc20 contract (say USDT-TRC20), 
            BASIC energy consumption would be 29,650. But actual (due to dynamic energy model https://developers.tron.network/docs/resource-model#dynamic-energy-model)
             = 64,895 (on 12.07.23)

            - if the receiver address already received a transaction on the specific trc20 contract, 
            BASIC energy consumption would be 14,650. But actual (due to dynamic energy model https://developers.tron.network/docs/resource-model#dynamic-energy-model)
             = 31,895 (on 12.07.23)

            --------------------------------------------------------------------------
            Currently 1 energy = 420 sun ,therefore max fee limit is 29,631 * 420 = 8,296,680

            Current price of 1 energy is 420

            need to determine the energy consumption (to withdraw USDT).
            So we need to transfer: 420 * en.cons. sun

            ----------------------------------------------------------------------------------

            For empty recipient: 29650 energy (previously 29631)
            For non-empty recipient: 14650 energy (previously 14631)
           
            */


            /* ContractTransaction rr = await TriggerConstantContract(
                ownerAddress: "TA5Qt9d6u7ipA46NQkZ8hNyBGSLy9hhGRV",
                contractAddress: contractAddress,
                functionSelector: "transfer(address,uint256)",
                parameter: "0000000000000000000000002ce5de57373427f799cc0a3dd03b841322514a8c00000000000000000000000000000000000000000000000000038d7ea4c68000",
                visible: true);

            if (rr.EnergyUsed <= 0)
                throw new Exception("Cannot estimate gas used");  */

            

            /* ContractInfo ci = await GetContractInfoAsync(contractAddress);
            long basicEnergyConsumption = 14_650; // todo: esitmate dynamically
            long extraEnergyConsumption = basicEnergyConsumption * ci.ContractState.EnergyFactor / 10_000;
            long energyConsumption = basicEnergyConsumption + extraEnergyConsumption;

            long energyUnitPrice = 420; // todo: estimate dynamically

            long gasRequired = energyUnitPrice * energyConsumption / 1_000_000;

            // double this to be sure
            return gasRequired * 2L * 1_000_000L; */

            await Task.FromResult(0);
            //return 10 * 1_000_000L; // it means the contract can consume up to 10_000_000 / 280 SUN =  1,000 energy 
            //return 15 * 1_000_000L;
            //return 20 * 1_000_000L;
            return 100 * 1_000_000L;
        }
       
       
    
 

       

       
    }
}