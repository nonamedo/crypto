
namespace Nonamedo.Crypto.Sample.Interfaces
{
    internal interface IInputOutput
    {
        string ReadLine();
        void WriteLine();
        void WriteLine(string text);
        void WriteLine(string format, params object[] args);
        void Write(string text);

    }
}