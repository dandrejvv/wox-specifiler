using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Wox.Plugin.Specifiler
{
    public class Settings
    {
        [JsonProperty]
        public List<FolderLink> FolderLinks { get; set; } = new List<FolderLink>();

        [JsonProperty]
        public List<string> Extensions { get; set; } = new List<string>();
    }
}
