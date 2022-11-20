using System;
using Nonamedo.Crypto.Sample.Factories;
using Nonamedo.Crypto.Sample.Interfaces;
using Nonamedo.Crypto.Sample.Services;
using Nonamedo.Crypto.Service.interfaces;
using static System.Console;

namespace Nonamedo.Crypto.Sample
{
    class Program
    {

        static void Main(string[] args)
        {
            bool again = false;
            do
            {
               IInputOutput io = new ConsoleInputOutput();

                InternalCryptoServiceFactory serviceFactory = new InternalCryptoServiceFactory(io);
                serviceFactory.OutputNetworks();
                ICryptoService cryptoService = serviceFactory.Create(ReadLine());
                if (cryptoService == null)
                {
                    WriteLine("Network not found");
                    break;
                }
                WriteLine("{0} is chosen",  cryptoService.GetType().Name);
                WriteLine();

                
                CommandFactory cmdFactory = new CommandFactory(cryptoService, io);
                cmdFactory.OutputCommands();
                ICommand cmd = cmdFactory.Create(Console.ReadLine());
                if (cmd == null)
                {
                    WriteLine("Command not found");
                    break;
                }
                WriteLine("{0} is chosen", cmd.GetType().Name);
                WriteLine();

                cmd.Execute();
                WriteLine();
                WriteLine("Type '1' to run again");
                again = ReadLine() == "1";
            } 
            while (again);

            
            
            

        }
    }
}
