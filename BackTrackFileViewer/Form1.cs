using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BackTrackArchiver;
using System.Security.Cryptography;

namespace BackTrackFileViewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Aes aes = Aes.Create();
            aes.Key = Convert.FromBase64String("");
            aes.IV = Convert.FromBase64String("");
            Archive archive = new Archive("E:/backtrack_backups/archive", aes);
            FileTableEntry[] files = archive.ListFilesInArchive(2);
            foreach(FileTableEntry file in files)
            {
                listBox1.Items.Add(file.name + "  " + file.uncompresedSize + "  ,  " + file.compressedSize);
            }
        }
    }
}
