using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PopCast {
    public class Decrypt {
        bool authenticator;
        public Decrypt(bool auth) {
            aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.ASCII.GetBytes(Form1.key);
            decryptor = aes.CreateDecryptor();
            authenticator = auth;
        }
        Aes aes;
        ICryptoTransform decryptor;

        public void stop() {
            decryptor.Dispose();
            aes.Clear();
            aes.Dispose();

        }

        bool fail, authenticated;

        public byte[] decrypt(byte[] data) {
            byte[] result = new byte[0];
            try {
                result = decryptor.TransformFinalBlock(data, 0, data.Length);
                if(!authenticated) authenticated = true;
            } catch (CryptographicException e) {
                if (authenticator) {
                    string msg = "Authentication failed. Make sure your password is correct.";
                    if (!fail && !authenticated) {
                        Task task = new Task(() => {
                            MessageBox.Show(msg, "PopCast");
                        });
                        task.Start();
                        fail = true;
                    }
                }
                else {
                    //Console.WriteLine("decrypt failed " + data.Length);
                    return null;
                }
            } catch(ArgumentNullException e) {
                
            }
            
            return result;
        }

    }
}
