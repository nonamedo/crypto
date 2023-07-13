using Newtonsoft.Json;

namespace Nonamedo.Crypto.Service.Tron
{

    internal class ContractState
    {
        [JsonProperty("energy_usage")]
        public long EnergyUsage { get; set; }

        [JsonProperty("energy_factor")]
        public long EnergyFactor { get; set; }

        [JsonProperty("update_cycle")]
        public long UpdateCycle { get; set; }
    }


}
