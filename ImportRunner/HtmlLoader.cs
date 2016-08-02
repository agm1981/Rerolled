using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using MySqlDAL;

namespace ImportRunner
{
    public class HtmlLoader
    {
        private string folderLocation = ConfigurationManager.AppSettings["Folder"];
        public void Start()
        {
            DirectoryInfo dir = new DirectoryInfo(folderLocation);
            if (!dir.Exists)
            {
                throw new Exception("Missing Folder Or Access");
            }
            string s;
            using (var context = new DenormalizeContext())
            {
                foreach (FileInfo file in dir.EnumerateFiles("showthread*.html"))
                {
                    //Console.WriteLine($"{file.Name}");
                    using (StreamReader sr = file.OpenText())
                    {
                        s = sr.ReadToEnd();
                    }
                    PostExtracter pst = new PostExtracter();
                    pst.FileName = file.Name;
                    pst.Html = s;
                    pst.ExtractPosts();
                    SaveData(pst, context);
                }
            }
        }

        private void SaveData(PostExtracter pst, DenormalizeContext context)
        {
            // here we save into EF
            context.Posts.AddRange(pst.Posts);
            context.SaveChanges();
            context.DetachAllEntities();
            
        }
    }
}
