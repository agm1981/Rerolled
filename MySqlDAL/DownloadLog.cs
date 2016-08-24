using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Common.Extensions;

namespace Common
{
    public class DownloadLog
    {
        public int ImageId
        {
            get;
            set;
        }

        public string Error
        {
            get;
            set;
        }

        public int UserId
        {
            get;
            set;
        }

        public int UploadDate
        {
            get;
            set;
        }

        public string FileName
        {
            get;
            set;
        }

        public int FileSize
        {
            get;
            set;
        }

        public string FileHash
        {
            get;
            set;
        }

        public string FilePath => string.Empty;

        public int Width
        {
            get;
            set;
        }

        public int Height
        {
            get;
            set;
        }

        public int ThumbnailWidth => 100;

        public int ThumbnailHeight => 100;

        public int AttachCount => 100;

        public string MimeType
        {
            get;
            set;
        }
        private string ComputeMd5(FileInfo fileData)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = fileData.OpenRead())
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream)).Substring(0,32);
                }
            }
        }

        public void CompleteFields(FileInfo fileData)
        {
            FileHash = ComputeMd5(fileData);
            FileName = $"rrr_{fileData.Name}.{fileData.Extension}";
            FileSize = Convert.ToInt32(fileData.Length);
            MimeType = ConvertToMime(fileData.Extension);
            UploadDate = DateTime.UtcNow.DateTimeToUnixTimestamp();
        }

        private string ConvertToMime(string fileExtension)
        {
            switch (fileExtension)
            {
                case "jpg":
                    return "image/jpg";
                case ".png":
                    return "image/png";

                case ".gif":
                    return "image/gif";
                default:
                    throw new Exception("invalid mime type" + fileExtension);
            }
        }

        
    }
}