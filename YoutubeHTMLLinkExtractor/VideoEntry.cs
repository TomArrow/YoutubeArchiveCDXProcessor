using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeHTMLLinkExtractor
{
    public class VideoEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string VideoId { get; set; }
        [Indexed]
        public string VideoName { get; set; }
        [Indexed]
        public string VideoNameSafe { get; set; }
        [Indexed]
        public Int64 Timestamp { get; set; }
        [Indexed]
        public int Year { get; set; }

    }
}
