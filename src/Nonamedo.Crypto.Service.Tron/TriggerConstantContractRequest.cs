
using Newtonsoft.Json;

namespace Nonamedo.Crypto.Service.Tron
{
    internal class TriggerConstantContractRequest
    {
        [JsonProperty("owner_address")]
        public string OwnerAddress {get;set;}

        [JsonProperty("contract_address")]
        public string ContractAddress {get;set;}

        [JsonProperty("function_selector")]
        public string FunctionSelector {get;set;}

        [JsonProperty("parameter")]
        public string Parameter {get;set;}

        [JsonProperty("data")]
        public string Data {get;set;}

        [JsonProperty("call_value")]
        public long? CallValue {get;set;}

        [JsonProperty("call_token_value")]
        public long? CallTokenValue {get;set;}

        [JsonProperty("token_id")]
        public long? TokenId {get;set;}

        [JsonProperty("visible")]
        public bool Visible { get; set; } = true;
    }
}