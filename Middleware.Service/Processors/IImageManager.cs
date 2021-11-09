using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Core.DTO;
using Middleware.Service.DTOs;
using Middleware.Service.Utilities;

namespace Middleware.Service.Processors
{
    public interface IImageManager
    {
        string GetFileViewUrl(string photoUrl, string returnedBaseUrl);
        string GetImage(string photoUrl, string returnedBaseUrl);
        String ConvertImageURLToBase64(String url);
        Task<Dictionary<bool, string>> DeletFileFromFileDirectory(string fileToPath);


        Task<bool> DoesDocumentExists(DocumentType documentType, string fileName);
        bool DocumentExists { get; set; }
        Task<string> SaveImage(string identifier, string fileName, DocumentType documentType, string rawImage); //Input image is base64 representation of the image
       
        
        
        Task<string> SaveImageToWebSite(string identifier, string fileName, DocumentType documentType, string rawImage); //Input image is base64 representation of the image
       Task<string>  SaveImage(string filePath, string fileName, string rawImage); //Input image is base64 representation of the image
        Task<string> GetImage(string identifier, DocumentType documentType);

        Task<string> UpdateImage(string identifier, string fileName, DocumentType documentType, string rawImage);
        Task<MessageResponse> SavePicture(PhotoUpdateRequest request, string folderName);
       
        Task<MessageResponse> SaveDocument(KYCUploadRequest_ request, string folderName);


           Task<string> GetSelfie(string identifier, DocumentType documentType);
        Task<string> GetIdentification(string identifier, DocumentType documentType);
        Task<string> GetDocument(string filePath);
        Task<string> UpdateImage(string filePath, string fileName, string rawImage);

        Task RemoveDocument(string path);
        Task<string> GetImaageBase64String(string path);

        Task OverrideImage(string sourceFilName, string destinatioFileName);


    }
}
