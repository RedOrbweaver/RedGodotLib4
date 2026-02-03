using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using Expression = System.Linq.Expressions.Expression;
using System.Threading.Tasks;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#if GODOT
using Godot;
public static partial class Utils
{
    public static bool FileExists(string path)
    {
        return FileAccess.FileExists(path);
    }
    public static bool IsFile(string path)
    {
        bool isf = FileAccess.FileExists(path);
        if(isf)
            return true;
        if(DirAccess.DirExistsAbsolute(path))
            return false;
        throw new Exception($"file/directory {path} does not exist");
    }
    public static bool IsDirectory(string path)
    {
        bool isd =DirAccess.DirExistsAbsolute(path);
        if(isd)
            return true;
        if(FileAccess.FileExists(path))
            return false;
        throw new Exception($"file/directory {path} does not exist");
    }
    public static string ReadFile(string path)
    {
        Assert(FileExists(path));
        FileAccess fa;
        fa = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        Assert(fa.GetError() == Error.Ok);
        Assert(fa.IsOpen());
        string dt = fa.GetAsText();
        fa.Close();
        return dt;
    }
    public static byte[] ReadFileBytes(string path)
    {
        Assert(FileExists(path));
        FileAccess fm;
        fm = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        Assert(fm.GetError() == Error.Ok);
        Assert(fm.IsOpen());
        byte[] dt = fm.GetBuffer((long)fm.GetLength());
        fm.Close();
        return dt;
    }
    public static void WriteFile(string path, string data)
    {
        Assert(data != null);
        Console.WriteLine("Writing to: " + path);
        FileAccess fm;
        fm = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        Assert(fm.GetError() == Error.Ok);
        Assert(fm.IsOpen());
        fm.StoreString(data);
        fm.Close();
    }
    public static void WriteFile(string path, byte[] data)
    {
        Assert(data != null);
        Console.WriteLine("Writing to: " + path);
        FileAccess fm = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        Assert(fm.GetError() == Error.Ok);
        Assert(fm.IsOpen());
        fm.StoreBuffer(data);
        fm.Close();
    }
    public static bool DirectoryExists(string path)
    {
        return DirAccess.DirExistsAbsolute(path);
    }
    public static void DeleteFile(string path)
    {
        Assert(FileExists(path));
        Assert(DirAccess.RemoveAbsolute(path) == Error.Ok);
    }
    public static void DeleteDirectory(string path)
    {
        Assert(DirectoryExists(path));
        Assert(DirAccess.RemoveAbsolute(path));
    }
    // Highly inefficient, especially for complex directory structures
    public static void DeleteDirectoryRecursively(string path) 
    {
        var filesanddirs = ListDirectoryContentsRecursively(path);
        List<string> directories = new List<string>();
        foreach(var it in filesanddirs)
        {
            if(IsDirectory(it))
                directories.Add(it);
            else
                DeleteFile(it);
        }
        while(directories.Count > 0)
        {
            var dircp = directories.ToList();
            directories.Clear();
            foreach(var it in directories)
            {
                if(DirAccess.RemoveAbsolute(it) != Error.Ok)
                {
                    directories.Add(it);
                }
            }
        }
    }
    public static string EnsurePathExists(string path)
    {
        Assert(!FileExists(path), "This function is meant for directories, not files");
        if (DirectoryExists(path))
            return "";
        string dif = "";
        List<string> parts = new List<string>();
        while (!DirectoryExists(path))
        {
            string subpath = "";
            if (path.Last() == '/')
                path = path.Remove(path.Length - 1);
            while (path.Last() != '/')
            {
                subpath += path.Last();
                path = path.Remove(path.Length - 1);
            }
            parts.Add(new string(subpath.Reverse().ToArray()));
        }
        parts.Reverse();
        foreach (var it in parts)
        {
            path = ConcatPaths(path, it);
            DirAccess.MakeDirAbsolute(path);
            dif = ConcatPaths(dif, it);
        }
        return dif;
    }
    public static List<string> ListDirectoryContentsRecursively(string path, Func<string, bool, bool> filter) // path, isfile -> bool
    {
        List<string> ret = new List<string>();
        Assert(DirAccess.DirExistsAbsolute(path));
        DirAccess dm = DirAccess.Open(path);
        void ListRecursively(string path)
        {
            foreach (var it in ListDirectoryContents(path))
            {
                if (dm.DirExists(it))
                {
                    ListRecursively(it);
                    if(filter(it, false))
                    {
                        ret.Add(it);
                    }
                }
                else
                {
                    Assert(dm.FileExists(it));
                    if (filter(it, true))
                    {
                        ret.Add(it);
                    }
                }
            }
        }
        ListRecursively(path);
        return ret;
    }
    public static List<string> ListDirectoryContentsRecursively(string path)
    {
        return ListDirectoryContentsRecursively(path, (s, b) => true);
    }
    public static List<string> ListDirectoryFilesRecursively(string path, Func<string, bool> filter)
    {
        return ListDirectoryContentsRecursively(path, (s, b) => b && filter(s));
    }
    public static List<string> ListDirectoryFilesRecursively(string path)
    {
        return ListDirectoryFilesRecursively(path, s => true);
    }
    public static List<string> ListDirectoryContents(string path, Func<string, bool> filter, bool relativepaths = false, bool skiphidden = true)
    {
        List<string> ret = new List<string>(8);
        DirAccess dm = DirAccess.Open(path);
        dm.IncludeHidden = !skiphidden;
        dm.ListDirBegin();
        while (true)
        {
            string s = dm.GetNext();
            if (s == "")
                break;
            if (filter(ConcatPaths(path, s)))
                ret.Add(relativepaths ? s : ConcatPaths(path, s));
        }
        dm.ListDirEnd();
        return ret;
    }
    public static List<string> ListDirectoryContents(string path, bool relativepaths = false, bool skiphidden = true)
    {
        return ListDirectoryContents(path, s => true, relativepaths, skiphidden);
    }
    public static List<string> ListDirectoryFiles(string path, bool relativepaths = false, bool skiphidden = true)
    {
        return ListDirectoryContents(path, IsFile, relativepaths, skiphidden);
    }
    public static List<string> ListDirectorySubDirs(string path, bool relativepaths = false, bool skiphidden = true)
    {
        return ListDirectoryContents(path, IsDirectory, relativepaths, skiphidden);
    }
    public static string GetRealUserDirectory(string path)
    {
        Assert(path.StartsWith("user://"));
        var end = path.Replace("user://", "");
        return ConcatPaths(OS.GetUserDataDir(), end);
    }

}
#endif