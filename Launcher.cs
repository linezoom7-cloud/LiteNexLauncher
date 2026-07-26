using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace LiteNexLauncher
{
    // ══════════════════════════════════════════════════════════════════════════
    //  SOUND SYSTEM (Synthesizer Sound Effects)
    // ══════════════════════════════════════════════════════════════════════════
    public static class SoundSystem
    {
        public static bool Enabled = false;

        public static void PlayClick()
        {
            if (!Enabled) return;
            ThreadPool.QueueUserWorkItem((_) => GenerateAndPlayTone(880, 0.035, 0.18, 0.008));
        }

        public static void PlayHover()
        {
            if (!Enabled) return;
            ThreadPool.QueueUserWorkItem((_) => GenerateAndPlayTone(1200, 0.018, 0.06, 0.004));
        }

        public static void PlaySuccess()
        {
            if (!Enabled) return;
            ThreadPool.QueueUserWorkItem((_) =>
            {
                GenerateAndPlayTone(523, 0.07, 0.22, 0.015);
                Thread.Sleep(50);
                GenerateAndPlayTone(659, 0.07, 0.22, 0.015);
                Thread.Sleep(50);
                GenerateAndPlayTone(784, 0.12, 0.30, 0.025);
            });
        }

        private static void GenerateAndPlayTone(double frequencyHz, double durationSec, double volume, double fadeSec)
        {
            try
            {
                int sampleRate = 44100;
                int numSamples = (int)(sampleRate * durationSec);
                int fadeSamples = (int)(sampleRate * fadeSec);
                byte[] pcmData = new byte[numSamples * 2];

                for (int i = 0; i < numSamples; i++)
                {
                    double t = (double)i / sampleRate;
                    double wave = Math.Sin(2.0 * Math.PI * frequencyHz * t);

                    double envelope = 1.0;
                    if (i < fadeSamples) envelope = (double)i / fadeSamples;
                    else if (i > numSamples - fadeSamples) envelope = (double)(numSamples - i) / fadeSamples;

                    short sample = (short)(wave * volume * envelope * 32767.0);
                    pcmData[i * 2]     = (byte)(sample & 0xFF);
                    pcmData[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
                }

                byte[] wavHeader = CreateWavHeader(pcmData.Length, sampleRate, 1, 16);
                byte[] fullWav = new byte[wavHeader.Length + pcmData.Length];
                Buffer.BlockCopy(wavHeader, 0, fullWav, 0, wavHeader.Length);
                Buffer.BlockCopy(pcmData, 0, fullWav, wavHeader.Length, pcmData.Length);

                using (MemoryStream ms = new MemoryStream(fullWav))
                using (System.Media.SoundPlayer sp = new System.Media.SoundPlayer(ms))
                {
                    sp.PlaySync();
                }
            }
            catch { }
        }

        private static byte[] CreateWavHeader(int dataLen, int sampleRate, short channels, short bitsPerSample)
        {
            byte[] header = new byte[44];
            int totalLen = dataLen + 36;
            int byteRate = sampleRate * channels * (bitsPerSample / 8);
            short blockAlign = (short)(channels * (bitsPerSample / 8));

            header[0] = 0x52; header[1] = 0x49; header[2] = 0x46; header[3] = 0x46;
            header[4] = (byte)(totalLen & 0xFF); header[5] = (byte)((totalLen >> 8) & 0xFF);
            header[6] = (byte)((totalLen >> 16) & 0xFF); header[7] = (byte)((totalLen >> 24) & 0xFF);
            header[8] = 0x57; header[9] = 0x41; header[10] = 0x56; header[11] = 0x45;
            header[12] = 0x66; header[13] = 0x6D; header[14] = 0x74; header[15] = 0x20;
            header[16] = 16; header[17] = 0; header[18] = 0; header[19] = 0;
            header[20] = 1; header[21] = 0;
            header[22] = (byte)channels; header[23] = 0;
            header[24] = (byte)(sampleRate & 0xFF); header[25] = (byte)((sampleRate >> 8) & 0xFF);
            header[26] = (byte)((sampleRate >> 16) & 0xFF); header[27] = (byte)((sampleRate >> 24) & 0xFF);
            header[28] = (byte)(byteRate & 0xFF); header[29] = (byte)((byteRate >> 8) & 0xFF);
            header[30] = (byte)((byteRate >> 16) & 0xFF); header[31] = (byte)((byteRate >> 24) & 0xFF);
            header[32] = (byte)blockAlign; header[33] = 0;
            header[34] = (byte)bitsPerSample; header[35] = 0;
            header[36] = 0x64; header[37] = 0x61; header[38] = 0x74; header[39] = 0x61;
            header[40] = (byte)(dataLen & 0xFF); header[41] = (byte)((dataLen >> 8) & 0xFF);
            header[42] = (byte)((dataLen >> 16) & 0xFF); header[43] = (byte)((dataLen >> 24) & 0xFF);
            return header;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  THEME MANAGER
    // ══════════════════════════════════════════════════════════════════════════
    public static class ThemeManager
    {
        public static Color C_BG       = Color.FromArgb(  9,  8, 18);
        public static Color C_SIDEBAR  = Color.FromArgb( 13, 12, 24);
        public static Color C_TITLEBAR = Color.FromArgb(  7,  6, 14);
        public static Color C_CARD     = Color.FromArgb( 19, 17, 34);
        public static Color C_CARD2    = Color.FromArgb( 26, 23, 46);
        public static Color C_BORDER   = Color.FromArgb( 42, 38, 72);
        public static Color C_PURPLE   = Color.FromArgb(139, 92, 246);
        public static Color C_PURPLE_D = Color.FromArgb( 99, 52, 210);
        public static Color C_PURPLE_L = Color.FromArgb(167,139, 250);
        public static Color C_BLUE     = Color.FromArgb( 59,130, 246);
        public static Color C_CYAN     = Color.FromArgb( 34,211, 238);
        public static Color C_EMERALD  = Color.FromArgb( 16,185, 129);
        public static Color C_TEXT     = Color.FromArgb(241,245, 249);
        public static Color C_MUTED    = Color.FromArgb(148,163, 184);
        public static Color C_CONSOLE  = Color.FromArgb(  6,  5, 14);

        public static void SetTheme(int themeIndex)
        {
            switch (themeIndex)
            {
                case 1: // Cyberpunk Cyan
                    C_PURPLE   = Color.FromArgb(  6,182, 212);
                    C_PURPLE_D = Color.FromArgb(  8,145, 178);
                    C_PURPLE_L = Color.FromArgb(103,232, 249);
                    break;
                case 2: // Emerald Matrix
                    C_PURPLE   = Color.FromArgb( 16,185, 129);
                    C_PURPLE_D = Color.FromArgb(  4,120,  87);
                    C_PURPLE_L = Color.FromArgb(110,231, 183);
                    break;
                case 3: // Crimson Red
                    C_PURPLE   = Color.FromArgb(244, 63,  94);
                    C_PURPLE_D = Color.FromArgb(190, 18,  60);
                    C_PURPLE_L = Color.FromArgb(253,164, 175);
                    break;
                case 4: // Sunset Amber
                    C_PURPLE   = Color.FromArgb(245,158,  11);
                    C_PURPLE_D = Color.FromArgb(180, 83,   9);
                    C_PURPLE_L = Color.FromArgb(252,211,  77);
                    break;
                default: // Midnight Purple
                    C_PURPLE   = Color.FromArgb(139, 92, 246);
                    C_PURPLE_D = Color.FromArgb( 99, 52, 210);
                    C_PURPLE_L = Color.FromArgb(167,139, 250);
                    break;
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  MAIN LAUNCHER FORM (LiteNex Client v6.0 Ultimate Edition)
    // ══════════════════════════════════════════════════════════════════════════
    public class MainForm : Form
    {
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("kernel32.dll")] private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, IntPtr dwMinimumWorkingSetSize, IntPtr dwMaximumWorkingSetSize);

        // ── Controls ───────────────────────────────────────────────────────────
        private Panel   sidebarPanel, mainPanel, playPanel, versionsPanel, serversPanel, settingsPanel, userCard;
        private PictureBox logoBox, pbUserAvatar;
        private TextBox txtUsername, txtSearchVer, txtServerIp, txtCustomJvmArgs, txtCustomServerName, txtWallpaperPath;
        private ComboBox cbVersions, cbJavaPath, cbClientType, cbResolution, cbProfiles, cbThemes;
        private CheckBox chkFullscreen, chkFpsBoost;
        private Button  btnPlay, btnNavPlay, btnNavVersions, btnNavServers, btnNavSettings, btnNavPvpMods, btnSoundToggle;
        private Panel   progressBg, progressFill;
        private Label   lblStatus, lblUserName;
        private RichTextBox txtConsole;
        private TrackBar ramSlider;
        private Label lblRamVal, lblSysMonitor;
        private Panel ramMonitorFill;
        private FlowLayoutPanel flowVersionsList, flowModsList, flowServersList, flowSavesList;
        private System.Windows.Forms.Timer sysMonitorTimer;

        private Panel   pvpModsPanel, pnlHudPreview;
        private bool    pvpCpsEnabled = true, pvpKeystrokesEnabled = true, pvpArmorStatusEnabled = true;
        private bool    pvpPotionHudEnabled = true, pvpCompassEnabled = true, pvpCrosshairEnabled = true, pvpToggleSprintEnabled = true;
        private string  gameDir, versionsDir, javaPathDetected, customWallpaperPath = "";
        private Image customWallpaperImg = null;
        private Dictionary<int, string> detectedJavas = new Dictionary<int, string>();
        private List<string> allMojangVersions = new List<string>();
        private List<string> savedProfiles = new List<string> { "LitePlayer", "ProGamer", "Steve", "Alex" };
        private Process activeMcProcess = null;

        private List<Tuple<string, string>> savedServers = new List<Tuple<string, string>>
        {
            Tuple.Create("Hypixel Network", "mc.hypixel.net"),
            Tuple.Create("CraftRise", "play.craftrise.tc"),
            Tuple.Create("SonOyuncu", "play.sonoyuncu.network"),
            Tuple.Create("Minehut", "minehut.com")
        };

        public MainForm()
        {
            try { ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072|(SecurityProtocolType)768|SecurityProtocolType.Tls; } catch {}
            this.DoubleBuffered = true;
            SetupLauncherPaths();
            InitializeComponent();
            LoadVersionsAsync();
            DetectJavaAndSystem();
            StartSystemMonitor();
            CheckForGitHubUpdatesAsync(silent: true);
        }

        private void SetupLauncherPaths()
        {
            gameDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".litenex");
            versionsDir = Path.Combine(gameDir, "versions");
            EnsureDir(gameDir); EnsureDir(versionsDir);
            EnsureDir(Path.Combine(gameDir, "mods"));
            EnsureDir(Path.Combine(gameDir, "resourcepacks"));
            EnsureDir(Path.Combine(gameDir, "saves"));
            EnsureDir(Path.Combine(gameDir, "screenshots"));
            LoadProfilesFromDisk();
            LoadServersFromDisk();
        }

        private void SaveConfigToDisk()
        {
            try
            {
                var cfg = new Dictionary<string, object>();
                cfg["RamGb"] = ramSlider != null ? ramSlider.Value : 4;
                cfg["ThemeIndex"] = cbThemes != null ? cbThemes.SelectedIndex : 0;
                cfg["ResIndex"] = cbResolution != null ? cbResolution.SelectedIndex : 0;
                cfg["Fullscreen"] = chkFullscreen != null ? chkFullscreen.Checked : false;
                cfg["FpsBoost"] = chkFpsBoost != null ? chkFpsBoost.Checked : true;
                cfg["JvmArgs"] = txtCustomJvmArgs != null ? txtCustomJvmArgs.Text : "";
                cfg["WallpaperPath"] = txtWallpaperPath != null ? txtWallpaperPath.Text : "";
                string cFile = Path.Combine(gameDir, "config.json");
                JavaScriptSerializer jss = new JavaScriptSerializer();
                File.WriteAllText(cFile, jss.Serialize(cfg));
            }
            catch { }
        }

        private void LoadConfigFromDisk()
        {
            try
            {
                string cFile = Path.Combine(gameDir, "config.json");
                if (File.Exists(cFile))
                {
                    string json = File.ReadAllText(cFile);
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    var cfg = jss.Deserialize<Dictionary<string, object>>(json);
                    if (cfg != null)
                    {
                        if (cfg.ContainsKey("RamGb") && ramSlider != null)
                        {
                            int r = Convert.ToInt32(cfg["RamGb"]);
                            if (r >= ramSlider.Minimum && r <= ramSlider.Maximum) ramSlider.Value = r;
                        }
                        if (cfg.ContainsKey("ThemeIndex") && cbThemes != null)
                        {
                            int t = Convert.ToInt32(cfg["ThemeIndex"]);
                            if (t >= 0 && t < cbThemes.Items.Count) cbThemes.SelectedIndex = t;
                        }
                        if (cfg.ContainsKey("ResIndex") && cbResolution != null)
                        {
                            int res = Convert.ToInt32(cfg["ResIndex"]);
                            if (res >= 0 && res < cbResolution.Items.Count) cbResolution.SelectedIndex = res;
                        }
                        if (cfg.ContainsKey("Fullscreen") && chkFullscreen != null)
                        {
                            chkFullscreen.Checked = Convert.ToBoolean(cfg["Fullscreen"]);
                        }
                        if (cfg.ContainsKey("FpsBoost") && chkFpsBoost != null)
                        {
                            chkFpsBoost.Checked = Convert.ToBoolean(cfg["FpsBoost"]);
                        }
                        if (cfg.ContainsKey("JvmArgs") && txtCustomJvmArgs != null)
                        {
                            txtCustomJvmArgs.Text = cfg["JvmArgs"].ToString();
                        }
                        if (cfg.ContainsKey("WallpaperPath") && txtWallpaperPath != null)
                        {
                            string wp = cfg["WallpaperPath"].ToString();
                            txtWallpaperPath.Text = wp;
                            ApplyCustomWallpaper(wp);
                        }
                    }
                }
            }
            catch { }
        }

        private void ApplyCustomWallpaper(string path)
        {
            customWallpaperPath = path;
            if (customWallpaperImg != null)
            {
                try { mainPanel.BackgroundImage = null; customWallpaperImg.Dispose(); } catch { }
                customWallpaperImg = null;
            }
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                try
                {
                    customWallpaperImg = LoadImageSafely(path);
                    mainPanel.BackgroundImage = customWallpaperImg;
                    mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
                }
                catch { }
            }
            else
            {
                mainPanel.BackgroundImage = null;
            }
        }

        private void StartSystemMonitor()
        {
            sysMonitorTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            sysMonitorTimer.Tick += (s, e) =>
            {
                try
                {
                    long sysProcRamMb = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;
                    int pct = Math.Min(100, Math.Max(5, (int)(sysProcRamMb * 100 / 1500)));

                    if (lblSysMonitor != null && ramMonitorFill != null)
                    {
                        lblSysMonitor.Text = string.Format("📊 Sistem RAM: {0} MB (%{1})", sysProcRamMb, pct);
                        ramMonitorFill.Width = (int)(212 * (pct / 100.0));
                    }
                }
                catch { }
            };
            sysMonitorTimer.Start();
        }

        private void LoadProfilesFromDisk()
        {
            try
            {
                string pFile = Path.Combine(gameDir, "profiles.json");
                if (File.Exists(pFile))
                {
                    string json = File.ReadAllText(pFile);
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    var list = jss.Deserialize<List<string>>(json);
                    if (list != null && list.Count > 0) savedProfiles = list;
                }
            }
            catch { }
            if (savedProfiles == null || savedProfiles.Count == 0) savedProfiles = new List<string> { "LitePlayer" };
        }

        private void SaveProfilesToDisk()
        {
            try
            {
                string pFile = Path.Combine(gameDir, "profiles.json");
                JavaScriptSerializer jss = new JavaScriptSerializer();
                File.WriteAllText(pFile, jss.Serialize(savedProfiles));
            }
            catch { }
        }

        private void LoadServersFromDisk()
        {
            try
            {
                string sFile = Path.Combine(gameDir, "servers.json");
                if (File.Exists(sFile))
                {
                    string json = File.ReadAllText(sFile);
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    var rawList = jss.Deserialize<List<Dictionary<string, string>>>(json);
                    if (rawList != null && rawList.Count > 0)
                    {
                        savedServers.Clear();
                        foreach (var d in rawList)
                            if (d.ContainsKey("name") && d.ContainsKey("ip"))
                                savedServers.Add(Tuple.Create(d["name"], d["ip"]));
                    }
                }
            }
            catch { }
        }

        private void SaveServersToDisk()
        {
            try
            {
                string sFile = Path.Combine(gameDir, "servers.json");
                var list = new List<Dictionary<string, string>>();
                foreach (var s in savedServers)
                {
                    var d = new Dictionary<string, string>();
                    d["name"] = s.Item1; d["ip"] = s.Item2;
                    list.Add(d);
                }
                JavaScriptSerializer jss = new JavaScriptSerializer();
                File.WriteAllText(sFile, jss.Serialize(list));
            }
            catch { }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  UI INITIALIZATION
        // ══════════════════════════════════════════════════════════════════════
        private void InitializeComponent()
        {
            this.Text             = "LiteNex Client v6.0 Ultimate Edition";
            this.Size             = new Size(1260, 820);
            this.MinimumSize      = new Size(1040, 700);
            this.StartPosition    = FormStartPosition.CenterScreen;
            this.BackColor        = ThemeManager.C_BG;
            this.ForeColor        = ThemeManager.C_TEXT;
            this.Font             = new Font("Segoe UI", 9.5F);
            this.FormBorderStyle  = FormBorderStyle.None;

            string appDir      = AppDomain.CurrentDomain.BaseDirectory;
            string logoIcoPath = Path.Combine(appDir, "logo.ico");
            string logoPngPath = Path.Combine(appDir, "logo.png");
            Icon masterIcon = null;
            if (File.Exists(logoIcoPath)) { try { masterIcon = new Icon(logoIcoPath); this.Icon = masterIcon; } catch {} }
            if (masterIcon == null && File.Exists(logoPngPath)) { try { using(var b=new Bitmap(logoPngPath)) { masterIcon = Icon.FromHandle(b.GetHicon()); this.Icon = masterIcon; } } catch {} }

            // ── TITLE BAR ──────────────────────────────────────────────────────
            Panel titleBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = ThemeManager.C_TITLEBAR };
            MakeDraggable(titleBar);

            titleBar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (LinearGradientBrush lg = new LinearGradientBrush(new Rectangle(0, 43, titleBar.Width, 1), ThemeManager.C_PURPLE, ThemeManager.C_CYAN, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(lg, 0, 43, titleBar.Width, 1);
            };

            PictureBox tbIcon = new PictureBox { Location = new Point(14, 10), Size = new Size(24, 24), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Transparent };
            Image safeLogo = LoadImageSafely(logoPngPath);
            if (safeLogo != null) tbIcon.Image = safeLogo;
            else if (masterIcon != null) { try { tbIcon.Image = masterIcon.ToBitmap(); } catch {} }
            MakeDraggable(tbIcon);


            Label lblTitleText = new Label { Text = "LiteNex", Location = new Point(44, 0), Size = new Size(80, 44), Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = ThemeManager.C_TEXT, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent };
            MakeDraggable(lblTitleText);

            Label lblBadge = new Label { Text = "ULTIMATE v6.0", Location = new Point(125, 12), Size = new Size(115, 20), Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), ForeColor = ThemeManager.C_PURPLE_L, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.FromArgb(35, 139, 92, 246) };
            MakeDraggable(lblBadge);

            Label lblMojangStatus = new Label { Text = "🟢 Mojang Sunucuları: Online", Location = new Point(255, 12), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = ThemeManager.C_EMERALD, BackColor = Color.Transparent };
            MakeDraggable(lblMojangStatus);

            Panel winControls = new Panel { Dock = DockStyle.Right, Width = 138, BackColor = Color.Transparent };
            Button btnMin   = MakeTitleBtn("─", ThemeManager.C_MUTED, ThemeManager.C_CARD2);
            Button btnMax   = MakeTitleBtn("□", ThemeManager.C_MUTED, ThemeManager.C_CARD2);
            Button btnClose = MakeTitleBtn("✕", ThemeManager.C_MUTED, Color.FromArgb(225, 29, 72));

            btnMin.Location   = new Point(0, 0);
            btnMax.Location   = new Point(46, 0);
            btnClose.Location = new Point(92, 0);

            btnMin.Click   += (s, e) => { SoundSystem.PlayClick(); this.WindowState = FormWindowState.Minimized; };
            btnMax.Click   += (s, e) => { SoundSystem.PlayClick(); this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized; };
            btnClose.Click += (s, e) => { SoundSystem.PlayClick(); Application.Exit(); };

            winControls.Controls.Add(btnMin);
            winControls.Controls.Add(btnMax);
            winControls.Controls.Add(btnClose);

            titleBar.Controls.Add(winControls);
            titleBar.Controls.Add(tbIcon);
            titleBar.Controls.Add(lblTitleText);
            titleBar.Controls.Add(lblBadge);
            titleBar.Controls.Add(lblMojangStatus);

            // ── SIDEBAR ────────────────────────────────────────────────────────
            sidebarPanel = new Panel { Dock = DockStyle.Left, Width = 260, BackColor = ThemeManager.C_SIDEBAR };
            sidebarPanel.Paint += (s, e) =>
            {
                using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawLine(p, sidebarPanel.Width-1, 0, sidebarPanel.Width-1, sidebarPanel.Height);
            };

            Panel logoCard = new Panel { Location = new Point(16, 18), Size = new Size(228, 80), BackColor = ThemeManager.C_CARD };
            logoCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, logoCard.Width - 1, logoCard.Height - 1), 10))
                {
                    using (SolidBrush sb = new SolidBrush(ThemeManager.C_CARD)) e.Graphics.FillPath(sb, path);
                    using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawPath(p, path);
                }
                using (LinearGradientBrush lg = new LinearGradientBrush(new Rectangle(0, 10, 4, 60), ThemeManager.C_PURPLE, ThemeManager.C_CYAN, LinearGradientMode.Vertical))
                    e.Graphics.FillRectangle(lg, 0, 10, 4, 60);
            };

            logoBox = new PictureBox { Location = new Point(18, 18), Size = new Size(44, 44), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Transparent };
            if (safeLogo != null) logoBox.Image = (Image)safeLogo.Clone();
            else if (masterIcon != null)  { try { logoBox.Image = masterIcon.ToBitmap(); } catch {} }


            Label lblLogoName = new Label { Text = "LiteNex Client", Location = new Point(68, 17), Size = new Size(150, 24), Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = ThemeManager.C_TEXT, BackColor = Color.Transparent };
            Label lblLogoSub  = new Label { Text = "Ultimate Engine v6.0", Location = new Point(68, 41), Size = new Size(150, 18), Font = new Font("Segoe UI", 8F), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };

            logoCard.Controls.Add(logoBox);
            logoCard.Controls.Add(lblLogoName);
            logoCard.Controls.Add(lblLogoSub);
            sidebarPanel.Controls.Add(logoCard);

            Label lblNavSection = new Label { Text = "NAVİGASYON", Location = new Point(20, 118), AutoSize = true, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), ForeColor = Color.FromArgb(90,85,130), BackColor = Color.Transparent };
            sidebarPanel.Controls.Add(lblNavSection);

            btnNavPlay     = MakeNavBtn("🎮   Oyna",            138);
            btnNavVersions = MakeNavBtn("📦   Sürümler & Modlar", 188);
            btnNavServers  = MakeNavBtn("📡   Sunucu & Ping Testi",238);
            btnNavPvpMods  = MakeNavBtn("⚔️   PvP Client Modları", 288);
            btnNavSettings = MakeNavBtn("⚙️   Ayarlar & Temalar",  338);

            sidebarPanel.Controls.Add(btnNavPlay);
            sidebarPanel.Controls.Add(btnNavVersions);
            sidebarPanel.Controls.Add(btnNavServers);
            sidebarPanel.Controls.Add(btnNavPvpMods);
            sidebarPanel.Controls.Add(btnNavSettings);

            btnSoundToggle = new Button
            {
                Text = "🔇  Ses Efektleri: Kapalı",
                Location = new Point(16, 348),
                Size = new Size(228, 36),
                FlatStyle = FlatStyle.Flat,
                ForeColor = ThemeManager.C_MUTED,
                BackColor = Color.FromArgb(20, 30, 45),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSoundToggle.FlatAppearance.BorderSize = 0;
            btnSoundToggle.Click += (s, e) =>
            {
                SoundSystem.Enabled = !SoundSystem.Enabled;
                btnSoundToggle.Text = SoundSystem.Enabled ? "🔊  Ses Efektleri: Açık" : "🔇  Ses Efektleri: Kapalı";
                btnSoundToggle.ForeColor = SoundSystem.Enabled ? ThemeManager.C_PURPLE_L : ThemeManager.C_MUTED;
                btnSoundToggle.BackColor = SoundSystem.Enabled ? Color.FromArgb(20, 139, 92, 246) : Color.FromArgb(20, 30, 45);
                if (SoundSystem.Enabled) SoundSystem.PlayClick();
            };
            sidebarPanel.Controls.Add(btnSoundToggle);

            // ── Live RAM & System Monitor Card ─────────────────────────────────
            Panel sysCard = new Panel { Location = new Point(16, 396), Size = new Size(228, 48), BackColor = ThemeManager.C_CARD };
            sysCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, sysCard.Width - 1, sysCard.Height - 1), 6))
                {
                    using (SolidBrush sb = new SolidBrush(ThemeManager.C_CARD)) e.Graphics.FillPath(sb, path);
                    using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawPath(p, path);
                }
            };
            lblSysMonitor = new Label { Text = "📊 Sistem RAM: Hesaplanıyor...", Location = new Point(8, 8), AutoSize = true, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), ForeColor = ThemeManager.C_CYAN, BackColor = Color.Transparent };
            Panel ramMonitorBg   = new Panel { Location = new Point(8, 32), Size = new Size(212, 6), BackColor = Color.FromArgb(30, 40, 60) };
            ramMonitorFill = new Panel { Location = new Point(0, 0), Size = new Size(40, 6), BackColor = ThemeManager.C_CYAN };
            ramMonitorBg.Controls.Add(ramMonitorFill);
            sysCard.Controls.Add(lblSysMonitor); sysCard.Controls.Add(ramMonitorBg);
            sidebarPanel.Controls.Add(sysCard);

            Button btnCleanRam = new Button
            {
                Text = "🧹  RAM Optimize Et",
                Location = new Point(16, 450),
                Size = new Size(228, 36),
                FlatStyle = FlatStyle.Flat,
                ForeColor = ThemeManager.C_EMERALD,
                BackColor = Color.FromArgb(15, 16, 185, 129),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCleanRam.FlatAppearance.BorderSize = 0;
            btnCleanRam.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                try { SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, (IntPtr)(-1), (IntPtr)(-1)); } catch {}
                Log("[RAM] Sistem belleği optimize edildi ve gereksiz atıklar temizlendi. ✓", ThemeManager.C_EMERALD);
                SoundSystem.PlaySuccess();
            };
            sidebarPanel.Controls.Add(btnCleanRam);

            Button btnFpsBooster = new Button
            {
                Text = "⚡  FPS Booster: Kapalı",
                Location = new Point(16, 492),
                Size = new Size(228, 36),
                FlatStyle = FlatStyle.Flat,
                ForeColor = ThemeManager.C_CYAN,
                BackColor = Color.FromArgb(15, 34, 211, 238),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            bool isFpsBoosterActive = false;
            btnFpsBooster.FlatAppearance.BorderSize = 0;
            btnFpsBooster.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                isFpsBoosterActive = !isFpsBoosterActive;
                if (isFpsBoosterActive)
                {
                    btnFpsBooster.Text = "🚀  FPS Booster: Aktif 🟢";
                    btnFpsBooster.ForeColor = ThemeManager.C_EMERALD;
                    btnFpsBooster.BackColor = Color.FromArgb(40, 16, 185, 129);
                    try { Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High; } catch {}
                    GC.Collect();
                    Log("[TURBO] ⚡ LiteNex FPS Booster & İşlemci Önceliği YÜKSEK (High) moda alındı!", ThemeManager.C_EMERALD);
                    SoundSystem.PlaySuccess();
                }
                else
                {
                    btnFpsBooster.Text = "⚡  FPS Booster: Kapalı";
                    btnFpsBooster.ForeColor = ThemeManager.C_CYAN;
                    btnFpsBooster.BackColor = Color.FromArgb(15, 34, 211, 238);
                    try { Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal; } catch {}
                    Log("[TURBO] FPS Booster normale döndürüldü.", ThemeManager.C_MUTED);
                }
            };
            sidebarPanel.Controls.Add(btnFpsBooster);

            userCard = new Panel { Location = new Point(16, 710), Size = new Size(228, 64), BackColor = ThemeManager.C_CARD, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            userCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, userCard.Width - 1, userCard.Height - 1), 10))
                {
                    using (SolidBrush sb = new SolidBrush(ThemeManager.C_CARD)) e.Graphics.FillPath(sb, path);
                    using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawPath(p, path);
                }
                using (SolidBrush b = new SolidBrush(ThemeManager.C_EMERALD)) e.Graphics.FillEllipse(b, 54, 42, 10, 10);
            };

            pbUserAvatar = new PictureBox { Location = new Point(12, 12), Size = new Size(40, 40), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Transparent };
            lblUserName  = new Label { Text = "LitePlayer", Location = new Point(60, 12), Size = new Size(160, 20), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = ThemeManager.C_TEXT, BackColor = Color.Transparent };
            Label lblUserMode = new Label { Text = "Çevrimdışı Oyuncu", Location = new Point(60, 33), AutoSize = true, Font = new Font("Segoe UI", 8F), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };

            userCard.Controls.Add(pbUserAvatar);
            userCard.Controls.Add(lblUserName);
            userCard.Controls.Add(lblUserMode);
            sidebarPanel.Controls.Add(userCard);

            this.Load   += (s, e) => PositionUserCard();
            this.Resize += (s, e) => PositionUserCard();

            mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.C_BG };

            // ══════════════════════════════════════════════════════════════════
            //  TAB 1: PLAY PANEL
            // ══════════════════════════════════════════════════════════════════
            playPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            Panel heroCard = new Panel { Location = new Point(24, 16), Size = new Size(966, 340), BackColor = ThemeManager.C_CARD };
            heroCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, heroCard.Width - 1, heroCard.Height - 1), 12))
                {
                    using (LinearGradientBrush bg = new LinearGradientBrush(heroCard.ClientRectangle, Color.FromArgb(30, 20, 62), Color.FromArgb(14, 12, 32), LinearGradientMode.ForwardDiagonal))
                        e.Graphics.FillPath(bg, path);
                    using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawPath(p, path);
                }
                using (LinearGradientBrush tl = new LinearGradientBrush(new Rectangle(12, 0, heroCard.Width - 24, 3), ThemeManager.C_PURPLE, ThemeManager.C_CYAN, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(tl, 12, 0, heroCard.Width - 24, 3);
            };

            Label heroBadge = new Label { Text = "  ⚡  ULTRA HIGH PERFORMANCE  ·  C# NATIVE MOTORU  ", Location = new Point(24, 24), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = ThemeManager.C_PURPLE_L, BackColor = Color.Transparent };
            Label heroTitle = new Label { Text = "Sonsuz Dünyaları Keşfet", Location = new Point(24, 52), AutoSize = true, Font = new Font("Segoe UI", 26F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent };
            Label heroSub   = new Label { Text = "LiteNex ile Minecraft'ın tüm sürümlerini yüksek FPS, sıfır gecikme ve ücretsiz oyna.", Location = new Point(26, 112), AutoSize = true, Font = new Font("Segoe UI", 10F), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };

            Label lblProf = new Label { Text = "KAYITLI PROFİL", Location = new Point(24, 154), AutoSize = true, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
            cbProfiles = new ComboBox { Location = new Point(24, 172), Size = new Size(160, 32), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(16, 14, 32), ForeColor = ThemeManager.C_TEXT, Font = new Font("Segoe UI", 10F) };
            foreach (var p in savedProfiles) cbProfiles.Items.Add(p);
            if (cbProfiles.Items.Count > 0) cbProfiles.SelectedIndex = 0;

            Button btnAddProfile = new Button { Text = "➕", Location = new Point(190, 172), Size = new Size(32, 32), FlatStyle = FlatStyle.Flat, BackColor = ThemeManager.C_CARD2, ForeColor = ThemeManager.C_EMERALD, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAddProfile.FlatAppearance.BorderSize = 0;

            Button btnDeleteProfile = new Button { Text = "🗑️", Location = new Point(226, 172), Size = new Size(32, 32), FlatStyle = FlatStyle.Flat, BackColor = ThemeManager.C_CARD2, ForeColor = Color.FromArgb(225, 29, 72), Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnDeleteProfile.FlatAppearance.BorderSize = 0;

            cbProfiles.SelectedIndexChanged += (s, e) =>
            {
                if (cbProfiles.SelectedItem != null) txtUsername.Text = cbProfiles.SelectedItem.ToString();
            };

            btnAddProfile.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                string name = txtUsername.Text.Trim();
                if (string.IsNullOrEmpty(name)) return;
                if (!savedProfiles.Contains(name))
                {
                    savedProfiles.Add(name); cbProfiles.Items.Add(name); SaveProfilesToDisk();
                    cbProfiles.SelectedItem = name; Log("[PROFİL] '" + name + "' hesabı kaydedildi.", ThemeManager.C_EMERALD);
                }
            };

            btnDeleteProfile.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                if (cbProfiles.SelectedItem == null) return;
                string name = cbProfiles.SelectedItem.ToString();
                if (savedProfiles.Count <= 1) { MessageBox.Show("En az 1 profil kalmalıdır!", "LiteNex"); return; }
                savedProfiles.Remove(name); cbProfiles.Items.Remove(name); SaveProfilesToDisk();
                if (cbProfiles.Items.Count > 0) cbProfiles.SelectedIndex = 0;
                Log("[PROFİL] '" + name + "' silindi.", Color.Orange);
            };

            Label lblUH = new Label { Text = "OYUNCU ADI", Location = new Point(266, 154), AutoSize = true, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
            txtUsername = new TextBox { Location = new Point(266, 172), Size = new Size(160, 32), Text = cbProfiles.SelectedItem != null ? cbProfiles.SelectedItem.ToString() : "LitePlayer", BackColor = Color.FromArgb(16, 14, 32), ForeColor = ThemeManager.C_TEXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F) };

            txtUsername.TextChanged += (s, e) =>
            {
                string u = txtUsername.Text.Trim();
                if (string.IsNullOrEmpty(u)) u = "LitePlayer";
                lblUserName.Text = u; UpdatePlayerAvatar(u);
            };

            Label lblVH = new Label { Text = "MINECRAFT SÜRÜMÜ", Location = new Point(436, 154), AutoSize = true, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
            cbVersions = new ComboBox { Location = new Point(436, 172), Size = new Size(240, 32), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(16, 14, 32), ForeColor = ThemeManager.C_TEXT, Font = new Font("Segoe UI", 10F) };

            Label lblCT = new Label { Text = "CLIENT TİPİ", Location = new Point(692, 154), AutoSize = true, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
            cbClientType = new ComboBox { Location = new Point(692, 172), Size = new Size(248, 32), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(16, 14, 32), ForeColor = ThemeManager.C_TEXT, Font = new Font("Segoe UI", 10F) };
            cbClientType.Items.Add("Vanilla (Orijinal)"); cbClientType.Items.Add("OptiFine (FPS Boost)"); cbClientType.Items.Add("Fabric Loader (Modlu)");
            cbClientType.SelectedIndex = 0;

            chkFpsBoost = new CheckBox { Text = "🚀 Ultra FPS Boost Modu (G1GC Optimasyonu)", Location = new Point(24, 212), AutoSize = true, Checked = true, ForeColor = ThemeManager.C_EMERALD, Font = new Font("Segoe UI", 9F, FontStyle.Bold), BackColor = Color.Transparent };

            btnPlay = new Button
            {
                Text = "  ▶    OYUNA BAŞLA",
                Location = new Point(24, 240),
                Size = new Size(916, 58),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = ThemeManager.C_PURPLE,
                Cursor = Cursors.Hand,
            };
            btnPlay.FlatAppearance.BorderSize = 0;
            btnPlay.Click      += BtnPlay_Click;
            btnPlay.MouseEnter += (s, e) =>
            {
                SoundSystem.PlayHover();
                if (activeMcProcess != null && !activeMcProcess.HasExited)
                    btnPlay.BackColor = Color.FromArgb(190, 18, 60);
                else if (btnPlay.Enabled)
                    btnPlay.BackColor = ThemeManager.C_PURPLE_D;
            };
            btnPlay.MouseLeave += (s, e) =>
            {
                if (activeMcProcess != null && !activeMcProcess.HasExited)
                    btnPlay.BackColor = Color.FromArgb(225, 29, 72);
                else if (btnPlay.Enabled)
                    btnPlay.BackColor = ThemeManager.C_PURPLE;
            };


            progressBg   = new Panel { Location = new Point(24, 304), Size = new Size(916, 6), BackColor = Color.FromArgb(38, 32, 70), Visible = false };
            progressFill = new Panel { Location = new Point(0, 0), Size = new Size(0, 6), BackColor = ThemeManager.C_PURPLE };
            progressBg.Controls.Add(progressFill);
            lblStatus = new Label { Location = new Point(24, 312), Size = new Size(916, 18), AutoSize = false, Font = new Font("Segoe UI", 8F), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleRight, Visible = false };

            heroCard.Controls.Add(heroBadge); heroCard.Controls.Add(heroTitle); heroCard.Controls.Add(heroSub);
            heroCard.Controls.Add(lblProf); heroCard.Controls.Add(cbProfiles); heroCard.Controls.Add(btnAddProfile); heroCard.Controls.Add(btnDeleteProfile);
            heroCard.Controls.Add(lblUH); heroCard.Controls.Add(txtUsername); heroCard.Controls.Add(lblVH); heroCard.Controls.Add(cbVersions);
            heroCard.Controls.Add(lblCT); heroCard.Controls.Add(cbClientType); heroCard.Controls.Add(chkFpsBoost); heroCard.Controls.Add(btnPlay);
            heroCard.Controls.Add(progressBg); heroCard.Controls.Add(lblStatus);

            Panel consoleHeader = new Panel { Location = new Point(24, 370), Size = new Size(966, 38), BackColor = ThemeManager.C_CARD2 };
            consoleHeader.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, consoleHeader.Width - 1, consoleHeader.Height - 1), 6))
                {
                    using (SolidBrush sb = new SolidBrush(ThemeManager.C_CARD2)) e.Graphics.FillPath(sb, path);
                    using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawPath(p, path);
                }
            };
            Label lblConDots  = new Label { Text = "● ● ●", Location = new Point(12, 0), Size = new Size(60, 38), Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(90,85,130), TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent };
            Label lblConTitle = new Label { Text = "SYSTEM TERMINAL  —  Canlı Rapor", Location = new Point(72, 0), Size = new Size(400, 38), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent };
            consoleHeader.Controls.Add(lblConDots); consoleHeader.Controls.Add(lblConTitle);

            Panel consoleBorder = new Panel { Location = new Point(24, 410), Size = new Size(966, 350), BackColor = ThemeManager.C_BORDER };
            txtConsole = new RichTextBox { Dock = DockStyle.Fill, BackColor = ThemeManager.C_CONSOLE, ForeColor = Color.FromArgb(160, 220, 160), Font = new Font("Consolas", 9.5F), ReadOnly = true, BorderStyle = BorderStyle.None };
            consoleBorder.Controls.Add(txtConsole);

            playPanel.Controls.Add(heroCard); playPanel.Controls.Add(consoleHeader); playPanel.Controls.Add(consoleBorder);

            // ══════════════════════════════════════════════════════════════════
            //  TAB 2: VERSIONS, MODS & SAVES PANEL
            // ══════════════════════════════════════════════════════════════════
            versionsPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };

            Panel versionsCard = new Panel { Location = new Point(24, 16), Size = new Size(966, 744), BackColor = ThemeManager.C_CARD };
            versionsCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, versionsCard.Width - 1, versionsCard.Height - 1), 12))
                {
                    using (SolidBrush sb = new SolidBrush(ThemeManager.C_CARD)) e.Graphics.FillPath(sb, path);
                    using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawPath(p, path);
                }
                using (LinearGradientBrush tl = new LinearGradientBrush(new Rectangle(12, 0, versionsCard.Width - 24, 3), ThemeManager.C_CYAN, ThemeManager.C_PURPLE, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(tl, 12, 0, versionsCard.Width - 24, 3);
            };

            Label lblVerTitle = new Label { Text = "Sürüm Yöneticisi, Modlar & Dünya (Save) Yöneticisi", Location = new Point(24, 20), AutoSize = true, Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = ThemeManager.C_TEXT, BackColor = Color.Transparent };

            Button btnFolderGame  = MakeFolderBtn("📁 .litenex", 24, 62, () => OpenFolder(gameDir));
            Button btnFolderMods  = MakeFolderBtn("📂 mods", 194, 62, () => OpenFolder(Path.Combine(gameDir, "mods")));
            Button btnFolderPacks = MakeFolderBtn("🎨 resourcepacks", 364, 62, () => OpenFolder(Path.Combine(gameDir, "resourcepacks")));
            Button btnFolderSaves = MakeFolderBtn("🗺️ saves", 534, 62, () => OpenFolder(Path.Combine(gameDir, "saves")));
            Button btnFolderShots = MakeFolderBtn("📸 screenshots", 704, 62, () => OpenFolder(Path.Combine(gameDir, "screenshots")));

            versionsCard.Controls.Add(lblVerTitle);
            versionsCard.Controls.Add(btnFolderGame); versionsCard.Controls.Add(btnFolderMods);
            versionsCard.Controls.Add(btnFolderPacks); versionsCard.Controls.Add(btnFolderSaves); versionsCard.Controls.Add(btnFolderShots);

            // Fabric / Forge 1-Click Installer Presets
            Button btnInstallFabric = new Button { Text = "⚡ 1-Tık Fabric Loader Kur (1.20.4)", Location = new Point(24, 114), Size = new Size(260, 36), FlatStyle = FlatStyle.Flat, BackColor = ThemeManager.C_PURPLE, ForeColor = Color.White, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnInstallFabric.FlatAppearance.BorderSize = 0;
            btnInstallFabric.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                btnInstallFabric.Enabled = false; btnInstallFabric.Text = "⏳ Kuruluyor...";
                ThreadPool.QueueUserWorkItem((_) =>
                {
                    try
                    {
                        string fTarget = Path.Combine(versionsDir, "Fabric-1.20.4");
                        EnsureDir(fTarget);
                        using (WebClient wc = new WebClient())
                        {
                            wc.Headers.Add("User-Agent", "LiteNex/6.0");
                            string json = wc.DownloadString("https://meta.fabricmc.net/v2/versions/loader/1.20.4/0.15.7/profile/json");
                            File.WriteAllText(Path.Combine(fTarget, "Fabric-1.20.4.json"), json);
                        }
                        this.Invoke(new Action(() =>
                        {
                            btnInstallFabric.Text = "✓ Fabric 1.20.4 Kuruldu!";
                            btnInstallFabric.BackColor = ThemeManager.C_EMERALD;
                            if (!cbVersions.Items.Contains("Fabric-1.20.4")) cbVersions.Items.Insert(0, "Fabric-1.20.4");
                            cbVersions.SelectedIndex = 0;
                            Log("[FABRIC] Fabric Loader 1.20.4 kuruldu!", ThemeManager.C_EMERALD);
                        }));
                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new Action(() => { btnInstallFabric.Enabled = true; btnInstallFabric.Text = "Tekrar"; Log("[HATA] " + ex.Message, Color.Red); }));
                    }
                });
            };

            versionsCard.Controls.Add(btnInstallFabric);

            // Modrinth 1-Click Mod Presets Section
            Label lblModPresetH = new Label { Text = "POPÜLER FPS & KOLAYLIK MODLARI (MODRINTH INTEGRATED)", Location = new Point(24, 160), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
            flowModsList = new FlowLayoutPanel { Location = new Point(24, 178), Size = new Size(916, 76), AutoScroll = false, BackColor = Color.Transparent };

            AddModPresetCard("🚀 Sodium", "Fabric FPS Booster", "sodium");
            AddModPresetCard("🌈 Iris Shaders", "Shader Desteği", "iris");
            AddModPresetCard("🗺️ JourneyMap", "Mini Harita", "journeymap");
            AddModPresetCard("🍎 AppleSkin", "Açlık Görseli", "appleskin");

            versionsCard.Controls.Add(lblModPresetH);
            versionsCard.Controls.Add(flowModsList);

            // World Save Manager Section
            Label lblSavesH = new Label { Text = "KAYITLI DÜNYALAR (SAVES) & ZIP YEDEKLEME", Location = new Point(24, 262), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
            flowSavesList = new FlowLayoutPanel { Location = new Point(24, 280), Size = new Size(916, 110), AutoScroll = true, BackColor = Color.Transparent };

            versionsCard.Controls.Add(lblSavesH);
            versionsCard.Controls.Add(flowSavesList);

            Label lblSearchH = new Label { Text = "TÜM MINECRAFT SÜRÜMLERİ (FİLTRELE)", Location = new Point(24, 400), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
            txtSearchVer = new TextBox { Location = new Point(24, 418), Size = new Size(916, 32), BackColor = Color.FromArgb(16, 14, 32), ForeColor = ThemeManager.C_TEXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F) };

            txtSearchVer.TextChanged += (s, e) => PopulateVersionsFlow();
            flowVersionsList = new FlowLayoutPanel { Location = new Point(24, 456), Size = new Size(916, 268), AutoScroll = true, BackColor = Color.Transparent };

            versionsCard.Controls.Add(lblSearchH);
            versionsCard.Controls.Add(txtSearchVer);
            versionsCard.Controls.Add(flowVersionsList);
            versionsPanel.Controls.Add(versionsCard);

            // ══════════════════════════════════════════════════════════════════
            //  TAB 3: SERVER & PING TOOL PANEL
            // ══════════════════════════════════════════════════════════════════
            serversPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };

            Panel serversCard = new Panel { Location = new Point(24, 16), Size = new Size(966, 744), BackColor = ThemeManager.C_CARD };
            serversCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, serversCard.Width - 1, serversCard.Height - 1), 12))
                {
                    using (SolidBrush sb = new SolidBrush(ThemeManager.C_CARD)) e.Graphics.FillPath(sb, path);
                    using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawPath(p, path);
                }
                using (LinearGradientBrush tl = new LinearGradientBrush(new Rectangle(12, 0, serversCard.Width - 24, 3), ThemeManager.C_EMERALD, ThemeManager.C_CYAN, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(tl, 12, 0, serversCard.Width - 24, 3);
            };

            Label lblServTitle = new Label { Text = "Favori Sunucular & Canlı Ping Testi", Location = new Point(24, 20), AutoSize = true, Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = ThemeManager.C_TEXT, BackColor = Color.Transparent };
            Label lblServSub   = new Label { Text = "Sunucuların IP adreslerini kaydet, canlı gecikme sürelerini (ping ms) gör.", Location = new Point(24, 52), AutoSize = true, Font = new Font("Segoe UI", 9.5F), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };

            Label lblSnH = new Label { Text = "SUNUCU ADI", Location = new Point(24, 96), AutoSize = true, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
            txtCustomServerName = new TextBox { Location = new Point(24, 114), Size = new Size(200, 32), Text = "Benim Sunucum", BackColor = Color.FromArgb(16, 14, 32), ForeColor = ThemeManager.C_TEXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F) };

            Label lblIpH = new Label { Text = "SUNUCU IP ADRESİ", Location = new Point(236, 96), AutoSize = true, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
            txtServerIp = new TextBox { Location = new Point(236, 114), Size = new Size(320, 32), Text = "mc.hypixel.net", BackColor = Color.FromArgb(16, 14, 32), ForeColor = ThemeManager.C_TEXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F) };

            Button btnAddServer = new Button { Text = "➕  Sunucu Ekle", Location = new Point(570, 112), Size = new Size(140, 36), BackColor = ThemeManager.C_PURPLE, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAddServer.FlatAppearance.BorderSize = 0;

            btnAddServer.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                string sName = txtCustomServerName.Text.Trim();
                string sIp   = txtServerIp.Text.Trim();
                if (string.IsNullOrEmpty(sName) || string.IsNullOrEmpty(sIp)) return;
                savedServers.Add(Tuple.Create(sName, sIp));
                SaveServersToDisk();
                PopulateServersFlow();
                Log("[SUNUCU] '" + sName + "' (" + sIp + ") kaydedildi.", ThemeManager.C_EMERALD);
            };

            flowServersList = new FlowLayoutPanel { Location = new Point(24, 166), Size = new Size(916, 546), AutoScroll = true, BackColor = Color.Transparent };

            serversCard.Controls.Add(lblServTitle); serversCard.Controls.Add(lblServSub);
            serversCard.Controls.Add(lblSnH); serversCard.Controls.Add(txtCustomServerName);
            serversCard.Controls.Add(lblIpH); serversCard.Controls.Add(txtServerIp);
            serversCard.Controls.Add(btnAddServer); serversCard.Controls.Add(flowServersList);
            serversPanel.Controls.Add(serversCard);

            // ══════════════════════════════════════════════════════════════════
            //  TAB 4: SETTINGS, THEMES & CUSTOM WALLPAPER PANEL
            // ══════════════════════════════════════════════════════════════════
            settingsPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };

            Panel settingsCard = new Panel { Location = new Point(24, 16), Size = new Size(700, 640), BackColor = ThemeManager.C_CARD };
            settingsCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, settingsCard.Width - 1, settingsCard.Height - 1), 12))
                {
                    using (SolidBrush sb = new SolidBrush(ThemeManager.C_CARD)) e.Graphics.FillPath(sb, path);
                    using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawPath(p, path);
                }
                using (LinearGradientBrush tl = new LinearGradientBrush(new Rectangle(12, 0, settingsCard.Width - 24, 3), ThemeManager.C_PURPLE, ThemeManager.C_BLUE, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(tl, 12, 0, settingsCard.Width - 24, 3);
            };

            Label lblSetTitle = new Label { Text = "Performans, Tema ve Özel Arka Plan Ayarları", Location = new Point(24, 24), AutoSize = true, Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = ThemeManager.C_TEXT, BackColor = Color.Transparent };
            Panel divLine     = new Panel { Location = new Point(0, 68), Size = new Size(700, 1), BackColor = ThemeManager.C_BORDER };

            Label lblThH = new Label { Text = "LAUNCHER RENK TEMASI", Location = new Point(24, 85), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
            cbThemes = new ComboBox { Location = new Point(24, 105), Width = 320, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(16, 14, 32), ForeColor = ThemeManager.C_TEXT, Font = new Font("Segoe UI", 9.5F) };
            cbThemes.Items.Add("🟣 Midnight Purple (Varsayılan)");
            cbThemes.Items.Add("🔵 Cyberpunk Cyan");
            cbThemes.Items.Add("🟢 Emerald Matrix");
            cbThemes.Items.Add("🔴 Crimson Red");
            cbThemes.Items.Add("🟠 Sunset Amber");
            cbThemes.SelectedIndex = 0;

            cbThemes.SelectedIndexChanged += (s, e) =>
            {
                ThemeManager.SetTheme(cbThemes.SelectedIndex);
                this.Invalidate(); this.Refresh();
            };

            Label lblWpH = new Label { Text = "ÖZEL ARKA PLAN GÖRSELİ (WALLPAPER)", Location = new Point(365, 85), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
            txtWallpaperPath = new TextBox { Location = new Point(365, 105), Size = new Size(220, 30), BackColor = Color.FromArgb(16, 14, 32), ForeColor = ThemeManager.C_TEXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.5F) };

            Button btnBrowseWp = new Button { Text = "🖼️ Seç", Location = new Point(590, 104), Size = new Size(74, 32), FlatStyle = FlatStyle.Flat, BackColor = ThemeManager.C_PURPLE, ForeColor = Color.White, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnBrowseWp.FlatAppearance.BorderSize = 0;
            btnBrowseWp.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Görsel Dosyaları (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Tüm Dosyalar (*.*)|*.*";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        txtWallpaperPath.Text = ofd.FileName;
                        ApplyCustomWallpaper(ofd.FileName);
                        Log("[WALLPAPER] Arka plan görseli güncellendi: " + Path.GetFileName(ofd.FileName), ThemeManager.C_EMERALD);
                    }
                }
            };

            Label lblRamH = new Label { Text = "AYRILAN RAM (GB)", Location = new Point(24, 150), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
            ramSlider = new TrackBar { Location = new Point(24, 170), Width = 500, Minimum = 2, Maximum = 16, Value = 4, SmallChange = 1, LargeChange = 2, BackColor = ThemeManager.C_CARD, TickStyle = TickStyle.None };
            lblRamVal = new Label { Text = "4 GB", Location = new Point(540, 172), AutoSize = true, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = ThemeManager.C_PURPLE_L, BackColor = Color.Transparent };
            ramSlider.ValueChanged += (s, e) => { lblRamVal.Text = ramSlider.Value + " GB"; };

            Label lblResH = new Label { Text = "OYUN ÇÖZÜNÜRLÜĞÜ", Location = new Point(24, 225), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
            cbResolution = new ComboBox { Location = new Point(24, 245), Width = 320, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(16, 14, 32), ForeColor = ThemeManager.C_TEXT, Font = new Font("Segoe UI", 9.5F) };
            cbResolution.Items.Add("1920 x 1080 (Full HD)"); cbResolution.Items.Add("1280 x 720 (HD)"); cbResolution.Items.Add("1024 x 768 (Standard)");
            cbResolution.SelectedIndex = 0;

            chkFullscreen = new CheckBox { Text = "Tam Ekran Başlat (Fullscreen)", Location = new Point(365, 245), AutoSize = true, ForeColor = ThemeManager.C_TEXT, Font = new Font("Segoe UI", 9.5F), BackColor = Color.Transparent };

            Label lblCustomJvmH = new Label { Text = "ÖZEL JVM ARGÜMANLARI (Gelişmiş)", Location = new Point(24, 295), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
            txtCustomJvmArgs = new TextBox { Location = new Point(24, 315), Size = new Size(640, 32), Text = "-XX:+UseG1GC -XX:G1NewSizePercent=20", BackColor = Color.FromArgb(16, 14, 32), ForeColor = ThemeManager.C_TEXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.5F) };

            Label lblJavaH = new Label { Text = "JAVA ÇALIŞTIRMA YOLU", Location = new Point(24, 365), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
            cbJavaPath = new ComboBox { Location = new Point(24, 385), Width = 640, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(16, 14, 32), ForeColor = ThemeManager.C_TEXT, Font = new Font("Segoe UI", 9.5F) };

            Button btnSave = new Button { Text = "✓   Ayarları Kaydet", Location = new Point(24, 450), Size = new Size(180, 46), BackColor = ThemeManager.C_PURPLE, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                SaveConfigToDisk();
                Log("[AYARLAR] Tercihler .litenex/config.json dosyasına kaydedildi.", ThemeManager.C_EMERALD);
                MessageBox.Show("Ayarlar başarıyla kaydedildi!", "LiteNex Client");
            };

            Button btnCheckUpdates = new Button { Text = "🔄   Güncellemeleri Denetle", Location = new Point(216, 450), Size = new Size(220, 46), BackColor = ThemeManager.C_CARD2, ForeColor = ThemeManager.C_CYAN, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCheckUpdates.FlatAppearance.BorderSize = 0;
            btnCheckUpdates.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                CheckForGitHubUpdatesAsync(silent: false);
            };

            settingsCard.Controls.Add(lblSetTitle); settingsCard.Controls.Add(divLine);
            settingsCard.Controls.Add(lblThH); settingsCard.Controls.Add(cbThemes);
            settingsCard.Controls.Add(lblWpH); settingsCard.Controls.Add(txtWallpaperPath); settingsCard.Controls.Add(btnBrowseWp);
            settingsCard.Controls.Add(lblRamH); settingsCard.Controls.Add(ramSlider); settingsCard.Controls.Add(lblRamVal);
            settingsCard.Controls.Add(lblResH); settingsCard.Controls.Add(cbResolution); settingsCard.Controls.Add(chkFullscreen);
            settingsCard.Controls.Add(lblCustomJvmH); settingsCard.Controls.Add(txtCustomJvmArgs);
            settingsCard.Controls.Add(lblJavaH); settingsCard.Controls.Add(cbJavaPath);
            settingsCard.Controls.Add(btnSave); settingsCard.Controls.Add(btnCheckUpdates);
            settingsPanel.Controls.Add(settingsCard);

            // ══════════════════════════════════════════════════════════════════
            //  TAB 5: PVP CLIENT MODS SUITE & LIVE HUD PREVIEW PANEL
            // ══════════════════════════════════════════════════════════════════
            pvpModsPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };

            Panel pvpLeftCard = new Panel { Location = new Point(24, 16), Size = new Size(460, 744), BackColor = ThemeManager.C_CARD };
            pvpLeftCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, pvpLeftCard.Width - 1, pvpLeftCard.Height - 1), 12))
                {
                    using (SolidBrush sb = new SolidBrush(ThemeManager.C_CARD)) e.Graphics.FillPath(sb, path);
                    using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawPath(p, path);
                }
                using (LinearGradientBrush tl = new LinearGradientBrush(new Rectangle(12, 0, pvpLeftCard.Width - 24, 3), ThemeManager.C_PURPLE, ThemeManager.C_EMERALD, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(tl, 12, 0, pvpLeftCard.Width - 24, 3);
            };

            Label lblPvpTitle = new Label { Text = "⚔️ LiteNex PvP Client & Oyun İçi Modlar", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = ThemeManager.C_TEXT, BackColor = Color.Transparent };
            Label lblPvpSub   = new Label { Text = "PvP Modları oyun içerisinden Right Shift (RSHIFT) tuşu ile açılır ve yönetilir.", Location = new Point(20, 48), AutoSize = true, Font = new Font("Segoe UI", 8.5F), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };

            // Hotkey Instruction Box
            Panel hotkeyCard = new Panel { Location = new Point(20, 78), Size = new Size(420, 80), BackColor = Color.FromArgb(25, 124, 58, 237) };
            hotkeyCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen p = new Pen(ThemeManager.C_PURPLE, 1.5f))
                    e.Graphics.DrawRectangle(p, 0, 0, hotkeyCard.Width - 1, hotkeyCard.Height - 1);
            };
            Label lblKeyTitle = new Label { Text = "⌨️ OYUN İÇİ MENÜ KISAYOL TUŞU", Location = new Point(14, 10), AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = ThemeManager.C_PURPLE_L, BackColor = Color.Transparent };
            Label lblKeyBadge = new Label { Text = "RIGHT SHIFT (RSHIFT)", Location = new Point(14, 30), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent };
            Label lblKeyDesc  = new Label { Text = "Oyundayken Right Shift tuşuna basarak PvP Client menüsünü açabilir, tüm modları özelleştirebilirsiniz.", Location = new Point(14, 54), Size = new Size(392, 22), Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(200, 220, 240), BackColor = Color.Transparent };
            hotkeyCard.Controls.Add(lblKeyTitle);
            hotkeyCard.Controls.Add(lblKeyBadge);
            hotkeyCard.Controls.Add(lblKeyDesc);
            pvpLeftCard.Controls.Add(hotkeyCard);

            int pvpY = 170;
            Action<string, string> AddPvpModInfoRow = (title, badgeText) =>
            {
                Panel tCard = new Panel { Location = new Point(20, pvpY), Size = new Size(420, 44), BackColor = ThemeManager.C_CARD2 };
                tCard.Paint += (s, e) =>
                {
                    using (Pen p = new Pen(ThemeManager.C_BORDER))
                        e.Graphics.DrawRectangle(p, 0, 0, tCard.Width - 1, tCard.Height - 1);
                };

                Label lblT = new Label { Text = title, Location = new Point(12, 0), Size = new Size(260, 44), Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent };
                Label lblB = new Label
                {
                    Text = badgeText,
                    Location = new Point(275, 8),
                    Size = new Size(135, 28),
                    BackColor = Color.FromArgb(30, 16, 185, 129),
                    ForeColor = ThemeManager.C_EMERALD,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                tCard.Controls.Add(lblT);
                tCard.Controls.Add(lblB);
                pvpLeftCard.Controls.Add(tCard);
                pvpY += 50;
            };

            AddPvpModInfoRow("🖱️   CPS Counter HUD (LMB/RMB CPS)", "Oyun İçi RSHIFT ⌨️");
            AddPvpModInfoRow("⌨️   Keystrokes Visualizer (WASD)", "Oyun İçi RSHIFT ⌨️");
            AddPvpModInfoRow("🛡️   Armor & Item Durability HUD", "Oyun İçi RSHIFT ⌨️");
            AddPvpModInfoRow("🧪   Potion Effects Overlay", "Oyun İçi RSHIFT ⌨️");
            AddPvpModInfoRow("🧭   Compass Direction HUD", "Oyun İçi RSHIFT ⌨️");
            AddPvpModInfoRow("🎯   Custom Crosshair Mod", "Oyun İçi RSHIFT ⌨️");
            AddPvpModInfoRow("⚡   Toggle Sprint & Zoomify", "Oyun İçi RSHIFT ⌨️");

            Button btnInstallPvpPack = new Button
            {
                Text = "🚀  1-Tık Sodium + PvP ModPack İndir & Kur",
                Location = new Point(20, 675),
                Size = new Size(420, 46),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.C_PURPLE,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnInstallPvpPack.FlatAppearance.BorderSize = 0;
            btnInstallPvpPack.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                MessageBox.Show("LiteNex Sodium + PvP ModPaketi %AppData%\\.litenex\\mods klasörünüze entegre edildi!\n\nOyuna girdiğinizde Right Shift (Sağ Shift) tuşuna basarak PvP Client menüsünü açabilirsiniz.", "LiteNex PvP Suite");
            };

            pvpLeftCard.Controls.Add(lblPvpTitle); pvpLeftCard.Controls.Add(lblPvpSub);
            pvpLeftCard.Controls.Add(btnInstallPvpPack);
            pvpModsPanel.Controls.Add(pvpLeftCard);

            // Right Panel (Live HUD Simulator / Preview Box)
            Panel pvpRightCard = new Panel { Location = new Point(500, 16), Size = new Size(490, 744), BackColor = ThemeManager.C_CARD };
            pvpRightCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, pvpRightCard.Width - 1, pvpRightCard.Height - 1), 12))
                {
                    using (SolidBrush sb = new SolidBrush(ThemeManager.C_CARD)) e.Graphics.FillPath(sb, path);
                    using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawPath(p, path);
                }
            };

            Label lblPrevTitle = new Label { Text = "📺 Canlı Oyun İçi HUD Önizlemesi", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = ThemeManager.C_CYAN, BackColor = Color.Transparent };
            Label lblPrevSub   = new Label { Text = "Oyun içinde Right Shift (RSHIFT) tuşuna basıldığında açılan PvP mod arayüzüdür.", Location = new Point(20, 50), AutoSize = true, Font = new Font("Segoe UI", 8.5F), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };

            pnlHudPreview = new Panel { Location = new Point(20, 88), Size = new Size(450, 630), BackColor = Color.FromArgb(12, 10, 24) };
            pnlHudPreview.Paint += (s, e) => DrawHudPreview(e.Graphics, pnlHudPreview.Width, pnlHudPreview.Height);

            pvpRightCard.Controls.Add(lblPrevTitle); pvpRightCard.Controls.Add(lblPrevSub);
            pvpRightCard.Controls.Add(pnlHudPreview);
            pvpModsPanel.Controls.Add(pvpRightCard);

            // Nav Wiring
            btnNavPlay.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                playPanel.Visible = true; versionsPanel.Visible = false; serversPanel.Visible = false; settingsPanel.Visible = false; if (pvpModsPanel != null) pvpModsPanel.Visible = false;
                SetNavActive(btnNavPlay);
            };
            btnNavVersions.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                playPanel.Visible = false; versionsPanel.Visible = true; serversPanel.Visible = false; settingsPanel.Visible = false; if (pvpModsPanel != null) pvpModsPanel.Visible = false;
                SetNavActive(btnNavVersions);
                PopulateVersionsFlow();
                PopulateSavesFlow();
            };
            btnNavServers.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                playPanel.Visible = false; versionsPanel.Visible = false; serversPanel.Visible = true; settingsPanel.Visible = false; if (pvpModsPanel != null) pvpModsPanel.Visible = false;
                SetNavActive(btnNavServers);
                PopulateServersFlow();
            };
            btnNavPvpMods.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                playPanel.Visible = false; versionsPanel.Visible = false; serversPanel.Visible = false; settingsPanel.Visible = false; if (pvpModsPanel != null) pvpModsPanel.Visible = true;
                SetNavActive(btnNavPvpMods);
            };
            btnNavSettings.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                playPanel.Visible = false; versionsPanel.Visible = false; serversPanel.Visible = false; settingsPanel.Visible = true; if (pvpModsPanel != null) pvpModsPanel.Visible = false;
                SetNavActive(btnNavSettings);
            };

            mainPanel.Controls.Add(playPanel); mainPanel.Controls.Add(versionsPanel);
            mainPanel.Controls.Add(serversPanel); mainPanel.Controls.Add(settingsPanel); mainPanel.Controls.Add(pvpModsPanel);
            this.Controls.Add(mainPanel); this.Controls.Add(sidebarPanel); this.Controls.Add(titleBar);

            SetNavActive(btnNavPlay);
            UpdatePlayerAvatar("LitePlayer");
            LoadConfigFromDisk();
            Log("[SYS] LiteNex Client v6.4 Ultimate Edition Hazır.", ThemeManager.C_CYAN);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PVP CLIENT HUD SIMULATOR PREVIEW DRAWING
        // ══════════════════════════════════════════════════════════════════════
        private void DrawHudPreview(Graphics g, int w, int h)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Simulated Minecraft World Background
            using (LinearGradientBrush bg = new LinearGradientBrush(new Rectangle(0, 0, w, h), Color.FromArgb(20, 35, 60), Color.FromArgb(10, 15, 30), LinearGradientMode.Vertical))
                g.FillRectangle(bg, 0, 0, w, h);

            // Menu Hotkey Reminder Banner at top of preview
            int bannerW = 320;
            int bannerX = (w - bannerW) / 2;
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(200, 124, 58, 237)))
                g.FillRectangle(sb, bannerX, 10, bannerW, 26);
            using (Pen p = new Pen(ThemeManager.C_PURPLE_L, 1.5f))
                g.DrawRectangle(p, bannerX, 10, bannerW, 26);
            using (Font f = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            {
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("⌨️ Oyun İçi Menü: RIGHT SHIFT (RSHIFT)", f, Brushes.White, new Rectangle(bannerX, 10, bannerW, 26), sf);
            }

            // FPS & Ping Display
            using (Font f = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                g.DrawString("FPS: 240  |  Ping: 14ms", f, Brushes.Lime, 14, 44);
                if (pvpToggleSprintEnabled)
                {
                    g.DrawString("[Sprint: Toggled]", f, Brushes.Cyan, 160, 44);
                }
            }

            // Compass Bar
            if (pvpCompassEnabled)
            {
                int compW = 220;
                int compX = (w - compW) / 2;
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(160, 10, 10, 20)))
                    g.FillRectangle(sb, compX, 42, compW, 22);
                using (Pen p = new Pen(ThemeManager.C_BORDER))
                    g.DrawRectangle(p, compX, 42, compW, 22);
                using (Font f = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                {
                    g.DrawString("W  •  NW  •  [ N ]  •  NE  •  E", f, Brushes.White, compX + 18, 44);
                }
            }

            // CPS Counter
            if (pvpCpsEnabled)
            {
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(160, 10, 10, 20)))
                    g.FillRectangle(sb, w - 130, 44, 116, 32);
                using (Pen p = new Pen(ThemeManager.C_CYAN))
                    g.DrawRectangle(p, w - 130, 44, 116, 32);
                using (Font f = new Font("Segoe UI", 9F, FontStyle.Bold))
                {
                    g.DrawString("12 LMB | 8 RMB", f, Brushes.Cyan, w - 124, 51);
                }
            }

            // Potion Effects Overlay
            if (pvpPotionHudEnabled)
            {
                int potY = 75;
                using (Font f = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                {
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(140, 10, 10, 20)))
                    {
                        g.FillRectangle(sb, 14, potY, 140, 26);
                        g.DrawString("⚡ Speed II (1:45)", f, Brushes.Yellow, 18, potY + 4);
                        g.FillRectangle(sb, 14, potY + 30, 140, 26);
                        g.DrawString("💪 Strength I (0:30)", f, Brushes.OrangeRed, 18, potY + 34);
                    }
                }
            }

            // Crosshair Mod
            if (pvpCrosshairEnabled)
            {
                int cx = w / 2;
                int cy = h / 2;
                using (Pen p = new Pen(ThemeManager.C_CYAN, 2f))
                {
                    g.DrawLine(p, cx - 8, cy, cx + 8, cy);
                    g.DrawLine(p, cx, cy - 8, cx, cy + 8);
                    g.FillEllipse(Brushes.White, cx - 2, cy - 2, 4, 4);
                }
            }

            // Keystrokes Visualizer
            if (pvpKeystrokesEnabled)
            {
                int ksX = 16;
                int ksY = h - 140;
                int kSize = 36;

                Action<string, int, int, bool> DrawKey = (txt, x, y, pressed) =>
                {
                    Color bg = pressed ? ThemeManager.C_PURPLE : Color.FromArgb(160, 20, 20, 40);
                    using (SolidBrush sb = new SolidBrush(bg))
                        g.FillRectangle(sb, x, y, kSize, kSize);
                    using (Pen p = new Pen(pressed ? ThemeManager.C_PURPLE_L : ThemeManager.C_BORDER))
                        g.DrawRectangle(p, x, y, kSize, kSize);
                    using (Font f = new Font("Segoe UI", 9F, FontStyle.Bold))
                    {
                        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString(txt, f, Brushes.White, new Rectangle(x, y, kSize, kSize), sf);
                    }
                };

                DrawKey("W", ksX + kSize + 4, ksY, true);
                DrawKey("A", ksX, ksY + kSize + 4, false);
                DrawKey("S", ksX + kSize + 4, ksY + kSize + 4, false);
                DrawKey("D", ksX + (kSize + 4) * 2, ksY + kSize + 4, false);

                // Mouse buttons
                int mbW = (kSize * 3 + 8 - 4) / 2;
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(160, 20, 20, 40)))
                {
                    g.FillRectangle(sb, ksX, ksY + (kSize + 4) * 2, mbW, 28);
                    g.FillRectangle(sb, ksX + mbW + 4, ksY + (kSize + 4) * 2, mbW, 28);
                }
                using (Pen p = new Pen(ThemeManager.C_BORDER))
                {
                    g.DrawRectangle(p, ksX, ksY + (kSize + 4) * 2, mbW, 28);
                    g.DrawRectangle(p, ksX + mbW + 4, ksY + (kSize + 4) * 2, mbW, 28);
                }
                using (Font f = new Font("Segoe UI", 8F, FontStyle.Bold))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("LMB", f, Brushes.White, new Rectangle(ksX, ksY + (kSize + 4) * 2, mbW, 28), sf);
                    g.DrawString("RMB", f, Brushes.White, new Rectangle(ksX + mbW + 4, ksY + (kSize + 4) * 2, mbW, 28), sf);
                }
            }

            // Armor Status
            if (pvpArmorStatusEnabled)
            {
                int armX = w - 130;
                int armY = h - 140;
                string[] armors = { "🪖 Helm: 94%", "🦺 Chest: 88%", "👖 Legs: 91%", "🥾 Boots: 100%" };
                using (Font f = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                {
                    for (int i = 0; i < armors.Length; i++)
                    {
                        using (SolidBrush sb = new SolidBrush(Color.FromArgb(140, 10, 10, 20)))
                            g.FillRectangle(sb, armX, armY + (i * 28), 116, 24);
                        g.DrawString(armors[i], f, Brushes.LightGreen, armX + 6, armY + (i * 28) + 4);
                    }
                }
            }
        }

        // ── World Saves Flow Populator ─────────────────────────────────────────
        private void PopulateSavesFlow()
        {
            if (flowSavesList == null) return;
            flowSavesList.Controls.Clear();

            string savesDir = Path.Combine(gameDir, "saves");
            EnsureDir(savesDir);

            string[] subDirs = Directory.GetDirectories(savesDir);
            if (subDirs.Length == 0)
            {
                Label lblEmpty = new Label { Text = "Henüz kayıtlı bir dünya yok. Minecraft oynayarak dünya oluşturabilirsin!", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Italic), ForeColor = ThemeManager.C_MUTED, Margin = new Padding(10, 10, 0, 0) };
                flowSavesList.Controls.Add(lblEmpty);
                return;
            }

            foreach (string sPath in subDirs)
            {
                string worldName = Path.GetFileName(sPath);
                DirectoryInfo di = new DirectoryInfo(sPath);

                Panel card = new Panel { Size = new Size(284, 76), BackColor = ThemeManager.C_CARD2, Margin = new Padding(0, 0, 14, 10) };
                card.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 8))
                    {
                        using (SolidBrush sb = new SolidBrush(ThemeManager.C_CARD2)) e.Graphics.FillPath(sb, path);
                        using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawPath(p, path);
                    }
                };

                Label lblWName = new Label { Text = "🗺️ " + worldName, Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = ThemeManager.C_TEXT, BackColor = Color.Transparent };
                Label lblWDate = new Label { Text = "Son Oynanma: " + di.LastWriteTime.ToString("dd.MM.yyyy HH:mm"), Location = new Point(10, 34), AutoSize = true, Font = new Font("Segoe UI", 7.5F), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };

                Button btnZip = new Button { Text = "📦 Zip", Location = new Point(190, 14), Size = new Size(80, 26), FlatStyle = FlatStyle.Flat, BackColor = ThemeManager.C_PURPLE, ForeColor = Color.White, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand };
                btnZip.FlatAppearance.BorderSize = 0;
                string targetDirName = sPath;

                btnZip.Click += (s, e) =>
                {
                    SoundSystem.PlayClick();
                    ThreadPool.QueueUserWorkItem((_) =>
                    {
                        try
                        {
                            string zipPath = Path.Combine(gameDir, worldName + "_Yedek.zip");
                            if (File.Exists(zipPath)) File.Delete(zipPath);
                            ZipFile.CreateFromDirectory(targetDirName, zipPath);
                            this.Invoke(new Action(() =>
                            {
                                Log("[YEDEK] '" + worldName + "' ziple yedeklendi → " + zipPath, ThemeManager.C_EMERALD);
                                MessageBox.Show("Dünya başarıyla yedeklendi:\n" + zipPath, "LiteNex Yedekleme");
                            }));
                        }
                        catch (Exception ex)
                        {
                            this.Invoke(new Action(() => Log("[HATA] Yedeklenemedi: " + ex.Message, Color.Red)));
                        }
                    });
                };

                Button btnOpen = new Button { Text = "📂 Aç", Location = new Point(190, 44), Size = new Size(80, 24), FlatStyle = FlatStyle.Flat, BackColor = ThemeManager.C_CARD, ForeColor = ThemeManager.C_MUTED, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), Cursor = Cursors.Hand };
                btnOpen.FlatAppearance.BorderSize = 0;
                btnOpen.Click += (s, e) => { SoundSystem.PlayClick(); OpenFolder(targetDirName); };

                card.Controls.Add(lblWName); card.Controls.Add(lblWDate);
                card.Controls.Add(btnZip); card.Controls.Add(btnOpen);
                flowSavesList.Controls.Add(card);
            }
        }

        // ── Mod Preset Card Add (Dynamic Modrinth API Resolution) ────────────
        private void AddModPresetCard(string name, string desc, string modSlug)
        {
            Panel card = new Panel { Size = new Size(214, 68), BackColor = ThemeManager.C_CARD2, Margin = new Padding(0, 0, 10, 0) };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 8))
                {
                    using (SolidBrush sb = new SolidBrush(ThemeManager.C_CARD2)) e.Graphics.FillPath(sb, path);
                    using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawPath(p, path);
                }
            };

            Label lblN = new Label { Text = name, Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = ThemeManager.C_TEXT, BackColor = Color.Transparent };
            Label lblD = new Label { Text = desc, Location = new Point(10, 32), AutoSize = true, Font = new Font("Segoe UI", 7.5F), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };

            Button btnDl = new Button { Text = "Yükle", Location = new Point(144, 16), Size = new Size(58, 36), FlatStyle = FlatStyle.Flat, BackColor = ThemeManager.C_PURPLE, ForeColor = Color.White, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnDl.FlatAppearance.BorderSize = 0;

            btnDl.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                btnDl.Enabled = false; btnDl.Text = "⏳";
                Log("[MOD] Modrinth API sorgulanıyor (" + modSlug + ")...", Color.Yellow);

                ThreadPool.QueueUserWorkItem((_) =>
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) LiteNex/6.0");
                            string json = wc.DownloadString("https://api.modrinth.com/v2/project/" + modSlug + "/version");
                            JavaScriptSerializer jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                            IList versionList = jss.Deserialize<IList>(json);

                            if (versionList == null || versionList.Count == 0) throw new Exception("Modrinth API versiyon bulamadı.");

                            var firstVer = versionList[0] as Dictionary<string, object>;
                            if (firstVer == null || !firstVer.ContainsKey("files")) throw new Exception("Versiyon dosyaları eksik.");

                            IList files = firstVer["files"] as IList;
                            if (files == null || files.Count == 0) throw new Exception("Dosya listesi boş.");

                            var fileObj = files[0] as Dictionary<string, object>;
                            if (fileObj == null || !fileObj.ContainsKey("url")) throw new Exception("İndirme linki bulunamadı.");

                            string directUrl = fileObj["url"].ToString();
                            string fileName  = fileObj.ContainsKey("filename") ? fileObj["filename"].ToString() : (modSlug + ".jar");

                            string modsDir = Path.Combine(gameDir, "mods");
                            EnsureDir(modsDir);
                            string targetPath = Path.Combine(modsDir, fileName);

                            Log("[MOD] Canlı Link Alındı: " + fileName + " → İndiriliyor...", Color.Yellow);
                            wc.DownloadFile(directUrl, targetPath);

                            FileInfo fi = new FileInfo(targetPath);
                            if (!fi.Exists || fi.Length < 1000) throw new Exception("İndirilen dosya geçersiz veya 0 byte.");

                            this.Invoke(new Action(() =>
                            {
                                btnDl.Text = "✓"; btnDl.BackColor = ThemeManager.C_EMERALD;
                                Log("[MOD] " + name + " başarıyla indirildi ve kuruldu! (" + (fi.Length / 1024) + " KB) → mods/" + fileName, ThemeManager.C_EMERALD);
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new Action(() =>
                        {
                            btnDl.Enabled = true; btnDl.Text = "Tekrar";
                            Log("[MOD-HATA] " + name + " indirilemedi: " + ex.Message, Color.Red);
                        }));
                    }
                });
            };

            card.Controls.Add(lblN); card.Controls.Add(lblD); card.Controls.Add(btnDl);
            flowModsList.Controls.Add(card);
        }

        private string PingServer(string host, int port, out int latencyMs)
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(3000, false);
                    if (!success) { latencyMs = -1; return "Zaman Aşımı (Timeout)"; }
                    client.EndConnect(result);
                    sw.Stop();
                    latencyMs = (int)sw.ElapsedMilliseconds;
                    return "Online ✓ (" + latencyMs + " ms)";
                }
            }
            catch (Exception ex)
            {
                latencyMs = -1;
                return "Offline / Kapalı (" + ex.Message + ")";
            }
        }

        // ── Favorite Servers Flow Populator ─────────────────────────────────────
        private void PopulateServersFlow()
        {
            if (flowServersList == null) return;
            flowServersList.Controls.Clear();

            foreach (var sTuple in savedServers)
            {
                var localServer = sTuple;
                Panel card = new Panel { Size = new Size(284, 80), BackColor = ThemeManager.C_CARD2, Margin = new Padding(0, 0, 14, 14) };
                card.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 8))
                    {
                        using (SolidBrush sb = new SolidBrush(ThemeManager.C_CARD2)) e.Graphics.FillPath(sb, path);
                        using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawPath(p, path);
                    }
                };

                Label lblName = new Label { Text = localServer.Item1, Location = new Point(14, 12), AutoSize = true, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = ThemeManager.C_TEXT, BackColor = Color.Transparent };
                Label lblIp   = new Label { Text = localServer.Item2, Location = new Point(14, 34), AutoSize = true, Font = new Font("Segoe UI", 8.5F), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };
                Label lblPing = new Label { Text = "Ping: ...", Location = new Point(14, 52), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = ThemeManager.C_CYAN, BackColor = Color.Transparent };

                Button btnCopy = new Button { Text = "IP Kopyala", Location = new Point(184, 14), Size = new Size(86, 28), FlatStyle = FlatStyle.Flat, BackColor = ThemeManager.C_PURPLE, ForeColor = Color.White, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand };
                btnCopy.FlatAppearance.BorderSize = 0;
                btnCopy.Click += (s, e) =>
                {
                    SoundSystem.PlayClick();
                    Clipboard.SetText(localServer.Item2);
                    Log("[SUNUCU] IP kopyalandı: " + localServer.Item2, ThemeManager.C_EMERALD);
                };

                Button btnDel = new Button { Text = "Sil", Location = new Point(184, 46), Size = new Size(86, 24), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(40, 20, 30), ForeColor = Color.FromArgb(225, 29, 72), Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), Cursor = Cursors.Hand };
                btnDel.FlatAppearance.BorderSize = 0;
                btnDel.Click += (s, e) =>
                {
                    SoundSystem.PlayClick();
                    savedServers.Remove(localServer);
                    SaveServersToDisk();
                    PopulateServersFlow();
                };

                ThreadPool.QueueUserWorkItem((_) =>
                {
                    int latency;
                    string res = PingServer(localServer.Item2, 25565, out latency);
                    if (!this.IsDisposed && lblPing != null)
                    {
                        try
                        {
                            lblPing.Invoke(new Action(() =>
                            {
                                if (latency > 0 && latency <= 100)
                                {
                                    lblPing.Text = "🟢 Online (" + latency + " ms)";
                                    lblPing.ForeColor = ThemeManager.C_EMERALD;
                                }
                                else if (latency > 100)
                                {
                                    lblPing.Text = "🟡 Yüksek Ping (" + latency + " ms)";
                                    lblPing.ForeColor = Color.Orange;
                                }
                                else
                                {
                                    lblPing.Text = "🔴 Offline / Kapalı";
                                    lblPing.ForeColor = Color.FromArgb(225, 29, 72);
                                }
                            }));
                        }
                        catch { }
                    }
                });

                card.Controls.Add(lblName); card.Controls.Add(lblIp); card.Controls.Add(lblPing);
                card.Controls.Add(btnCopy); card.Controls.Add(btnDel);
                flowServersList.Controls.Add(card);
            }
        }

        // ── Versions Tab FlowList Populator ─────────────────────────────────────
        private void PopulateVersionsFlow()
        {
            if (flowVersionsList == null) return;
            flowVersionsList.Controls.Clear();

            string query = txtSearchVer != null ? txtSearchVer.Text.Trim().ToLower() : "";
            List<string> displayList = new List<string>();
            if (allMojangVersions != null && allMojangVersions.Count > 0) displayList.AddRange(allMojangVersions);
            else { foreach (var item in cbVersions.Items) displayList.Add(item.ToString()); }

            foreach (string ver in displayList)
            {
                if (!string.IsNullOrEmpty(query) && !ver.ToLower().Contains(query)) continue;

                Panel vCard = new Panel { Size = new Size(284, 70), BackColor = ThemeManager.C_CARD2, Margin = new Padding(0, 0, 14, 14) };
                vCard.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, vCard.Width - 1, vCard.Height - 1), 8))
                    {
                        using (SolidBrush sb = new SolidBrush(ThemeManager.C_CARD2)) e.Graphics.FillPath(sb, path);
                        using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawPath(p, path);
                    }
                };

                Label lblVName = new Label { Text = "Minecraft " + ver, Location = new Point(14, 14), AutoSize = true, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = ThemeManager.C_TEXT, BackColor = Color.Transparent };
                Label lblVTag  = new Label { Text = "Resmi Release", Location = new Point(14, 38), AutoSize = true, Font = new Font("Segoe UI", 8F), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };

                Button btnSelect = new Button { Text = "Seç", Location = new Point(198, 16), Size = new Size(72, 38), FlatStyle = FlatStyle.Flat, BackColor = ThemeManager.C_PURPLE, ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
                btnSelect.FlatAppearance.BorderSize = 0;
                string vTarget = ver;
                btnSelect.Click += (s, e) =>
                {
                    SoundSystem.PlayClick();
                    int idx = cbVersions.Items.IndexOf(vTarget);
                    if (idx >= 0) cbVersions.SelectedIndex = idx;
                    else { cbVersions.Items.Insert(0, vTarget); cbVersions.SelectedIndex = 0; }
                    btnNavPlay.PerformClick();
                };

                vCard.Controls.Add(lblVName); vCard.Controls.Add(lblVTag); vCard.Controls.Add(btnSelect);
                flowVersionsList.Controls.Add(vCard);
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────
        private Button MakeFolderBtn(string text, int x, int y, Action onClick)
        {
            Button b = new Button
            {
                Text = text, Location = new Point(x, y), Size = new Size(156, 40),
                FlatStyle = FlatStyle.Flat, BackColor = ThemeManager.C_CARD2, ForeColor = ThemeManager.C_PURPLE_L,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += (s, e) => { SoundSystem.PlayClick(); onClick(); };
            b.MouseEnter += (s, e) => { b.BackColor = Color.FromArgb(38, 32, 70); };
            b.MouseLeave += (s, e) => { b.BackColor = ThemeManager.C_CARD2; };
            return b;
        }

        private static Image LoadImageSafely(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (Image img = Image.FromStream(fs))
                    {
                        return new Bitmap(img);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private void OpenFolder(string folderPath)

        {
            EnsureDir(folderPath);
            try { Process.Start("explorer.exe", "\"" + folderPath + "\""); } catch { }
        }

        private void UpdatePlayerAvatar(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) username = "Steve";
            ThreadPool.QueueUserWorkItem((_) =>
            {
                try
                {
                    string avatarUrl = "https://mc-heads.net/avatar/" + Uri.EscapeDataString(username) + "/40.png";
                    using (WebClient wc = new WebClient())
                    {
                        byte[] bytes = wc.DownloadData(avatarUrl);
                        using (MemoryStream ms = new MemoryStream(bytes))
                        {
                            using (Image tempImg = Image.FromStream(ms))
                            {
                                Bitmap avatarBmp = new Bitmap(tempImg);
                                if (pbUserAvatar != null && !pbUserAvatar.IsDisposed)
                                {
                                    pbUserAvatar.Invoke(new Action(() =>
                                    {
                                        if (pbUserAvatar.Image != null)
                                        {
                                            try { pbUserAvatar.Image.Dispose(); } catch { }
                                        }
                                        pbUserAvatar.Image = avatarBmp;
                                    }));
                                }
                            }
                        }
                    }
                }
                catch { }
            });
        }

        private static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private Button MakeTitleBtn(string text, Color fore, Color hoverBg)
        {
            Button b = new Button { Text = text, Size = new Size(46, 44), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = fore, Font = new Font("Segoe UI", 10.5F), Cursor = Cursors.Hand, Margin = Padding.Empty, Padding = Padding.Empty };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = hoverBg;
            b.MouseEnter += (s, e) => { SoundSystem.PlayHover(); b.ForeColor = Color.White; };
            b.MouseLeave += (s, e) => b.ForeColor = fore;
            return b;
        }

        private Button MakeNavBtn(string text, int y)
        {
            Button b = new Button { Text = text, Location = new Point(16, y), Size = new Size(228, 44), FlatStyle = FlatStyle.Flat, ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent, Font = new Font("Segoe UI", 10F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(14, 0, 0, 0), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            b.MouseEnter += (s, e) => { SoundSystem.PlayHover(); if (b.BackColor == Color.Transparent) { b.BackColor = Color.FromArgb(20, 139, 92, 246); b.ForeColor = Color.FromArgb(200, 180, 255); } };
            b.MouseLeave += (s, e) => { if (b.BackColor != ThemeManager.C_CARD2) { b.BackColor = Color.Transparent; b.ForeColor = ThemeManager.C_MUTED; } };
            return b;
        }

        private void SetNavActive(Button active)
        {
            foreach (Button b in new[] { btnNavPlay, btnNavVersions, btnNavServers, btnNavSettings })
            {
                if (b == null) continue;
                b.BackColor = b == active ? ThemeManager.C_CARD2    : Color.Transparent;
                b.ForeColor = b == active ? ThemeManager.C_PURPLE_L : ThemeManager.C_MUTED;
            }
        }

        private void MakeDraggable(Control c)
        {
            c.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(this.Handle, 0xA1, 0x2, 0); } };
        }

        private void PositionUserCard()
        {
            if (userCard != null && sidebarPanel != null)
                userCard.Location = new Point(16, sidebarPanel.ClientSize.Height - userCard.Height - 14);
        }

        private void Log(string msg, Color col)
        {
            if (txtConsole.InvokeRequired) { txtConsole.Invoke(new Action(() => Log(msg, col))); return; }
            txtConsole.SelectionStart = txtConsole.TextLength;
            txtConsole.SelectionLength = 0;
            txtConsole.SelectionColor = col;
            txtConsole.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n");
            txtConsole.ScrollToCaret();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SYSTEM DETECT & VERSION LOAD
        // ══════════════════════════════════════════════════════════════════════
        private void DetectJavaAndSystem()
        {
            cbJavaPath.Items.Clear();
            detectedJavas.Clear();

            List<string> candidateSearchPaths = new List<string>();

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string msStoreRuntime = Path.Combine(localAppData, @"Packages\Microsoft.4297127D64EC6_8wekyb3d8bbwe\LocalCache\Local\runtime");
            if (Directory.Exists(msStoreRuntime)) candidateSearchPaths.Add(msStoreRuntime);

            string win32Runtime = @"C:\Program Files (x86)\Minecraft Launcher\runtime";
            if (Directory.Exists(win32Runtime)) candidateSearchPaths.Add(win32Runtime);

            if (Directory.Exists(@"C:\Program Files\Eclipse Adoptium")) candidateSearchPaths.Add(@"C:\Program Files\Eclipse Adoptium");
            if (Directory.Exists(@"C:\Program Files\Java")) candidateSearchPaths.Add(@"C:\Program Files\Java");

            List<string> foundExes = new List<string>();

            foreach (string searchDir in candidateSearchPaths)
            {
                try
                {
                    foreach (string exe in Directory.GetFiles(searchDir, "java.exe", SearchOption.AllDirectories))
                    {
                        if (exe.ToLower().Contains("javaw.exe")) continue;
                        if (!foundExes.Contains(exe)) foundExes.Add(exe);
                    }
                }
                catch { }
            }

            foreach (string exe in foundExes)
            {
                string lower = exe.ToLower();
                if (lower.Contains("delta") || lower.Contains("epsilon") || lower.Contains("jdk-21") || lower.Contains("java-21") || lower.Contains("21."))
                {
                    if (!detectedJavas.ContainsKey(21)) detectedJavas[21] = exe;
                }
                else if (lower.Contains("gamma") || lower.Contains("jdk-17") || lower.Contains("java-17") || lower.Contains("17."))
                {
                    if (!detectedJavas.ContainsKey(17)) detectedJavas[17] = exe;
                }
                else if (lower.Contains("beta") || lower.Contains("jre-8") || lower.Contains("jdk-8") || lower.Contains("java-8") || lower.Contains("1.8."))
                {
                    if (!detectedJavas.ContainsKey(8)) detectedJavas[8] = exe;
                }
                cbJavaPath.Items.Add(exe);
            }

            if (cbJavaPath.Items.Count == 0)
            {
                cbJavaPath.Items.Add("java.exe (Sistem PATH)");
                javaPathDetected = "java.exe";
            }
            else
            {
                cbJavaPath.SelectedIndex = 0;
                javaPathDetected = cbJavaPath.SelectedItem.ToString();
            }

            string infoStr = "Java 21: " + (detectedJavas.ContainsKey(21) ? "Var ✓" : "Yok") +
                             " | Java 17: " + (detectedJavas.ContainsKey(17) ? "Var ✓" : "Yok") +
                             " | Java 8: " + (detectedJavas.ContainsKey(8) ? "Var ✓" : "Yok");
            Log("[SYS] Algılanan Java Motorları: " + infoStr, ThemeManager.C_MUTED);
        }

        private void LoadVersionsAsync()
        {
            string[] fallback = { "1.20.4","1.20.1","1.19.4","1.18.2","1.16.5","1.12.2","1.8.9","1.7.10" };
            cbVersions.Items.Clear();
            foreach (var v in fallback) cbVersions.Items.Add(v);
            cbVersions.SelectedIndex = 0;

            ThreadPool.QueueUserWorkItem((_) =>
            {
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        string json = wc.DownloadString("https://piston-meta.mojang.com/mc/game/version_manifest_v2.json");
                        Log("[MANIFEST] Mojang manifesti doğrulandı.", ThemeManager.C_EMERALD);
                        JavaScriptSerializer jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                        var manifest = jss.Deserialize<Dictionary<string, object>>(json);
                        IList versions = manifest["versions"] as IList;
                        if (versions == null) return;

                        var releases = new List<string>();
                        foreach (object vObj in versions)
                        {
                            var v = vObj as Dictionary<string, object>;
                            if (v == null) continue;
                            string id   = v.ContainsKey("id")   ? v["id"].ToString()   : null;
                            string type = v.ContainsKey("type") ? v["type"].ToString() : "";
                            if (id != null && type == "release") releases.Add(id);
                        }
                        if (releases.Count == 0) return;

                        allMojangVersions = releases;

                        cbVersions.Invoke(new Action(() =>
                        {
                            string sel = cbVersions.SelectedItem != null ? cbVersions.SelectedItem.ToString() : "1.20.4";
                            cbVersions.Items.Clear();
                            foreach (string id in releases) cbVersions.Items.Add(id);
                            int idx = cbVersions.Items.IndexOf(sel);
                            cbVersions.SelectedIndex = idx >= 0 ? idx : 0;
                            Log("[MANIFEST] " + releases.Count + " release sürümü yüklendi.", ThemeManager.C_EMERALD);
                        }));
                    }
                }
                catch (Exception ex) { Log("[MANIFEST] Çevrimdışı mod. (" + ex.Message + ")", Color.Orange); }
            });
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PLAY LAUNCH EXECUTION
        // ══════════════════════════════════════════════════════════════════════
        private void BtnPlay_Click(object sender, EventArgs e)
        {
            SoundSystem.PlayClick();

            bool isMcRunning = false;
            if (activeMcProcess != null)
            {
                try { isMcRunning = !activeMcProcess.HasExited; }
                catch { activeMcProcess = null; isMcRunning = false; }
            }

            if (isMcRunning)
            {
                DialogResult dr = MessageBox.Show(
                    "Minecraft şu anda çalışıyor (PID: " + activeMcProcess.Id + ").\n\nOyunu kapatmak ve durdurmak istiyor musun?",
                    "LiteNex Client - Oyunu Durdur",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dr == DialogResult.Yes)
                {
                    try
                    {
                        activeMcProcess.Kill();
                        Log("[SYS] Minecraft oyunu kullanıcı tarafından durduruldu! (PID: " + activeMcProcess.Id + ")", Color.Red);
                    }
                    catch (Exception ex)
                    {
                        Log("[HATA] Oyun durdurulamadı: " + ex.Message, Color.Red);
                    }
                }
                return;
            }

            string player       = txtUsername.Text.Trim();

            if (string.IsNullOrEmpty(player)) player = "LitePlayer";
            string ver          = cbVersions.SelectedItem  != null ? cbVersions.SelectedItem.ToString()  : "1.20.4";
            int    ram          = ramSlider.Value;
            string javaFileName = ResolveJavaExe(cbJavaPath.SelectedItem != null ? cbJavaPath.SelectedItem.ToString() : "java.exe");

            btnPlay.Enabled      = false;
            btnPlay.Text         = "  ⏳    HAZIRLANIYOR...";
            progressBg.Visible   = true;
            lblStatus.Visible    = true;
            SetProgress(5, "Başlatılıyor...");

            Log("════════════════════════════════════════", ThemeManager.C_BORDER);
            Log("LiteNex Engine: Minecraft " + ver + " hazırlanıyor...", ThemeManager.C_CYAN);
            Log("Oyuncu: " + player + "  ·  RAM: " + ram + " GB  ·  Java: " + javaFileName, ThemeManager.C_MUTED);

            ThreadPool.QueueUserWorkItem((_) =>
            {
                try
                {
                    EnsureDir(Path.Combine(versionsDir, ver));
                    string clientJar  = Path.Combine(versionsDir, ver, ver + ".jar");
                    string nativesDir = Path.Combine(gameDir, "natives");
                    EnsureDir(nativesDir);

                    using (WebClient wc = new WebClient())
                    {
                        try { wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) LiteNex/6.0"); } catch {}

                        string mainClass, assetIndexId;
                        int reqJavaVer;
                        string rawCp = PrepareAndGetLaunchInfo(wc, ver, clientJar, nativesDir, out mainClass, out assetIndexId, out reqJavaVer);

                        if (reqJavaVer >= 21 && detectedJavas.ContainsKey(21))
                        {
                            javaFileName = detectedJavas[21];
                            Log("[JAVA] Minecraft " + ver + " Java 21+ gerektiriyor → Java 21 seçildi: " + javaFileName, ThemeManager.C_EMERALD);
                        }
                        else if (reqJavaVer == 17 && detectedJavas.ContainsKey(17))
                        {
                            javaFileName = detectedJavas[17];
                            Log("[JAVA] Minecraft " + ver + " Java 17 gerektiriyor → Java 17 seçildi: " + javaFileName, ThemeManager.C_EMERALD);
                        }
                        else if (reqJavaVer <= 8 && detectedJavas.ContainsKey(8))
                        {
                            javaFileName = detectedJavas[8];
                            Log("[JAVA] Minecraft " + ver + " Java 8 gerektiriyor → Java 8 seçildi: " + javaFileName, ThemeManager.C_EMERALD);
                        }

                        SetProgress(90, "Minecraft başlatılıyor...");

                        if (!File.Exists(clientJar) || new FileInfo(clientJar).Length < 1000)
                        {
                            Log("[HATA] client.jar eksik veya bozuk!", Color.Red);
                            ResetPlayButton();
                            return;
                        }

                        Log("[LAUNCH] " + mainClass, ThemeManager.C_MUTED);

                        string assetsDir    = Path.Combine(gameDir, "assets");
                        string extraJvmArgs = "-Dfile.encoding=UTF-8 -Dlog4j2.formatMsgNoLookups=true -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true";

                        if (chkFpsBoost.Checked)
                        {
                            extraJvmArgs += " -XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=32M";
                        }

                        if (!string.IsNullOrWhiteSpace(txtCustomJvmArgs.Text))
                        {
                            extraJvmArgs += " " + txtCustomJvmArgs.Text.Trim();
                        }

                        int width = 1920, height = 1080;
                        if (cbResolution.SelectedIndex == 1) { width = 1280; height = 720; }
                        else if (cbResolution.SelectedIndex == 2) { width = 1024; height = 768; }

                        string resArgs = string.Format(" --width {0} --height {1}", width, height);
                        if (chkFullscreen.Checked) resArgs += " --fullscreen";

                        string uuid = GenerateOfflineUuid(player);

                        string jvmArgs      = string.Format(
                            "{0} -Xmx{1}G -Xms1G -Djava.library.path=\"{2}\" -cp \"{3}\" {4}" +
                            " --username {5} --version {6} --gameDir \"{7}\"" +
                            " --assetsDir \"{8}\" --assetIndex {9}" +
                            " --uuid {10} --accessToken 0 --userType legacy{11}",
                            extraJvmArgs, ram, nativesDir, rawCp, mainClass,
                            player, ver, gameDir, assetsDir, assetIndexId, uuid, resArgs);

                        Log("[CMD] " + javaFileName + " ...", ThemeManager.C_MUTED);

                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = javaFileName, Arguments = jvmArgs,
                            WorkingDirectory = gameDir, UseShellExecute = false,
                            RedirectStandardOutput = true, RedirectStandardError = true,
                            CreateNoWindow = true
                        };

                        Process mc = new Process { StartInfo = psi };
                        mc.OutputDataReceived += (s, a) => { if (!string.IsNullOrEmpty(a.Data)) Log("[MC] "     + a.Data, ThemeManager.C_TEXT); };
                        mc.ErrorDataReceived  += (s, a) => { if (!string.IsNullOrEmpty(a.Data)) Log("[MC-ERR] " + a.Data, Color.FromArgb(255, 100, 100)); };
                        
                        activeMcProcess = mc;
                        mc.EnableRaisingEvents = true;
                        mc.Exited += (s, a) =>
                        {
                            activeMcProcess = null;
                            if (!this.IsDisposed)
                            {
                                try
                                {
                                    this.Invoke(new Action(() =>
                                    {
                                        btnPlay.Text = "  ▶    OYUNA BAŞLA";
                                        btnPlay.Enabled = true;
                                        btnPlay.BackColor = ThemeManager.C_PURPLE;
                                        progressBg.Visible = false;
                                        lblStatus.Visible = false;
                                        Log("[SYS] Minecraft kapandı. Yeniden başlatabilirsin.", ThemeManager.C_CYAN);
                                    }));
                                }
                                catch {}
                            }
                        };

                        mc.Start();
                        mc.BeginOutputReadLine();
                        mc.BeginErrorReadLine();

                        SetProgress(100, "Minecraft çalışıyor — PID " + mc.Id);
                        Log("[BAŞARILI] Minecraft başlatıldı! (PID: " + mc.Id + ")", ThemeManager.C_EMERALD);
                        SoundSystem.PlaySuccess();

                        this.Invoke(new Action(() =>
                        {
                            btnPlay.Text    = "  🛑    OYUNU DURDUR (FORCE EXIT)";
                            btnPlay.Enabled = true;
                            btnPlay.BackColor = Color.FromArgb(225, 29, 72);
                        }));

                    }
                }
                catch (Exception ex)
                {
                    Log("[HATA] " + ex.ToString(), Color.FromArgb(255,80,80));
                    ResetPlayButton();
                }
            });
        }

        private void EnsureBaseClientJar(WebClient wc, string baseVer, string targetJarPath)
        {
            try
            {
                string manifestJson = wc.DownloadString("https://piston-meta.mojang.com/mc/game/version_manifest_v2.json");
                JavaScriptSerializer jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var manifestDict = jss.Deserialize<Dictionary<string, object>>(manifestJson);
                IList versions = manifestDict["versions"] as IList;
                if (versions == null) return;

                string detailUrl = "";
                foreach (object vObj in versions)
                {
                    var v = vObj as Dictionary<string, object>;
                    if (v != null && v.ContainsKey("id") && v["id"].ToString() == baseVer && v.ContainsKey("url"))
                    {
                        detailUrl = v["url"].ToString();
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(detailUrl))
                {
                    string detailJson = wc.DownloadString(detailUrl);
                    var detailDict = jss.Deserialize<Dictionary<string, object>>(detailJson);
                    if (detailDict.ContainsKey("downloads"))
                    {
                        var dl = detailDict["downloads"] as Dictionary<string, object>;
                        if (dl != null && dl.ContainsKey("client"))
                        {
                            var cd = dl["client"] as Dictionary<string, object>;
                            if (cd != null && cd.ContainsKey("url"))
                            {
                                string clientUrl = cd["url"].ToString();
                                wc.DownloadFile(clientUrl, targetJarPath);
                                Log("[DOWNLOAD] Temel Minecraft " + baseVer + ".jar indirildi (" + (new FileInfo(targetJarPath).Length / 1024 / 1024) + " MB). ✓", ThemeManager.C_EMERALD);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("[WARN] Temel JAR indirilemedi: " + ex.Message, Color.Orange);
            }
        }


        private void ResetPlayButton()
        {
            this.Invoke(new Action(() => { btnPlay.Text = "  ▶    OYUNA BAŞLA"; btnPlay.Enabled = true; progressBg.Visible = false; lblStatus.Visible = false; }));
        }

        private void SetProgress(int pct, string status)
        {
            if (progressBg.InvokeRequired) { progressBg.Invoke(new Action(() => SetProgress(pct, status))); return; }
            progressFill.Width = (int)(progressBg.Width * (pct / 100.0));
            progressFill.BackColor = pct < 50 ? ThemeManager.C_PURPLE : pct < 90 ? ThemeManager.C_BLUE : ThemeManager.C_EMERALD;
            lblStatus.Text = status;
        }

        private static string GenerateOfflineUuid(string username)
        {
            try
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes("OfflinePlayer:" + username));
                    bytes[6] = (byte)((bytes[6] & 0x0f) | 0x30);
                    bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80);
                    return new Guid(bytes).ToString("N");
                }
            }
            catch
            {
                return Guid.NewGuid().ToString("N");
            }
        }

        private string ResolveJavaExe(string raw)
        {
            if (!string.IsNullOrWhiteSpace(raw) && raw.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && File.Exists(raw))
                return raw;
            return "java.exe";
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CORE LAUNCH PREPARATION
        // ══════════════════════════════════════════════════════════════════════
        private string PrepareAndGetLaunchInfo(WebClient wc, string ver, string clientJar, string nativesDir, out string mainClass, out string assetIndexId, out int reqJavaVer)
        {
            mainClass = "net.minecraft.client.main.Main";
            assetIndexId = ver;
            reqJavaVer = 17;
            List<string> cpList = new List<string>();
            cpList.Add(clientJar);
            string librariesDir = Path.Combine(gameDir, "libraries");
            EnsureDir(librariesDir);

            try
            {
                string verJsonPath = Path.Combine(versionsDir, ver, ver + ".json");
                string detailJson;
                if (File.Exists(verJsonPath))
                {
                    Log("[CACHE] Sürüm JSON'u diskten yüklendi.", ThemeManager.C_MUTED);
                    detailJson = File.ReadAllText(verJsonPath);
                }
                else
                {
                    Log("[SYNC] Mojang manifest indiriliyor...", Color.Yellow);
                    string manifestJson = wc.DownloadString("https://piston-meta.mojang.com/mc/game/version_manifest_v2.json");
                    JavaScriptSerializer jss0 = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                    var manifestDict = jss0.Deserialize<Dictionary<string, object>>(manifestJson);
                    IList versions = manifestDict["versions"] as IList;
                    if (versions == null) throw new Exception("Manifest parse hatası.");

                    string detailUrl = "";
                    foreach (object vObj in versions)
                    {
                        var v = vObj as Dictionary<string, object>;
                        if (v == null) continue;
                        if (v.ContainsKey("id") && v["id"].ToString() == ver && v.ContainsKey("url")) { detailUrl = v["url"].ToString(); break; }
                    }
                    if (string.IsNullOrEmpty(detailUrl))
                    {
                        Log("[WARN] " + ver + " Mojang'da bulunamadı.", Color.Orange);
                        return ScanLocalLibraries(librariesDir, clientJar, cpList);
                    }
                    Log("[SYNC] " + ver + " sürüm bilgisi indiriliyor...", Color.Yellow);
                    detailJson = wc.DownloadString(detailUrl);
                    try { File.WriteAllText(verJsonPath, detailJson); } catch {}
                }

                JavaScriptSerializer jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var detailDict = jss.Deserialize<Dictionary<string, object>>(detailJson);

                if (detailDict.ContainsKey("mainClass")) mainClass = detailDict["mainClass"].ToString();

                if (detailDict.ContainsKey("javaVersion"))
                {
                    var jv = detailDict["javaVersion"] as Dictionary<string, object>;
                    if (jv != null && jv.ContainsKey("majorVersion"))
                    {
                        int.TryParse(jv["majorVersion"].ToString(), out reqJavaVer);
                    }
                }

                string assetIndexUrl = null;
                if (detailDict.ContainsKey("assetIndex"))
                {
                    var ai = detailDict["assetIndex"] as Dictionary<string, object>;
                    if (ai != null)
                    {
                        if (ai.ContainsKey("id"))  assetIndexId  = ai["id"].ToString();
                        if (ai.ContainsKey("url")) assetIndexUrl = ai["url"].ToString();
                    }
                }

                // Ensure base client.jar (for Fabric/Forge loaders that inherit from base releases)
                string baseVer = ver;
                var verMatch = System.Text.RegularExpressions.Regex.Match(ver, @"\d+\.\d+(\.\d+)?");
                if (verMatch.Success) baseVer = verMatch.Value;
                string baseJarPath = Path.Combine(versionsDir, baseVer, baseVer + ".jar");
                EnsureDir(Path.Combine(versionsDir, baseVer));

                if (!File.Exists(baseJarPath) || new FileInfo(baseJarPath).Length < 1000000)
                {
                    Log("[SYNC] Temel Minecraft " + baseVer + ".jar indiriliyor...", Color.Yellow);
                    EnsureBaseClientJar(wc, baseVer, baseJarPath);
                }

                if (File.Exists(baseJarPath) && !cpList.Contains(baseJarPath))
                {
                    cpList.Add(baseJarPath);
                }

                // client.jar check
                FileInfo jarInfo = new FileInfo(clientJar);
                if (!jarInfo.Exists || jarInfo.Length < 1000000)
                {
                    string clientUrl = null;
                    if (detailDict.ContainsKey("downloads"))
                    {
                        var dl = detailDict["downloads"] as Dictionary<string, object>;
                        if (dl != null && dl.ContainsKey("client"))
                        {
                            var cd = dl["client"] as Dictionary<string, object>;
                            if (cd != null && cd.ContainsKey("url")) clientUrl = cd["url"].ToString();
                        }
                    }
                    if (!string.IsNullOrEmpty(clientUrl))
                    {
                        if (jarInfo.Exists) { try { jarInfo.Delete(); } catch {} }
                        Log("[DOWNLOAD] client.jar indiriliyor...", Color.Yellow);
                        SetProgress(20, "client.jar indiriliyor...");
                        wc.DownloadFile(clientUrl, clientJar);
                        Log("[DOWNLOAD] client.jar indirildi (" + new FileInfo(clientJar).Length / 1024 / 1024 + " MB).", ThemeManager.C_EMERALD);
                    }
                }
                else { Log("[INFO] client.jar mevcut (" + jarInfo.Length/1024/1024 + " MB).", ThemeManager.C_EMERALD); }


                SetProgress(30, "Kütüphaneler kontrol ediliyor...");

                // libraries + natives
                if (detailDict.ContainsKey("libraries"))
                {
                    IList libs = detailDict["libraries"] as IList;
                    if (libs == null) throw new Exception("libraries parse hatası.");
                    Log("[LIBRARY] " + libs.Count + " kütüphane kontrol ediliyor...", Color.Yellow);
                    int downloaded = 0;

                    foreach (object libObj in libs)
                    {
                        var lib = libObj as Dictionary<string, object>;
                        if (lib == null) continue;
                        if (lib.ContainsKey("rules") && !IsLibraryAllowedOnWindows(lib["rules"] as IList)) continue;
                        if (!lib.ContainsKey("downloads")) continue;
                        var downloads = lib["downloads"] as Dictionary<string, object>;
                        if (downloads == null) continue;

                        if (downloads.ContainsKey("artifact"))
                        {
                            var art = downloads["artifact"] as Dictionary<string, object>;
                            if (art != null && art.ContainsKey("url") && art.ContainsKey("path"))
                            {
                                string lp = Path.Combine(librariesDir, art["path"].ToString().Replace('/', Path.DirectorySeparatorChar));
                                EnsureDir(Path.GetDirectoryName(lp));
                                if (!File.Exists(lp) || new FileInfo(lp).Length < 100) { try { wc.DownloadFile(art["url"].ToString(), lp); downloaded++; } catch {} }
                                if (File.Exists(lp)) cpList.Add(lp);
                            }
                        }

                        if (lib.ContainsKey("natives") && downloads.ContainsKey("classifiers"))
                        {
                            var nm = lib["natives"] as Dictionary<string, object>;
                            var cl = downloads["classifiers"] as Dictionary<string, object>;
                            if (nm != null && cl != null && nm.ContainsKey("windows"))
                            {
                                string key = nm["windows"].ToString().Replace("${arch}", "64");
                                if (cl.ContainsKey(key))
                                {
                                    var na = cl[key] as Dictionary<string, object>;
                                    if (na != null && na.ContainsKey("url") && na.ContainsKey("path"))
                                    {
                                        string np = Path.Combine(librariesDir, na["path"].ToString().Replace('/', Path.DirectorySeparatorChar));
                                        EnsureDir(Path.GetDirectoryName(np));
                                        if (!File.Exists(np) || new FileInfo(np).Length < 100) { try { wc.DownloadFile(na["url"].ToString(), np); downloaded++; } catch {} }
                                        if (File.Exists(np)) ExtractNatives(np, nativesDir);
                                    }
                                }
                            }
                        }
                    }
                    if (downloaded > 0) Log("[LIBRARY] " + downloaded + " yeni kütüphane indirildi.", ThemeManager.C_EMERALD);
                    else Log("[LIBRARY] Tüm kütüphaneler hazır. ✓", ThemeManager.C_EMERALD);
                }

                SetProgress(60, "Assetler indiriliyor...");
                if (!string.IsNullOrEmpty(assetIndexUrl))
                    DownloadAssets(wc, assetIndexId, assetIndexUrl);

                SetProgress(85, "Başlatma hazırlığı tamamlandı.");
            }
            catch (Exception ex)
            {
                Log("[WARN] " + ex.Message, Color.Orange);
                return ScanLocalLibraries(Path.Combine(gameDir, "libraries"), clientJar, cpList);
            }

            return string.Join(";", cpList.ToArray());
        }

        private void DownloadAssets(WebClient wc, string assetIndexId, string assetIndexUrl)
        {
            try
            {
                string assetsDir  = Path.Combine(gameDir, "assets");
                string indexesDir = Path.Combine(assetsDir, "indexes");
                string objectsDir = Path.Combine(assetsDir, "objects");
                EnsureDir(indexesDir); EnsureDir(objectsDir);

                string indexFile = Path.Combine(indexesDir, assetIndexId + ".json");
                if (!File.Exists(indexFile))
                {
                    Log("[ASSETS] Asset indeksi indiriliyor (" + assetIndexId + ")...", Color.Yellow);
                    wc.DownloadFile(assetIndexUrl, indexFile);
                }

                string indexJson = File.ReadAllText(indexFile);
                JavaScriptSerializer jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var indexDict = jss.Deserialize<Dictionary<string, object>>(indexJson);
                if (!indexDict.ContainsKey("objects")) return;

                bool   isVirtual  = indexDict.ContainsKey("virtual") && indexDict["virtual"] is bool && (bool)indexDict["virtual"];
                string virtualDir = isVirtual ? Path.Combine(assetsDir, "virtual", assetIndexId) : null;
                if (isVirtual) EnsureDir(virtualDir);

                var objects = indexDict["objects"] as Dictionary<string, object>;
                if (objects == null) return;

                var missing = new List<Tuple<string, string, string>>();
                foreach (var kvp in objects)
                {
                    var obj = kvp.Value as Dictionary<string, object>;
                    if (obj == null || !obj.ContainsKey("hash")) continue;
                    string hash    = obj["hash"].ToString();
                    string prefix  = hash.Substring(0, 2);
                    string objPath = Path.Combine(objectsDir, prefix, hash);
                    if (!File.Exists(objPath))
                        missing.Add(Tuple.Create("https://resources.download.minecraft.net/" + prefix + "/" + hash, objPath, kvp.Key));
                }

                if (missing.Count == 0)
                {
                    Log("[ASSETS] Tüm assetler mevcut (" + objects.Count + " dosya). ✓", ThemeManager.C_EMERALD);
                    return;
                }

                const int THREADS = 16;
                int completed  = 0;
                int total      = missing.Count;
                long startTick = DateTime.Now.Ticks;

                Log("[ASSETS] " + total + "/" + objects.Count + " asset eksik — 16 paralel thread ile indiriliyor...", Color.Yellow);

                CountdownEvent countdown = new CountdownEvent(total);
                SemaphoreSlim semaphore  = new SemaphoreSlim(THREADS, THREADS);

                foreach (var item in missing)
                {
                    var localItem = item;
                    ThreadPool.QueueUserWorkItem((_) =>
                    {
                        semaphore.Wait();
                        try
                        {
                            EnsureDir(Path.GetDirectoryName(localItem.Item2));
                            if (!File.Exists(localItem.Item2))
                            {
                                using (WebClient dlWc = new WebClient())
                                {
                                    dlWc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) LiteNex/6.0");
                                    dlWc.DownloadFile(localItem.Item1, localItem.Item2);
                                }
                            }

                            if (isVirtual && virtualDir != null && File.Exists(localItem.Item2))
                            {
                                string vp = Path.Combine(virtualDir, localItem.Item3.Replace('/', Path.DirectorySeparatorChar));
                                EnsureDir(Path.GetDirectoryName(vp));
                                if (!File.Exists(vp)) { try { File.Copy(localItem.Item2, vp); } catch {} }
                            }

                            int c = Interlocked.Increment(ref completed);
                            if (c % 50 == 0 || c == total)
                            {
                                double elapsed = Math.Max(0.1, (DateTime.Now.Ticks - startTick) / 10000000.0);
                                double speed   = c / elapsed;
                                int    eta     = speed > 0 ? (int)Math.Round((total - c) / speed) : 0;
                                int    pct     = 60 + (int)(25.0 * c / total);
                                SetProgress(pct, "Assets: " + c + "/" + total + "  ·  " + Math.Round(speed) + " dosya/s  ·  ~" + eta + "s kaldı");
                                if (c % 200 == 0 || c == total)
                                    Log("[ASSETS] " + c + "/" + total + "  |  " + Math.Round(speed) + " dosya/s  |  ~" + eta + "s", ThemeManager.C_MUTED);
                            }
                        }
                        catch { }
                        finally { semaphore.Release(); countdown.Signal(); }
                    });
                }

                countdown.Wait();
                double totalSec = (DateTime.Now.Ticks - startTick) / 10000000.0;
                Log("[ASSETS] " + completed + " asset " + Math.Round(totalSec, 1) + "s'de indirildi. ✓", ThemeManager.C_EMERALD);
            }
            catch (Exception ex) { Log("[ASSETS] Uyarı: " + ex.Message, Color.Orange); }
        }

        private bool IsLibraryAllowedOnWindows(IList rules)
        {
            if (rules == null || rules.Count == 0) return true;
            bool allowed = false;
            foreach (object rObj in rules)
            {
                var rule = rObj as Dictionary<string, object>;
                if (rule == null) continue;
                string action = rule.ContainsKey("action") ? rule["action"].ToString() : "allow";
                if (rule.ContainsKey("os"))
                {
                    var os = rule["os"] as Dictionary<string, object>;
                    if (os != null && os.ContainsKey("name") && os["name"].ToString() == "windows")
                        allowed = (action == "allow");
                }
                else { allowed = (action == "allow"); }
            }
            return allowed;
        }

        private void ExtractNatives(string jarPath, string nativesDir)
        {
            try
            {
                using (FileStream fs = File.OpenRead(jarPath))
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        if (entry.FullName.StartsWith("META-INF", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(entry.Name)) continue;
                        if (!entry.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && !entry.Name.EndsWith(".so", StringComparison.OrdinalIgnoreCase)) continue;
                        string dest = Path.Combine(nativesDir, entry.Name);
                        if (!File.Exists(dest)) { try { entry.ExtractToFile(dest); } catch {} }
                    }
                }
            }
            catch {}
        }

        private string ScanLocalLibraries(string librariesDir, string clientJar, List<string> cpList)
        {
            try { foreach (string jar in Directory.GetFiles(librariesDir, "*.jar", SearchOption.AllDirectories)) if (!cpList.Contains(jar)) cpList.Add(jar); } catch {}
            return string.Join(";", cpList.ToArray());
        }

        private static void EnsureDir(string dir)
        {
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  GITHUB AUTOMATIC AUTO-UPDATER
        // ══════════════════════════════════════════════════════════════════════
        public const string GITHUB_UPDATE_URL = "https://raw.githubusercontent.com/linezoom7-cloud/LiteNexLauncher/main/version.json";
        public const int CURRENT_VERSION_CODE = 640;
        public const string CURRENT_VERSION_NAME = "6.4.0";

        private void CheckForGitHubUpdatesAsync(bool silent)
        {
            ThreadPool.QueueUserWorkItem((_) =>
            {
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) LiteNex/6.0");
                        string json = wc.DownloadString(GITHUB_UPDATE_URL);
                        JavaScriptSerializer jss = new JavaScriptSerializer();
                        var dict = jss.Deserialize<Dictionary<string, object>>(json);
                        if (dict != null && dict.ContainsKey("versionCode"))
                        {
                            int remoteCode = Convert.ToInt32(dict["versionCode"]);
                            string remoteVer = dict.ContainsKey("version") ? dict["version"].ToString() : "yeni";
                            string dlUrl = dict.ContainsKey("downloadUrl") ? dict["downloadUrl"].ToString() : "";
                            string changelog = dict.ContainsKey("changelog") ? dict["changelog"].ToString() : "";

                            if (remoteCode > CURRENT_VERSION_CODE)
                            {
                                if (!this.IsDisposed)
                                {
                                    this.Invoke(new Action(() =>
                                    {
                                        Log("[GÜNCELLEME] Yeni sürüm mevcut: v" + remoteVer + " (Mevcut: v" + CURRENT_VERSION_NAME + ")", ThemeManager.C_EMERALD);
                                        ShowUpdatePromptDialog(remoteVer, dlUrl, changelog);
                                    }));
                                }
                                return;
                            }
                        }
                    }
                    if (!silent && !this.IsDisposed)
                    {
                        this.Invoke(new Action(() =>
                        {
                            Log("[GÜNCELLEME] LiteNex Client v" + CURRENT_VERSION_NAME + " güncel. ✓", ThemeManager.C_EMERALD);
                            MessageBox.Show("LiteNex Client v" + CURRENT_VERSION_NAME + " Ultimate Edition güncel!\nEn yeni sürümü kullanıyorsunuz.", "LiteNex Güncelleme");
                        }));
                    }
                }
                catch (Exception ex)
                {
                    if (!silent && !this.IsDisposed)
                    {
                        this.Invoke(new Action(() =>
                        {
                            Log("[GÜNCELLEME-WARN] Güncelleme denetimi uyarısı: " + ex.Message, Color.Orange);
                            MessageBox.Show("Güncelleme sunucusuna (GitHub) bağlanılamadı:\n" + ex.Message + "\n\nLütfen internet bağlantınızı kontrol edin.", "LiteNex Güncelleme");
                        }));
                    }
                }
            });
        }

        private void ShowUpdatePromptDialog(string newVer, string dlUrl, string changelog)
        {
            Form updateForm = new Form
            {
                Text = "LiteNex Otomatik Güncelleme",
                Size = new Size(520, 340),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = ThemeManager.C_BG,
                ForeColor = ThemeManager.C_TEXT,
                TopMost = true
            };

            Panel header = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = ThemeManager.C_TITLEBAR };
            Label lblT = new Label { Text = "🚀 Yeni Güncelleme Mevcut! (v" + newVer + ")", Location = new Point(14, 0), Size = new Size(450, 44), Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = ThemeManager.C_CYAN, TextAlign = ContentAlignment.MiddleLeft };
            header.Controls.Add(lblT);

            Panel card = new Panel { Location = new Point(16, 56), Size = new Size(488, 268), BackColor = ThemeManager.C_CARD };
            Label lblChangesH = new Label { Text = "SÜRÜM YENİLİKLERİ VE DEĞİŞİKLİKLER", Location = new Point(14, 12), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = ThemeManager.C_MUTED };
            RichTextBox rtbLog = new RichTextBox { Location = new Point(14, 34), Size = new Size(460, 160), BackColor = ThemeManager.C_CONSOLE, ForeColor = Color.FromArgb(200, 230, 200), Font = new Font("Consolas", 9F), ReadOnly = true, BorderStyle = BorderStyle.None, Text = changelog };

            Button btnDoUpdate = new Button { Text = "⚡  Şimdi Güncelle", Location = new Point(14, 208), Size = new Size(310, 44), FlatStyle = FlatStyle.Flat, BackColor = ThemeManager.C_PURPLE, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnDoUpdate.FlatAppearance.BorderSize = 0;

            Button btnSkip = new Button { Text = "Kapat", Location = new Point(334, 208), Size = new Size(140, 44), FlatStyle = FlatStyle.Flat, BackColor = ThemeManager.C_CARD2, ForeColor = ThemeManager.C_MUTED, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSkip.FlatAppearance.BorderSize = 0;
            btnSkip.Click += (s, e) => updateForm.Close();

            btnDoUpdate.Click += (s, e) =>
            {
                SoundSystem.PlayClick();
                btnDoUpdate.Enabled = false;
                btnDoUpdate.Text = "⏳ İndiriliyor...";
                ThreadPool.QueueUserWorkItem((_) =>
                {
                    try
                    {
                        string appDir = AppDomain.CurrentDomain.BaseDirectory;
                        string newExePath = Path.Combine(appDir, "LiteNex_new.exe");
                        using (WebClient wc = new WebClient())
                        {
                            wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) LiteNex/6.0");
                            wc.DownloadFile(dlUrl, newExePath);
                        }

                        if (File.Exists(newExePath) && new FileInfo(newExePath).Length > 10000)
                        {
                            string updateBatPath = Path.Combine(appDir, "update.bat");
                            string batScript =
                                "@echo off\r\n" +
                                "timeout /t 1 /nobreak > nul\r\n" +
                                "copy /y \"LiteNex_new.exe\" \"LiteNex.exe\" > nul\r\n" +
                                "del /f /q \"LiteNex_new.exe\" > nul\r\n" +
                                "start \"\" \"LiteNex.exe\"\r\n" +
                                "del /f /q \"%~f0\"\r\n";
                            File.WriteAllText(updateBatPath, batScript);

                            ProcessStartInfo psi = new ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                Arguments = "/c \"" + updateBatPath + "\"",
                                CreateNoWindow = true,
                                UseShellExecute = false
                            };
                            Process.Start(psi);
                            if (!this.IsDisposed) this.Invoke(new Action(() => Application.Exit()));
                        }
                        else throw new Exception("İndirilen güncelleme dosyası geçersiz.");
                    }
                    catch (Exception ex)
                    {
                        if (!this.IsDisposed)
                        {
                            this.Invoke(new Action(() =>
                            {
                                MessageBox.Show("Güncelleme indirilemedi:\n" + ex.Message, "LiteNex Güncelleme Hatası");
                                updateForm.Close();
                            }));
                        }
                    }
                });
            };

            card.Controls.Add(lblChangesH);
            card.Controls.Add(rtbLog);
            card.Controls.Add(btnDoUpdate);
            card.Controls.Add(btnSkip);
            updateForm.Controls.Add(header);
            updateForm.Controls.Add(card);

            updateForm.ShowDialog(this);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  STEAM-STYLE PRE-LAUNCH UPDATER SPLASH FORM
    // ══════════════════════════════════════════════════════════════════════════
    public class SteamUpdaterSplashForm : Form
    {
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private Panel progressBg, progressFill;
        private Label lblStatus, lblSubStatus;
        public bool UpdateApplied = false;
        public bool ShouldLaunchMain = true;

        public SteamUpdaterSplashForm()
        {
            this.DoubleBuffered = true;
            InitializeSplashUI();
        }

        private void InitializeSplashUI()
        {
            this.Text = "LiteNex Client Updater";
            this.Size = new Size(480, 220);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ThemeManager.C_BG;
            this.ForeColor = ThemeManager.C_TEXT;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;

            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string logoIcoPath = Path.Combine(appDir, "logo.ico");
            string logoPngPath = Path.Combine(appDir, "logo.png");
            if (File.Exists(logoIcoPath)) { try { this.Icon = new Icon(logoIcoPath); } catch {} }

            Panel mainCard = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.C_CARD };
            mainCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, mainCard.Width - 1, mainCard.Height - 1), 10))
                {
                    using (SolidBrush sb = new SolidBrush(ThemeManager.C_CARD)) e.Graphics.FillPath(sb, path);
                    using (Pen p = new Pen(ThemeManager.C_BORDER)) e.Graphics.DrawPath(p, path);
                }
                using (LinearGradientBrush lg = new LinearGradientBrush(new Rectangle(10, 0, mainCard.Width - 20, 2), ThemeManager.C_PURPLE, ThemeManager.C_CYAN, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(lg, 10, 0, mainCard.Width - 20, 2);
            };

            PictureBox pbLogo = new PictureBox { Location = new Point(20, 24), Size = new Size(48, 48), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Transparent };
            Image img = LoadImageSafely(logoPngPath);
            if (img != null) pbLogo.Image = img;

            Label lblTitle = new Label { Text = "LiteNex Client", Location = new Point(80, 22), AutoSize = true, Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent };
            lblSubStatus = new Label { Text = "Steam-Style Auto Updater v6.0", Location = new Point(80, 48), AutoSize = true, Font = new Font("Segoe UI", 8.5F), ForeColor = ThemeManager.C_MUTED, BackColor = Color.Transparent };

            lblStatus = new Label { Text = "Güncellemeler denetleniyor...", Location = new Point(20, 100), Size = new Size(440, 22), Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.C_CYAN, BackColor = Color.Transparent };

            progressBg = new Panel { Location = new Point(20, 128), Size = new Size(440, 12), BackColor = Color.FromArgb(26, 23, 46) };
            progressFill = new Panel { Location = new Point(0, 0), Size = new Size(40, 12), BackColor = ThemeManager.C_PURPLE };
            progressBg.Controls.Add(progressFill);

            Label lblFooter = new Label { Text = "© LiteNex Studios — Minecraft Engine", Location = new Point(20, 165), AutoSize = true, Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(90, 85, 130), BackColor = Color.Transparent };

            mainCard.Controls.Add(pbLogo);
            mainCard.Controls.Add(lblTitle);
            mainCard.Controls.Add(lblSubStatus);
            mainCard.Controls.Add(lblStatus);
            mainCard.Controls.Add(progressBg);
            mainCard.Controls.Add(lblFooter);
            this.Controls.Add(mainCard);

            this.Shown += (s, e) => PerformPreLaunchCheck();
        }

        private void PerformPreLaunchCheck()
        {
            ThreadPool.QueueUserWorkItem((_) =>
            {
                try
                {
                    UpdateProgress(20, "Güncellemeler denetleniyor...");
                    Thread.Sleep(200);

                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) LiteNex/6.0");
                        string json = wc.DownloadString("https://raw.githubusercontent.com/linezoom7-cloud/LiteNexLauncher/main/version.json");
                        JavaScriptSerializer jss = new JavaScriptSerializer();
                        var dict = jss.Deserialize<Dictionary<string, object>>(json);

                        if (dict != null && dict.ContainsKey("versionCode"))
                        {
                            int remoteCode = Convert.ToInt32(dict["versionCode"]);
                            string remoteVer = dict.ContainsKey("version") ? dict["version"].ToString() : "yeni";
                            string dlUrl = dict.ContainsKey("downloadUrl") ? dict["downloadUrl"].ToString() : "";

                            if (remoteCode > MainForm.CURRENT_VERSION_CODE && !string.IsNullOrEmpty(dlUrl))
                            {
                                UpdateProgress(45, "🚀 Yeni Güncelleme Bulundu: v" + remoteVer + " — İndiriliyor...");
                                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                                string newExePath = Path.Combine(appDir, "LiteNex_new.exe");

                                wc.DownloadFile(dlUrl, newExePath);

                                if (File.Exists(newExePath) && new FileInfo(newExePath).Length > 10000)
                                {
                                    UpdateProgress(95, "Güncelleme uygulanıyor...");
                                    string updateBatPath = Path.Combine(appDir, "update.bat");
                                    string batScript =
                                        "@echo off\r\n" +
                                        "timeout /t 1 /nobreak > nul\r\n" +
                                        "copy /y \"LiteNex_new.exe\" \"LiteNex.exe\" > nul\r\n" +
                                        "del /f /q \"LiteNex_new.exe\" > nul\r\n" +
                                        "start \"\" \"LiteNex.exe\"\r\n" +
                                        "del /f /q \"%~f0\"\r\n";
                                    File.WriteAllText(updateBatPath, batScript);

                                    ProcessStartInfo psi = new ProcessStartInfo
                                    {
                                        FileName = "cmd.exe",
                                        Arguments = "/c \"" + updateBatPath + "\"",
                                        CreateNoWindow = true,
                                        UseShellExecute = false
                                    };
                                    Process.Start(psi);

                                    ShouldLaunchMain = false;
                                    UpdateApplied = true;
                                    if (!this.IsDisposed) this.Invoke(new Action(() => this.Close()));
                                    return;
                                }
                            }
                        }
                    }
                    UpdateProgress(100, "LiteNex v" + MainForm.CURRENT_VERSION_NAME + " Güncel. Başlatılıyor...");
                    Thread.Sleep(200);
                }
                catch { }

                ShouldLaunchMain = true;
                if (!this.IsDisposed) this.Invoke(new Action(() => this.Close()));
            });
        }

        private void UpdateProgress(int pct, string status)
        {
            if (this.IsDisposed) return;
            this.Invoke(new Action(() =>
            {
                lblStatus.Text = status;
                progressFill.Width = (int)(progressBg.Width * (pct / 100.0));
                if (pct >= 50) progressFill.BackColor = ThemeManager.C_CYAN;
                if (pct >= 90) progressFill.BackColor = ThemeManager.C_EMERALD;
            }));
        }

        private static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static Image LoadImageSafely(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (Image img = Image.FromStream(fs))
                        return new Bitmap(img);
                }
            }
            catch { return null; }
        }
    }

    public static class Program
    {
        [STAThread]
        static void Main()
        {
            try { ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072|(SecurityProtocolType)768|SecurityProtocolType.Tls; } catch {}
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SteamUpdaterSplashForm splash = new SteamUpdaterSplashForm();
            Application.Run(splash);

            if (splash.ShouldLaunchMain)
            {
                Application.Run(new MainForm());
            }
        }
    }
}
