using System.Threading.Tasks.Dataflow;
using System;
using Microsoft.AspNetCore.Mvc;
using Daemon.Model;
using Microsoft.AspNetCore.Http;
using Daemon.Common;
using Daemon.Common.Filter;
using System.IO;
using System.Text;
using Daemon.Repository.Contract;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net;
namespace Daemon.Controllers.V1
{
    [ApiController]
    [Route("blueearth/upload")]
    public class UploadController : ControllerBase
    {

        private readonly IBlogSysInfoRepository _blogSysInfoRepository;

        private readonly IBlogFileRepository _blogFileRepository;

        private readonly IConfiguration _configuration;

        public UploadController(IBlogSysInfoRepository blogSysInfoRepository, IBlogFileRepository blogFileRepository, IConfiguration configuration)
        {
            _blogSysInfoRepository = blogSysInfoRepository;
            _blogFileRepository = blogFileRepository;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("")]
        [DisableRequestSizeLimit]
        public ActionResult Upload(IFormFileCollection files)
        {
            files = Request.Form.Files;
            var path = GetFileAddress()?.InfoValue;
            List<BlogFile> addFiles = new List<BlogFile>();
            for (var i = 0; i < files.Count; i++)
            {
                BlogFile blogFile = new BlogFile() { IsDelete = 0, AddTime = DateTime.Now };
                var currentFile = files[i];
                var contentType = currentFile.ContentType;
                var filenameMine = currentFile.FileName.Split('\\');
                blogFile.Name = currentFile.FileName;
                string[] fileNameArray = filenameMine[filenameMine.Length - 1].Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                blogFile.Ext = fileNameArray[fileNameArray.Length - 1];
                // fileBasicModel.ExtName = fileNameArray[fileNameArray.Length - 1];
                //存放当前文件的地址  年月日\\文件格式\\文件
                var dirName = $"{DateTime.Now.ToString("yyyyMMdd")}\\{fileNameArray[fileNameArray.Length - 1]}";
                var currentFilePath = $"{path}\\{dirName}";
                //根据当前日期创建文件夹
                DirFile.CreateDir($"{currentFilePath}");
                string finalPath = Path.Combine(currentFilePath, currentFile.FileName);
                blogFile.FilePath = finalPath;
                blogFile.FileVersionId = Guid.NewGuid().ToString();
                SaveFileToDir(finalPath, GetByte(currentFile));
                //将保存的文件访问地址返回到前端
                var serverFilePath = $"http://{_configuration.GetSection("AppSettings:ip")?.Value}:{_configuration.GetSection("AppSettings:port")?.Value}\\Daemon\\{dirName}\\{currentFile.FileName}";
                blogFile.ServerFilePath = serverFilePath;
                addFiles.Add(blogFile);
            }
            return new ResultModel(HttpStatusCode.OK, "上传成功！", SaveFile(addFiles));
        }

        private List<BlogFile> SaveFile(List<BlogFile> files)
        {
            var addFiles = _blogFileRepository.AddRangeWithRelationships(files);
            return addFiles.ToList();
        }

        private void SaveFileToDir(string filePath, byte[] buffer)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Write(buffer, 0, buffer.Length);
                //文件大小
                // string fileMax = System.Math.Ceiling(fs.Length / 1024.0) + "kb";
            }
        }

        /// <summary>
        /// 获取保存文件的磁盘路径
        /// </summary>
        /// <returns></returns>
        private BlogSysInfo GetFileAddress()
        {
            var sysInfo = _blogSysInfoRepository.FindAll().FirstOrDefault(r => r.InfoId == "fileaddress");
            return sysInfo;
        }

        protected byte[] GetByte(IFormFile file)
        {
            byte[] bytes = new byte[file.Length];

            using (var stream = file.OpenReadStream())
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    reader.BaseStream.Read(bytes, 0, bytes.Length);
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                }
            }
            return bytes;
        }
    }
}
