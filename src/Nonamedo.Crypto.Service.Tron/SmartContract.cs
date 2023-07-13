using Newtonsoft.Json;

namespace Nonamedo.Crypto.Service.Tron
{

     internal class SmartContract 
    {
        [JsonProperty("parameter")]
        public SmartContractParameter Parameter {get;set;}
        
        [JsonProperty("type")]
        public SmartContractParameter Type {get;set;}
    }


}
