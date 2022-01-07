using System;
using System.Security.Cryptography;
using BackTrackArchiver;

using log4net;
using log4net.Repository.Hierarchy;
using log4net.Core;
using log4net.Appender;
using log4net.Layout;

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
            ConfigureLogging();

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

        static void ConfigureLogging()
        {
            //log4net.Config.BasicConfigurator.Configure();
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            FileAppender fileAppender = new FileAppender();
            fileAppender.AppendToFile = true;
            fileAppender.Encoding = System.Text.Encoding.UTF8;
            fileAppender.File = configuration.log_directory + '\\' + "log.txt";
            fileAppender.Layout = patternLayout;
            fileAppender.ActivateOptions();
            hierarchy.Root.AddAppender(fileAppender);

            hierarchy.Root.Level = Level.Debug;
            hierarchy.Configured = true;
        }

        private static void OnArchivingFile(object sender, string file)
        {
            ILog log = LogManager.GetLogger("archiver");
            log.Info($"Archiving File: {file}");
        }
    }
}
