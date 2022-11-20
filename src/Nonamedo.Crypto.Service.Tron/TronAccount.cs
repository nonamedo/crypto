
using Newtonsoft.Json;

namespace Nonamedo.Crypto.Service.Tron
{
    internal class TronAccount
    {
        [JsonProperty("address")]
        public string Address {get;set;}

        [JsonProperty("balance")]
        public long Balance  {get;set;}
        
    }

}