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
        public Guid guid = Guid.NewGuid();
        public event EventHandler<string> archivingEvent;
        public event EventHandler<ArchivingFailedEventArgs> archivingFailedEvent;

        public Archive(string directory, Aes aes)
        {
            this.directory = directory;
            this.aes = aes;
            FindArchiveFiles();
        }

        public void ArchiveFiles(string[] addedFiles, string[] removedFiles)
        {
            ArchiveFile archiveFile = new ArchiveFile();
            //TODO: We should be able to open the file even if it isn't named by the index number
            string archiveFileName = directory + '\\' + nextFileIndex + ".bta";
            archiveFile.Create(archiveFileName, nextFileIndex, aes, addedFiles, removedFiles, guid, archivingEvent, archivingFailedEvent);
            archiveFile.Close();
        }

        public string[] ListArchiveFiles()
        {
            return archiveFiles.ToArray();
        }

        public FileTableEntry[] ListFilesInArchive(int archiveIndex)
        {
            ArchiveFile archiveFile = new ArchiveFile();
            //TODO: We should be able to open the file even if it isn't named by the index number
            string archiveFileName = directory + '\\' + archiveIndex + ".bta";
            archiveFile.Open(archiveFileName, true, aes);
            FileTableEntry[] files = archiveFile.fileTable.ToArray();
            archiveFile.Close();
            return files;
        }

        private void FindArchiveFiles()
        {
            foreach(string file in Directory.GetFiles(directory))
            {
                try
                {
                    BinaryReader reader = new BinaryReader(File.Open(file, FileMode.Open));
                    BtaHeader header = new BtaHeader();
                    header.Read(reader);
                    archiveFiles.Add(file);
                    guid = header.guid;
                    if (header.index >= nextFileIndex)
                        nextFileIndex = header.index + 1;
                    reader.Close();
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }
    }
}
