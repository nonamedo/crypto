using Newtonsoft.Json;

namespace Nonamedo.Crypto.Service.Tron
{
    internal class GetAccountRequest
    {
        [JsonProperty("address")]
        public string Address {get;set;}
        
    }

}