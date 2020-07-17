using com.hideakin.textsearch.data;
using com.hideakin.textsearch.exception;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.service;
using com.hideakin.textsearch.utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace com.hideakin.textsearch.command
{
    internal class FileCommand : ICommand
    {
        private static readonly string DEFAULT_GROUP = "default";

        private readonly CancellationTokenSource cts;
        private readonly FileService file;
        private readonly PreferenceService pref;
        private model.FileInfo[] alreadyUploaded;
        private int concurrencyLevel = 8;

        public FileCommand()
        {
            cts = new CancellationTokenSource();
            file = new FileService(cts.Token);
            pref = new PreferenceService(cts.Token);
        }

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
                        var fi = file.GetFiles(group);
                        foreach (var entry in fi.OrderBy(x => x.Fid))
                        {
                            Console.WriteLine("[{0}] {1}", entry.Fid, entry.Path);
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
                        var stats = file.GetFileStats(group);
                        Console.WriteLine("{0}", stats);
                    });
                })
                .AddHandler("-print-file", (e) =>
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
                    var path = (string)e.Current;
                    commandQueue.Add(() =>
                    {
                        var info = file.GetFile(group, path);
                        var contents = FileContents.Find(info.Fid);
                        if (contents == null)
                        {
                            contents = file.DownloadFile(info.Fid);
                        }
                        foreach (var line in contents.Lines)
                        {
                            Console.WriteLine(line);
                        }
                    });
                })
                .AddHandler("-index-files", (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("Group name is not specified.");
                    }
                    var forceIndexing = false;
                    var commandLine2 = new CommandLine();
                    commandLine2
                        .AddHandler("-force", (ee) =>
                        {
                            forceIndexing = true;
                        })
                        .AddHandler("-concurrency", (ee) => 
                        {
                            if (!e.MoveNext())
                            {
                                throw new Exception("Concurrency level number is not specified.");
                            }
                            concurrencyLevel = int.Parse((string)ee.Current);
                            if (concurrencyLevel < 1 || 255 < concurrencyLevel)
                            {
                                throw new Exception("Concurrency level number is out of the valid range.");
                            }
                        })
                        .AddTranslation("-f", "-force")
                        .AddTranslation("-c", "-concurrency");
                    e = commandLine2.Parse(e);
                    if (e == null)
                    {
                        throw new Exception("Group name is not specified.");
                    }
                    var group = (string)e.Current;
                    if (!e.MoveNext())
                    {
                        throw new Exception("Path is not specified.");
                    }
                    List<string> paths = new List<string>() { (string)e.Current };
                    while (e.MoveNext())
                    {
                        paths.Add((string)e.Current);
                    }
                    commandLine.NoMoreArg = true;
                    commandQueue.Add(() =>
                    {
                        IndexFiles(group, paths, forceIndexing);
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
                        file.DeleteFiles(group);
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
                        file.DeleteStaleFiles(group);
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
                .AddUsage("{0} -print-file GROUPNAME PATH", Program.Name)
                .AddUsage("{0} -index-files [-force] [-concurrency NUMBER] GROUPNAME PATH...", Program.Name)
                .AddUsage("{0} -delete-files [GROUPNAME]", Program.Name)
                .AddUsage("{0} -delete-stale-files [GROUPNAME]", Program.Name);
        }

        private void IndexFiles(string group, List<string> paths, bool forceIndexing)
        {
            var t1 = DateTime.Now;
            Console.WriteLine("Started indexing...");
            alreadyUploaded = forceIndexing ? new model.FileInfo[0] : file.GetFiles(group);
            var extensions = pref.GetExtensions();
            var skipDirs = pref.GetSkipDirs();
            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    if (skipDirs.Contains(Path.GetFileName(path)))
                    {
                        Console.WriteLine("Skipping {0}...", path);
                    }
                    else if (!IndexDir(group, path, extensions, skipDirs))
                    {
                        break;
                    }
                }
                else if (File.Exists(path))
                {
                    if (extensions.Count > 0 && !extensions.Contains(Path.GetExtension(path)))
                    {
                        Console.WriteLine("Skipping {0}...", path);
                    }
                    else if (!IndexFile(group, path))
                    {
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: Unable to find '{0}'.", path);
                }
            }
            while (file.Uploading > 0)
            {
                WaitForIndexFileCompletion();
            }
            var t2 = DateTime.Now;
            Console.WriteLine("Done. Elapsed time: {0}", t2 - t1);
        }

        private bool IndexDir(string group, string path, List<string> extensions, List<string> skipDirs)
        {
            var dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                if (skipDirs.Contains(Path.GetFileName(dir)))
                {
                    Console.WriteLine("Skipping {0}...", dir);
                }
                else if (!IndexDir(group, dir, extensions, skipDirs))
                {
                    return false;
                }
            }
            var files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                if (extensions.Count > 0 && !extensions.Contains(Path.GetExtension(file)))
                {
                    Console.WriteLine("Skipping {0}...", file);
                }
                else if (!IndexFile(group, file))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IndexFile(string group, string path)
        {
            path = Path.GetFullPath(path).NormalizePath();
            var fileInfo = alreadyUploaded.Where(x => x.Path == path).Select(x => x).FirstOrDefault();
            if (fileInfo != null)
            {
                Console.WriteLine("{0}", path);
                Console.WriteLine("    Already exists. FID={0}", fileInfo.Fid);
            }
            else
            {
                if (file.Uploading >= concurrencyLevel)
                {
                    if (!WaitForIndexFileCompletion())
                    {
                        return false;
                    }
                }
                file.UploadFileAsync(group, path);
            }
            return true;
        }

        private bool WaitForIndexFileCompletion()
        {
            try
            {
                var fileInfo = file.WaitForUploadFileCompletion(out var result);
                Console.WriteLine("{0}", fileInfo.Path);
                Console.WriteLine("    Uploaded. FID={0}{1}", fileInfo.Fid, result == UploadFileStatus.Created ? " (NEW)" : "");
                return true;
            }
            catch (UploadFileException e)
            {
                if (e.InnerException is ErrorResponseException er)
                {
                    Console.WriteLine("ERROR: Failed to upload '{0}'.", e.Path);
                    Console.WriteLine("\t{0}", er.ErrorResponse.ErrorDescription);
                    return false;
                }
                else if (e.InnerException is GroupNotFoundException g)
                {
                    Console.WriteLine("ERROR: {0}", g.Message);
                    return false;
                }
                else
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
                for (e = e.InnerException; e != null; e = e.InnerException)
                {
                    Console.WriteLine("\t{0}", e.Message);
                }
                return true;
            }
        }
    }
}
