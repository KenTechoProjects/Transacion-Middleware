using System;
using System.Threading.Tasks;
using Middleware.Core.DTO;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using System.IO;
namespace Middleware.Service.Fakes
{
    public class FakeImageManager //: IImageManager
    {

        private const string image = "R0lGODlhAQABAIAAAAAAAAAAACH5BAAAAAAALAAAAAABAAEAAAICTAEAOw=="; //Black rectangle
        public Task<string> GetImage(string identifier, DocumentType documentType)
        {
            return Task.FromResult(image);
        }

        public Task<string> SaveImage(string identifier, string fileName, DocumentType documentType, string rawImage)
        {
            return Task.FromResult("<dummy_path>");
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

        public Task<string> UpdateImage(string identifier, string fileName, DocumentType documentType, string rawImage)
        {
            return Task.FromResult("<dummy_path>");
        }
    }
}
