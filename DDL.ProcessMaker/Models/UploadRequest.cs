using System;
using System.Collections.Generic;
using System.Text;

namespace DDL.ProcessMaker.Models
{
    class UploadRequest
    { 
        public string taskId { get; set; }
        public string docUid { get; set; }
        public string comment { get; set; }
    }

    public class DocUploadRequest
    {
        public string appDocUid { get; set; }
        public string name { get; set; }
        public string version { get; set; }
    }
}