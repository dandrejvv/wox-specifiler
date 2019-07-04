using Newtonsoft.Json;
using System;
using System.Linq;

namespace Wox.Plugin.Specifiler
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FolderLink
    {
        [JsonProperty]
        public string Path { get; set; }
    }
}
