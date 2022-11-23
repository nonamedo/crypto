using System;
using System.Net.Http;
using Nonamedo.Crypto.Service.Ethereum.Services;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Service.Tron;

namespace Nonamedo.Crypto.Factories
{
    public static class CryptoServiceFactory
    {
        public static ICryptoService CreateTronService(string fullNode, string solidityNode, HttpClient httpClient)
        {
            return new TronService(fullNode, solidityNode, httpClient);
        }


         public static ICryptoService CreateEthereumService(string node)
        {
            return new EthereumService(node);
        }
    

    }
}
