using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.IO.Compression;

namespace BackTrackArchiver
{
    public class ArchiveFile
    {
        BtaHeader header = new BtaHeader();
        public List<string> addedFiles = new List<string>();
        public List<string> removedFiles = new List<string>();
        public List<FileTableEntry> fileTable = new List<FileTableEntry>();
        public string filePath;
        public FileStream fileStream;
        public Aes aes;
        SHA256 sha = SHA256.Create();

        public void Create(string filePath, uint index, Aes aes, string[] addedFiles, string[] removedFiles,
            Guid guid, EventHandler<string> archivingEventHandler, EventHandler<ArchivingFailedEventArgs> archivingFailedHandler)
        {
            this.filePath = filePath;
            this.aes = aes;

            fileStream = File.Create(filePath);
            fileStream.Seek(BtaHeader.headerSize, SeekOrigin.Begin);
            BinaryWriter binaryWriter = new BinaryWriter(fileStream);


            byte[] removedFilesEncrypted;
            using (MemoryStream memStream = new MemoryStream())
            {
                using (BinaryWriter memBinaryWriter = new BinaryWriter(memStream))
                {
                    foreach (string removedFile in removedFiles)
                        memBinaryWriter.Write(removedFile);
                    memBinaryWriter.Flush();
                    removedFilesEncrypted = AesCoder.encodeBytes(memStream.ToArray(), aes.Key, aes.IV);
                }
            }
            
            foreach (string addedFile in addedFiles)
            {
                archivingEventHandler(this, addedFile);
                BinaryReader reader;
                try
                {
                    reader = new BinaryReader(File.OpenRead(addedFile));
                }
                catch(Exception ex)
                {
                    ArchivingFailedEventArgs args = new ArchivingFailedEventArgs();
                    args.fileName = addedFile;
                    args.message = ex.Message;
                    continue;
                }
                FileTableEntry entry = new FileTableEntry();
                entry.name = addedFile;
                entry.lastModified = DateTimeToEpochTime(File.GetLastWriteTimeUtc(addedFile));
                entry.uncompresedSize = (ulong)reader.BaseStream.Length;
                entry.offset = (ulong)binaryWriter.BaseStream.Position;
                fileTable.Add(entry);


                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    MemoryStream compressedMemStream = new MemoryStream();
                    GZipStream compressionStream = new GZipStream(compressedMemStream, CompressionLevel.Optimal);

                    byte[] plainTextData = reader.ReadBytes(1024 * 1024 * 64);
                    compressionStream.Write(plainTextData, 0, plainTextData.Length);
                    compressionStream.Dispose();
                    byte[] compressedPlainText = compressedMemStream.ToArray();
                    entry.compressedSize += (ulong)compressedPlainText.Length;
                    byte[] cypherData = AesCoder.encodeBytes(compressedPlainText, aes.Key, aes.IV);
                    entry.encryptedSize += (ulong)cypherData.Length;
                    binaryWriter.Write(cypherData);
                }
            }

            binaryWriter.Flush();

            byte[] encryptedFileTable;
            using (MemoryStream memStream = new MemoryStream())
            using (BinaryWriter memBinaryWriter = new BinaryWriter(memStream))
            {
                foreach (FileTableEntry entry in fileTable)
                {
                    memBinaryWriter.Write(entry.name);
                    memBinaryWriter.Write(entry.encryptedSize);
                    memBinaryWriter.Write(entry.compressedSize);
                    memBinaryWriter.Write(entry.uncompresedSize);
                    memBinaryWriter.Write(entry.lastModified);
                    memBinaryWriter.Write(entry.offset);
                }
                memBinaryWriter.Flush();
                encryptedFileTable = AesCoder.encodeBytes(memStream.ToArray(), aes.Key, aes.IV);
            }
            
            long fileTableOffset = binaryWriter.BaseStream.Position;
            binaryWriter.Write(encryptedFileTable);
            long removedFilesOffset = binaryWriter.BaseStream.Position;
            binaryWriter.Write(removedFilesEncrypted);


            header.guid = guid;
            header.epochTime = DateTimeToEpochTime(DateTime.UtcNow);
            header.index = index;
            header.version = 1;
            header.encrypted = true;
            header.aesKeyHash = GenerateKeyHash();
            header.removedFilesCount = removedFiles.Length;
            header.removedFilesEncryptedSize = removedFilesEncrypted.Length;
            header.removedFilesOffset = removedFilesOffset;
            header.fileTableEntryCount = fileTable.Count;
            header.fileTableEncryptedSize = encryptedFileTable.Length;
            header.fileTableOffset = fileTableOffset;

            binaryWriter.BaseStream.Seek(0, SeekOrigin.Begin);
            header.Write(binaryWriter, true);
            binaryWriter.Flush();
            binaryWriter.BaseStream.Seek(0, SeekOrigin.Begin);
            header.WriteMagic(binaryWriter);
            binaryWriter.Flush();
        }

        public void Open(string filePath, bool filetableOnly, Aes aes)
        {
            this.filePath = filePath;
            this.aes = aes;

            fileStream = File.Open(filePath, FileMode.Open);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            header.Read(binaryReader);

            byte[] keyHash = GenerateKeyHash();
            for(int i = 0; i < 32; i++)
            {
                if (keyHash[i] != header.aesKeyHash[i])
                    throw new IncorrectKeyException();
            }

            decryptRemovedFiles();
            decryptFiletable();
        }

        byte[] GenerateKeyHash()
        {
            byte[] combinedKey = aes.IV.Concat(aes.Key).ToArray();
            byte[] keyCheckValue = sha.ComputeHash(combinedKey);
            return keyCheckValue;
        }

        uint DateTimeToEpochTime(DateTime dateTime)
        {
            TimeSpan t = dateTime - new DateTime(1970, 1, 1);
            return ((uint)t.TotalMilliseconds);
        }

        void decryptRemovedFiles()
        {
            BinaryReader binaryReader = new BinaryReader(fileStream);
            binaryReader.BaseStream.Seek(header.removedFilesOffset, SeekOrigin.Begin);
            byte[] encryptedFileList = binaryReader.ReadBytes(header.removedFilesEncryptedSize);
            BinaryReader decryptReader = new BinaryReader(AesCoder.decodeBytes(encryptedFileList, aes.Key, aes.IV));
            for(int i = 0; i < header.removedFilesCount; i++)
                removedFiles.Add(decryptReader.ReadString());
        }

        void decryptFiletable()
        {
            BinaryReader binaryReader = new BinaryReader(fileStream);
            binaryReader.BaseStream.Seek(header.fileTableOffset, SeekOrigin.Begin);
            byte[] encryptedFileList = binaryReader.ReadBytes(header.fileTableEncryptedSize);
            BinaryReader decryptReader = new BinaryReader(AesCoder.decodeBytes(encryptedFileList, aes.Key, aes.IV));
            for(int i = 0; i < header.fileTableEntryCount; i++)
            {
                FileTableEntry entry = new FileTableEntry();
                entry.name = decryptReader.ReadString();
                entry.encryptedSize = decryptReader.ReadUInt64();
                entry.compressedSize = decryptReader.ReadUInt64();
                entry.uncompresedSize = decryptReader.ReadUInt64();
                entry.lastModified = decryptReader.ReadUInt64();
                entry.offset = decryptReader.ReadUInt64();
                fileTable.Add(entry);
            }
        }

        public void ExtractFile(string sourcePath, string destinationPath)
        {
            //Must be a multiple of 16 so we are sure not to decrypt a partial block
            ulong chunkSize = 16 * 1000000;
            foreach(FileTableEntry entry in fileTable)
            {
                if(entry.name == sourcePath)
                {
                    BinaryReader binaryReader = new BinaryReader(fileStream);
                    using (FileStream destFileStream = File.Create(destinationPath))
                    {
                        fileStream.Seek((long)entry.offset, SeekOrigin.Begin);
                        ulong bytesRead = 0;
                        while (bytesRead < entry.encryptedSize)
                        {
                            ulong bytesLeftToRead = entry.encryptedSize - bytesRead;
                            byte[] encryptedCompressedData;

                            if (bytesLeftToRead < chunkSize)
                            {
                                encryptedCompressedData = binaryReader.ReadBytes((int)bytesLeftToRead);
                                bytesRead += bytesLeftToRead;
                            }
                            else
                            {
                                encryptedCompressedData = binaryReader.ReadBytes((int)chunkSize);
                                bytesRead += chunkSize;
                            }

                            using (MemoryStream compressedStream = AesCoder.decodeBytes(encryptedCompressedData, aes.Key, aes.IV))
                            using (GZipStream decompressedStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                                decompressedStream.CopyTo(destFileStream);
                        }
                        return; 
                    }
                }
            }
            throw new FileNotFoundException("The specified file is not present in the archive");
        }

        public void Close()
        {
            try
            {
                fileStream.Flush();
                fileStream.Close();
            }
            catch(Exception)
            {

            }
        }
    }
}
