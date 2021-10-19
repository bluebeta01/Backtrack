using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace BackTrack
{
    class Manifest
    {
        public Dictionary<string, uint> tracked_files { get; set; } = new Dictionary<string, uint>();

        public static Manifest loadManifest(string filePath)
        {
            filePath += "/manifest.json";
            try
            {
                string rawJson = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<Manifest>(rawJson);
            }
            catch (Exception e)
            {
                return new Manifest();
            }
        }

        public void Save(string directory)
        {
            string path = directory + "/manifest.json";
            string rawJson = JsonConvert.SerializeObject(this, Formatting.Indented);
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllText(path, rawJson);
        }
    }
}
