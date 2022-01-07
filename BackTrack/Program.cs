using System;
using System.Security.Cryptography;
using BackTrackArchiver;

namespace BackTrack
{
    class Program
    {
        static Configuration configuration;
        static Manifest manifest;
        static Archiver archiver;

        static void Main(string[] args)
        {
            Arguments.parse(args);
            configuration = Configuration.loadConfiguration(Arguments.configFilePath);
            manifest = Manifest.loadManifest(configuration.manifest_directory);

            if (configuration.aes_key == "" || configuration.aes_iv == "")
                throw new Exception("Invalid AES key or IV");
            byte[] key = Convert.FromBase64String(configuration.aes_key);
            byte[] iv = Convert.FromBase64String(configuration.aes_iv);
            Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            Archive archive = new Archive(configuration.archive_directory, aes);
            archive.archivingEvent += OnArchivingFile;
            archiver = new Archiver(configuration, manifest, archive);
            Manifest newManifest = archiver.ArchiveDirectories();
            newManifest.Save(configuration.manifest_directory);
        }

        private static void OnArchivingFile(object sender, string file)
        {
            Console.WriteLine("Archiving File: " + file);
        }
    }
}
