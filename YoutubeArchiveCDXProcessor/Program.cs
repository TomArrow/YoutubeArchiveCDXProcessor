using SQLite;
using System;
using System.Collections.Generic;
using System.IO;

namespace YoutubeArchiveCDXProcessor
{
    class Program
    {
        static void Main(string[] args)
        {

            ParseData();

        }

        static void ParseData()
        {

            string[] lines = File.ReadAllLines("cdx@url=youtube.com%2Fwatch%2A&output=json.json");




            var db = new SQLiteConnection("data.db");
            db.CreateTable<Capture>();

            db.BeginTransaction();

            // i=1 bc ignore first line
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                line = line.Trim("[], ".ToCharArray());
                int lineLength = line.Length;

                List<string> parts = new List<string>();
                int partindex = 0;
                bool isInsideQuote = false;
                string currentChar = "";
                string currentCharRaw = "";
                string currentPart = "";
                for(int k = 0; k < lineLength; k++)
                {
                    if(isInsideQuote && line[k].ToString() == "\\")
                    {
                        k++;
                        currentChar = "\\"+line[k].ToString();
                        currentCharRaw = line[k].ToString();
                    } else
                    {
                        currentChar = currentCharRaw = line[k].ToString();
                    }

                    if(currentChar == "\"")
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
                    else if(isInsideQuote)
                    {
                        currentPart += currentCharRaw;
                    }


                }

                int statuscode=0, length=0;
                int.TryParse(parts[4], out statuscode);
                int.TryParse(parts[6], out length);

                Capture cap = new Capture() { 
                    Urlkey = parts[0],
                    Timestamp = parts[1],
                    Original = parts[2],
                    Mimetype = parts[3],
                    Statuscode = statuscode,
                    Digest = parts[5],
                    Length = length//,
                    //Dupecount = int.Parse(parts[7])
                };

                db.Insert(cap);

                Console.WriteLine(String.Join("     ,    ",parts));
                /*
                if (i > 100)
                {
                    break;
                }*/
            }
            db.Commit();
            db.Close();
            db.Dispose();
            Console.ReadKey();
        }
    }
}
