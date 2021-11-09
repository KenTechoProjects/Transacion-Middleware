using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Core.DTO;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;

namespace Middleware.Service.Utilities
{
    public class FileSystemImageManager : IImageManager
    {
        private readonly SystemSettings _settings;
        private IConfiguration _configuration;
        //private const string SELFIE_SURFIX = "_PIC";
        //private const string ID_SURFIX = "_ID";
        //private const string SIGNATURE_SURFIX = "_SIGN";
        //private const string UTILITY_BILL_SURFIX = "_UBILL";
        private readonly ILogger _log;
        private bool documentExists = false;
        public bool DocumentExists { get => documentExists; set => value = documentExists; }

        public FileSystemImageManager(IOptions<SystemSettings> settingsProvider, IConfiguration configuration, ILoggerFactory log)
        {
            _settings = settingsProvider.Value;
            _configuration = configuration;
            _log = log.CreateLogger(typeof(FileSystemImageManager));
        }

        public async Task<string> GetImage(string identifier, DocumentType documentType)
        {
            try
            {
                _log.LogInformation(" Inside the  GetImage of the FileSystemImageManager class at {0} ", DateTime.UtcNow);

                var file = $"{_settings.DocumentDirectory}{Path.DirectorySeparatorChar}{identifier}{Path.DirectorySeparatorChar}{identifier}-{documentType.ToString()}.{_settings.ImageFormat}";
                var fileNew = $"{_settings.DocumentDaseUrl}{Path.DirectorySeparatorChar}{identifier}{Path.DirectorySeparatorChar}{identifier}-{documentType.ToString()}.{_settings.ImageFormat}";

                var image = await File.ReadAllBytesAsync(file);
                return Convert.ToBase64String(image, Base64FormattingOptions.None);
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "Server error occurred in the GetImage of the FileSystemImageManager class at {0} ", DateTime.UtcNow);
                return null;
            }

        }

        public async Task<string> GetImageNew(string identifier, DocumentType documentType)
        {
            try
            {
                _log.LogInformation("Inside the  GetImage of the FileSystemImageManager class at {0} ", DateTime.UtcNow);

                var fileNew = $"{_settings.DocumentDaseUrl}{Path.DirectorySeparatorChar}{identifier}{Path.DirectorySeparatorChar}{identifier}-{documentType.ToString()}.{_settings.ImageFormat}";

                var image = await File.ReadAllBytesAsync(fileNew);
                return Convert.ToBase64String(image, Base64FormattingOptions.None);
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "Server error occurred in the GetImage of the FileSystemImageManager class at {0} ", DateTime.UtcNow);
                return null;
            }

        }



        public string GenerateFileName(string folder, string extension)
        {

            var fileName =
                $"{Guid.NewGuid():N}{extension}";

            if (System.IO.File.Exists(fileName))
                return GenerateFileName(folder, extension);
            return Path.Combine(folder, fileName);
        }

        public async Task<MessageResponse> SaveDocument(KYCUploadRequest_ request, string folderName)
        {

            var folderPath = GetRootPath(folderName);
            // var folderPath = $"{root}\\{request.FolderName}";
            var fileName = GenerateFileName(folderPath, request.Document.Extension);

            var file = $"{folderPath}{Path.DirectorySeparatorChar}{fileName}";
            await File.WriteAllBytesAsync(file, Convert.FromBase64String(request.Document.RawData));


            var respMessage = new MessageResponse { FileName = Path.GetFileName(fileName), FolderName = folderName };
            return respMessage;


        }

        public async Task<MessageResponse> SavePicture(PhotoUpdateRequest request, string folderName)
        {

            var folderPath = GetRootPath(folderName);
            // var folderPath = $"{root}\\{request.FolderName}";
            var fileName = GenerateFileName(folderPath, "jpg");

            var file = $"{folderPath}{Path.DirectorySeparatorChar}{fileName}";
            await File.WriteAllBytesAsync(file, Convert.FromBase64String(request.Picture));


            var respMessage = new MessageResponse { FileName = Path.GetFileName(fileName), FolderName = folderName };
            return respMessage;


        }
        public string GetRootPath(string folder)
        {
            var root = $"{_configuration.GetValue<string>("Util:Root")}";
            var folderPath = $"{root}\\{folder}";
            var isExisting = Directory.Exists(folderPath);
            if (isExisting)
                return Path.GetFullPath(folderPath);
            Directory.CreateDirectory(folderPath);
            return Path.GetFullPath(folderPath);

        }


        public async Task<string> SaveImage(string identifier, string fileName, DocumentType documentType, string rawImage)
        {
            try
            {
                _log.LogInformation("Inside the save SaveImage method of FileSystemImageManager class ");
                if (string.IsNullOrEmpty(rawImage))
                {
                    return string.Empty;
                }

                string filePath = documentType == DocumentType.PICTURE ? _settings.SelfieDirectory : _settings.DocumentDirectory;// $"{_settings.DocumentDirectory}{Path.DirectorySeparatorChar}{identifier}";


                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                var file = Path.Combine(filePath, fileName);//   $"{filePath}{Path.DirectorySeparatorChar}{fileName}";




                if (File.Exists(file))
                {
                    _log.LogInformation(" Start setting attribute for the file  in the SaveImage of the FileSystemImageManager");
                    _log.LogInformation($" {file} exists");

                    //if (File.Exists(file))
                    //{
                    // var documentExists = await this.DeletFileFromFileDirectory(file);
                    if (File.Exists(file))
                    {
                        documentExists = true;
                    }


                    await Util.SetFileAttribute(filePath, file);
                    _log.LogInformation(" Finished setting attribute for the file  in the SaveImage of the FileSystemImageManager");
                    await File.WriteAllBytesAsync(file, Convert.FromBase64String(rawImage));
                    _log.LogInformation("Start setting attribute for the file  in the SaveImage of the FileSystemImageManager");

                }
                _log.LogInformation($" {file} does not exist");
                await File.WriteAllBytesAsync(file, Convert.FromBase64String(rawImage));

                return file;
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "Server error occurred in the SaveImage of the FileSystemImageManager class at {0} ", DateTime.UtcNow);
                return null;
            }


        }

        public async Task<string> Paths(string fileName, DocumentType docType, string phoneNumber)
        {


            var folderName = Path.Combine("Resources", "Images");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var path = $"{pathToSave}{Path.DirectorySeparatorChar}{fileName}";
            return await Task.FromResult(path);
        }
        public async Task<string> SaveImageToWebSite(string identifier, string fileName, DocumentType documentType, string rawImage)
        {



            var folderName = Path.Combine("Resources", "Images");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);


            if (string.IsNullOrEmpty(rawImage))
            {
                return string.Empty;
            }

            var file = $"{pathToSave}{Path.DirectorySeparatorChar}{identifier}{fileName}";
            await File.WriteAllBytesAsync(file, Convert.FromBase64String(rawImage));
            return file;
        }


        public async Task<string> UpdateImage(string identifier, string fileName, DocumentType documentType, string rawImage)
        {
            if (string.IsNullOrEmpty(rawImage))
            {
                return string.Empty;
            }

            string filePath = documentType == DocumentType.PICTURE ? _settings.SelfieDirectory : $"{_settings.DocumentDirectory}{Path.DirectorySeparatorChar}{identifier}";
            var file = $"{filePath}{Path.DirectorySeparatorChar}{fileName}";

            if (!File.Exists(file))
            {
                File.Delete(file);
            }
            await File.WriteAllBytesAsync(file, Convert.FromBase64String(rawImage));

            return file;

        }
        public async Task<string> SaveImage(string filePath, string fileName, string rawImage)
        {
            if (string.IsNullOrEmpty(rawImage))
            {
                return string.Empty;
            }
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            else
            {
                if (File.Exists($"{filePath}{Path.DirectorySeparatorChar}{fileName}"))
                {
                    File.Delete($"{filePath}{Path.DirectorySeparatorChar}{fileName}");
                }
            }
            var file = $"{filePath}{Path.DirectorySeparatorChar}{fileName}";
            await File.WriteAllBytesAsync(file, Convert.FromBase64String(rawImage));
            return file;
        }



        public async Task<string> GetSelfie(string identifier, DocumentType documentType)
        {
            var file = $"{_settings.DocumentDirectory}{Path.DirectorySeparatorChar}{identifier}{Path.DirectorySeparatorChar}{identifier}-{documentType.ToString()}.{_settings.ImageFormat}";

            var image = await File.ReadAllBytesAsync(file);
            return Convert.ToBase64String(image, Base64FormattingOptions.None);
        }

        public async Task<string> GetIdentification(string identifier, DocumentType documentType)
        {
            var file = $"{_settings.DocumentDirectory}{Path.DirectorySeparatorChar}{identifier}{Path.DirectorySeparatorChar}{identifier}-{documentType.ToString()}.{_settings.ImageFormat}";

            var image = await File.ReadAllBytesAsync(file);
            return Convert.ToBase64String(image, Base64FormattingOptions.None);
        }

        public async Task<string> GetDocument(string filePath)
        {

            var document = await File.ReadAllBytesAsync(filePath);
            return Convert.ToBase64String(document, Base64FormattingOptions.None);
        }

        public async Task OverrideImage(string sourceFilName, string destinatioFileName)
        {
            try
            {
                File.Copy(sourceFilName, destinatioFileName, true);
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "Error ocurred");
            }
            await Task.CompletedTask;
        }
        public async Task<string> UpdateImage(string filePath, string fileName, string rawImage)
        {
            if (string.IsNullOrEmpty(rawImage))
            {
                return string.Empty;
            }
            var file = $"{filePath}{Path.DirectorySeparatorChar}{fileName}";

            if (!File.Exists(file))
            {
                File.Delete(file);
            }
            await File.WriteAllBytesAsync(file, Convert.FromBase64String(rawImage));

            return file;
        }

        public async Task RemoveDocument(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            await Task.CompletedTask;
        }
        public string GetFileViewUrl(string photoUrl, string returnedBaseUrl)
        {
            if (!string.IsNullOrWhiteSpace(photoUrl))
            {
                var photoLocationSplit = photoUrl.Split('\\');
                var photoLocationSplitIsValid = photoLocationSplit.Length > 3;
                var directoryPath = string.Empty;

                if (photoLocationSplitIsValid)
                {
                    var photoImage = photoLocationSplit[4];
                    directoryPath = System.IO.Path.Combine(returnedBaseUrl, photoImage);
                    directoryPath = directoryPath.Replace("\\", "//");
                }
                return directoryPath.Replace(@"\", "//");
            }
            return null;

        }

        public string GetImage(string photoUrl, string returnedBaseUrl)
        {
            if (!string.IsNullOrWhiteSpace(photoUrl))
            {
                var photoLocationSplit = photoUrl.Split('\\');


                var photoImage = photoLocationSplit[photoLocationSplit.Length - 1];

                //string imageWithPath = Path.Combine(returnedBaseUrl, photoImage);
                string imageWithPath = $"{returnedBaseUrl}/{photoImage}";

                return imageWithPath;
            }
            return null;

        }


        public async Task<Dictionary<bool, string>> DeletFileFromFileDirectory(string fileToPath)
        {
            string message = string.Empty;
            bool deleted = false;
            var dict = new Dictionary<bool, string>();

            try
            {

                // Check if file exists with its full path    
                if (File.Exists(fileToPath))
                {
                    // If file found, delete it  
                    File.Delete(fileToPath);
                    message = "deleted";
                    deleted = true;

                }
                else
                {
                    message = "not deleted";

                };
                dict.Add(deleted, message);
                return await Task.FromResult(dict);
            }
            catch (IOException ioExp)
            {
                message = "eror";
                _log.LogCritical(ioExp, "Server Error in the DeletFileFromFileDirectory of FileSystemImageManager at {0}. file path sent:===>{1}", DateTime.UtcNow, fileToPath);
                dict.Add(deleted, message);
                return await Task.FromResult(dict);
            }

        }

        public Task<string> GetImaageBase64String(string path)
        {
            throw new NotImplementedException();
        }
        private string ImageToBase64(string path)
        {
            string base64String = string.Empty;
            Bitmap bp = new Bitmap(path);
            //using (System.Drawing.Image image = System.Drawing.Image.FromFile(path))
            using (Bitmap image = new Bitmap(path))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();
                    base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }


        }

        public String ConvertImageURLToBase64(String url)
        {
            _log.LogInformation("Inside the  ConvertImageURLToBase64 method of the DocumetReviewService");
            StringBuilder _sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(url))
            {
                Byte[] _byte = this.GetImage(url);

                _sb.Append(Convert.ToBase64String(_byte, 0, _byte.Length));
            }

            _log.LogInformation("Finished converting imageto base64String in the ConvertImageURLToBase64  method of the FileSystemManager");
            return _sb.ToString();
        }

        private byte[] GetImage(string url)
        {
            Stream stream = null;
            byte[] buf;
            _log.LogInformation("Inside the GetImage byteArray  method of the FileSystemManager");
            try
            {
                WebProxy myProxy = new WebProxy();
                if (!string.IsNullOrWhiteSpace(url))
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

                    HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                    stream = response.GetResponseStream();

                    using (BinaryReader br = new BinaryReader(stream))
                    {
                        int len = (int)(response.ContentLength);
                        buf = br.ReadBytes(len);
                        br.Close();
                    }

                    stream.Close();
                    response.Close();
                    _log.LogInformation("Gtten the image and converted to byte in  the GetImage method of the DocumetReviewService");
                    return buf;
                }

            }
            catch (Exception exp)
            {
                _log.LogCritical(exp, "error occurred  the GetDocument method of the DocumetReviewService");
                buf = null;
            }

            return null;

        }
        //not used
         [DefaultValue(false)] 
        public async Task<bool> DoesDocumentExists(DocumentType documentType, string fileName)
        {
            _log.LogInformation("Inside the   DoesDocumentExists  method of FileSystemImageManager class ");
          

            string filePath = documentType == DocumentType.PICTURE ? _settings.SelfieDirectory : _settings.DocumentDirectory;// $"{_settings.DocumentDirectory}{Path.DirectorySeparatorChar}{identifier}";


            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            var file = Path.Combine(filePath, fileName);//   $"{filePath}{Path.DirectorySeparatorChar}{fileName}";

 await  Util.SetFileAttribute(filePath, file);


            if (File.Exists(file))
            {
                _log.LogInformation(" Start setting attribute for the file  in the SaveImage of the FileSystemImageManager");
                _log.LogInformation($" {file} exists");

                //if (File.Exists(file))
                //{
                // var documentExists = await this.DeletFileFromFileDirectory(file);

               
                if (File.Exists(file))
                {
                    documentExists = true;
                  return await  Task.FromResult(documentExists);
                }
            }
            return default; 

        }
    }
    public class MessageResponse
    {
        public string FileName { get; set; }
        public object FolderName { get; set; }
    }
}
