using System;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Sample.Interfaces;

namespace Nonamedo.Crypto.Sample.Commands
{
    internal class GetUsdtContractCommand: ICommand
    {
        private readonly ICryptoService _service;
        private readonly IInputOutput _io;

        public GetUsdtContractCommand(ICryptoService service, IInputOutput io)
        {
            _service = service;
            _io = io;
        }
        

        public void Execute()
        {
           _io.WriteLine(_service.GetUsdtContract());
        }
    }
}