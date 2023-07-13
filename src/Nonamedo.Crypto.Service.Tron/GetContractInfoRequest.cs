using Newtonsoft.Json;

namespace Nonamedo.Crypto.Service.Tron
{
    internal class GetContractInfoRequest
    {
        [JsonProperty("value")]
        public string Value {get;set;}

        [JsonProperty("visible")]
        public bool Visible { get; set; } = false;

    }

}