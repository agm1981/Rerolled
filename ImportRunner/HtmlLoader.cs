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
        //private string folderLocation2 = ConfigurationManager.AppSettings["Folder2"];
        private string folderLocation = ConfigurationManager.AppSettings["Folder"];
        public void Start()
        {
            AllPostsRepository repository = new AllPostsRepository();
            AllThreadsRepository repository2 = new AllThreadsRepository();
            PostList = new HashSet<int>(repository.GetAllPosts());
            ThreadNames = new HashSet<string>(repository2.GetAllThreads().Select(c=>c.CombinedName));
            DirectoryInfo dir = new DirectoryInfo(folderLocation);
            if (!dir.Exists)
            {
                throw new Exception("Missing Folder Or Access");
            }
            string s;
            
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
                ThreadNameForumExtracter trd = new ThreadNameForumExtracter
                {
                    FileName = file.Name,
                    Html = s
                };
                trd.ExtractThreads();
                SaveThreadData(trd, repository2);

            }
        //    dir = new DirectoryInfo(folderLocation2);
        //    if (!dir.Exists)
        //    {
        //        throw new Exception("Missing Folder Or Access");
        //    }
            

        //    foreach (FileInfo file in dir.EnumerateFiles("showthread*.html"))
        //    {
        //        //Console.WriteLine($"{file.Name}");
        //        using (StreamReader sr = file.OpenText())
        //        {
        //            s = sr.ReadToEnd();
        //        }
        //        //PostExtracter pst = new PostExtracter
        //        //{
        //        //    FileName = file.Name,
        //        //    Html = s
        //        //};
        //        //pst.ExtractPosts();
        //        //SaveData(pst, repository);

        //        ThreadNameForumExtracter trd = new ThreadNameForumExtracter
        //        {
        //            FileName = file.Name,
        //            Html = s
        //        };
        //        trd.ExtractThreads();
        //        SaveThreadData(trd, repository2);


        //    }

        }

        private HashSet<int> PostList
        {
            get;
            set;
        }

        private HashSet<string> ThreadNames
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
        private void SaveThreadData(ThreadNameForumExtracter trd, AllThreadsRepository repository)
        {
            // here we save into EF

            bool alreadyThere = ThreadNames.Contains(trd.Thread.CombinedName);
            if (alreadyThere)
            {
                return;
            }
            
            repository.Save(trd.Thread);
            ThreadNames.Add(trd.Thread.CombinedName);
        }
    }
}
