using com.hideakin.textsearch.data;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using com.hideakin.textsearch.service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace com.hideakin.textsearch
{
    enum CommandType
    {
        None,
        Help,
        Authenticate,
        Query,
        PrintExtensions,
        AddExtensions,
        ClearExtensions,
        PrintSkipDirs,
        AddSkipDirs,
        ClearSkipDirs,
        PrintUsers,
        PrintUserById,
        PrintUserByName,
        CreateUser,
        UpdateUser,
        DeleteUser,
        PrintGroups,
        CreateGroup,
        UpdateGroup,
        DeleteGroup,
        PrintFiles,
        PrintFileStats,
        IndexFiles,
        DeleteFiles
    }

    class Program
    {
        public static readonly string BAD_COMMAND_LINE_SYNTAX = "Bad command line syntax.";

        public string Name { get; }

        private Dictionary<string, Action<System.Collections.IEnumerator>> OptionMap { get; } = new Dictionary<string, Action<System.Collections.IEnumerator>>();

        private Dictionary<string, string> OptionAltMap { get; } = new Dictionary<string, string>();

        private CommandType commandType = CommandType.None;

        private Dictionary<CommandType, Action> CommandMap { get; } = new Dictionary<CommandType, Action>();

        private string groupName = "default";

        private List<string> OperandList { get; } = new List<string>();

        private UserService UserSvc { get; } = new UserService();

        private FileGroupService FileGrpSvc { get; } = new FileGroupService();

        private FileService FileSvc { get; } = new FileService();

        private IndexService IndexSvc { get; } = new IndexService();

        private PreferenceService PrefSvc { get; } = new PreferenceService();

        private List<string> extensions;

        private List<string> skipDirs;

        private bool formatHTML = false;

        private static readonly string DTFMT = "yyyy-MM-ddTHH:mm:ss.fff";

        public static int DebugLevel { get; set; } = 0;

        public Program()
        {
            Name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            OptionMap.Add("-help", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.Help;
            });
            OptionMap.Add("-authenticate", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.Authenticate;
            });
            OptionMap.Add("-group", (e) =>
            {
                if (!e.MoveNext())
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                groupName = (string)e.Current;
            });
            OptionMap.Add("-query", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.Query;
                if (!e.MoveNext())
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                OperandList.Add((string)e.Current);
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-print-extensions", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.PrintExtensions;
            });
            OptionMap.Add("-extensions", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.AddExtensions;
                if (!e.MoveNext())
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                OperandList.Add((string)e.Current);
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-clear-extensions", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.ClearExtensions;
            });
            OptionMap.Add("-print-skip-directories", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.PrintSkipDirs;
            });
            OptionMap.Add("-skip-directories", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.AddSkipDirs;
                if (!e.MoveNext())
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                OperandList.Add((string)e.Current);
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-clear-skip-directories", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.ClearSkipDirs;
            });
            OptionMap.Add("-print-users", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.PrintUsers;
            });
            OptionMap.Add("-print-user-by-id", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.PrintUserById;
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-print-user-by-name", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.PrintUserByName;
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-create-user", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.CreateUser;
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-update-user", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.UpdateUser;
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-delete-user", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.DeleteUser;
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-print-groups", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.PrintGroups;
            });
            OptionMap.Add("-create-group", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.CreateGroup;
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-update-group", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.UpdateGroup;
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-delete-group", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.DeleteGroup;
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-print-files", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.PrintFiles;
            });
            OptionMap.Add("-print-file-stats", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.PrintFileStats;
            });
            OptionMap.Add("-index-files", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.IndexFiles;
                if (!e.MoveNext())
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                OperandList.Add((string)e.Current);
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-delete-files", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.DeleteFiles;
            });
            OptionMap.Add("-index-api", (e) =>
            {
                if (!e.MoveNext())
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                IndexNetClient.Url = (string)e.Current;
            });
            OptionMap.Add("-username", (e) =>
            {
                if (!e.MoveNext())
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                IndexNetClient.Credentials.Username = (string)e.Current;
            });
            OptionMap.Add("-password", (e) =>
            {
                if (!e.MoveNext())
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                IndexNetClient.Credentials.Password = (string)e.Current;
            });
            OptionMap.Add("-debug", (e) =>
            {
                DebugLevel++;
            });
#if DEBUG
            OptionMap.Add("-debugger", (e) =>
            {
                System.Diagnostics.Debugger.Launch();
            });
#endif

            OptionAltMap.Add("-h", "-help");
            OptionAltMap.Add("-?", "-help");
            OptionAltMap.Add("-auth", "-authenticate");
            OptionAltMap.Add("-login", "-authenticate");
            OptionAltMap.Add("-pg", "-print-groups");
            OptionAltMap.Add("-print-grp", "-print-groups");
            OptionAltMap.Add("-pf", "-print-files");
            OptionAltMap.Add("-g", "-group");
            OptionAltMap.Add("-q", "-query");
            OptionAltMap.Add("-pe", "-print-extensions");
            OptionAltMap.Add("-print-ext", "-print-extensions");
            OptionAltMap.Add("-e", "-extensions");
            OptionAltMap.Add("-ext", "-extensions");
            OptionAltMap.Add("-ce", "-clear-extensions");
            OptionAltMap.Add("-clear-ext", "-clear-extensions");
            OptionAltMap.Add("-psd", "-print-skip-directories");
            OptionAltMap.Add("-print-skip-dir", "-print-skip-directories");
            OptionAltMap.Add("-sd", "-skip-directories");
            OptionAltMap.Add("-skip-dir", "-skip-directories");
            OptionAltMap.Add("-csd", "-clear-skip-directories");
            OptionAltMap.Add("-clear-skip-dir", "-clear-skip-directories");
            OptionAltMap.Add("-u", "-username");
            OptionAltMap.Add("-user", "-username");
            OptionAltMap.Add("-p", "-password");
            OptionAltMap.Add("-pass", "-password");
            OptionAltMap.Add("-i", "-index-files");
            OptionAltMap.Add("-index", "-index-files");

            CommandMap.Add(CommandType.None, Help);
            CommandMap.Add(CommandType.Help, Help);
            CommandMap.Add(CommandType.Authenticate, Authenticate);
            CommandMap.Add(CommandType.Query, Query);
            CommandMap.Add(CommandType.PrintExtensions, PrintExtensions);
            CommandMap.Add(CommandType.AddExtensions, AddExtensions);
            CommandMap.Add(CommandType.ClearExtensions, ClearExtensions);
            CommandMap.Add(CommandType.PrintSkipDirs, PrintSkipDirs);
            CommandMap.Add(CommandType.AddSkipDirs, AddSkipDirs);
            CommandMap.Add(CommandType.ClearSkipDirs, ClearSkipDirs);
            CommandMap.Add(CommandType.PrintUsers, PrintUsers);
            CommandMap.Add(CommandType.PrintUserById, PrintUserById);
            CommandMap.Add(CommandType.PrintUserByName, PrintUserByName);
            CommandMap.Add(CommandType.CreateUser, CreateUser);
            CommandMap.Add(CommandType.UpdateUser, UpdateUser);
            CommandMap.Add(CommandType.DeleteUser, DeleteUser);
            CommandMap.Add(CommandType.PrintGroups, PrintGroups);
            CommandMap.Add(CommandType.CreateGroup, CreateGroup);
            CommandMap.Add(CommandType.UpdateGroup, UpdateGroup);
            CommandMap.Add(CommandType.DeleteGroup, DeleteGroup);
            CommandMap.Add(CommandType.PrintFiles, PrintFiles);
            CommandMap.Add(CommandType.PrintFileStats, PrintFileStats);
            CommandMap.Add(CommandType.IndexFiles, IndexFiles);
            CommandMap.Add(CommandType.DeleteFiles, DeleteFiles);
        }

        public void ParseCommandLine(string[] args)
        {
            System.Collections.IEnumerator e = args.GetEnumerator();
            while (e.MoveNext())
            {
                string a = (string)e.Current;
                if (OptionAltMap.TryGetValue(a, out var a2))
                {
                    a = a2;
                }
                if (OptionMap.TryGetValue(a, out var action))
                {
                    action(e);
                }
                else
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
            }
        }

        public void Run()
        {
            CommandMap[commandType]();
        }

        private void Help()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  {0} -authenticate -username USERNAME -password PASSWORD", Name);
            Console.WriteLine("Usage <user>:");
            Console.WriteLine("  {0} -print-users", Name);
            Console.WriteLine("  {0} -print-user-by-id UID", Name);
            Console.WriteLine("  {0} -print-user-by-name USERNAME", Name);
            Console.WriteLine("  {0} -create-user USERNAME PASSWORD ROLE...", Name);
            Console.WriteLine("  {0} -update-user UID [-username USERNAME] [-password PASSWORD] [-roles ROLE...]", Name);
            Console.WriteLine("  {0} -delete-user UID", Name);
            Console.WriteLine("Usage <group>:");
            Console.WriteLine("  {0} -print-groups", Name);
            Console.WriteLine("  {0} -create-group GROUPNAME [USERNAME...]", Name);
            Console.WriteLine("  {0} -update-group GID [-group GROUPNAME] [-owned-by USERNAME...]", Name);
            Console.WriteLine("  {0} -delete-group GID", Name);
            Console.WriteLine("Usage <file>:");
            Console.WriteLine("  {0} -group GROUPNAME -print-files", Name);
            Console.WriteLine("  {0} -group GROUPNAME -print-file-stats", Name);
            Console.WriteLine("  {0} -group GROUPNAME -index-files PATH...", Name);
            Console.WriteLine("  {0} -group GROUPNAME -delete-files", Name);
            Console.WriteLine("Usage <search>:");
            Console.WriteLine("  {0} -group GROUPNAME -query EXPR [-html]", Name);
            Console.WriteLine("Usage <configuration>:");
            Console.WriteLine("  {0} -print-ext", Name);
            Console.WriteLine("  {0} -ext EXT1,EXT2,...", Name);
            Console.WriteLine("  {0} -clear-ext", Name);
            Console.WriteLine("  {0} -print-skip-dir", Name);
            Console.WriteLine("  {0} -skip-dir DIR1,DIR2,...", Name);
            Console.WriteLine("  {0} -clear-skip-dir", Name);
            Console.WriteLine("Options:");
            Console.WriteLine("  -index-api URL");
            Console.WriteLine("  -username USERNAME");
            Console.WriteLine("  -password PASSWORD");
        }

        #region USER

        private void Authenticate()
        {
            UserSvc.Authenticate();
        }

        private void PrintUsers()
        {
            var users = UserSvc.GetUsers();

            foreach (var entry in users.OrderBy(x => x.Uid))
            {
                if (entry.Expiry != null)
                {
                    Console.WriteLine("uid={0} username={1} roles={2} created={3} updated={4} expires={5} apikey={6}", entry.Uid, entry.Username, entry.RolesString, entry.CreatedAt.ToString(DTFMT), entry.UpdatedAt.ToString(DTFMT), entry.Expiry.Value.ToString(DTFMT), entry.ApiKey);
                }
                else
                {
                    Console.WriteLine("uid={0} username={1} roles={2} created={3} updated={4} expires= apikey=", entry.Uid, entry.Username, entry.RolesString, entry.CreatedAt.ToString(DTFMT), entry.UpdatedAt.ToString(DTFMT));
                }
            }
        }

        private void PrintUserById()
        {
            for (int index = 0; index < OperandList.Count; index++)
            {
                var entry = UserSvc.GetUser(int.Parse(OperandList[index]));
                if (entry.Expiry != null)
                {
                    Console.WriteLine("uid={0} username={1} roles={2} created={3} updated={4} expires={5} apikey={6}", entry.Uid, entry.Username, entry.RolesString, entry.CreatedAt.ToString(DTFMT), entry.UpdatedAt.ToString(DTFMT), entry.Expiry.Value.ToString(DTFMT), entry.ApiKey);
                }
                else
                {
                    Console.WriteLine("uid={0} username={1} roles={2} created={3} updated={4} expires= apikey=", entry.Uid, entry.Username, entry.RolesString, entry.CreatedAt.ToString(DTFMT), entry.UpdatedAt.ToString(DTFMT));
                }
            }
        }

        private void PrintUserByName()
        {
            for (int index = 0; index < OperandList.Count; index++)
            {
                var entry = UserSvc.GetUser(OperandList[index]);
                if (entry.Expiry != null)
                {
                    Console.WriteLine("uid={0} username={1} roles={2} created={3} updated={4} expires={5} apikey={6}", entry.Uid, entry.Username, entry.RolesString, entry.CreatedAt.ToString(DTFMT), entry.UpdatedAt.ToString(DTFMT), entry.Expiry.Value.ToString(DTFMT), entry.ApiKey);
                }
                else
                {
                    Console.WriteLine("uid={0} username={1} roles={2} created={3} updated={4} expires= apikey=", entry.Uid, entry.Username, entry.RolesString, entry.CreatedAt.ToString(DTFMT), entry.UpdatedAt.ToString(DTFMT));
                }
            }
        }

        private void CreateUser()
        {
            if (OperandList.Count < 3)
            {
                throw new Exception("Specify username, password, and one or more roles.");
            }
            var username = OperandList[0];
            var password = OperandList[1];
            var roles = new List<string>();
            roles.Add(OperandList[2]);
            for (int index = 3; index < OperandList.Count; index++)
            {
                roles.Add(OperandList[index]);
            }
            var entry = UserSvc.CreateUser(username, password, roles.ToArray());
            Console.WriteLine("uid={0} username={1} roles={2} created={3} updated={4}", entry.Uid, entry.Username, entry.RolesString, entry.CreatedAt.ToString(DTFMT), entry.UpdatedAt.ToString(DTFMT));
        }

        private void UpdateUser()
        {
            if (OperandList.Count < 1)
            {
                throw new Exception("Specify username to update at least.");
            }
            var uid = int.Parse(OperandList[0]);
            string username = null;
            string password = null;
            List<string> roles = null;
            for (int index = 1; index < OperandList.Count; index++)
            {
                if (roles != null)
                {
                    roles.Add(OperandList[index]);
                }
                else if (OperandList[index] == "-username")
                {
                    if (username != null)
                    {
                        throw new Exception("You can specify username only once.");
                    }
                    if (++index == OperandList.Count)
                    {
                        throw new Exception("Specify username after -username.");
                    }
                    username = OperandList[index];
                }
                else if (OperandList[index] == "-password")
                {
                    if (password != null)
                    {
                        throw new Exception("You can specify password only once.");
                    }
                    if (++index == OperandList.Count)
                    {
                        throw new Exception("Specify password after -password.");
                    }
                    password = OperandList[index];
                }
                else if (OperandList[index] == "-roles")
                {
                    roles = new List<string>();
                    if (++index == OperandList.Count)
                    {
                        throw new Exception("Specify one or more roles after -roles.");
                    }
                    roles.Add(OperandList[index]);
                }
                else
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
            }
            var entry = UserSvc.UpdateUser(uid, username, password, roles != null ? roles.ToArray() : null);
            Console.WriteLine("uid={0} username={1} roles={2} created={3} updated={4}", entry.Uid, entry.Username, entry.RolesString, entry.CreatedAt.ToString(DTFMT), entry.UpdatedAt.ToString(DTFMT));
        }

        private void DeleteUser()
        {
            if (OperandList.Count < 1)
            {
                throw new Exception("Specify username to delete.");
            }
            var uid = int.Parse(OperandList[0]);
            var entry = UserSvc.DeleteUser(uid);
            Console.WriteLine("uid={0} username={1} roles={2} created={3} updated={4}", entry.Uid, entry.Username, entry.RolesString, entry.CreatedAt.ToString(DTFMT), entry.UpdatedAt.ToString(DTFMT));
        }

        #endregion

        #region GROUP

        private void PrintGroups()
        {
            var values = FileGrpSvc.GetFileGroups();
            if (values == null)
            {
                return;
            }
            foreach (FileGroupInfo entry in values)
            {
                Console.WriteLine("{0}", entry);
            }
        }

        private void CreateGroup()
        {
            if (OperandList.Count < 1)
            {
                throw new Exception("Specify groupname and zero or more owner usernames.");
            }
            var groupname = OperandList[0];
            var ownedBy = new List<string>();
            for (int index = 1; index < OperandList.Count; index++)
            {
                ownedBy.Add(OperandList[index]);
            }
            var entry = FileGrpSvc.CreateFileGroup(groupname, ownedBy.ToArray());
            Console.WriteLine("{0}", entry);
        }

        private void UpdateGroup()
        {
            if (OperandList.Count < 1)
            {
                throw new Exception("Specify GID to update at least.");
            }
            var gid = int.Parse(OperandList[0]);
            string groupname = null;
            List<string> ownedBy = null;
            for (int index = 1; index < OperandList.Count; index++)
            {
                if (ownedBy != null)
                {
                    ownedBy.Add(OperandList[index]);
                }
                else if (OperandList[index] == "-group")
                {
                    if (groupname != null)
                    {
                        throw new Exception("You can specify groupname only once.");
                    }
                    if (++index == OperandList.Count)
                    {
                        throw new Exception("Specify groupname after -group.");
                    }
                    groupname = OperandList[index];
                }
                else if (OperandList[index] == "-owned-by")
                {
                    ownedBy = new List<string>();
                    if (++index == OperandList.Count)
                    {
                        throw new Exception("Specify one or more roles after -owned-by.");
                    }
                    ownedBy.Add(OperandList[index]);
                }
                else
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
            }
            var entry = FileGrpSvc.UpdateFileGroup(gid, groupname, ownedBy != null ? ownedBy.ToArray() : null);
            Console.WriteLine("{0}", entry);
        }

        private void DeleteGroup()
        {
            if (OperandList.Count < 1)
            {
                throw new Exception("Specify GID of the group to delete.");
            }
            else if(OperandList.Count > 1)
            {
                throw new Exception(BAD_COMMAND_LINE_SYNTAX);
            }
            var gid = int.Parse(OperandList[0]);
            var entry = FileGrpSvc.DeleteFileGroup(gid);
            Console.WriteLine("{0}", entry);
        }

        #endregion

        #region FILE

        private void PrintFiles()
        {
            var fi = FileSvc.GetFiles(groupName);
            if (fi == null)
            {
                return;
            }
            foreach (var entry in fi.OrderBy(x => x.Fid))
            {
                Console.WriteLine("[{0}] {1}", entry.Fid, entry.Path);
            }
        }

        private void PrintFileStats()
        {
            var stats = FileSvc.GetFileStats(groupName);
            Console.WriteLine("{0}", stats);
        }

        private void IndexFiles()
        {
            Console.WriteLine("Started indexing...");
            extensions = PrefSvc.GetExtensions();
            skipDirs = PrefSvc.GetSkipDirs();
            foreach (string path in OperandList)
            {
                if (Directory.Exists(path))
                {
                    if (skipDirs.Contains(Path.GetFileName(path)))
                    {
                        Console.WriteLine("Skipping {0}...", path);
                    }
                    else
                    {
                        IndexDir(path);
                    }
                }
                else if (File.Exists(path))
                {
                    if (extensions.Count == 0 || extensions.Contains(Path.GetExtension(path)))
                    {
                        IndexFile(path);
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
        }

        private void IndexDir(string path)
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
                    IndexDir(dir);
                }
            }
            var files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                if (extensions.Count == 0 || extensions.Contains(Path.GetExtension(file)))
                {
                    IndexFile(file);
                }
                else
                {
                    Console.WriteLine("Skipping {0}...", file);
                }
            }
        }

        private void IndexFile(string path)
        {
            path = Path.GetFullPath(path);
            Console.WriteLine("{0}", path);
            var fileInfo = FileSvc.UploadFile(groupName, path, out var result);
            Console.WriteLine("    Uploaded. FID={0}{1}", fileInfo.Fid, result == UploadFileStatus.Created ? " (NEW)" : "");
        }

        public void DeleteFiles()
        {
            Console.WriteLine("Started deleting files...");
            FileSvc.DeleteFiles(groupName);
            Console.WriteLine("Done.");
        }

        #endregion

        #region SEARCH

        private void Query()
        {
            var sb = new StringBuilder();
            foreach (var s in OperandList)
            {
                if (s == "-html")
                {
                    formatHTML = true;
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(' ');
                    }
                    sb.Append(s);
                }
            }
            var rsp = IndexSvc.FindText(groupName, sb.ToString());
            if (formatHTML)
            {
                FormatQueryResultsInHtml(rsp);
            }
            else
            {
                FormatQueryResults(rsp);
            }
        }

        private void FormatQueryResults(PathRowColumns[] results)
        {
            foreach (var prc in results)
            {
                Console.WriteLine("{0}", prc.Path);
                var lines = FileSvc.DownloadFile(prc.Fid);
                foreach (var entry in prc.Rows)
                {
                    Console.WriteLine("{0,6}: {1}", entry.Row + 1, lines[entry.Row]);
                }
            }
        }

        private void FormatQueryResultsInHtml(PathRowColumns[] results)
        {
            var sb = new StringBuilder();
            Console.WriteLine("<!doctype html>");
            Console.WriteLine("<html>");
            Console.WriteLine("<head>");
            Console.WriteLine("<style>");
            Console.WriteLine("table { border-collapse:collapse; border-width:thin; border-style:solid; width:100%; }");
            Console.WriteLine("tr td { border-width:thin; border-style:solid; }");
            Console.WriteLine("td.lineno { width:6em; text-align:right; background-color:lightgray; }");
            Console.WriteLine("font.path { color:darkgreen; }");
            Console.WriteLine("font.match { color:red; }");
            Console.WriteLine("</style>");
            Console.WriteLine("</head>");
            Console.WriteLine("<body>");
            foreach (var prc in results)
            {
                Console.WriteLine("<p>");
                Console.WriteLine("<font class=\"path\">{0}</font>", prc.Path);
                Console.WriteLine("<table>");
                var lines = FileSvc.DownloadFile(prc.Fid);
                foreach (var entry in prc.Rows)
                {
                    var line = lines[entry.Row];
                    sb.Length = 0;
                    int u = 0;
                    int v = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (u < entry.Columns.Count && i == entry.Columns[u].Start)
                        {
                            if (u++ == v)
                            {
                                sb.Append("<font class=\"match\">");
                            }
                        }
                        if (v < entry.Columns.Count && i == entry.Columns[v].End)
                        {
                            if (++v == u)
                            {
                                sb.Append("</font>");
                            }
                        }
                        char c = line[i];
                        if (c == '&') sb.Append("&amp;");
                        else if (c == '<') sb.Append("&lt;");
                        else if (c == '>') sb.Append("&gt;");
                        else sb.Append(c);
                    }
                    if (v < u)
                    {
                        sb.Append("</font>");
                    }
                    Console.WriteLine("<tr><td class=\"lineno\">{0}</td><td>{1}</td></tr>", entry.Row + 1, sb.ToString());
                }
                Console.WriteLine("</table>");
                Console.WriteLine("</p>");
            }
            Console.WriteLine("</body>");
            Console.WriteLine("</html>");
        }

        #endregion

        #region CONFIGURATION

        private void PrintExtensions()
        {
            var list = PrefSvc.GetExtensions();
            foreach (string ext in list)
            {
                Console.WriteLine("{0}", ext);
            }
        }

        private void AddExtensions()
        {
            Console.WriteLine("Adding extensions setting...");
            var sb = new StringBuilder();
            foreach (string s in OperandList)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append(s);
            }
            PrefSvc.AddExtensions(sb.ToString());
            Console.WriteLine("Done.");
        }

        private void ClearExtensions()
        {
            Console.WriteLine("Clearing extensions setting...");
            PrefSvc.ClearExtensions();
            Console.WriteLine("Done.");
        }

        private void PrintSkipDirs()
        {
            var list = PrefSvc.GetSkipDirs();
            foreach (string dir in list)
            {
                Console.WriteLine("{0}", dir);
            }
        }

        private void AddSkipDirs()
        {
            Console.WriteLine("Adding skip-dirs setting...");
            var sb = new StringBuilder();
            foreach (string s in OperandList)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append(s);
            }
            PrefSvc.AddSkipDirs(sb.ToString());
            Console.WriteLine("Done.");
        }

        private void ClearSkipDirs()
        {
            Console.WriteLine("Clearing skip-dirs setting...");
            PrefSvc.ClearSkipDirs();
            Console.WriteLine("Done.");
        }

        #endregion

        static void Main(string[] args)
        {
            try 
            {
                var app = new Program();
                app.ParseCommandLine(args);
                app.Run();
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("ERROR: {0}", e.Message);
                for (e = e.InnerException; e != null; e = e.InnerException)
                {
                    Console.Error.WriteLine("\t{0}", e.Message);
                }
            }
            Environment.Exit(1);
        }
    }
}
