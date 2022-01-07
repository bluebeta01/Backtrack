using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTrackArchiver
{
    public class IncorrectKeyException : Exception
    {
        public IncorrectKeyException()
            :base("Incorrect encryption key used")
        {

        }
    }
}
