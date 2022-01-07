using System;
using System.IO;

namespace BackTrackArchiver
{
    class BtaHeader
    {
        public static readonly uint sMagic = 0x46415442;
        public static readonly int headerSize = 101;
        public uint magic;                      //Magic number
        public Guid guid = Guid.NewGuid();      //Guid to identify the archive
        public ulong epochTime;                 //Time the archive was created
        public uint index;                      //Index of the file in the archive
        public uint version;                    //Version of the BTA file standard
        public bool encrypted;                  //If the BTA file is encrypted
        public byte[] aesKeyHash = new byte[32];//Hash of the AES key used in encryption
        public int removedFilesCount;           //Number of removed files
        public int removedFilesEncryptedSize;   //Size of the removed files list in bytes (0 if not encrypted)
        public long removedFilesOffset;          //Offset to the start of the removed files list
        public int fileTableEntryCount;         //Number of entries in the filetable
        public int fileTableEncryptedSize;      //Size in bytes of the encrypted filetable (0 if not encrypted)
        public long fileTableOffset;             //Offset to the start of the filetable

        public void Read(BinaryReader reader)
        {
            magic = reader.ReadUInt32();
            
            if(magic != sMagic)
                throw new ArchiveInvalidException();

            guid = new Guid(reader.ReadBytes(16));
            epochTime = reader.ReadUInt64();
            index = reader.ReadUInt32();
            version = reader.ReadUInt32();
            encrypted = reader.ReadBoolean();
            aesKeyHash = reader.ReadBytes(32);
            removedFilesCount = reader.ReadInt32();
            removedFilesEncryptedSize = reader.ReadInt32();
            removedFilesOffset = reader.ReadInt64();
            fileTableEntryCount = reader.ReadInt32();
            fileTableEncryptedSize = reader.ReadInt32();
            fileTableOffset = reader.ReadInt64();
        }

        public void Write(BinaryWriter writer, bool skipMagic)
        {
            if (!skipMagic)
                writer.Write(sMagic);
            else
                writer.Write((uint)0);
            writer.Write(guid.ToByteArray());
            writer.Write(epochTime);
            writer.Write(index);
            writer.Write(version);
            writer.Write(encrypted);
            writer.Write(aesKeyHash);
            writer.Write(removedFilesCount);
            writer.Write(removedFilesEncryptedSize);
            writer.Write(removedFilesOffset);
            writer.Write(fileTableEntryCount);
            writer.Write(fileTableEncryptedSize);
            writer.Write(fileTableOffset);
        }

        public void WriteMagic(BinaryWriter writer)
        {
            writer.Write(sMagic);
        }

        public bool Validate()
        {
            return magic == sMagic;
        }
    }
}
