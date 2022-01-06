using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackTrackFileViewer
{
    public partial class GetDataForm : Form
    {
        public string archiveDir = "C:/bt/archive";
        public string aesKey = "3Ds0SrSHYR99MpLmFJ6iNGEYMGeA0F8TSvxZkxEi6Ek=";
        public string aesIv = "av1wCzXTgLFPYDgUVNG3vQ==";

        public GetDataForm()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            archiveDir = archDirTextbox.Text;
            aesKey = aesKeyTextbox.Text;
            aesIv = aesIvTextbox.Text;
            Close();
        }
    }
}
