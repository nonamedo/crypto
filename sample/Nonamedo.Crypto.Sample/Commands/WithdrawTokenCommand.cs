using System;
using Nonamedo.Crypto.Service.interfaces;
using Nonamedo.Crypto.Service;
using Nonamedo.Crypto.Sample.Interfaces;

namespace Nonamedo.Crypto.Sample.Commands
{
    internal class WithdrawTokenCommand: ICommand
    {
        private readonly ICryptoService _service;
        private readonly IInputOutput _io;

        public WithdrawTokenCommand(ICryptoService service, IInputOutput io)
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
            
            _io.WriteLine("Enter permission id (if required): ");
            string permissionIdStr = _io.ReadLine();
            _io.WriteLine();
            
            int? permissionId = null;
            if (!string.IsNullOrWhiteSpace(permissionIdStr))
            {
                try
                {
                    permissionId = Convert.ToInt32(permissionIdStr);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            _io.WriteLine("Enter receiver's address: ");
            string toAddress = _io.ReadLine();
            _io.WriteLine();

            _io.WriteLine("Enter contract address: ");
            string contract = _io.ReadLine();
            _io.WriteLine();

            _io.WriteLine("Enter token amount: ");
            decimal tokenAmount = Convert.ToDecimal(_io.ReadLine());
            _io.WriteLine();


            var from = new CryptoAccount(fromAddress, "", fromPrivateKey, permissionId: permissionId);
            var to = new CryptoAccount(toAddress, "", "");

            var txid = _service.WithdrawTokenAsync(
                from: from,
                toAddress: toAddress,
                contractAddress: contract,
                tokenAmount: tokenAmount).GetAwaiter().GetResult();


            _io.WriteLine("Txid: {0}", txid);
           _io.WriteLine();
        }
    }
}