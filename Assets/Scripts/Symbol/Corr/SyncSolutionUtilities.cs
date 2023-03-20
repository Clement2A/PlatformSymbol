using System.Reflection;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Reflection.Emit;
using System.Linq;

public class SyncSolutionUtilities
{
    private static Type syncVsType = null;
    private static MethodInfo syncSolutionMethod = null;
    private static FieldInfo synchroniserField = null;
    private static object synchronizerObject = null;
    public static Type synchronizerType = null;
    private static MethodInfo synchronizerSyncMethodInfo = null;

    static SyncSolutionUtilities()
    {
        syncVsType = Type.GetType("UnityEditor.SyncVs,UnityEditor");
        synchroniserField = syncVsType?.GetField("Synchronizer", BindingFlags.NonPublic | BindingFlags.Static);
        syncSolutionMethod = syncVsType?.GetMethod("SyncSolution", BindingFlags.Public | BindingFlags.Static);

        synchronizerObject = synchroniserField?.GetValue(syncVsType);
        synchronizerType = synchronizerObject?.GetType();
        synchronizerSyncMethodInfo = synchronizerType?.GetMethod("Sync", BindingFlags.Public | BindingFlags.Instance);
    }

    public static void Sync(bool _logs)
    {
        CleanOldFiles(_logs);
        if(_logs)
            Debug.Log("Call method: SyncVs.Sync()\nCall method: SyncVs.Synchronizer.Sync()");
        syncSolutionMethod?.Invoke(null, null);
        synchronizerSyncMethodInfo.Invoke(synchronizerObject, null);
    }

    private static void CleanOldFiles(bool _logs)
    {
        DirectoryInfo _assetsDirectoryInfo = new DirectoryInfo(Application.dataPath);
        DirectoryInfo _projectDirectoryInfo = _assetsDirectoryInfo.Parent;

        IEnumerable<FileInfo> _files = GetFilesByExtensions(_projectDirectoryInfo, "*.sln", "*csproj");
        foreach(FileInfo _file in _files)
        {
            if (_logs)
                Debug.Log($"Remove old solution file: {_file.Name}");
            _file.Delete();
        }
    }

    private static IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo _directory, params string[] _extensions)
    {
        _extensions ??= new[] { "*" };
        IEnumerable<FileInfo> _files = new List<FileInfo>();
        return _extensions.Aggregate(_files, (_current, _ext) => _current.Concat(_directory.GetFiles(_ext)));
    }
}
