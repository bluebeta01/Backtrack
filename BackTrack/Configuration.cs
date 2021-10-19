﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace BackTrack
{
    class Configuration
    {
        public string manifest_directory { get; set; } = "./";
        public string archive_directory { get; set; } = "./";
        public string archive_temp_directory { get; set; } = "./";
        public string log_directory { get; set; } = "./";
        public List<string> tracked_directories { get; set; } = new List<string>();

        public static Configuration loadConfiguration(string filePath)
        {
            try
            {
                string rawJson = File.ReadAllText(filePath);
                Configuration config = JsonConvert.DeserializeObject<Configuration>(rawJson);
                return config;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}