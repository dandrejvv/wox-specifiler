﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wox.Infrastructure.Storage;

namespace Wox.Plugin.Specifiler
{
    public class FileLookupService : IPlugin, ISettingProvider, ISavable
    {
        private const string ImagePath = "images/copy.png";

        private PluginInitContext _context;
        private readonly Settings _settings;
        private readonly PluginJsonStorage<Settings> _storage;
        private readonly List<FileResult> _cachedFiles = new List<FileResult>();
        private DateTime _cacheExpiry = DateTime.Now;
        private volatile bool _isReady;

        public FileLookupService()
        {
            _storage = new PluginJsonStorage<Settings>();
            _settings = _storage.Load();
        }

        public void Init(PluginInitContext context)
        {
            _context = context;
            Task.Run(() => PrepareFileCache());
        }

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();
            string search = query.Search.ToLower();

            if (!string.IsNullOrWhiteSpace(search))
            {
                if (_isReady)
                {
                    PrepareFileCache();

                    foreach (var file in FilterFilesFromCache(search))
                    {
                        results.Add(new Result
                        {
                            Title = file.FileName,
                            SubTitle = file.OriginalFilePath,
                            IcoPath = ImagePath,
                            Action = c =>
                            {
                                try
                                {
                                    if (Path.GetExtension(file.OriginalFilePath) == ".ps1")
                                    {
                                        var procInfo = new ProcessStartInfo
                                        {
                                            FileName = "Powershell.exe",
                                            Arguments = file.OriginalFilePath,
                                            WorkingDirectory = Path.GetDirectoryName(file.OriginalFilePath)
                                        };
                                        Process.Start(procInfo);
                                    }
                                    else
                                    {
                                        Process.Start(file.OriginalFilePath);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message, "Could not open " + file);
                                }

                                return true;
                            }
                        });
                    }
                }
                else
                {
                    results.Add(new Result
                    {
                        Title = "Specifiler not ready",
                        IcoPath = ImagePath
                    });
                }
            }

            return results;
        }

        public Control CreateSettingPanel()
        {
            return new SpecifilerPluginSettings(_context.API, _settings);
        }

        public void Save()
        {
            _storage.Save();
        }

        private void PrepareFileCache()
        {
            if (_cacheExpiry < DateTime.Now)
            {
                _cacheExpiry = DateTime.Now.AddMinutes(1);

                _cachedFiles.Clear();

                var allowAll = _settings.Extensions == null || _settings.Extensions.Count == 0;
                var extensionLookup = new HashSet<string>(_settings.Extensions.Select(s => s.Replace("*", string.Empty)));

                foreach (var folderLink in _settings.FolderLinks)
                {
                    _cachedFiles.AddRange(Directory.GetFiles(
                            folderLink.Path,
                            "*.*",
                            SearchOption.AllDirectories)
                        .Where(p => allowAll || extensionLookup.Contains(Path.GetExtension(p)?.ToLower()))
                        .Select(s => new FileResult(s)));
                }
            }

            _isReady = true;
        }

        private IReadOnlyCollection<FileResult> FilterFilesFromCache(string search)
        {
            if (!_isReady)
            {
                return new List<FileResult>();
            }
            return _cachedFiles.Where(p => p.SearchText.StartsWith(search)).ToList();
        }
    }
}
