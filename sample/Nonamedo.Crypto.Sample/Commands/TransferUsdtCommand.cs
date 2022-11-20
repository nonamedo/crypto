using System;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Sample.Interfaces;

namespace Nonamedo.Crypto.Sample.Commands
{
    internal class TransferUsdtCommand: ICommand
    {
        private readonly ICryptoService _service;
        private readonly IInputOutput _io;

        public TransferUsdtCommand(ICryptoService service, IInputOutput io)
        {
            _service = service;
            _io = io;
        }
        
        public void Execute()
        {
            _io.WriteLine("Enter sender's address: ");
            string fromAddress = _io.ReadLine();
            _io.WriteLine();

             _io.WriteLine("Enter sender's private key: ");
            string fromPrivateKey = _io.ReadLine();
            _io.WriteLine();

             _io.WriteLine("Enter receiver's address: ");
            string toAddress = _io.ReadLine();
            _io.WriteLine();

            _io.WriteLine("Enter amount: ");
            decimal amount = Convert.ToDecimal(_io.ReadLine());
            _io.WriteLine();


            var from = new CryptoAccount(fromAddress, "", fromPrivateKey);
            var to = new CryptoAccount(toAddress, "", "");

            var txid = _service.TransferTokenAsync(
                from: from,
                contractAddress: _service.GetUsdtContract(),
                toAddress: toAddress,
                amount: amount).GetAwaiter().GetResult();


            _io.WriteLine("Txid: {0}", txid);
           _io.WriteLine();
        }
    }
}