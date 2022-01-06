using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace BackTrackArchiver
{
    public class AesCoder
    {
        public static MemoryStream decodeBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            Aes aes = Aes.Create();
            aes.Padding = PaddingMode.Zeros;
            aes.Key = key;
            aes.IV = iv;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            MemoryStream memStream = new MemoryStream(cipherText);
            CryptoStream cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read);
            BinaryReader reader = new BinaryReader(cryptoStream);

            MemoryStream plainTextStream = new MemoryStream();
            const int bufferSize = 4096;
            byte[] buffer = new byte[bufferSize];
            int count;
            while((count = reader.Read(buffer, 0, bufferSize)) != 0)
            {
                plainTextStream.Write(buffer, 0, count);
            }

            plainTextStream.Seek(0, SeekOrigin.Begin);
            return plainTextStream;
        }

        public static byte[] encodeBytes(byte[] clearText, byte[] key, byte[] iv)
        {
            Aes aes = Aes.Create();
            aes.Padding = PaddingMode.Zeros;
            aes.Key = key;
            aes.IV = iv;


            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            MemoryStream memStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write);
            BinaryWriter writer = new BinaryWriter(cryptoStream);
            writer.Write(clearText);
            writer.Flush();
            cryptoStream.FlushFinalBlock();
            return memStream.ToArray();
        }
    }
}
