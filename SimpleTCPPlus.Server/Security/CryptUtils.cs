using SimpleTCPPlus.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTCPPlus.Server.Security
{
    public static class CryptUtils
    {
        private static Random Rand = new Random();
        public static Packet CryptPacket(Packet packet)
        {
            string password = GeneratePassword();
            string packetSec = Convert.ToBase64String(PreAwwareB1(Encoding.Default.GetBytes(password)));
            byte[] rawData = EncryptBytesAES(packet.RawData, password);
            string packetType = Convert.ToBase64String(PreAwwareB1(Encoding.Default.GetBytes(packet.PacketType)));
            return new Packet(rawData, packetType, packetSec);
        }
        public static string GeneratePassword()
        {
            StringBuilder sb = new StringBuilder(32);
            string alpha = "abcdefghABCDEFGH1234567890!@#$%^&*()_+-={}[],.?";
            for (int i = 0; i <= 32; i++)
                sb.Append(alpha[Rand.Next(alpha.Length)]);
            return sb.ToString();
        }
        private static byte[] EncryptBytesAES(byte[] data, string key)
        {
            try
            {
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes bdp = new Rfc2898DeriveBytes(key, new byte[]
                    { 0x29, 0x86, 0x21, 0x6E, 0xF0, 0x4E, 0x6A, 0x6B, 0xC6, 0x66, 0x14, 0x28, 0x2F });
                    encryptor.Key = bdp.GetBytes(32);
                    encryptor.IV = bdp.GetBytes(16);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(data, 0, data.Length);
                            cs.Close();
                        }
                        data = ms.ToArray();
                    }
                }
                return data;
            }
            catch { return null; }
        }
        private static byte[] PreAwwareB1(byte[] data)
        {
            int length = data.Length;
            for (int x = 0; x < length; x++)
            {
                if (length - x > 7)
                {
                    data[x] ^= (byte)((x + 1) << 7);
                    data[x] ^= (byte)((x + 2) << 6);
                    data[x] ^= (byte)((x + 3) << 5);
                    data[x] ^= (byte)((x + 4) << 4);
                    data[x] ^= (byte)((x + 5) << 3);
                    data[x] ^= (byte)((x + 6) << 2);
                    data[x] ^= (byte)((x + 7) << 1);
                }
                else
                    data[x] ^= (byte)(x + 1 + (x % 32));
            }

            return data;
        }
    }
}
