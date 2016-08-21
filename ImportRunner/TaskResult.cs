using System;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace ImportRunner
{
    public class TaskResult
    {
        public string Url
        {
            get;
            set;
        }
        public byte[] FileBytes
        {
            get;
            set;
        }

        public bool Success
        {
            get;
            set;
        }

        public Exception Errors
        {
            get;
            set;
        }

        public MediaTypeHeaderValue MimeType
        {
            get;
            set;
        }
            
        public string FileName
        {
            get;
            set;
        }

        public int ImageId
        {
            get
            {
                Regex reg = new Regex(@"attachmentid=\d+");
                string name = reg.Match(Url).Value;
                reg = new Regex(@"\d+");
                return int.Parse(reg.Match(name).Value);
            }
        }

        public string GetFileName()
        {
            Regex reg = new Regex(@"attachmentid=\d+");
            string name = reg.Match(Url).Value;
            reg = new Regex(@"\d+");
            name= reg.Match(name).Value;
            // jpg, jpeg, png, gif

            switch (this.MimeType.MediaType)
                {
                    case "image/jpeg":
                        return $"{name}.jpg";
                    
                    case "image/jpg":
                        return $"{name}.jpg";
                    
                    case "image/png":
                        return $"{name}.png";
                    
                    case "image/gif":
                        return $"{name}.gif";
                    default:
                        throw new Exception("invalid mime type" + this.MimeType.MediaType);
                }
            
            
        }
    }
}