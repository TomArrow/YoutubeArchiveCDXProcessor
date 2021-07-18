using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeArchiveCDXProcessor
{
    public class Capture
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Urlkey { get; set; }
        public string Timestamp { get; set; }
        public string Original { get; set; }
        public string Mimetype { get; set; }
        public int Statuscode { get; set; }
        public string Digest { get; set; }
        public int Length { get; set; }
        public int Dupecount { get; set; }

    }
}
