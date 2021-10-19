using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            archiver = new Archiver(configuration, manifest);
            Manifest newManifest = archiver.ArchiveDirectories();
            newManifest.Save(configuration.manifest_directory);
        }
    }
}
