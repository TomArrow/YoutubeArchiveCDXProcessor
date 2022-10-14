using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Readers;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace YoutubeHTMLLinkExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.Write("\a");
            //Console.Write(" … ");
            //Console.Write("\U+0007");
            //Console.Write((char)7);
            //Console.ReadKey();
            Parse7zWithYotuubeHTMLLists(args[0]);
            //Parse7zWithYotuubeHTMLLists("dl list m flexible.7z");
            Console.ReadKey();
        }
        //
        static Regex linkfinderTitleTag = new Regex("<a[^>]*?href=\"[^\"]*?v=(?<id>[a-zA-Z0-9_-]+)[^\"]*?\"[^>]*?title=\"(?<title>.*?)\"[^>]*>(?<titleShort>[^<>]*?)</a>", RegexOptions.Singleline|RegexOptions.IgnoreCase|RegexOptions.Compiled);
        static Regex linkfinder = new Regex("<a[^>]*?href=\"[^\"]*?v=(?<id>[a-zA-Z0-9_-]+)[^\"]*?\"[^>]*>(?<title>[^<>]*?)</a>", RegexOptions.Singleline|RegexOptions.IgnoreCase|RegexOptions.Compiled);

        static void Parse7zWithYotuubeHTMLLists(string inputfile)
        {

            //string[] lines = File.ReadAllLines("cdx@url=youtube.com%2Fwatch%2A&output=json.json");
            byte[] extractData = File.ReadAllBytes(inputfile);
            MemoryStream readStream = new MemoryStream(extractData);

            SevenZipArchive archive = SevenZipArchive.Open(readStream);

            var db = new SQLiteConnection(inputfile + ".videoEntries.db");
            db.CreateTable<VideoEntry>();
            Console.OutputEncoding = Encoding.UTF8;

            db.BeginTransaction();

            using (IReader reader = archive.ExtractAllEntries())
            {
                while (reader.MoveToNextEntry())
                {
                    if (reader.Entry.Size > 0 && !reader.Entry.IsDirectory)
                    {
                        using (EntryStream entryStream = reader.OpenEntryStream())
                        {
                            MemoryStream ms = new MemoryStream();
                            //extract.BeginExtractFiles()
                            //entryStream.CopyTo();
                            entryStream.CopyTo(ms);

                            Console.WriteLine(reader.Entry.ToString());

                            string timestamp = reader.Entry.ToString().Replace(".html", "");

                            Int64  timestampInt = 0;
                            Int64.TryParse(reader.Entry.ToString().Replace(".html", ""), out timestampInt);

                            string file1 = Encoding.UTF8.GetString(ms.ToArray());


                            MatchCollection matches = linkfinderTitleTag.Matches(file1);

                            if(matches.Count == 0)
                            {
                                matches = linkfinder.Matches(file1);
                            }

                            foreach(Match match in matches)
                            {
                                if (match.Success && match.Groups.ContainsKey("id") && match.Groups.ContainsKey("title"))
                                {
                                    string id = match.Groups["id"].Value;
                                    string title = HttpUtility.HtmlDecode(match.Groups["title"].Value).Trim();
                                    string safetitle = ReplaceInvalidChars(title).Replace(",","_").Replace(".","_").Replace("\a", "");
                                    if (safetitle.Length > 100)
                                    {
                                        safetitle = safetitle.Substring(0, 100);
                                    }
                                    //safetitle = new string(safetitle.Where(c => !char.IsControl(c)).ToArray()).Replace("\a",""); 
                                    int year = 0;
                                    int.TryParse(timestamp.Substring(0,4),out year);

                                    Console.WriteLine(year + ": " + id + ": " + safetitle) ;
                                    //Console.WriteLine(year+": "+id+": ");


                                    VideoEntry entry = new VideoEntry()
                                    {
                                        VideoId = id,
                                        VideoName = title,
                                        VideoNameSafe = safetitle,
                                        Year = year,
                                        Timestamp = timestampInt,
                                    };

                                    db.Insert(entry);
                                }
                            }

                        }
                    }
                }
            }

            db.Commit();
            db.Close();
            db.Dispose();

        }

        // following 2 from: https://stackoverflow.com/a/23182807
        public static string RemoveInvalidChars(string filename)
        {

            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static string ReplaceInvalidChars(string filename,string replacement = "_")
        {
            return string.Join(replacement, filename.Split(Path.GetInvalidFileNameChars()));
        }

    }
}
