using Salaros.Configuration;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace YoutubeArchiveCDXProcessor
{
    class Program
    {

        struct EssenceType
        {
            public string name;
            public Regex matcher;
            public Regex essenceExtractor;
        }

        static List<EssenceType> essenceTypes;

        static void Main(string[] args)
        {



            ConfigParser configParser = new ConfigParser(AppDomain.CurrentDomain.BaseDirectory+"/"+"essenceExtractors.ini");

            essenceTypes = new List<EssenceType>();

            foreach (ConfigSection section in configParser.Sections)
            {

                EssenceType essenceTypeHere = new EssenceType();

                essenceTypeHere.name = section.SectionName;
                essenceTypeHere.matcher = new Regex(configParser.GetValue(section.SectionName, "matcher").Trim(), RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
                essenceTypeHere.essenceExtractor = new Regex(configParser.GetValue(section.SectionName, "extractor").Trim(), RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

                essenceTypes.Add(essenceTypeHere);
            }
            Console.WriteLine(essenceTypes.Count);

            DecideTypeAndParse(args[0]);
            //ParseData(@"J:\Archival\rian johnson twitter\cdx@url=twitter.com%2Frianjohnson%2A&output=json");
            //ParseData(@"J:\Archival\various sites\cdx@url=akamaized.net&output=json&matchType=domain");

            Console.ReadKey();
        }

        //static Regex videoIdRegex = new Regex("v(=|/)([A-Za-z0-9_-]{11})");

        static void DecideTypeAndParse(string inputfile)
        {
            string[] lines = File.ReadAllLines(inputfile);
            string firstLine = lines[0];
            string[] firstLineParts = splitLine(firstLine);
            if (firstLineParts[0] == "urlkey") // CDX
            {
                ParseDataCDX(inputfile,ref lines);
            } else if (firstLineParts[0] == "original") // timemap
            {
                ParseDataTimeMap(inputfile, ref lines);
            }
        }

        static void ParseDataCDX(string inputfile,ref string[] lines)
        {

            //string[] lines = File.ReadAllLines("cdx@url=youtube.com%2Fwatch%2A&output=json.json");
            




            var db = new SQLiteConnection(inputfile+".dataCDX.db");
            db.CreateTable<CaptureCDX>();

            db.BeginTransaction();

            try
            {
                // i=1 bc ignore first line
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];


                    string[] parts = splitLine(line);


                    int statuscode = 0, length = 0;
                    Int64 timestamp = 0;
                    int.TryParse(parts[4], out statuscode);
                    int.TryParse(parts[6], out length);
                    Int64.TryParse(parts[1], out timestamp);


                    // Get Video ID
                    string essence = "";
                    string essenceTypeString = "";
                    foreach (EssenceType essenceType in essenceTypes)
                    {
                        if (essenceType.matcher.IsMatch(parts[2]))
                        {

                            Match myMatch = essenceType.essenceExtractor.Match(parts[2]);
                            if (myMatch.Success && myMatch.Groups.ContainsKey("essence"))
                            {
                                essence = myMatch.Groups["essence"].Value;
                                essenceTypeString = essenceType.name;
                                break;
                            }

                        }
                    }
                    /*Match videoIdMatch = videoIdRegex.Match(parts[2]);

                    if(videoIdMatch.Length > 0 && videoIdMatch.Groups.Count == 3)
                    {
                        videoId = videoIdMatch.Groups[2].Value;
                    }*/

                    CaptureCDX cap = new CaptureCDX()
                    {
                        Urlkey = parts[0],
                        Timestamp = timestamp,
                        Original = parts[2],
                        Mimetype = parts[3],
                        Statuscode = statuscode,
                        Digest = parts[5],
                        Length = length,
                        Essence = essence,//,
                        EssenceType = essenceTypeString//,
                                                       //Dupecount = int.Parse(parts[7])
                    };



                    db.Insert(cap);

                    Console.WriteLine(String.Join("     ,    ", parts));

                    /*if (i > 100)
                    {
                        break;
                    }*/
                }
            }
            catch
            {
                Console.WriteLine("ERROR. Presumably file ended abruptly/didn't download completely.");
            }
            db.Commit();
            db.Close();
            db.Dispose();
        }
        
        static void ParseDataTimeMap(string inputfile,ref string[] lines)
        {

            //string[] lines = File.ReadAllLines("cdx@url=youtube.com%2Fwatch%2A&output=json.json");
            

            var db = new SQLiteConnection(inputfile+".dataTimeMap.db");
            db.CreateTable<CaptureTimeMap>();

            db.BeginTransaction();

            try
            {
                // i=1 bc ignore first line
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];


                    string[] parts = splitLine(line);


                    int groupCount=0, uniqCount = 0;
                    Int64 timestamp = 0;
                    Int64 endTimestamp = 0;
                    int.TryParse(parts[4], out groupCount);
                    int.TryParse(parts[5], out uniqCount);
                    Int64.TryParse(parts[2], out timestamp);
                    Int64.TryParse(parts[3], out endTimestamp);


                    // Get Video ID
                    string essence = "";
                    string essenceTypeString = "";
                    foreach (EssenceType essenceType in essenceTypes)
                    {
                        if (essenceType.matcher.IsMatch(parts[0]))
                        {

                            Match myMatch = essenceType.essenceExtractor.Match(parts[0]);
                            if (myMatch.Success && myMatch.Groups.ContainsKey("essence"))
                            {
                                essence = myMatch.Groups["essence"].Value;
                                essenceTypeString = essenceType.name;
                                break;
                            }

                        }
                    }
                    /*Match videoIdMatch = videoIdRegex.Match(parts[2]);

                    if(videoIdMatch.Length > 0 && videoIdMatch.Groups.Count == 3)
                    {
                        videoId = videoIdMatch.Groups[2].Value;
                    }*/

                    CaptureTimeMap cap = new CaptureTimeMap()
                    {
                        Timestamp = timestamp,
                        EndTimestamp = endTimestamp,
                        GroupCount = groupCount,
                        UniqCount = uniqCount,
                        Original = parts[0],
                        Mimetype = parts[1],
                        Essence = essence,//,
                        EssenceType = essenceTypeString//,
                                                       //Dupecount = int.Parse(parts[7])
                    };



                    db.Insert(cap);

                    Console.WriteLine(String.Join("     ,    ", parts));

                    /*if (i > 100)
                    {
                        break;
                    }*/
                }
            }
            catch
            {
                Console.WriteLine("ERROR. Presumably file ended abruptly/didn't download completely.");
            }
            db.Commit();
            db.Close();
            db.Dispose();
        }

        static string[] splitLine(string line)
        {
            line = line.Trim("[], ".ToCharArray());
            int lineLength = line.Length;

            List<string> parts = new List<string>();
            int partindex = 0;
            bool isInsideQuote = false;
            string currentChar = "";
            string currentCharRaw = "";
            string currentPart = "";
            for (int k = 0; k < lineLength; k++)
            {
                if (isInsideQuote && line[k].ToString() == "\\")
                {
                    k++;
                    currentChar = "\\" + line[k].ToString();
                    currentCharRaw = line[k].ToString();
                }
                else
                {
                    currentChar = currentCharRaw = line[k].ToString();
                }

                if (currentChar == "\"")
                {
                    if (isInsideQuote)
                    {
                        parts.Add(currentPart);
                        currentPart = "";
                        isInsideQuote = false;
                    }
                    else
                    {
                        isInsideQuote = true;
                    }
                }
                else if (isInsideQuote)
                {
                    currentPart += currentCharRaw;
                }


            }
            return parts.ToArray();
        }
    }
}
