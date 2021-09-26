using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TvPlayer
{
    public class Show
    {
        public string Name { get; set; }
        public bool Current { get; set; }
        public static List<Show> Load()
        {
            var Data = System.IO.File.ReadAllText(Settings.Instance.TvShowsRoot + "Shows.txt");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Show>>(Data);
        }
        public static void Save(List<Show> Shows)
        {
            System.IO.File.WriteAllText(Settings.Instance.TvShowsRoot + "Shows.txt", Newtonsoft.Json.JsonConvert.SerializeObject(Shows, Newtonsoft.Json.Formatting.Indented));
        }
    }

    public class Current
    {
        public float Position { get; set; }
        public Episode CurrentEpisode { get; set; }
        public int Volume { get; set; }

        public static Current Load(Show show)
        {
            if (!System.IO.File.Exists(Settings.Instance.TvShowsRoot + show.Name + @"\Current.txt"))
            {
                (new Current() { CurrentEpisode = Form1.Episodes[0] , Volume=Settings.Instance.DefaultVolume }).Save(show);
            }
            var Data = System.IO.File.ReadAllText(Settings.Instance.TvShowsRoot + show.Name + @"\Current.txt");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Current>(Data);
        }

        public void Save(Show show)
        {
            System.IO.File.WriteAllText(Settings.Instance.TvShowsRoot + show.Name + @"\Current.txt", Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented));
        }
    }

    public class Episode
    {
        public string Season { get; set; }
        public string Name { get; set; }
        public int EpisodeNumber { get; set; }

        public static List<Episode> Load(Show show)
        {
            var Data = System.IO.File.ReadAllText(Settings.Instance.TvShowsRoot+ show.Name + @"\Episodes.txt");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Episode>>(Data);
        }

        public static void Save(List<Episode> Episodes, Show show)
        {
            System.IO.File.WriteAllText(Settings.Instance.TvShowsRoot + show.Name + @"\Episodes.txt", Newtonsoft.Json.JsonConvert.SerializeObject(Episodes, Newtonsoft.Json.Formatting.Indented));
        }
    }


}
