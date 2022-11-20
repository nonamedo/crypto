
using Newtonsoft.Json;

namespace Nonamedo.Crypto.Service.Tron
{
    internal class TriggerSmartContractRequest
    {
        [JsonProperty("contract_address")]
        public string ContractAddress {get;set;}

        [JsonProperty("function_selector")]
        public string FunctionSelector {get;set;}

        [JsonProperty("parameter")]
        public string Parameter {get;set;}

        [JsonProperty("owner_address")]
        public string OwnerAddress {get;set;}

        [JsonProperty("fee_limit")]
        public long FeeLimit {get;set;}


        
    }
}