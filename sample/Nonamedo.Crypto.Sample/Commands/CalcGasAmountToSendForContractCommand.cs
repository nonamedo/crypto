using System;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Sample.Interfaces;

namespace Nonamedo.Crypto.Sample.Commands
{
    internal class CalcGasAmountToSendForContractCommand: ICommand
    {
        private readonly ICryptoService _service;
        private readonly IInputOutput _io;

        public CalcGasAmountToSendForContractCommand(ICryptoService service, IInputOutput io)
        {
            _service = service;
            _io = io;
        }
        
        public void Execute()
        {
            _io.WriteLine("Enter contract address: ");
            string contract = _io.ReadLine();
            _io.WriteLine();

            decimal amount = _service.CalcGasAmountToSendAsync(contract).GetAwaiter().GetResult();

           _io.WriteLine("Gas amount: {0}", amount);
           _io.WriteLine();
        }
    }
}