using Newtonsoft.Json;

namespace Nonamedo.Crypto.Service.Tron
{
    internal class CreateTransactionRequest
    {
        [JsonProperty("to_address")] public string ToAddress { get; set; }

        [JsonProperty("owner_address")] public string OwnerAddress { get; set; }

        [JsonProperty("amount")] public long Amount { get; set; }

        [JsonProperty("Permission_id", NullValueHandling = NullValueHandling.Ignore)]
        public int? PermissionId { get; set; }

    }

}