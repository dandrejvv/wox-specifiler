using System;
using System.IO;

namespace Wox.Plugin.Specifiler
{
    public class FileResult
    {
        public FileResult(string filePath)
        {
            FileName = Path.GetFileName(filePath);
            SearchText = FileName.ToLower();
            OriginalFilePath = filePath;
        }

        public string FileName { get; }
        public string SearchText { get; }
        public string OriginalFilePath { get; }
    }
}
