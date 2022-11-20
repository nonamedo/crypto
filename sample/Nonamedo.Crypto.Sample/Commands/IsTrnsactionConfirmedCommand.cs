using System;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Sample.Interfaces;

namespace Nonamedo.Crypto.Sample.Commands
{
    internal class IsTrnsactionConfirmedCommand: ICommand
    {
        private readonly ICryptoService _service;
        private readonly IInputOutput _io;

        public IsTrnsactionConfirmedCommand(ICryptoService service, IInputOutput io)
        {
            _service = service;
            _io = io;
        }
        
        public void Execute()
        {
            _io.WriteLine("Enter txid: ");
            string txid = _io.ReadLine();
            _io.WriteLine();

            bool result = _service.IsTrnsactionConfirmedAsync(txid).GetAwaiter().GetResult();

            if (result)
            {
                _io.WriteLine("Yes");
            }
            else
            {
                _io.WriteLine("No");
            }

           _io.WriteLine();
        }
    }
}