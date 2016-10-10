using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;

using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using Common.DataLayer;
using Microsoft.SqlServer.Server;

namespace ImportRunner
{

    //public class OrderedFileName<T> : List<T>
    //{
    //    private static readonly Regex regex = new Regex(@"\d+$");
    //    public new void Add(T item)
    //    {
    //        if (!(item is FileInfo))
    //        {
    //            base.Add(item);
    //            return;
    //        }


    //        // here is the fun. we must extract the thing and place at the corrent spot
    //        // idea is to insert it at the correct position.
    //        // first is to split by name and page, then add bypage, we can add page 1 to the first page to help with logic.
    //        // get th e last 4 digits of the name
    //        // 2659-imitation-picture-thread.html
    //        //2659-imitation-picture-thread-4.html
    //        var data = item as FileInfo;
    //        var filename = Path.GetFileNameWithoutExtension(data.Name);

    //        string filenameWithoutDigit;
    //        int pageNumber;
    //        if (regex.Match(filename).Success)
    //        {
    //            // ends with digit.
    //            filenameWithoutDigit = filename.Substring(0, filename.LastIndexOf("-", StringComparison.OrdinalIgnoreCase));
    //            pageNumber = int.Parse(regex.Match(filename).Value);
    //        }
    //        else
    //        {
    //            // add 1 to the end.

    //            filenameWithoutDigit = filename;
    //            pageNumber = 1;

    //        }

    //        base.Add(item);

    //    }
    //}
    public class HtmlLoader
    {
        //private string folderLocation2 = ConfigurationManager.AppSettings["Folder2"];
        private string folderLocation = ConfigurationManager.AppSettings["Folder"];
        public void Start()
        {
            AllPostsRepository repository = new AllPostsRepository();
            AllThreadsRepository repository2 = new AllThreadsRepository();
            PostList = new HashSet<int>(repository.GetAllPosts());
            ThreadNames = new HashSet<string>(repository2.GetAllThreads().Select(c => c.CombinedName));
            DirectoryInfo dir = new DirectoryInfo(folderLocation);
            if (!dir.Exists)
            {
                throw new Exception("Missing Folder Or Access");
            }
            string s;
            List<FileInfo> filesOrdered = new List<FileInfo>();

            foreach (FileInfo file in dir.EnumerateFiles("*.html"))
            {
                filesOrdered.Add(file);
            }
            filesOrdered.Sort(ComparisonFileinfo);

            foreach (FileInfo file in filesOrdered)
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
                pst.ExtractFoHPosts();
                repository.Save(pst.Posts);
                // SaveData(pst, repository); // We dont need to check for not parsed
                // for Foh we skip

                continue;
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
            //        //pst.ExtractFoHPosts();
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

        /// <summary>
        /// -1 is A less than B
        /// 0 if equal
        /// 1 if a greater than B
        /// </summary>
        /// <param name="fileA"></param>
        /// <param name="fileB"></param>
        /// <returns></returns>
        private int ComparisonFileinfo(FileInfo fileA, FileInfo fileB)
        {
            Regex regex = new Regex(@"\d+$");
            string filenameA = Path.GetFileNameWithoutExtension(fileA.Name);
            string filenameB = Path.GetFileNameWithoutExtension(fileB.Name);
            int digitA = 1;
            int digitB = 1;

            if (regex.Match(filenameA).Success)
            {
                // ends with digit.
                digitA = int.Parse(regex.Match(filenameA).Value);
                filenameA = filenameA.Substring(0, filenameA.LastIndexOf("-", StringComparison.OrdinalIgnoreCase));
            }

            if (regex.Match(filenameB).Success)
            {
                // ends with digit.
                digitB = int.Parse(regex.Match(filenameB).Value);
                filenameB = filenameB.Substring(0, filenameB.LastIndexOf("-", StringComparison.OrdinalIgnoreCase));
            }

            if (string.Compare(filenameA, filenameB, StringComparison.Ordinal) == 0)
            {
                return digitA.CompareTo(digitB);
            }

            return string.Compare(filenameA, filenameB, StringComparison.Ordinal);

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

            repository.InsertThread(trd.Thread);
            ThreadNames.Add(trd.Thread.CombinedName);
        }
    }
}
