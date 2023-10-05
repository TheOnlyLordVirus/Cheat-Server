using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheatServer.Security
{
    public interface IEncrypter
    {
        byte[] Encrypt(byte[] rsaEncryptionKey, string plainText);
    }
}
