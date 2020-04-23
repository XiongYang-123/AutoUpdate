using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateServer
{
    public class ZipFloClass
    {
        /// <summary>
        /// 压缩单个文件
        /// </summary>
        /// <param name="filePath">被压缩的文件名称(包含文件路径)，文件的全路径</param>
        /// <param name="zipedFileName">压缩后的文件名称(包含文件路径)，保存的文件名称</param>
        /// <param name="compressionLevel">压缩率0（无压缩）到 9（压缩率最高）</param>
        public static void ZipFile(string filePath, string zipedFileName, int compressionLevel = 9)
        {
            // 如果文件没有找到，则报错 
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("文件：" + filePath + "没有找到！");
            }
            // 如果压缩后名字为空就默认使用源文件名称作为压缩文件名称
            if (string.IsNullOrEmpty(zipedFileName))
            {
                string oldValue = Path.GetFileName(filePath);
                if (oldValue != null)
                {
                    zipedFileName = filePath.Replace(oldValue, "") + Path.GetFileNameWithoutExtension(filePath) + ".zip";
                }
            }
            // 如果压缩后的文件名称后缀名不是zip，就是加上zip，防止是一个乱码文件
            if (Path.GetExtension(zipedFileName) != ".zip")
            {
                zipedFileName = zipedFileName + ".zip";
            }
            string zipedDir = zipedFileName.Substring(0, zipedFileName.LastIndexOf("\\", StringComparison.Ordinal));
            if (!Directory.Exists(zipedDir))
            {
                Directory.CreateDirectory(zipedDir);
            }
            // 被压缩文件名称
            string filename = filePath.Substring(filePath.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            var streamToZip = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var zipFile = File.Create(zipedFileName);
            var zipStream = new ZipOutputStream(zipFile);
            var zipEntry = new ZipEntry(filename);
            zipStream.PutNextEntry(zipEntry);
            zipStream.SetLevel(compressionLevel);
            var buffer = new byte[2048];
            Int32 size = streamToZip.Read(buffer, 0, buffer.Length);
            zipStream.Write(buffer, 0, size);
            try
            {
                while (size < streamToZip.Length)
                {
                    int sizeRead = streamToZip.Read(buffer, 0, buffer.Length);
                    zipStream.Write(buffer, 0, sizeRead);
                    size += sizeRead;
                }
            }
            finally
            {
                zipStream.Finish();
                zipStream.Close();
                streamToZip.Close();
            }
        }

        /// <summary>  
        /// 功能：解压zip格式的文件。  
        /// </summary>  
        /// <param name="zipFilePath">压缩文件路径</param>  
        /// <param name="unZipDir">解压文件存放路径,为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹</param>  
        /// <returns>解压是否成功</returns>  
        public static void UnZip(string zipFilePath, string unZipDir)
        {
            if (zipFilePath == string.Empty)
            {
                throw new Exception("压缩文件不能为空！");
            }
            if (!File.Exists(zipFilePath))
            {
                throw new FileNotFoundException("压缩文件不存在！");
            }
            //解压文件夹为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹  
            if (unZipDir == string.Empty)
                unZipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));
            if (!unZipDir.EndsWith("/"))
                unZipDir += "/";
            if (!Directory.Exists(unZipDir))
                Directory.CreateDirectory(unZipDir);

            using (var s = new ZipInputStream(File.OpenRead(zipFilePath)))
            {

                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);
                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        Directory.CreateDirectory(unZipDir + directoryName);
                    }
                    if (directoryName != null && !directoryName.EndsWith("/"))
                    {
                    }
                    if (fileName != String.Empty)
                    {
                        using (FileStream streamWriter = File.Create(unZipDir + theEntry.Name))
                        {

                            int size;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }


    }
}
