using Newtonsoft.Json;

namespace Nonamedo.Crypto.Service.Tron
{
  
    internal class ContractInfo
    {
        [JsonProperty("runtimecode")]
        public string RunTimeCode {get;set;}

        [JsonProperty("smart_contract")]
        public SmartContract SmartContract  {get;set;}

        [JsonProperty("contract_state")]
        public ContractState ContractState  {get;set;}
    }

   
}
