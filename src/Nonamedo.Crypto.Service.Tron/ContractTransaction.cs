using Newtonsoft.Json;

namespace Nonamedo.Crypto.Service.Tron
{
    internal class ContractTransactionResult
    {
        [JsonProperty("result")]
        public bool Result {get;set;}
    }
    internal class ContractTransaction
    {
        [JsonProperty("result")]
        public ContractTransactionResult Result {get;set;}

        [JsonProperty("energy_used")]
        public long EnergyUsed  {get;set;}

        [JsonProperty("energy_penalty")]
        public long EnergyPenalty  {get;set;}

        [JsonProperty("constant_result")]
        public string[] ConstantResult  {get;set;}

        [JsonProperty("transaction")]
        public TronTransaction Transaction  {get;set;}
        
    }

   
}
