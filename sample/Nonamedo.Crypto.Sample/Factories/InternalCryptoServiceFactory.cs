using System.Net.Http;
using Nonamedo.Crypto.Sample.Interfaces;
using Nonamedo.Crypto.Service.interfaces;
using static Nonamedo.Crypto.Factories.CryptoServiceFactory;

namespace Nonamedo.Crypto.Sample.Factories
{
    internal class InternalCryptoServiceFactory
    {
        private readonly IInputOutput _io;

        public InternalCryptoServiceFactory(IInputOutput io) 
        {
            _io = io;
        }

        public void OutputNetworks()
        {
            _io.WriteLine("Choose Network: ");
            _io.WriteLine("1. tron");
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
            else
            {
                return null;
            }
        }
        
    }
}