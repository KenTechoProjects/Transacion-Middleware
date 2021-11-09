using System;
namespace Middleware.Reversal.Settings
{
    public class ProcessMakerSettings
    {
        public string ServerAddress { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ProcessId { get; set; }
        public string TaskId { get; set; }
        public DocumentIdentifiers DocumentIds { get; set; }
    }

    public class DocumentIdentifiers
    {
        public string Photo { get; set; }
        public string Identification { get; set; }
    }
}
