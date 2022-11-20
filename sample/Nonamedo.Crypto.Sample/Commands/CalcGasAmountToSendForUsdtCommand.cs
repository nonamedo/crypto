using System;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Sample.Interfaces;

namespace Nonamedo.Crypto.Sample.Commands
{
    internal class CalcGasAmountToSendForUsdtCommand: ICommand
    {
        private readonly ICryptoService _service;
        private readonly IInputOutput _io;

        public CalcGasAmountToSendForUsdtCommand(ICryptoService service, IInputOutput io)
        {
            _service = service;
            _io = io;
        }
        
        public void Execute()
        {
            decimal amount = _service.CalcGasAmountToSendAsync(_service.GetUsdtContract()).GetAwaiter().GetResult();

           _io.WriteLine("Gas amount: {0}", amount);
           _io.WriteLine();
        }
    }
}