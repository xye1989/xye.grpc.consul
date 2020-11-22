#if !NETCORE
using Newtonsoft.Json;
using System;
using System.IO;
using System.Web;
using System.Xml.Serialization;

namespace Xye.Grpc.Internal
{
    internal class ConfigHelper
    {
        /// <summary>
        /// 加载文件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static object LoadFile(Type type, string fileName)
        {
            if (fileName.EndsWith("json", StringComparison.InvariantCultureIgnoreCase))
            {
                return LoadJson(type, fileName);
            }

            return LoadXml(type, fileName);
        }

        /// <summary>
        /// 加载JSON
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static object LoadJson(Type type, string fileName)
        {
            StreamReader file = null;
            JsonTextReader reader = null;
            try
            {
                file = System.IO.File.OpenText(fileName);
                reader = new JsonTextReader(file);
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize(reader, type);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// 加载XML
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static object LoadXml(Type type, string fileName)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                XmlSerializer serializer = new XmlSerializer(type);
                return serializer.Deserialize(fs);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// 获得当前绝对路径
        /// </summary>
        /// <param name="path">指定的路径</param>
        /// <returns>绝对路径</returns>
        public static string GetFullPath(string path)
        {
            if (HttpContext.Current != null)
            {
                path = path.Trim().TrimStart(new char[] { '~', '/' });
                path = HttpContext.Current.Server.MapPath("~/" + path);
                if (!File.Exists(path))
                {
                    path = HttpContext.Current.Server.MapPath("/" + path);
                }
            }
            else 
            {
                path = path?.Replace("/", "\\") ?? "";
                if (path.StartsWith("\\"))
                {
                    path = path.Substring(path.IndexOf('\\', 0)).TrimStart('\\');
                }
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            } //非web程序引用

            if (!File.Exists(path))
            {
                CommonUtilsHelper._.LoggerWriter(string.Format("配置模块中，没有正确的{0}配置文件！", path), null);
            }
            return path;
        }
    }
}
#endif