using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTrackArchiver
{
    public class FileTableEntry
    {
        public string name { get; set; }
        public ulong encryptedSize { get; set; }
        public ulong compressedSize { get; set; }
        public ulong uncompresedSize { get; set; }
        public ulong lastModified { get; set; }
        public ulong offset { get; set; }
    }
}
