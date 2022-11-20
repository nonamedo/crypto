using Newtonsoft.Json;

namespace Nonamedo.Crypto.Service.Tron
{
    internal class GettTansactionByIdRequest
    {
        [JsonProperty("value")]
        public string Value {get;set;}
        
    }

}