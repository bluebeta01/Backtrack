using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTrackArchiver
{
    public class ArchivingFailedEventArgs : EventArgs
    {
        public string fileName;
        public string message;
    }
}
