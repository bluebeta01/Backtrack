using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTrackArchiver
{
    public class ArchiveInvalidException : Exception
    {
        public ArchiveInvalidException()
            : base("Archive not valid")
        {

        }
    }
}
