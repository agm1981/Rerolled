using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ImportRunner
{
    public class HtmlLoader
    {
        public void Start()
        {
            string s;
            using (StreamReader sr = File.OpenText("showthread0aa0.html"))
            {
                s = sr.ReadToEnd();
            }
            PostExtracter pst = new PostExtracter();
            pst.Html = s;
            pst.ExtractPosts();
            int t = 9;

        }
    }
}
