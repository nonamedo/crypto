using System;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Sample.Interfaces;

namespace Nonamedo.Crypto.Sample.Commands
{
    internal class CalcGasToWithdrawTokenCommand: ICommand
    {
        private readonly ICryptoService _service;
        private readonly IInputOutput _io;

        public CalcGasToWithdrawTokenCommand(ICryptoService service, IInputOutput io)
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

            _io.WriteLine("Enter token amount: ");
            decimal tokenAmount = Convert.ToDecimal(_io.ReadLine());
            _io.WriteLine();

            _io.WriteLine("Enter contract address: ");
            string contract = _io.ReadLine();
            _io.WriteLine();

            var from = new CryptoAccount(fromAddress, "", fromPrivateKey);
            var to = new CryptoAccount(toAddress, "", "");

            decimal amount = _service.CalcGasToWithdrawTokenAsync(from: from, toAddress: to.Address, contractAddress:contract, tokenAmount).GetAwaiter().GetResult();

           _io.WriteLine("Gas amount: {0}", amount);
           _io.WriteLine();
        }
    }
}