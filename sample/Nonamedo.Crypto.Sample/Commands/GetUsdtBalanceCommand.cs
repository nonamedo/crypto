using System;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Sample.Interfaces;

namespace Nonamedo.Crypto.Sample.Commands
{
    internal class GetUsdtBalanceCommand: ICommand
    {
        private readonly ICryptoService _service;
        private readonly IInputOutput _io;

        public GetUsdtBalanceCommand(ICryptoService service, IInputOutput io)
        {
            _service = service;
            _io = io;
        }
        
        public void Execute()
        {
            _io.WriteLine("Enter account address: ");
            string address = _io.ReadLine();
            _io.WriteLine();

            var acc = new CryptoAccount(address, "", "");

            decimal balance = _service.GetTokenBalanceAsync(acc, _service.GetUsdtContract()).GetAwaiter().GetResult();

           _io.WriteLine("Token balance: {0}", balance);
           _io.WriteLine();
        }
    }
}