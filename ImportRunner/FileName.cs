using System.Collections.Generic;
using System.IO;

namespace ImportRunner
{
    public class FileName
    {
        public void Start()
        {
            string prefix = "rrr_img_";
            string folder = @"e:\rr_images";
            HashSet<FileInfo> fileNamess = new HashSet<FileInfo>(new DirectoryInfo(folder).GetFiles());
            foreach (FileInfo fileInfo in fileNamess)
            {
                fileInfo.MoveTo($@"{folder}\{prefix}{fileInfo.Name}");
            }
        }
    }
}