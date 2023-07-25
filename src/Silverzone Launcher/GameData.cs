using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silverzone_Launcher {
    public class GameData {
        //url to download data
        public string URL;
        //data string downloaded from url
        public string? data;

        //processed data
        public string? name;
        public string? id;
        public string? desc;

        public string? download;
        public string? zip;
        public string? exe;

        public string? latestVersion;
        public string? version;


        public GameData(string _url) { 
            URL = _url;
        }
    }
    
}
