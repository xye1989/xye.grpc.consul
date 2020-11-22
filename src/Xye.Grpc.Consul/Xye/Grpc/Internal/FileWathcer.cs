#if !NETCORE
using Xye.Grpc.Configuration;
using System;
using System.IO;

namespace Xye.Grpc.Internal
{
    internal class FileWathcer
    {
        /// <summary>
        /// 监控文件
        /// </summary>
        /// <param name="fullPath">带目录的文件名</param>
        /// <param name="actCallback">回调</param>
        public static void Monitor(string fullPath, Action<string, string, Exception> callback)
        {
            if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
            {
                var fileName = Path.GetFileName(fullPath); //文件名
                var dir = Path.GetDirectoryName(fullPath); //所在目录

                var fsw = new FileSystemWatcher(dir, fileName);
                fsw.NotifyFilter = NotifyFilters.LastWrite;
                fsw.Changed += new FileSystemEventHandler((o, e) =>
                {
                    var watcher = o as FileSystemWatcher;
                    if (watcher.EnableRaisingEvents)
                    {
                        watcher.EnableRaisingEvents = false;
                        callback(FileChangeTypeConst.CHANGED, e.FullPath, null);
                        watcher.EnableRaisingEvents = true;
                    }
                });
                fsw.Created += new FileSystemEventHandler((o, e) =>
                 {
                     var watcher = o as FileSystemWatcher;
                     watcher.EnableRaisingEvents = false;
                     callback(FileChangeTypeConst.CREATED, e.FullPath, null);
                     watcher.EnableRaisingEvents = true;
                });
                fsw.Renamed += new RenamedEventHandler((o, e) =>
                {
                    var watcher = o as FileSystemWatcher;
                    watcher.EnableRaisingEvents = false;
                    callback(FileChangeTypeConst.RENAMED, e.FullPath, null);
                    watcher.EnableRaisingEvents = true;
                });
                fsw.Deleted += new FileSystemEventHandler((o, f) =>
                {
                    callback(FileChangeTypeConst.DELETED, f.FullPath, null);
                });
                fsw.Error += new ErrorEventHandler((o, e) =>
                {
                    callback(FileChangeTypeConst.ERROR, fullPath, e.GetException());
                });
                fsw.EnableRaisingEvents = true;
            }
        }
    }
}
#endif