using System;

namespace Nonamedo.Crypto.Service.Tron
{
    internal class Helper
    {
        public static byte[] HexToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-","");
        }

        public static string ToHex(string address)
        {
            var bytes = Base58Encoder.DecodeFromBase58Check(address);
            var hex = BitConverter.ToString(bytes);
            hex = hex.Replace("-", "").ToLower();
            return hex;
        }

        public static string ToParameter(string hex)
        {
            return hex.PadLeft(64, '0');
        }

    }
}



