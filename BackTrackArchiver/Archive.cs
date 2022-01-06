using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace BackTrackArchiver
{
    public class Archive
    {
        public string directory = "";
        public Aes aes;
        public List<string> archiveFiles = new List<string>();
        public uint nextFileIndex = 0;

        public Archive(string directory, Aes aes)
        {
            this.directory = directory;
            this.aes = aes;
            FindArchiveFiles();
        }

        public void ArchiveFiles(string[] addedFiles, string[] removedFiles)
        {
            ArchiveFile archiveFile = new ArchiveFile();
            string archiveFileName = directory + "/" + nextFileIndex + ".bta";
            archiveFile.Create(archiveFileName, nextFileIndex, aes, addedFiles, removedFiles);
            archiveFile.Close();
        }

        public string[] ListArchiveFiles()
        {
            return archiveFiles.ToArray();
        }

        public FileTableEntry[] ListFilesInArchive(int archiveIndex)
        {
            ArchiveFile archiveFile = new ArchiveFile();
            string archiveFileName = directory + "/" + archiveIndex + ".bta";
            archiveFile.Open(archiveFileName, true, aes);
            FileTableEntry[] files = archiveFile.fileTable.ToArray();
            archiveFile.Close();
            return files;
        }

        private void FindArchiveFiles()
        {
            foreach(string file in Directory.GetFiles(directory))
            {
                BinaryReader reader;
                try
                {
                    reader = new BinaryReader(File.Open(file, FileMode.Open));
                }
                catch (Exception e)
                {
                    continue;
                }
                uint magic = reader.ReadUInt32();
                if (magic == 0x46415442)
                {
                    archiveFiles.Add(file);
                    reader.BaseStream.Seek(28, SeekOrigin.Begin);
                    uint index = reader.ReadUInt32();
                    if (index >= nextFileIndex)
                        nextFileIndex = index + 1;
                }
                reader.Close();
            }
        }
    }
}
