using System;
using System.Collections.Generic;
using System.Configuration;

using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.DataLayer;
using Microsoft.SqlServer.Server;

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
            AllPostsRepository repository = new AllPostsRepository();
            PostList = new HashSet<int>(repository.GetAllPosts());
            foreach (FileInfo file in dir.EnumerateFiles("showthread*.html"))
            {
                //Console.WriteLine($"{file.Name}");
                using (StreamReader sr = file.OpenText())
                {
                    s = sr.ReadToEnd();
                }
                PostExtracter pst = new PostExtracter
                {
                    FileName = file.Name,
                    Html = s
                };
                pst.ExtractPosts();
                SaveData(pst, repository);
            }

        }

        private HashSet<int> PostList
        {
            get;
            set;
        }

        private void SaveData(PostExtracter pst, AllPostsRepository repository)
        {
            // here we save into EF

            HashSet<int> listToAdd = new HashSet<int>(pst.Posts.Where(c => !PostList.Contains(c.PostId)).Select(c => c.PostId));
            if (listToAdd.Count == 0)
            {
                return;
            }
            IEnumerable<Post> postToAdd = pst.Posts.Where(c => listToAdd.Contains(c.PostId));
            repository.Save(postToAdd);
            PostList.UnionWith(listToAdd);
        }
    }
}
