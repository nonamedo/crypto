using Newtonsoft.Json;

namespace Nonamedo.Crypto.Service.Tron
{
    internal class BroadcastTransactionResult
    {
        [JsonProperty("result")]
        public bool Result {get;set;}

        [JsonProperty("txid")]
        public string Txid  {get;set;}

        [JsonProperty("message")]
        public string Message  {get;set;}

        [JsonProperty("code")]
        public string Code  {get;set;}
    }

    
  

}