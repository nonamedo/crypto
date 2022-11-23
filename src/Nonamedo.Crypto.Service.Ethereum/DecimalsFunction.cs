using System;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nonamedo.Crypto.Service.interfaces;
using Nethereum.Web3.Accounts;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Nonamedo.Crypto.Service.Ethereum
{
    [Function("decimals", "uint256")]
    internal class DecimalsFunction : FunctionMessage
    {
        
    }

}