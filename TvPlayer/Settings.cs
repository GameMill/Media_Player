using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TvPlayer
{
    class Settings
    {

        private static Settings _instance { get; set; }
        public static Settings Instance { get
            {
                if (_instance != null)
                    return _instance;

                if (System.IO.File.Exists("settings.db"))
                    _instance = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(System.IO.File.ReadAllText("settings.db"));
                else { _instance = new Settings(); }
                return _instance;
            }
        }

        public int DefaultVolume { get; internal set; } = 100;
        public string TvShowsRoot { get; set; } = "";

        public void Save()
        {
            System.IO.File.WriteAllText("settings.db", Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented));
        }
    }
}
