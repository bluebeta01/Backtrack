using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using BackTrackArchiver;

namespace BackTrack
{
    class Archiver
    {
        Configuration configuration;
        Manifest manifest;
        Manifest newManifest = new Manifest();
        uint epochTime;
        Archive archive;
        List<string> filesToArchive = new List<string>();
        List<string> removedFiles = new List<string>();

        private void FindFilesToArchive(string directory, string rootDirectory)
        {
            string s1 = Path.GetFullPath(directory);
            foreach (string blacklistedDir in configuration.blacklisted_directories)
            {
                string s2 = Path.GetFullPath(rootDirectory + "/" + blacklistedDir);
                if (s1 == s2)
                    return;
            }

            string[] dirs = Directory.GetDirectories(directory);
            foreach (string dir in dirs)
                FindFilesToArchive(dir, rootDirectory);

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

                if (shouldArchive)
                    filesToArchive.Add(filename);
            }
        }

        public Archiver(Configuration configuration, Manifest manifest, Archive archive)
        {
            this.configuration = configuration;
            this.manifest = manifest;
            this.archive = archive;
        }

        public Manifest ArchiveDirectories()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            epochTime = (uint)t.TotalMilliseconds;

            foreach (string dir in configuration.tracked_directories)
                FindFilesToArchive(dir, dir);

            if (filesToArchive.Count == 0)
                return manifest;

            foreach (KeyValuePair<string, uint> pair in manifest.tracked_files)
                if (!File.Exists(pair.Key))
                    removedFiles.Add(pair.Key);

            archive.ArchiveFiles(filesToArchive.ToArray(), removedFiles.ToArray());

            return newManifest;
        }
    }
}
