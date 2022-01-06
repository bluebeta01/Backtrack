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
        public readonly uint magic = 0x46415442;
        public Guid guid  = Guid.NewGuid();
        public ulong epochTime = 0;
        public uint index = 0;
        public uint version = 1;
        public List<string> addedFiles = new List<string>();
        public List<string> removedFiles = new List<string>();
        public List<FileTableEntry> fileTable = new List<FileTableEntry>();
        public string filePath;
        public FileStream fileStream;
        public Aes aes;
        SHA256 sha = SHA256.Create();

        public void Create(string filePath, uint index, Aes aes, string[] addedFiles, string[] removedFiles)
        {
            this.filePath = filePath;
            this.index = index;
            this.aes = aes;

            fileStream = File.Create(filePath);
            BinaryWriter binaryWriter = new BinaryWriter(fileStream);
            binaryWriter.Write(magic);
            binaryWriter.Write(guid.ToByteArray());
            binaryWriter.Write(epochTime);
            binaryWriter.Write(index);
            binaryWriter.Write(version);

            byte[] combinedKey = aes.IV.Concat(aes.Key).ToArray();
            byte[] keyCheckValue = sha.ComputeHash(combinedKey);
            binaryWriter.Write(keyCheckValue);


            MemoryStream memStream = new MemoryStream();
            BinaryWriter memBinaryWriter = new BinaryWriter(memStream);
            foreach (string removedFile in removedFiles)
                memBinaryWriter.Write(removedFile);
            memBinaryWriter.Flush();
            byte[] removedFilesEncrypted = AesCoder.encodeBytes(memStream.ToArray(), aes.Key, aes.IV);


         

            //Calculate the size that the encrypted filetable will be
            int encryptedFileTableSize = 0;
            foreach (string addedFile in addedFiles)
                encryptedFileTableSize += addedFile.Length;
            //Every string is prepended with an int to specify its length
            encryptedFileTableSize += sizeof(uint) * addedFiles.Length;

            encryptedFileTableSize += sizeof(ulong) * 5 * addedFiles.Length;
            //Added padding so that the size is a multiple of 16 since it will be encrypted
            encryptedFileTableSize += 16 - (encryptedFileTableSize % 16);

            binaryWriter.Write(removedFilesEncrypted.Length);
            binaryWriter.Write(removedFiles.Length);
            binaryWriter.Write(encryptedFileTableSize);
            binaryWriter.Write(addedFiles.Length);
            binaryWriter.Write(removedFilesEncrypted);
            long filetablePosition = binaryWriter.BaseStream.Position;
            binaryWriter.BaseStream.Seek(filetablePosition + encryptedFileTableSize, SeekOrigin.Begin);

            foreach(string addedFile in addedFiles)
            {
                Console.WriteLine("Archving File: " + addedFile);
                BinaryReader reader = new BinaryReader(File.OpenRead(addedFile));
                FileTableEntry entry = new FileTableEntry();
                entry.name = addedFile;
                entry.encryptedSize = 0;
                entry.compressedSize = 0;
                entry.uncompresedSize = (ulong)reader.BaseStream.Length;
                entry.lastModified = 0;
                entry.offset = (ulong)binaryWriter.BaseStream.Position;
                fileTable.Add(entry);

                

                while(reader.BaseStream.Position < reader.BaseStream.Length)
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

            memStream = new MemoryStream();
            memBinaryWriter = new BinaryWriter(memStream);
            foreach(FileTableEntry entry in fileTable)
            {
                memBinaryWriter.Write(entry.name);
                memBinaryWriter.Write(entry.encryptedSize);
                memBinaryWriter.Write(entry.compressedSize);
                memBinaryWriter.Write(entry.uncompresedSize);
                memBinaryWriter.Write(entry.lastModified);
                memBinaryWriter.Write(entry.offset);
            }
            memBinaryWriter.Flush();
            byte[] encryptedFileTable = AesCoder.encodeBytes(memStream.ToArray(), aes.Key, aes.IV);
            binaryWriter.BaseStream.Seek(filetablePosition, SeekOrigin.Begin);
            binaryWriter.Write(encryptedFileTable);
        }

        public void Open(string filePath, bool filetableOnly, Aes aes)
        {
            this.filePath = filePath;
            this.aes = aes;

            fileStream = File.Open(filePath, FileMode.Open);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            uint magic = binaryReader.ReadUInt32();
            if (magic != this.magic)
                throw new Exception("Not a valid archive file");
            guid = new Guid(binaryReader.ReadBytes(16));
            epochTime = binaryReader.ReadUInt64();
            index = binaryReader.ReadUInt32();
            version = binaryReader.ReadUInt32();

            byte[] readKeyCheckValue = binaryReader.ReadBytes(32);
            byte[] keyCheckValue = sha.ComputeHash(aes.IV.Concat(aes.Key).ToArray());
            for(int i = 0; i < 32; i++)
            {
                if (readKeyCheckValue[i] != keyCheckValue[i])
                    throw new Exception("Bad encryption key");
            }

            int removedFilesLength = binaryReader.ReadInt32();
            int removedFilesCount = binaryReader.ReadInt32();
            int fileTableLength = binaryReader.ReadInt32();
            int fileTableCount = binaryReader.ReadInt32();

            decryptRemovedFiles(removedFilesLength, removedFilesCount);
            decryptFiletable(fileTableLength, fileTableCount);
        }

        void decryptRemovedFiles(int length, int count)
        {
            BinaryReader binaryReader = new BinaryReader(fileStream);
            byte[] encryptedFileList = binaryReader.ReadBytes((int)length);
            BinaryReader decryptReader = new BinaryReader(AesCoder.decodeBytes(encryptedFileList, aes.Key, aes.IV));
            for(int i = 0; i < count; i++)
                removedFiles.Add(decryptReader.ReadString());
        }

        void decryptFiletable(int length, int count)
        {
            BinaryReader binaryReader = new BinaryReader(fileStream);
            byte[] encryptedFileList = binaryReader.ReadBytes((int)length);
            BinaryReader decryptReader = new BinaryReader(AesCoder.decodeBytes(encryptedFileList, aes.Key, aes.IV));
            for(int i = 0; i < count; i++)
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

        public void extractFile(string sourcePath, string destinationPath)
        {
            //Must be a multiple of 16 so we are sure not to decrypt a partial block
            ulong chunkSize = 16 * 1000000;
            foreach(FileTableEntry entry in fileTable)
            {
                if(entry.name == sourcePath)
                {
                    BinaryReader binaryReader = new BinaryReader(fileStream);
                    FileStream destFileStream = File.Create(destinationPath);
                    fileStream.Seek((long)entry.offset, SeekOrigin.Begin);
                    ulong bytesRead = 0;
                    while(bytesRead < entry.encryptedSize)
                    {
                        MemoryStream encryptedCompressedStream = new MemoryStream();
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

                        MemoryStream compressedStream = AesCoder.decodeBytes(encryptedCompressedData, aes.Key, aes.IV);
                        GZipStream decompressedStream = new GZipStream(compressedStream, CompressionMode.Decompress);
                        decompressedStream.CopyTo(destFileStream);
                        decompressedStream.Dispose();
                    }
                    return;
                }
            }
            throw new FileNotFoundException("The specified file is not present in the archive");
        }

        public void Close()
        {
            fileStream.Flush();
            fileStream.Close();
        }
    }
}
