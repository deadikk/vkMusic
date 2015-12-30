using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vkMusic
{
    class Song
    {
        public string Artist { get; set; }
        public string Name { get; set; }
        public string url { get; set; }
        public string LyricsId { get; set; }
        public bool LyricsExist => LyricsId != String.Empty;
    }
}
