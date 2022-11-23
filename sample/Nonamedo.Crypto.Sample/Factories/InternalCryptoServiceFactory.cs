using System.Net.Http;
using Nonamedo.Crypto.Sample.Interfaces;
using Nonamedo.Crypto.Service.interfaces;
using static Nonamedo.Crypto.Factories.CryptoServiceFactory;

namespace Nonamedo.Crypto.Sample.Factories
{
    internal class InternalCryptoServiceFactory
    {
        private readonly IInputOutput _io;
        private readonly string _apiKey;

        public InternalCryptoServiceFactory(IInputOutput io, string apiKey) 
        {
            _io = io;
            _apiKey = apiKey;
        }

        public void OutputNetworks()
        {
            _io.WriteLine("Choose Network: ");
            _io.WriteLine("1. tron");
            _io.WriteLine("2. ethereum");
        }

        public ICryptoService Create(string network)
        {
            if (network.Equals("tron", System.StringComparison.OrdinalIgnoreCase) || network.Equals("1", System.StringComparison.OrdinalIgnoreCase))
            {
                return CreateTronService(
                    fullNode: "http://35.180.51.163:8090", // public node
                    solidityNode: "http://35.180.51.163:8091", // public node
                    httpClient: new HttpClient());
            }
            else if (network.Equals("ethereum", System.StringComparison.OrdinalIgnoreCase) || network.Equals("2", System.StringComparison.OrdinalIgnoreCase))
            {
                return CreateEthereumService(node: "https://mainnet.infura.io/v3/" + _apiKey);
            }
            else
            {
                return null;
            }
        }
        
    }
}