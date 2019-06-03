using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;

namespace BugzillaXml2Csv
{
    /* First export bugs from bugzilla 5.3 by creating a query and using the XML button at the bottom for the format.
     * 
     */
    class Program
    {
        static void Main(string[] args)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("bugs.xml"); //make sure the bugs.xml is in the debug dir

            StringBuilder sbFile = new StringBuilder();

            string headerRow = "type,bug_id,creation_ts,short_desc,version,bug_status,estimated_time,description,comments"; //adjust these fields as required
            Write(headerRow, sbFile);

            foreach(XmlNode bug in doc.LastChild.ChildNodes)
            {
                Console.WriteLine("##########");

                string bug_id = bug.SelectSingleNode("bug_id").InnerText;
                string creation_ts = bug.SelectSingleNode("creation_ts").InnerText;
                string short_desc = System.Net.WebUtility.HtmlEncode(String.Format("Bug {0} - {1}", bug_id, bug.SelectSingleNode("short_desc").InnerText));

                string version = bug.SelectSingleNode("version").InnerText;
                string bug_status = bug.SelectSingleNode("bug_status").InnerText;
                string estimated_time = bug.SelectSingleNode("estimated_time").InnerText;

                string description = null;
                List<string> comments = new List<string>();

                foreach (XmlNode comment in bug.SelectNodes("long_desc"))
                {
                    string who = comment.SelectSingleNode("who").Attributes["name"].InnerText;

                    string theText = System.Net.WebUtility.HtmlEncode(comment.SelectSingleNode("thetext").InnerText);

                    if (String.IsNullOrEmpty(theText))
                    {
                        var timeNode = comment.SelectSingleNode("work_time");
                        if (null != timeNode)
                        {
                            theText = String.Format("{0} worked {1} hours", who, timeNode.InnerText);
                        }
                    }
                    string commentID = comment.SelectSingleNode("comment_count").InnerText;
                    string commentDate = comment.SelectSingleNode("bug_when").InnerText;
                    

                    if (String.IsNullOrEmpty(description))
                    {

                        description = String.Format(@"""BzUrl: http://BUGZILLA_URL/show_bug.cgi?id={0} 
BzDate: {1} 
BzUser: {5} 
BzStatus: {2} 
BzVersion: {3} 
BzDescription:\\
{4}""", bug_id, commentDate, bug_status, version, theText, who);
                        description = AddJiraNewLines(description);
                    }
                    else
                    {
                        string c = String.Format(@"*******************************
Comment Id: {0}
BzDate: {1} 
BzUser: {3} 
BzComment:\\
{2}", commentID, commentDate, theText, who);
                        c = AddJiraNewLines(c);
                        comments.Add(c);
                    }
                }

                string row = String.Format("task,{0},{1},\"{2}\",{3},{4},{5},{6},", bug_id, creation_ts, short_desc, version, bug_status, estimated_time, description);
                if (comments.Count > 0)
                {
                    string commentsAsText = String.Join("\\\\\r\n \\\\\r\n", comments);
                    row += "\"Bugzilla Comments:\\\\\r\n" + commentsAsText + "\"";
                }

                Write(row, sbFile);
            }

            System.IO.File.WriteAllText("bugs.csv", sbFile.ToString());

            Console.WriteLine("Done");
            Console.ReadKey();
        }

        static string AddJiraNewLines(string s)
        {
            Regex r = new Regex("(\r\n|\r|\n)");
            string newEndings = r.Replace(s, " \\\\ \r\n");
            return newEndings;
        }

        static void Write(string s, StringBuilder sb)
        {


            
            sb.AppendLine(s);
            Console.WriteLine(s);
        }
    }
}
