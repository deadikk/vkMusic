using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vkMusic
{
    class Song
    {
        public Song() { }

        public Song(string a, string n, string u)
        {
            Artist = a;
            Name = n;
            url = u;
        }

        public string Artist { get; set; }
        public string Name { get; set; }
        public string url { get; set; }
    }
}
