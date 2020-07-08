using com.hideakin.textsearch.data;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace com.hideakin.textsearch.command
{
    internal class FileCommand : ICommand
    {
        private static readonly string DEFAULT_GROUP = "default";

        private FileService FileSvc { get; } = new FileService();

        private PreferenceService PrefSvc { get; } = new PreferenceService();

        private model.FileInfo[] FileInfoArray { get; set; }

        private List<(string Path, Exception Exception)> IndexErrors { get; } = new List<(string Path, Exception Exception)>();

        public void Register(CommandLine commandLine, CommandQueue commandQueue)
        {
            commandLine
                .AddHandler("-print-files", (e) =>
                {
                    string group = DEFAULT_GROUP;
                    if (e.MoveNext())
                    {
                        group = (string)e.Current;
                    }
                    else
                    {
                        commandLine.NoMoreArg = true;
                    }
                    commandQueue.Add(() =>
                    {
                        var fi = FileSvc.GetFiles(group);
                        if (fi != null)
                        {
                            foreach (var entry in fi.OrderBy(x => x.Fid))
                            {
                                Console.WriteLine("[{0}] {1}", entry.Fid, entry.Path);
                            }
                        }
                    });
                })
                .AddHandler("-print-file-stats", (e) =>
                {
                    string group = DEFAULT_GROUP;
                    if (e.MoveNext())
                    {
                        group = (string)e.Current;
                    }
                    else
                    {
                        commandLine.NoMoreArg = true;
                    }
                    commandQueue.Add(() =>
                    {
                        var stats = FileSvc.GetFileStats(group);
                        Console.WriteLine("{0}", stats);
                    });
                })
                .AddHandler("-index-files", (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("Group name is not specified.");
                    }
                    var group = (string)e.Current;
                    if (!e.MoveNext())
                    {
                        throw new Exception("Path is not specified.");
                    }
                    List<string> paths = new List<string>();
                    paths.Add((string)e.Current);
                    while (e.MoveNext())
                    {
                        paths.Add((string)e.Current);
                    }
                    commandLine.NoMoreArg = true;
                    commandQueue.Add(() =>
                    {
                        IndexFiles(group, paths);
                    });
                })
                .AddHandler("-delete-files", (e) =>
                {
                    string group = DEFAULT_GROUP;
                    if (e.MoveNext())
                    {
                        group = (string)e.Current;
                    }
                    else
                    {
                        commandLine.NoMoreArg = true;
                    }
                    commandQueue.Add(() =>
                    {
                        Console.WriteLine("Started deleting files...");
                        FileSvc.DeleteFiles(group);
                        Console.WriteLine("Done.");
                    });
                })
                .AddHandler("-delete-stale-files", (e) =>
                {
                    string group = DEFAULT_GROUP;
                    if (e.MoveNext())
                    {
                        group = (string)e.Current;
                    }
                    else
                    {
                        commandLine.NoMoreArg = true;
                    }
                    commandQueue.Add(() =>
                    {
                        Console.WriteLine("Started deleting stale files...");
                        FileSvc.DeleteStaleFiles(group);
                        Console.WriteLine("Done.");
                    });
                })
                .AddTranslation("-pf", "-print-files")
                .AddTranslation("-pfs", "-print-file-stats")
                .AddTranslation("-i", "-index-files")
                .AddTranslation("-index", "-index-files")
                .AddUsageHeader("Usage <file>:")
                .AddUsage("{0} -print-files [GROUPNAME]", Program.Name)
                .AddUsage("{0} -print-file-stats [GROUPNAME]", Program.Name)
                .AddUsage("{0} -index-files GROUPNAME PATH...", Program.Name)
                .AddUsage("{0} -delete-files [GROUPNAME]", Program.Name)
                .AddUsage("{0} -delete-stale-files [GROUPNAME]", Program.Name);
        }

        private void IndexFiles(string group, List<string> paths)
        {
            Console.WriteLine("Started indexing...");
            IndexErrors.Clear();
            FileInfoArray = FileSvc.GetFiles(group);
            var extensions = PrefSvc.GetExtensions();
            var skipDirs = PrefSvc.GetSkipDirs();
            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    if (skipDirs.Contains(Path.GetFileName(path)))
                    {
                        Console.WriteLine("Skipping {0}...", path);
                    }
                    else
                    {
                        IndexDir(group, path, extensions, skipDirs);
                    }
                }
                else if (File.Exists(path))
                {
                    if (extensions.Count == 0 || extensions.Contains(Path.GetExtension(path)))
                    {
                        IndexFile(group, path);
                    }
                    else
                    {
                        Console.WriteLine("Skipping {0}...", path);
                    }
                }
                else
                {
                    Console.WriteLine("Unable to find {0}...", path);
                }
            }
            Console.WriteLine("Done.");
            int errorCount = IndexErrors.Count;
            if (errorCount > 0)
            {
                Console.WriteLine("Errors: {0}", errorCount);
                Console.WriteLine("Attempting to retry indexing...");
                for (int index = 0; index < errorCount; index++)
                {
                    IndexFile(group, IndexErrors[index].Path);
                }
                Console.WriteLine("Done.");
                int errorCount2 = IndexErrors.Count - errorCount;
                if (errorCount2 > 0)
                {
                    Console.WriteLine("Errors: {0}", errorCount2);
                    for (int index = errorCount; index < IndexErrors.Count; index++)
                    {
                        Console.WriteLine("{0}", IndexErrors[index].Path);
                        for (var e = IndexErrors[index].Exception; e != null; e = e.InnerException)
                        {
                            Console.WriteLine("\t{0}", e.Message);
                        }
                    }
                }
            }
        }

        private void IndexDir(string group, string path, List<string> extensions, List<string> skipDirs)
        {
            var dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                if (skipDirs.Contains(Path.GetFileName(dir)))
                {
                    Console.WriteLine("Skipping {0}...", dir);
                }
                else
                {
                    IndexDir(group, dir, extensions, skipDirs);
                }
            }
            var files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                if (extensions.Count == 0 || extensions.Contains(Path.GetExtension(file)))
                {
                    IndexFile(group, file);
                }
                else
                {
                    Console.WriteLine("Skipping {0}...", file);
                }
            }
        }

        private void IndexFile(string group, string path)
        {
            try
            {
                path = Path.GetFullPath(path);
                Console.WriteLine("{0}", path);
                var fileInfo = FileInfoArray.Where(x => x.Path == path).Select(x => x).FirstOrDefault();
                if (fileInfo != null)
                {
                    Console.WriteLine("    Already exists. FID={0}", fileInfo.Fid);
                }
                else
                {
                    fileInfo = FileSvc.UploadFile(group, path, out var result);
                    Console.WriteLine("    Uploaded. FID={0}{1}", fileInfo.Fid, result == UploadFileStatus.Created ? " (NEW)" : "");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
                for (e = e.InnerException; e != null; e = e.InnerException)
                {
                    Console.WriteLine("\t{0}", e.Message);
                }
                IndexErrors.Add((path, e));
            }
        }
    }
}
