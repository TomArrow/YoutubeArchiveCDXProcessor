using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeArchiveCDXProcessor
{
    public class CaptureCDX
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Urlkey { get; set; }
        [Indexed]
        public string EssenceType { get; set; } // for example "youtube"
        [Indexed]
        public string Essence { get; set; } // for example a video Id.
        [Indexed]
        public Int64 Timestamp { get; set; }
        [Indexed]
        public string Original { get; set; }
        public string Mimetype { get; set; }
        public int Statuscode { get; set; }
        public string Digest { get; set; }
        [Indexed]
        public int Length { get; set; }
        public int Dupecount { get; set; }

    }
}
