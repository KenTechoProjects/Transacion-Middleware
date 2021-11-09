using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DDL.ProcessMaker
{
    public class Utility
    {
        public static async Task<byte[]> FileToByteArrayAsync(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                // Create a byte array of file stream length
                byte[] bytes = await System.IO.File.ReadAllBytesAsync(filename);
                //Read block of bytes from stream into the byte array
                await fs.ReadAsync(bytes, 0, System.Convert.ToInt32(fs.Length));
                //Close the File Stream
                fs.Close();
                return bytes; //return the byte data
            }
        }
    }
}
