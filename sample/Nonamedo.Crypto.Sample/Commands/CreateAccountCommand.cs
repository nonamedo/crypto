using System;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Sample.Interfaces;

namespace Nonamedo.Crypto.Sample.Commands
{
    internal class CreateAccountCommand: ICommand
    {
        private readonly ICryptoService _service;
        private readonly IInputOutput _io;

        public CreateAccountCommand(ICryptoService service, IInputOutput io)
        {
            _service = service;
            _io = io;
        }

        public void Execute()
        {
            var newAcc = _service.GenerateAccountAsync().GetAwaiter().GetResult();

           _io.WriteLine("Address: {0}", newAcc.Address);
           _io.WriteLine("PublicKey:  {0}", newAcc.PublicKey);
           _io.WriteLine("PrivateKey:  {0}", newAcc.PrivateKey);
           _io.WriteLine();
        }
    }
}