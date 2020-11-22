#if !NETCORE
namespace Xye.Grpc.Configuration
{
    internal class FileChangeTypeConst
    {
        /// <summary>
        /// 文件创建
        /// </summary>
        public const string CREATED = "Created";

        /// <summary>
        /// 文件删除
        /// </summary>
        public const string DELETED = "Deleted";

        /// <summary>
        /// 文件改变
        /// </summary>
        public const string CHANGED = "Changed";

        /// <summary>
        /// 文件重命名
        /// </summary>
        public const string RENAMED = "Renamed";

        /// <summary>
        /// 文件操作发生错误
        /// </summary>
        public const string ERROR = "Error";
    }
}
#endif