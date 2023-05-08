/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public static class FileHelper
{
    public static Stream GetResource(string name)
    {
        var execResources = Assembly.GetExecutingAssembly().GetManifestResourceNames().Distinct().ToList();
        var execResource = execResources.FirstOrDefault(x => x.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        if (execResource != null)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(execResource);
            return stream;
        }

        var entryResources = Assembly.GetEntryAssembly()?.GetManifestResourceNames();
        var entryResource = entryResources?.FirstOrDefault(x => x.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        if (entryResource != null)
        {
            var stream = Assembly.GetEntryAssembly()?.GetManifestResourceStream(entryResource);
            return stream;
        }

        var commonResources = Assembly.GetAssembly(typeof(FileHelper))?.GetManifestResourceNames();
        var commonResource = commonResources?.FirstOrDefault(x => x.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        if (commonResource != null)
        {
            var stream = Assembly.GetEntryAssembly()?.GetManifestResourceStream(commonResource);
            return stream;
        }

        var fileStream = OpenFile(FindFile(name));
        if (fileStream != null)
        {
            return fileStream;
        }

        return null;
    }

    public static bool ResourceExists(string name)
    {
        var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        var resource = resources.FirstOrDefault(x => x.EndsWith(name));
        return resource != null;
    }

    public static Stream OpenFile(string dirPath, string fileName)
    {
        if (string.IsNullOrEmpty(dirPath))
        {
            throw new ArgumentNullException(nameof(dirPath));
        }

        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        var filePath = dirPath.TrimEnd('/') + "/" + fileName.TrimStart('/');
        return OpenFile(filePath);
    }

    public static Stream OpenFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {

            Debug.WriteLine($"OpenFile: {filePath} not found.");
            return null;
        }

        return File.OpenRead(filePath);
    }

    public static string FindFile(string fileName)
    {
        var fi = new FileInfo(fileName);
        if (fi.Exists)
        {
            return fi.FullName;
        }

        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        foreach (var di in currentDir.GetDirectories())
        {
            var file = FindFile(fileName, di);
            if (!fileName.Equals(file))
            {
                return file;
            }
        }

        return fileName;
    }

    public static string FindFile(string fileName, DirectoryInfo directory)
    {
        if (directory == null)
        {
            return fileName;
        }

        var files = directory.GetFiles();
        foreach (var f in files)
        {
            if (f.Name == fileName)
            {
                return f.FullName;
            }
        }

        var dirs = directory.GetDirectories();
        foreach (var dir in dirs)
        {
            var tmp = FindFile(fileName, dir);
            if (!fileName.Equals(tmp))
            {
                return tmp;
            }
        }

        return fileName;
    }
}