using System;
using Nonamedo.Crypto.Sample.Interfaces;

namespace Nonamedo.Crypto.Sample.Services
{
    internal class ConsoleInputOutput: IInputOutput
    {
        public string ReadLine(){
            return Console.ReadLine();
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void WriteLine(string text){
            Console.WriteLine(text);
        }
        public void Write(string text){
            Console.Write(text);
        }

    }
}