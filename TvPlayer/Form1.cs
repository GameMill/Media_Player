using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TvPlayer
{
    public partial class Form1 : Form
    {
       // public static string Settings.Instance.TvShowsRoot { get; } = @"\\192.168.0.3\Project\Chris\";
        public static List<Show> Shows { get; set; } = TvPlayer.Show.Load();

        
        public Show CurrentShow { 
            get 
            {
                TvPlayer.Show Temp = null;
                foreach (var Test in Shows)
                {
                    if (Test.Current)
                    {
                        Temp = Test;
                        break;
                    }
                }
                if (Temp == null)
                    Temp = Shows[0];
                return Temp;
            }
        }

        public static List<TvPlayer.Episode> Episodes { get; set; }
        public static TvPlayer.Current CurrentEpisode { get; set; }

        public void LoadCurrentShowData()
        {
            Episodes = TvPlayer.Episode.Load(CurrentShow);
            CurrentEpisode = TvPlayer.Current.Load(CurrentShow);
            SetupPlayer();
        }
        Timer Saver = new Timer();


        public Form1()
        {
            InitializeComponent();

            
                
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.OnApplicationExit);
            Core.Initialize();
            libvlc = new LibVLC(enableDebugLogs: false);
            _SetupPlayerDelagater = new SetupPlayerDelagater(SetupPlayer);
            listBox1.DataSource = Shows;
            LoadCurrentShowData();


            Saver.Interval = 5000;
            Saver.Tick += Saver_Tick;
            Saver.Start();

            listBox1.SelectedItem = CurrentShow;
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            buttonHook_Click();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            Saver_Tick(null,null);
        }

        public SetupPlayerDelagater _SetupPlayerDelagater;


        private void Saver_Tick(object sender, EventArgs e)
        {
            if (videoView1.MediaPlayer == null || CurrentEpisode == null)
                return;
            CurrentEpisode.Position = videoView1.MediaPlayer.Position;
            CurrentEpisode.Save(CurrentShow);
            UpdateTitle();
        }
        LibVLC libvlc { get; set; }

        string CurrentShowName { get; set; }
        public void SetupPlayer()
        {
            if(videoView1.MediaPlayer == null)
            {

                videoView1.MediaPlayer = new MediaPlayer(libvlc);

                videoView1.MediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
                videoView1.MediaPlayer.EndReached += MediaPlayer_EndReached;
                SetupAudio();

            }
            System.Threading.ThreadPool.QueueUserWorkItem(_ => {
                    if (CurrentShow.Name != CurrentShowName)
                    {
                        CurrentShowName = CurrentShow.Name; // add but of delay fixs bug
                    }
                    
                    var media = new Media(libvlc, new Uri(Settings.Instance.TvShowsRoot + CurrentShow.Name + "\\" + CurrentEpisode.CurrentEpisode.Season + "\\" + CurrentEpisode.CurrentEpisode.Name));

                    videoView1.MediaPlayer.Play(media);
                    videoView1.MediaPlayer.Position = CurrentEpisode.Position;
                    videoView1.MediaPlayer.Volume = CurrentEpisode.Volume;
             });

            //videoView1.MediaPlayer = mediaplayer;
            
            UpdateTitle();
        }

        private void MediaPlayer_EncounteredError(object sender, EventArgs e)
        {
            
            MessageBox.Show(libvlc.LastLibVLCError);
        }

        public void UpdateTitle()
        {
            this.Text = CurrentEpisode.CurrentEpisode.Name + " - " + CurrentEpisode.Position + " - " + CurrentEpisode.Volume;

        }
        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            MediaPlayer_EndReached(sender, e, 1);
        }
        private void MediaPlayer_EndReached(object sender, EventArgs e, int Add)
        {
            Saver.Stop();
            
           var NextEpisode = CurrentEpisode.CurrentEpisode.EpisodeNumber + Add;
           if (Episodes.Count <= NextEpisode)
           {
                NextEpisode = 0;
           }
           else if(NextEpisode < 0)
           {
                NextEpisode = Episodes.Count - 1;
           }
            CurrentEpisode.CurrentEpisode = Episodes[NextEpisode];
            CurrentEpisode.Position = 0;
            CurrentEpisode.Save(CurrentShow);
            Invoke(_SetupPlayerDelagater);
            Saver.Start();

        }
        public delegate void SetupPlayerDelagater();


        private void buttonHook_Click()
        {
            // Hooks only into specified Keys (here "A" and "B").
            _globalKeyboardHook = new GlobalKeyboardHook(new Keys[] { Keys.Escape, Keys.Enter,Keys.Space, Keys.H , Keys.Oemplus,Keys.OemMinus,Keys.D1,Keys.D2,Keys.F,Keys.R});
            _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
        }

        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            
            // EDT: No need to filter for VkSnapshot anymore. This now gets handled
            // through the constructor of GlobalKeyboardHook(...).
            if ((e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown) && IsOverForm())
            {
                if (e.KeyboardData.Key == Keys.Space)
                {
                    if (videoView1.MediaPlayer.IsPlaying)
                        videoView1.MediaPlayer.Pause();
                    else
                        videoView1.MediaPlayer.Play();
                    e.Handled = true;
                }
                else if (e.KeyboardData.Key == Keys.H)
                {
                    HideControlls(!button1.Visible);
                    e.Handled = true;
                }
                else if (e.KeyboardData.Key == Keys.Oemplus)
                {
                    videoView1.MediaPlayer.Volume = Math.Min(Math.Max(0, videoView1.MediaPlayer.Volume + 5), 500);

                    CurrentEpisode.Volume = videoView1.MediaPlayer.Volume;

                    VolumeUpdated();
                    e.Handled = true;
                }
                else if (e.KeyboardData.Key == Keys.OemMinus)
                {
                    videoView1.MediaPlayer.Volume = Math.Min(Math.Max(0, videoView1.MediaPlayer.Volume - 5), 500);
                    CurrentEpisode.Volume = videoView1.MediaPlayer.Volume;
                    VolumeUpdated();
                    e.Handled = true;
                }
                else if (e.KeyboardData.Key == Keys.Escape)
                {
                    button1_Click(null, null);
                    e.Handled = true;
                }
                else if (e.KeyboardData.Key == Keys.D1)
                {
                    MediaPlayer_EndReached(null, null, -1);
                    e.Handled = true;
                }
                else if (e.KeyboardData.Key == Keys.D2)
                {
                    MediaPlayer_EndReached(null, null);
                    e.Handled = true;
                }
                else if (e.KeyboardData.Key == Keys.F)
                {
                    videoView1.MediaPlayer.Position += 0.01f;
                    e.Handled = true;
                }
                else if (e.KeyboardData.Key == Keys.R)
                {
                    videoView1.MediaPlayer.Position -= 0.01f;
                    e.Handled = true;
                }
            }
        }
        bool IsFullscreen = false;
        private static GlobalKeyboardHook _globalKeyboardHook;

        public void VolumeUpdated() { UpdateTitle(); }

        private void button1_Click(object sender, EventArgs e)
        {
            GoFullscreen(!IsFullscreen);
        }
        private void GoFullscreen(bool fullscreen)
        {

            if (fullscreen)
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.Bounds = Screen.FromControl(this).Bounds;
                HideControlls(false);
                videoView1.Focus();
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                HideControlls(true);
            }
            IsFullscreen = fullscreen;
        }

        public void HideControlls(bool Visible)
        {
            button1.Visible = Visible;
            listBox1.Visible = Visible;
            button2.Visible = Visible;
            label1.Visible = Visible;
        }

        public bool IsOverForm()
        {
           return ClientRectangle.Contains(
           Form.MousePosition.X - Location.X,
           Form.MousePosition.Y - Location.Y);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedShow = ((TvPlayer.Show)listBox1.SelectedItem);

            Saver.Stop();
            Saver_Tick(null,null);
            CurrentShow.Current = false;
            selectedShow.Current = true;
            LoadCurrentShowData();
            SetupPlayer();
            TvPlayer.Show.Save(Shows);
            Saver_Tick(null, null);
            Saver.Start();
            //CurrentShow = selectedShow;


        }

        private void button2_Click(object sender, EventArgs e)
        {
            var ShowsFoulders = System.IO.Directory.GetDirectories(Settings.Instance.TvShowsRoot);
            List<TvPlayer.Show> shows = new List<Show>();
            
            foreach (var item in ShowsFoulders)
            {
                var Name = item.Substring(Settings.Instance.TvShowsRoot.Length);
                if (!Name.StartsWith("_"))
                    shows.Add(new TvPlayer.Show() { Name = Name, Current = (CurrentShow.Name== Name) }); ;
            }
            Form1.Shows = shows;
            TvPlayer.Show.Save(shows);
            foreach (var show in Form1.Shows)
            {
                updateEpisodes(show);
            }
            TvPlayer.Form1.Shows = TvPlayer.Show.Load();
            LoadCurrentShowData();
            SetupPlayer();
        }

        public void updateEpisodes(Show show)
        {
            List<TvPlayer.Episode> AllEpisodes = new List<Episode>();
            var seasons = System.IO.Directory.GetDirectories(Settings.Instance.TvShowsRoot + show.Name);
            foreach (var season in seasons)
            {
                var Name = season.Substring((Settings.Instance.TvShowsRoot + show.Name + "\\").Length);
                if(!Name.StartsWith("_"))
                {
                    foreach (var item in System.IO.Directory.GetFiles(season))
                    {
                        var EpisodeName = item.Substring((season+"\\").Length);
                        if (!EpisodeName.StartsWith("_"))
                        {
                            AllEpisodes.Add(new Episode() { Season = Name, Name = EpisodeName });
                        }
                    }
                }
            }
            AllEpisodes.OrderBy(a => a.Name);
            for (int i = 0; i < AllEpisodes.Count; i++)
            {
                AllEpisodes[i].EpisodeNumber = i;
            }
            TvPlayer.Episode.Save(AllEpisodes, show);
        }


        public void SetupAudio()
        {
            foreach (var item in videoView1.MediaPlayer.AudioOutputDeviceEnum)
            {
                if (item.Description.Contains("LG TV"))
                {
                    videoView1.MediaPlayer.SetOutputDevice(item.DeviceIdentifier);
                }
            } 
        }
    }
}
