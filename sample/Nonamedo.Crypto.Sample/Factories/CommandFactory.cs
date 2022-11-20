using Nonamedo.Crypto.Sample.Commands;
using Nonamedo.Crypto.Sample.Interfaces;
using Nonamedo.Crypto.Service.interfaces;

namespace Nonamedo.Crypto.Sample.Factories
{
    internal class CommandFactory
    {
        private readonly ICryptoService _service;
        private readonly IInputOutput _io;

        public CommandFactory(ICryptoService service, IInputOutput io) 
        {
            _service = service;
            _io = io;
        }

        public void OutputCommands()
        {
            _io.WriteLine("Select command: ");
            _io.WriteLine("1. Create an account");
            _io.WriteLine("2. Get Usdt contract address");
            _io.WriteLine("3. Get token balance");
            _io.WriteLine("4. Get USDT token balance");
            _io.WriteLine("5. Get Gas balance");
            _io.WriteLine("6. Calc Gas amount to send for a contract");
            _io.WriteLine("7. Calc Gas amount to send for Usdt");
            _io.WriteLine("8. Calc Gas amount to withdraw");
            _io.WriteLine("9. Is a transaction confirmed");
            _io.WriteLine("10. Transfer token");
            _io.WriteLine("11. Transfer Usdt token");
            _io.WriteLine("12. Send transaction");
        }

        public ICommand Create(string cmd)
        {
            cmd = cmd.Trim();
            if (cmd.Equals("1", System.StringComparison.OrdinalIgnoreCase))
            {
                return new CreateAccountCommand(_service, _io);
            }
            else if (cmd.Equals("2", System.StringComparison.OrdinalIgnoreCase))
            {
                return new GetUsdtContractCommand(_service, _io);
            }
            else if (cmd.Equals("3", System.StringComparison.OrdinalIgnoreCase))
            {
                return new GetTokenBalanceCommand(_service, _io);
            }
            else if (cmd.Equals("4", System.StringComparison.OrdinalIgnoreCase))
            {
                return new GetUsdtBalanceCommand(_service, _io);
            }
            else if (cmd.Equals("5", System.StringComparison.OrdinalIgnoreCase))
            {
                return new GetGasBalanceCommand(_service, _io);
            }
            else if (cmd.Equals("6", System.StringComparison.OrdinalIgnoreCase))
            {
                return new CalcGasAmountToSendForContractCommand(_service, _io);
            }
            else if (cmd.Equals("7", System.StringComparison.OrdinalIgnoreCase))
            {
                return new CalcGasAmountToSendForUsdtCommand(_service, _io);
            }
            else if (cmd.Equals("8", System.StringComparison.OrdinalIgnoreCase))
            {
                return new CalcGasAmountToWithdrawCommand(_service, _io);
            }
            else if (cmd.Equals("9", System.StringComparison.OrdinalIgnoreCase))
            {
                return new IsTrnsactionConfirmedCommand(_service, _io);
            }
            else if (cmd.Equals("10", System.StringComparison.OrdinalIgnoreCase))
            {
                return new TransferTokenCommand(_service, _io);
            }
            else if (cmd.Equals("11", System.StringComparison.OrdinalIgnoreCase))
            {
                return new TransferUsdtCommand(_service, _io);
            }
            else if (cmd.Equals("12", System.StringComparison.OrdinalIgnoreCase))
            {
                return new SendTransactionCommand(_service, _io);
            }
            else
            {
                return null;
            }
        }
        
    }
}