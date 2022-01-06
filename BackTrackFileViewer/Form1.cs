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
        GetDataForm dataForm = new GetDataForm();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataForm.ShowDialog();
            if (dataForm.archiveDir == "" || dataForm.aesKey == "" || dataForm.aesIv == "")
                return;

            Aes aes = Aes.Create();
            aes.Key = Convert.FromBase64String(dataForm.aesKey);
            aes.IV = Convert.FromBase64String(dataForm.aesIv);
            Archive archive = new Archive(dataForm.archiveDir, aes);
            FileTableEntry[] files = archive.ListFilesInArchive(0);
            foreach(FileTableEntry file in files)
            {
                listBox1.Items.Add(file.name + "  " + file.uncompresedSize + "  ,  " + file.compressedSize);
            }
        }
    }
}
