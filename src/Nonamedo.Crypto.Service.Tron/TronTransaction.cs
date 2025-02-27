using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nonamedo.Crypto.Service.Tron
{
    internal class SmartContractParameterValue
    {
        [JsonProperty("data")]
        public string Data {get;set;}

        [JsonProperty("owner_address")]
        public string OwnerAddress {get;set;}

        [JsonProperty("contract_address")]
        public string ContractAddress {get;set;}
    }
    internal class SmartContractParameter
    {
        [JsonProperty("value")]
        public SmartContractParameterValue Value {get;set;}

        [JsonProperty("type_url")]
        public string TypeUrl {get;set;}
    }
   
    internal class RawData
    {
        [JsonProperty("contract")]
        public SmartContract[] Contract {get;set;}

        [JsonProperty("ref_block_bytes")]
        public string RefBlockBytes {get;set;}

        [JsonProperty("ref_block_hash")]
        public string RefBlockHash {get;set;}

        [JsonProperty("expiration")]
        public long Expiration {get;set;}

        [JsonProperty("fee_limit")]
        public long FeeLimit {get;set;}

        [JsonProperty("timestamp")]
        public long Timestamp {get;set;}
    }

    internal class ContractReseult 
    {
        [JsonProperty("contractRet")]
        public string ContractRet {get;set;} //"SUCCESS"
    }

    internal class TronTransaction
    {
        [JsonProperty("ret")]
        public ContractReseult[] Ret {get;set;}
        
        [JsonProperty("visible")]
        public bool Visible {get;set;}

        [JsonProperty("txID")]
        public string TxID  {get;set;}

        [JsonProperty("raw_data")]
        public JObject RawData  {get;set;}

        [JsonProperty("raw_data_hex")]
        public string RawDataHex  {get;set;}
        
        // [JsonProperty("permission_id")]
        // public int? PermissionId  {get;set;}

        [JsonProperty("signature")]
        public string[] Signature {get;set;}
    }
}
    
