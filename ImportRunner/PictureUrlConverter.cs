using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Common.DataLayer;

namespace ImportRunner
{
    public class PictureUrlConverter
    {

        private HashSet<MySqlPost> workListItems = new HashSet<MySqlPost>();
        /// <summary>
        /// here we loop on the pictures and we
        /// </summary>
        public void Start()
        {
            AllPostMySqlRepository rep = new AllPostMySqlRepository();

            workListItems = new HashSet<MySqlPost>(rep.GetAllPostIncompleteInMySql());

            //IEnumerable<MySqlPost> batch = workListItems.Take(1000);
            DirectoryInfo dir = new DirectoryInfo(ConfigurationManager.AppSettings["OutputFolder"]);
            HashSet<string> files = new HashSet<string>(dir.GetFiles("*.*").Select(c => c.Name));
            foreach (MySqlPost post in workListItems)
            {
                string oldPost = post.Content;
                post.Content = PostConverter(post.Content, files);
                if (oldPost != post.Content)
                {
                    rep.UpdatePostContent(post);
                }
            }
        }

        private string PostConverter(string oldPost, HashSet<string> allFiles)
        {
            // do the regext
            string returnValue = oldPost;
            Regex regexForImgtag = new Regex(@"\[img].+?www\.rerolled\.org/attachment\.php.+?\[/img]", RegexOptions.IgnoreCase);
            Regex regexForurltag = new Regex(@"\[url=.+?www\.rerolled\.org/attachment\.php.+?].+?\[/url]", RegexOptions.IgnoreCase);
            Regex onlyDigits = new Regex(@"\d+");
            string newUrl = "http://test.suineg.org/";
            string prefix = "rrr_img_";
            foreach (Match match in regexForImgtag.Matches(oldPost))
            {
                string wholeQuote = match.Value;

                // now we go and convert it to th enew one, get the text first
                string attachmetnId = onlyDigits.Match(wholeQuote).Value;

                // now go to the disk and get the correct thing
                bool found = allFiles.Any(c => c == $"{prefix}{attachmetnId}.jpg");
                if (found)
                {
                    string newTag = $"[IMG]{newUrl}{prefix}{attachmetnId}.jpg[/IMG]";
                    returnValue = returnValue.Replace(wholeQuote, newTag);
                }

                found = allFiles.Any(c => c == $"{prefix}{attachmetnId}.gif");
                if (found)
                {
                    string newTag = $"[IMG]{newUrl}{prefix}{attachmetnId}.gif[/IMG]";
                    returnValue = returnValue.Replace(wholeQuote, newTag);
                }

                found = allFiles.Any(c => c == $"{prefix}{attachmetnId}.png");
                if (found)
                {
                    string newTag = $"[IMG]{newUrl}{prefix}{attachmetnId}.png[/IMG]";
                    returnValue = returnValue.Replace(wholeQuote, newTag);
                }

            }

            foreach (Match match in regexForurltag.Matches(oldPost))
            {
                string wholeQuote = match.Value;

                // now we go and convert it to th enew one, get the text first
                string attachmetnId = onlyDigits.Match(wholeQuote).Value;

                // now go to the disk and get the correct thing
                bool found = allFiles.Any(c => c == $"{prefix}{attachmetnId}.jpg");
                if (found)
                {
                    string newTag = $"[IMG]{newUrl}{prefix}{attachmetnId}.jpg[/IMG]";
                    returnValue = returnValue.Replace(wholeQuote, newTag);
                }

                found = allFiles.Any(c => c == $"{prefix}{attachmetnId}.gif");
                if (found)
                {
                    string newTag = $"[IMG]{newUrl}{prefix}{attachmetnId}.gif[/IMG]";
                    returnValue = returnValue.Replace(wholeQuote, newTag);
                }

                found = allFiles.Any(c => c == $"{prefix}{attachmetnId}.png");
                if (found)
                {
                    string newTag = $"[IMG]{newUrl}{prefix}{attachmetnId}.png[/IMG]";
                    returnValue = returnValue.Replace(wholeQuote, newTag);
                }

            }

            return returnValue;

        }
    }
}