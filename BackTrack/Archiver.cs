using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;

namespace BackTrack
{
    class Archiver
    {
        Configuration configuration;
        Manifest manifest;
        Manifest newManifest = new Manifest();
        ZipArchive zipArchive;
        uint epochTime;
        string zipArchiveTempPath;

        private bool ArchiveFiles(string directory)
        {
            bool fileHasBeenArchived = false;

            string[] dirs = Directory.GetDirectories(directory);
            foreach (string dir in dirs)
                fileHasBeenArchived |= ArchiveFiles(dir);

            string[] filenames = Directory.GetFiles(directory);
            foreach(string filename in filenames)
            {
                uint fileDate = (uint)(File.GetLastWriteTimeUtc(filename) - new DateTime(1970, 1, 1)).TotalMilliseconds;
                newManifest.tracked_files[filename] = fileDate;

                bool shouldArchive = false;
                if (manifest.tracked_files.ContainsKey(filename))
                {
                    if (fileDate > manifest.tracked_files[filename])
                        shouldArchive = true;
                }
                else
                    shouldArchive = true;

                if(shouldArchive)
                {
                    zipArchive.CreateEntryFromFile(filename, filename);
                    fileHasBeenArchived = true;
                }
            }

            return fileHasBeenArchived;
        }

        private void SaveArchive()
        {
            File.Move(zipArchiveTempPath, configuration.archive_directory + "/" + epochTime + ".zip");
        }

        public Archiver(Configuration configuration, Manifest manifest)
        {
            this.configuration = configuration;
            this.manifest = manifest;
        }

        public Manifest ArchiveDirectories()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            epochTime = (uint)t.TotalMilliseconds;

            try
            {
                zipArchiveTempPath = configuration.archive_temp_directory + "/archive.temp";
                if (File.Exists(zipArchiveTempPath))
                    File.Delete(zipArchiveTempPath);
                zipArchive = ZipFile.Open(zipArchiveTempPath, ZipArchiveMode.Create);
            }
            catch (Exception e)
            {

            }

            bool fileArchived = false;
            foreach (string directory in configuration.tracked_directories)
            {
                fileArchived |= ArchiveFiles(directory);
            }

            zipArchive.Dispose();

            if (!fileArchived)
                File.Delete(zipArchiveTempPath);
            else
                SaveArchive();

            return newManifest;
        }
    }
}
