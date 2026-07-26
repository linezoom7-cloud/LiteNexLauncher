using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

// ── ASSEMBLY METADATA (Windows Defender False Positive Azaltıcı) ──────────────
[assembly: AssemblyTitle("LiteNex Client Setup")]
[assembly: AssemblyDescription("LiteNex Minecraft Client and Installer Bundle")]
[assembly: AssemblyCompany("LiteNex Studios")]
[assembly: AssemblyProduct("LiteNex Installer")]
[assembly: AssemblyCopyright("Copyright © 2026 LiteNex Studios")]
[assembly: AssemblyTrademark("LiteNex")]
[assembly: AssemblyVersion("6.8.1.0")]
[assembly: AssemblyFileVersion("6.8.1.0")]
[assembly: Guid("8f3954ce-c84a-4d2c-8cb9-bc22394fae7a")]

namespace LiteNexSetup
{
    // ══════════════════════════════════════════════════════════════════════════
    //  THEME CONSTANTS & NEON COLOR SYSTEM
    // ══════════════════════════════════════════════════════════════════════════
    public static class SetupTheme
    {
        public static Color C_BG       = Color.FromArgb(  9,  8, 18);
        public static Color C_CARD     = Color.FromArgb( 19, 17, 34);
        public static Color C_CARD2    = Color.FromArgb( 26, 23, 46);
        public static Color C_TITLEBAR = Color.FromArgb(  7,  6, 14);
        public static Color C_BORDER   = Color.FromArgb( 42, 38, 72);
        public static Color C_PURPLE   = Color.FromArgb(139, 92, 246);
        public static Color C_PURPLE_D = Color.FromArgb( 99, 52, 210);
        public static Color C_PURPLE_L = Color.FromArgb(167,139, 250);
        public static Color C_CYAN     = Color.FromArgb( 34,211, 238);
        public static Color C_EMERALD  = Color.FromArgb( 16,185, 129);
        public static Color C_TEXT     = Color.FromArgb(241,245, 249);
        public static Color C_MUTED    = Color.FromArgb(148,163, 184);
        public static Color C_CONSOLE  = Color.FromArgb(  6,  5, 14);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  SYNTHESIZER AUDIO SYSTEM (Futuristic UI Sounds)
    // ══════════════════════════════════════════════════════════════════════════
    public static class SetupSoundSystem
    {
        public static bool Enabled = true;

        public static void PlayClick()
        {
            if (!Enabled) return;
            ThreadPool.QueueUserWorkItem((_) => GenerateAndPlayTone(880, 0.03, 0.15, 0.005));
        }

        public static void PlayHover()
        {
            if (!Enabled) return;
            ThreadPool.QueueUserWorkItem((_) => GenerateAndPlayTone(1200, 0.015, 0.05, 0.003));
        }

        public static void PlaySuccess()
        {
            if (!Enabled) return;
            ThreadPool.QueueUserWorkItem((_) =>
            {
                GenerateAndPlayTone(523, 0.06, 0.20, 0.010);
                Thread.Sleep(40);
                GenerateAndPlayTone(659, 0.06, 0.20, 0.010);
                Thread.Sleep(40);
                GenerateAndPlayTone(784, 0.10, 0.25, 0.020);
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

                byte[] header = new byte[44];
                int dataLen = pcmData.Length;
                int totalLen = dataLen + 36;
                header[0] = 0x52; header[1] = 0x49; header[2] = 0x46; header[3] = 0x46;
                header[4] = (byte)(totalLen & 0xFF); header[5] = (byte)((totalLen >> 8) & 0xFF);
                header[6] = (byte)((totalLen >> 16) & 0xFF); header[7] = (byte)((totalLen >> 24) & 0xFF);
                header[8] = 0x57; header[9] = 0x41; header[10] = 0x56; header[11] = 0x45;
                header[12] = 0x66; header[13] = 0x6D; header[14] = 0x74; header[15] = 0x20;
                header[16] = 16; header[17] = 0; header[18] = 0; header[19] = 0;
                header[20] = 1; header[21] = 0; header[22] = 1; header[23] = 0;
                header[24] = (byte)(sampleRate & 0xFF); header[25] = (byte)((sampleRate >> 8) & 0xFF);
                header[26] = (byte)((sampleRate >> 16) & 0xFF); header[27] = (byte)((sampleRate >> 24) & 0xFF);
                header[28] = (byte)((sampleRate * 2) & 0xFF); header[29] = (byte)(((sampleRate * 2) >> 8) & 0xFF);
                header[30] = (byte)(((sampleRate * 2) >> 16) & 0xFF); header[31] = (byte)(((sampleRate * 2) >> 24) & 0xFF);
                header[32] = 2; header[33] = 0; header[34] = 16; header[35] = 0;
                header[36] = 0x64; header[37] = 0x61; header[38] = 0x74; header[39] = 0x61;
                header[40] = (byte)(dataLen & 0xFF); header[41] = (byte)((dataLen >> 8) & 0xFF);
                header[42] = (byte)((dataLen >> 16) & 0xFF); header[43] = (byte)((dataLen >> 24) & 0xFF);

                byte[] fullWav = new byte[header.Length + pcmData.Length];
                Buffer.BlockCopy(header, 0, fullWav, 0, header.Length);
                Buffer.BlockCopy(pcmData, 0, fullWav, header.Length, pcmData.Length);

                using (MemoryStream ms = new MemoryStream(fullWav))
                using (System.Media.SoundPlayer sp = new System.Media.SoundPlayer(ms))
                {
                    sp.PlaySync();
                }
            }
            catch { }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  MAIN SETUP INSTALLER FORM
    // ══════════════════════════════════════════════════════════════════════════
    public class SetupForm : Form
    {
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private Panel pnlHeader, pnlStep1, pnlStep2, pnlStep3, pnlStep4, pnlFooter;
        private PictureBox pbLogo;
        private Label lblHeaderTitle, lblHeaderSub;
        private RichTextBox rtbTerms;
        private CheckBox chkAcceptTerms, chkDesktopShortcut, chkStartMenuShortcut, chkLaunchAfterSetup, chkRegisterUninstall;
        private TextBox txtInstallDir;
        private Button btnBrowseDir, btnNext, btnBack, btnCancel;
        private Panel progressBg, progressFill;
        private Label lblStatus, lblDiskSpace;

        private int currentStep = 1; // 1: Terms, 2: Config, 3: Installing, 4: Finished
        private string targetDirectory = "";

        public SetupForm()
        {
            this.DoubleBuffered = true;
            InitializeSetupUI();
        }

        private void InitializeSetupUI()
        {
            this.Text            = "LiteNex Client v6.1 Kurulum Sihirbazı";
            this.Size            = new Size(740, 540);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.BackColor       = SetupTheme.C_BG;
            this.ForeColor       = SetupTheme.C_TEXT;
            this.Font            = new Font("Segoe UI", 9.5F);
            this.FormBorderStyle = FormBorderStyle.None;

            string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LiteNexClient");
            targetDirectory = defaultPath;

            Icon setupIcon = LoadEmbeddedIcon("logo.ico");
            if (setupIcon != null) this.Icon = setupIcon;
            Image setupLogoImg = LoadEmbeddedImage("logo.png");

            // ── TITLE BAR ──────────────────────────────────────────────────────
            Panel titleBar = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = SetupTheme.C_TITLEBAR };
            MakeDraggable(titleBar);

            titleBar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (LinearGradientBrush lg = new LinearGradientBrush(new Rectangle(0, 41, titleBar.Width, 1), SetupTheme.C_PURPLE, SetupTheme.C_CYAN, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(lg, 0, 41, titleBar.Width, 1);
            };

            PictureBox tbIcon = new PictureBox { Location = new Point(14, 9), Size = new Size(24, 24), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Transparent };
            if (setupLogoImg != null) tbIcon.Image = setupLogoImg;
            else if (setupIcon != null) { try { tbIcon.Image = setupIcon.ToBitmap(); } catch {} }
            MakeDraggable(tbIcon);

            Label lblTitle = new Label { Text = "LiteNex Client v6.1 Ultimate Setup Installer", Location = new Point(46, 0), Size = new Size(350, 42), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = SetupTheme.C_TEXT, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent };
            MakeDraggable(lblTitle);

            Button btnMin   = MakeTitleBtn("─", SetupTheme.C_MUTED, SetupTheme.C_CARD2);
            Button btnClose = MakeTitleBtn("✕", SetupTheme.C_MUTED, Color.FromArgb(225, 29, 72));
            btnMin.Location   = new Point(titleBar.Width - 92, 0);
            btnClose.Location = new Point(titleBar.Width - 46, 0);
            btnMin.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            btnMin.Click   += (s, e) => { SetupSoundSystem.PlayClick(); this.WindowState = FormWindowState.Minimized; };
            btnClose.Click += (s, e) => { SetupSoundSystem.PlayClick(); Application.Exit(); };

            titleBar.Controls.Add(tbIcon);
            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(btnMin);
            titleBar.Controls.Add(btnClose);
            this.Controls.Add(titleBar);

            // ── HEADER BANNER & STEP CHIPS ─────────────────────────────────────
            pnlHeader = new Panel { Location = new Point(0, 42), Size = new Size(740, 88), BackColor = SetupTheme.C_CARD };
            pnlHeader.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                using (LinearGradientBrush lg = new LinearGradientBrush(new Rectangle(0, 87, pnlHeader.Width, 1), SetupTheme.C_BORDER, SetupTheme.C_PURPLE, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(lg, 0, 87, pnlHeader.Width, 1);

                // Draw Glowing Step Chips
                string[] stepTitles = { "1. Şartlar", "2. Ayarlar", "3. Kurulum", "4. Son" };
                int chipWidth = 84;
                int gap = 8;
                int startX = pnlHeader.Width - ((4 * chipWidth) + (3 * gap)) - 16;
                int chipY = 30;

                for (int i = 0; i < stepTitles.Length; i++)
                {
                    int x = startX + (i * (chipWidth + gap));
                    bool isActive = (currentStep == (i + 1));
                    bool isDone   = (currentStep > (i + 1));

                    Color bgCol   = isActive ? Color.FromArgb(40, 139, 92, 246) : isDone ? Color.FromArgb(30, 16, 185, 129) : Color.FromArgb(20, 26, 23, 46);
                    Color borderC = isActive ? SetupTheme.C_PURPLE_L : isDone ? SetupTheme.C_EMERALD : SetupTheme.C_BORDER;
                    Color textC   = isActive ? Color.White : isDone ? SetupTheme.C_EMERALD : SetupTheme.C_MUTED;

                    Rectangle chipRect = new Rectangle(x, chipY, chipWidth, 28);
                    using (GraphicsPath path = GetRoundedPath(chipRect, 6))
                    {
                        using (SolidBrush sb = new SolidBrush(bgCol)) e.Graphics.FillPath(sb, path);
                        using (Pen p = new Pen(borderC, isActive ? 1.5f : 1f)) e.Graphics.DrawPath(p, path);
                    }

                    using (Font chipFont = new Font("Segoe UI", 8.5F, isActive ? FontStyle.Bold : FontStyle.Regular))
                    using (SolidBrush tb = new SolidBrush(textC))
                    {
                        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString(stepTitles[i], chipFont, tb, chipRect, sf);
                    }
                }
            };

            pbLogo = new PictureBox { Location = new Point(20, 18), Size = new Size(52, 52), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Transparent };
            if (setupLogoImg != null) pbLogo.Image = setupLogoImg;

            lblHeaderTitle = new Label { Text = "LiteNex Client Kurulumu", Location = new Point(84, 18), Size = new Size(265, 24), Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent };
            lblHeaderSub   = new Label { Text = "Devam etmek için kullanım şartlarını onaylayın.", Location = new Point(84, 44), Size = new Size(265, 36), Font = new Font("Segoe UI", 8.5F), ForeColor = SetupTheme.C_MUTED, BackColor = Color.Transparent };

            pnlHeader.Controls.Add(pbLogo);
            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(lblHeaderSub);
            this.Controls.Add(pnlHeader);

            // ── FOOTER NAVIGATION ──────────────────────────────────────────────
            pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = SetupTheme.C_CARD };
            pnlFooter.Paint += (s, e) =>
            {
                using (Pen p = new Pen(SetupTheme.C_BORDER))
                    e.Graphics.DrawLine(p, 0, 0, pnlFooter.Width, 0);
            };

            btnCancel = new Button { Text = "İptal", Location = new Point(20, 14), Size = new Size(100, 36), FlatStyle = FlatStyle.Flat, BackColor = SetupTheme.C_CARD2, ForeColor = SetupTheme.C_MUTED, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { SetupSoundSystem.PlayClick(); Application.Exit(); };
            btnCancel.MouseEnter += (s, e) => SetupSoundSystem.PlayHover();

            btnBack = new Button { Text = "◀ Geri", Location = new Point(500, 14), Size = new Size(100, 36), FlatStyle = FlatStyle.Flat, BackColor = SetupTheme.C_CARD2, ForeColor = SetupTheme.C_TEXT, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand, Visible = false };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => { SetupSoundSystem.PlayClick(); SwitchStep(currentStep - 1); };
            btnBack.MouseEnter += (s, e) => SetupSoundSystem.PlayHover();

            btnNext = new Button { Text = "İleri ▶", Location = new Point(610, 14), Size = new Size(110, 36), FlatStyle = FlatStyle.Flat, BackColor = SetupTheme.C_PURPLE, ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand, Enabled = false };
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.Click += (s, e) => { SetupSoundSystem.PlayClick(); OnNextClicked(); };
            btnNext.MouseEnter += (s, e) => SetupSoundSystem.PlayHover();

            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Controls.Add(btnBack);
            pnlFooter.Controls.Add(btnNext);
            this.Controls.Add(pnlFooter);

            // ══════════════════════════════════════════════════════════════════
            //  STEP 1: TERMS OF SERVICE & LICENSE AGREEMENT
            // ══════════════════════════════════════════════════════════════════
            pnlStep1 = new Panel { Location = new Point(0, 130), Size = new Size(740, 346), BackColor = Color.Transparent };

            Label lblTermsH = new Label { Text = "📜 LiteNex Hizmet Şartları ve Kullanım Koşulları", Location = new Point(20, 8), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = SetupTheme.C_CYAN };

            Panel rtbBorder = new Panel { Location = new Point(20, 34), Size = new Size(700, 250), BackColor = SetupTheme.C_BORDER, Padding = new Padding(1) };
            rtbTerms = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = SetupTheme.C_CONSOLE,
                ForeColor = Color.FromArgb(220, 225, 235),
                Font = new Font("Segoe UI", 9F),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            rtbTerms.Text = GetTermsOfServiceText();
            rtbBorder.Controls.Add(rtbTerms);

            chkAcceptTerms = new CheckBox
            {
                Text = "LiteNex Client Hizmet Şartlarını ve Kullanım Koşullarını okudum, kabul ediyorum.",
                Location = new Point(20, 298),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = SetupTheme.C_TEXT,
                Cursor = Cursors.Hand
            };
            chkAcceptTerms.CheckedChanged += (s, e) =>
            {
                SetupSoundSystem.PlayClick();
                btnNext.Enabled = chkAcceptTerms.Checked;
                btnNext.BackColor = chkAcceptTerms.Checked ? SetupTheme.C_PURPLE : Color.FromArgb(60, 50, 90);
            };

            pnlStep1.Controls.Add(lblTermsH);
            pnlStep1.Controls.Add(rtbBorder);
            pnlStep1.Controls.Add(chkAcceptTerms);
            this.Controls.Add(pnlStep1);

            // ══════════════════════════════════════════════════════════════════
            //  STEP 2: INSTALLATION CONFIGURATION & DISK SPACE CHECK
            // ══════════════════════════════════════════════════════════════════
            pnlStep2 = new Panel { Location = new Point(0, 130), Size = new Size(740, 346), BackColor = Color.Transparent, Visible = false };

            Label lblDirH = new Label { Text = "📁 Kurulum Hedef Klasörü", Location = new Point(20, 16), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = SetupTheme.C_CYAN };
            txtInstallDir = new TextBox { Location = new Point(20, 42), Size = new Size(580, 32), Text = defaultPath, BackColor = SetupTheme.C_CARD2, ForeColor = SetupTheme.C_TEXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F) };

            btnBrowseDir = new Button { Text = "Gözat...", Location = new Point(610, 40), Size = new Size(110, 34), FlatStyle = FlatStyle.Flat, BackColor = SetupTheme.C_CARD2, ForeColor = SetupTheme.C_PURPLE_L, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnBrowseDir.FlatAppearance.BorderSize = 0;
            btnBrowseDir.Click += (s, e) =>
            {
                SetupSoundSystem.PlayClick();
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.Description = "LiteNex Client'ın kurulacağı klasörü seçin:";
                    fbd.SelectedPath = txtInstallDir.Text;
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        txtInstallDir.Text = fbd.SelectedPath;
                    }
                }
            };
            btnBrowseDir.MouseEnter += (s, e) => SetupSoundSystem.PlayHover();

            lblDiskSpace = new Label { Text = "💾 Sürücü Boş Alanı Hesaplanıyor...", Location = new Point(20, 78), AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = SetupTheme.C_EMERALD, BackColor = Color.Transparent };
            txtInstallDir.TextChanged += (s, e) => UpdateDiskSpaceInfo(txtInstallDir.Text);

            Label lblOptH = new Label { Text = "⚙️ Kurulum ve Kısayol Seçenekleri", Location = new Point(20, 115), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = SetupTheme.C_CYAN };

            chkDesktopShortcut = new CheckBox { Text = "Masaüstüne Kısayol Simgesi Oluştur (LiteNex Launcher.lnk)", Location = new Point(20, 145), AutoSize = true, Checked = true, ForeColor = SetupTheme.C_TEXT, Font = new Font("Segoe UI", 9.5F), Cursor = Cursors.Hand };
            chkStartMenuShortcut = new CheckBox { Text = "Başlat Menüsü Programlarına LiteNex Kısayolu Ekle", Location = new Point(20, 178), AutoSize = true, Checked = true, ForeColor = SetupTheme.C_TEXT, Font = new Font("Segoe UI", 9.5F), Cursor = Cursors.Hand };
            chkRegisterUninstall = new CheckBox { Text = "Windows Denetim Masası Program Ekle/Kaldır Listesine Kaydet", Location = new Point(20, 211), AutoSize = true, Checked = true, ForeColor = SetupTheme.C_PURPLE_L, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };

            Panel infoBox = new Panel { Location = new Point(20, 252), Size = new Size(700, 80), BackColor = SetupTheme.C_CARD };
            infoBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, infoBox.Width - 1, infoBox.Height - 1), 8))
                {
                    using (SolidBrush sb = new SolidBrush(SetupTheme.C_CARD)) e.Graphics.FillPath(sb, path);
                    using (Pen p = new Pen(SetupTheme.C_BORDER)) e.Graphics.DrawPath(p, path);
                }
            };

            Label lblInfoText = new Label
            {
                Text = "⚡ LiteNex Client yüksek performans motoru ve Java kütüphaneleri otomatik çıkartılacaktır.\n" +
                       "   Kurulum doğrudan tek dosya içerisinden saniyeler içinde tamamlanacaktır.",
                Location = new Point(14, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = SetupTheme.C_MUTED,
                BackColor = Color.Transparent
            };
            infoBox.Controls.Add(lblInfoText);

            pnlStep2.Controls.Add(lblDirH); pnlStep2.Controls.Add(txtInstallDir); pnlStep2.Controls.Add(btnBrowseDir);
            pnlStep2.Controls.Add(lblDiskSpace);
            pnlStep2.Controls.Add(lblOptH); pnlStep2.Controls.Add(chkDesktopShortcut); pnlStep2.Controls.Add(chkStartMenuShortcut); pnlStep2.Controls.Add(chkRegisterUninstall);
            pnlStep2.Controls.Add(infoBox);
            this.Controls.Add(pnlStep2);

            UpdateDiskSpaceInfo(defaultPath);

            // ══════════════════════════════════════════════════════════════════
            //  STEP 3: INSTALLING PROGRESS (SHINE ANIMATION)
            // ══════════════════════════════════════════════════════════════════
            pnlStep3 = new Panel { Location = new Point(0, 130), Size = new Size(740, 346), BackColor = Color.Transparent, Visible = false };

            Label lblInstallTitle = new Label { Text = "⚡ LiteNex Client Bilgisayarınıza Kuruluyor...", Location = new Point(20, 35), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.White };
            lblStatus = new Label { Text = "Hazırlanıyor...", Location = new Point(20, 70), Size = new Size(700, 24), Font = new Font("Segoe UI", 9F), ForeColor = SetupTheme.C_MUTED };

            progressBg   = new Panel { Location = new Point(20, 105), Size = new Size(700, 18), BackColor = SetupTheme.C_CARD2 };
            progressFill = new Panel { Location = new Point(0, 0), Size = new Size(0, 18), BackColor = SetupTheme.C_PURPLE };
            progressBg.Controls.Add(progressFill);

            RichTextBox rtbLog = new RichTextBox { Location = new Point(20, 142), Size = new Size(700, 185), BackColor = SetupTheme.C_CONSOLE, ForeColor = Color.FromArgb(160, 220, 160), Font = new Font("Consolas", 8.5F), ReadOnly = true, BorderStyle = BorderStyle.None };

            pnlStep3.Controls.Add(lblInstallTitle); pnlStep3.Controls.Add(lblStatus);
            pnlStep3.Controls.Add(progressBg); pnlStep3.Controls.Add(rtbLog);
            this.Controls.Add(pnlStep3);

            // ══════════════════════════════════════════════════════════════════
            //  STEP 4: SETUP FINISHED
            // ══════════════════════════════════════════════════════════════════
            pnlStep4 = new Panel { Location = new Point(0, 130), Size = new Size(740, 346), BackColor = Color.Transparent, Visible = false };

            PictureBox pbSuccess = new PictureBox { Location = new Point(330, 20), Size = new Size(80, 80), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Transparent };
            if (setupLogoImg != null) pbSuccess.Image = setupLogoImg;

            Label lblFinishTitle = new Label { Text = "🎉 Kurulum Başarıyla Tamamlandı!", Location = new Point(20, 115), Size = new Size(700, 32), Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = SetupTheme.C_EMERALD, TextAlign = ContentAlignment.MiddleCenter };
            Label lblFinishSub   = new Label { Text = "LiteNex Client v6.1 Ultimate Edition oynamaya hazır.", Location = new Point(20, 155), Size = new Size(700, 24), Font = new Font("Segoe UI", 10F), ForeColor = SetupTheme.C_TEXT, TextAlign = ContentAlignment.MiddleCenter };

            chkLaunchAfterSetup = new CheckBox { Text = "🚀 LiteNex Client'ı Şimdi Çalıştır ve Oyuna Başla", Location = new Point(200, 210), AutoSize = true, Checked = true, ForeColor = SetupTheme.C_CYAN, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };

            pnlStep4.Controls.Add(pbSuccess);
            pnlStep4.Controls.Add(lblFinishTitle);
            pnlStep4.Controls.Add(lblFinishSub);
            pnlStep4.Controls.Add(chkLaunchAfterSetup);
            this.Controls.Add(pnlStep4);
        }

        private void UpdateDiskSpaceInfo(string path)
        {
            try
            {
                string root = Path.GetPathRoot(Path.GetFullPath(path));
                DriveInfo drive = new DriveInfo(root);
                long freeGb = drive.AvailableFreeSpace / 1024 / 1024 / 1024;
                if (freeGb >= 1)
                {
                    lblDiskSpace.Text = string.Format("💾 Sürücü ({0}) Boş Alan: {1} GB  (Gerekli: ~150 MB) ✓", root, freeGb);
                    lblDiskSpace.ForeColor = SetupTheme.C_EMERALD;
                }
                else
                {
                    long freeMb = drive.AvailableFreeSpace / 1024 / 1024;
                    lblDiskSpace.Text = string.Format("💾 Sürücü ({0}) Boş Alan: {1} MB", root, freeMb);
                    lblDiskSpace.ForeColor = freeMb > 150 ? SetupTheme.C_EMERALD : Color.Red;
                }
            }
            catch
            {
                lblDiskSpace.Text = "💾 Sürücü Boş Alanı: Kontrol Edilemedi";
                lblDiskSpace.ForeColor = SetupTheme.C_MUTED;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  STEP SWITCHING LOGIC
        // ══════════════════════════════════════════════════════════════════════
        private void SwitchStep(int step)
        {
            currentStep = step;
            pnlStep1.Visible = (step == 1);
            pnlStep2.Visible = (step == 2);
            pnlStep3.Visible = (step == 3);
            pnlStep4.Visible = (step == 4);

            btnBack.Visible = (step == 2);
            pnlHeader.Invalidate();

            if (step == 1)
            {
                lblHeaderTitle.Text = "LiteNex Client Kurulumu";
                lblHeaderSub.Text   = "Devam etmek için şartları onaylayın.";
                btnNext.Text = "İleri ▶";
                btnNext.Enabled = chkAcceptTerms.Checked;
                btnNext.BackColor = chkAcceptTerms.Checked ? SetupTheme.C_PURPLE : Color.FromArgb(60, 50, 90);
                btnCancel.Enabled = true;
            }
            else if (step == 2)
            {
                lblHeaderTitle.Text = "Kurulum Seçenekleri";
                lblHeaderSub.Text   = "Konum ve kısayol ayarlarını seçin.";
                btnNext.Text = "Kurulumu Başlat ⚡";
                btnNext.Enabled = true;
                btnNext.BackColor = SetupTheme.C_EMERALD;
                btnCancel.Enabled = true;
            }
            else if (step == 3)
            {
                lblHeaderTitle.Text = "Yükleniyor...";
                lblHeaderSub.Text   = "Dosyalar çıkartılıyor, lütfen bekleyin.";
                btnNext.Enabled = false;
                btnBack.Enabled = false;
                btnCancel.Enabled = false;
                StartInstallationProcess();
            }
            else if (step == 4)
            {
                lblHeaderTitle.Text = "Tebrikler!";
                lblHeaderSub.Text   = "LiteNex Client başarıyla kuruldu.";
                btnNext.Text = "Bitir & Oyuna Başla";
                btnNext.Enabled = true;
                btnNext.BackColor = SetupTheme.C_PURPLE;
                btnBack.Visible = false;
                btnCancel.Visible = false;
                SetupSoundSystem.PlaySuccess();
            }
        }

        private void OnNextClicked()
        {
            if (currentStep == 1)
            {
                SwitchStep(2);
            }
            else if (currentStep == 2)
            {
                targetDirectory = txtInstallDir.Text.Trim();
                if (string.IsNullOrEmpty(targetDirectory))
                {
                    MessageBox.Show("Lütfen geçerli bir kurulum klasörü belirtin!", "LiteNex Setup");
                    return;
                }
                SwitchStep(3);
            }
            else if (currentStep == 4)
            {
                if (chkLaunchAfterSetup.Checked)
                {
                    string exePath = Path.Combine(targetDirectory, "LiteNex.exe");
                    if (File.Exists(exePath))
                    {
                        try { System.Diagnostics.Process.Start(exePath); } catch {}
                    }
                }
                Application.Exit();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  INSTALLATION THREAD WORKER
        // ══════════════════════════════════════════════════════════════════════
        private void StartInstallationProcess()
        {
            ThreadPool.QueueUserWorkItem((_) =>
            {
                try
                {
                    UpdateProgress(10, "Kurulum klasörü hazırlanıyor...");
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }
                    Thread.Sleep(300);

                    UpdateProgress(35, "LiteNex.exe motoru çıkartılıyor...");
                    string exePath = Path.Combine(targetDirectory, "LiteNex.exe");
                    byte[] exeBytes = GetEmbeddedResourceBytes("LiteNex.exe");
                    if (exeBytes != null && exeBytes.Length > 0)
                    {
                        File.WriteAllBytes(exePath, exeBytes);
                    }
                    else
                    {
                        string localExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LiteNex.exe");
                        if (File.Exists(localExe)) File.Copy(localExe, exePath, true);
                    }
                    Thread.Sleep(300);

                    UpdateProgress(60, "Simgeler ve Uninstaller kopyalanıyor...");
                    string icoPath = Path.Combine(targetDirectory, "logo.ico");
                    byte[] icoBytes = GetEmbeddedResourceBytes("logo.ico");
                    if (icoBytes != null && icoBytes.Length > 0)
                    {
                        File.WriteAllBytes(icoPath, icoBytes);
                    }
                    else
                    {
                        string localIco = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.ico");
                        if (File.Exists(localIco)) File.Copy(localIco, icoPath, true);
                    }

                    string pngPath = Path.Combine(targetDirectory, "logo.png");
                    byte[] pngBytes = GetEmbeddedResourceBytes("logo.png");
                    if (pngBytes != null && pngBytes.Length > 0)
                    {
                        File.WriteAllBytes(pngPath, pngBytes);
                    }
                    else
                    {
                        string localPng = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
                        if (File.Exists(localPng)) File.Copy(localPng, pngPath, true);
                    }

                    string uninstallerPath = Path.Combine(targetDirectory, "Uninstaller.exe");
                    try
                    {
                        File.Copy(Application.ExecutablePath, uninstallerPath, true);
                    }
                    catch { }
                    Thread.Sleep(300);

                    UpdateProgress(80, "Kısayollar ve Windows entegrasyonu oluşturuluyor...");
                    if (chkDesktopShortcut.Checked)
                    {
                        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        CreateShortcut(Path.Combine(desktopPath, "LiteNex Launcher.lnk"), exePath, targetDirectory, icoPath);
                    }

                    if (chkStartMenuShortcut.Checked)
                    {
                        string startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "LiteNex Client");
                        if (!Directory.Exists(startMenuPath)) Directory.CreateDirectory(startMenuPath);
                        CreateShortcut(Path.Combine(startMenuPath, "LiteNex Launcher.lnk"), exePath, targetDirectory, icoPath);
                    }

                    if (chkRegisterUninstall.Checked)
                    {
                        RegisterInWindowsUninstall(targetDirectory, exePath, icoPath, uninstallerPath);
                    }
                    Thread.Sleep(300);

                    UpdateProgress(100, "Kurulum tamamlandı!");
                    Thread.Sleep(400);

                    this.Invoke(new Action(() => SwitchStep(4)));
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("Kurulum sırasında bir hata oluştu:\n" + ex.Message, "LiteNex Setup Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        SwitchStep(2);
                    }));
                }
            });
        }

        private void UpdateProgress(int pct, string status)
        {
            if (this.IsDisposed) return;
            this.Invoke(new Action(() =>
            {
                lblStatus.Text = status;
                progressFill.Width = (int)(progressBg.Width * (pct / 100.0));
            }));
        }

        private void CreateShortcut(string shortcutPath, string targetExe, string workDir, string iconPath)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                dynamic shell = Activator.CreateInstance(shellType);
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetExe;
                shortcut.WorkingDirectory = workDir;
                shortcut.Description = "LiteNex Client Ultimate Launcher";
                if (File.Exists(iconPath)) shortcut.IconLocation = iconPath;
                shortcut.Save();
            }
            catch { }
        }

        private void RegisterInWindowsUninstall(string installDir, string exePath, string iconPath, string uninstallerPath)
        {
            try
            {
                string regPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\LiteNexClient";
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(regPath))
                {
                    if (key != null)
                    {
                        key.SetValue("DisplayName", "LiteNex Client v6.4 Ultimate Edition");
                        key.SetValue("DisplayVersion", "6.4.0");
                        key.SetValue("Publisher", "LiteNex Studios");
                        key.SetValue("DisplayIcon", iconPath);
                        key.SetValue("InstallLocation", installDir);
                        key.SetValue("UninstallString", "\"" + uninstallerPath + "\" /uninstall");
                        key.SetValue("NoModify", 1);
                        key.SetValue("NoRepair", 1);
                    }
                }
            }
            catch { }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  RESOURCE & HELPER UTILITIES
        // ══════════════════════════════════════════════════════════════════════
        private static byte[] GetEmbeddedResourceBytes(string name)
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                foreach (string resourceName in asm.GetManifestResourceNames())
                {
                    if (resourceName.Equals(name, StringComparison.OrdinalIgnoreCase) || resourceName.EndsWith("." + name, StringComparison.OrdinalIgnoreCase))
                    {
                        using (Stream stream = asm.GetManifestResourceStream(resourceName))
                        {
                            if (stream == null) return null;
                            byte[] buffer = new byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);
                            return buffer;
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private static Image LoadEmbeddedImage(string name)
        {
            byte[] bytes = GetEmbeddedResourceBytes(name);
            if (bytes != null && bytes.Length > 0)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        using (Image img = Image.FromStream(ms))
                            return new Bitmap(img);
                    }
                }
                catch { }
            }
            string localFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
            if (File.Exists(localFile))
            {
                try { return new Bitmap(localFile); } catch {}
            }
            return null;
        }

        private static Icon LoadEmbeddedIcon(string name)
        {
            byte[] bytes = GetEmbeddedResourceBytes(name);
            if (bytes != null && bytes.Length > 0)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream(bytes))
                        return new Icon(ms);
                }
                catch { }
            }
            string localFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
            if (File.Exists(localFile))
            {
                try { return new Icon(localFile); } catch {}
            }
            return null;
        }

        private static string GetTermsOfServiceText()
        {
            return
                "LITENEX CLIENT HİZMET ŞARTLARI VE KULLANIM KOŞULLARI\n" +
                "════════════════════════════════════════════════════════════════════\n\n" +
                "1. HİZMETİN KAPSAMI VEYA KULLANIMI\n" +
                "LiteNex Client, Minecraft oyuncuları için gelişmiş performans optimizasyonu, " +
                "otomatik kütüphane ve mod yönetimi sağlayan ücretsiz bir başlatıcı (launcher) yazılımıdır.\n\n" +
                "2. ADİL OYUN VE TOPLULUK KURALLARI\n" +
                "Kullanıcılar, katıldıkları çok oyunculu (multiplayer) sunucuların kurallarına uymakla " +
                "yükümlüdür. LiteNex Client sunucularda haksız avantaj sağlayacak zararlı yazılımların " +
                "kullanımını teşvik etmez ve sorumluluk kabul etmez.\n\n" +
                "3. GİZLİLİK VE VERİ GÜVENLİĞİ\n" +
                "LiteNex Client kişisel verilerinizi kesinlikle 3. taraf sunuculara aktarmaz. " +
                "Tüm oyuncu profilleri, tema tercihleri ve oyun kayıtları tamamen sizin bilgisayarınızda " +
                "(%AppData%\\.litenex) yerel olarak saklanır.\n\n" +
                "4. SORUMLULUK REDDİ (DISCLAIMER)\n" +
                "LiteNex Client, Mojang Studios veya Microsoft Corporation ile resmi olarak bağlantılı " +
                "değildir. Minecraft, Mojang Synergies AB firmasının tescilli ticari markasıdır.\n\n" +
                "5. KABUL VE ONAY\n" +
                "Bu kurulumu tamamlayarak yukarıda belirtilen şartları ve kullanım ilkelerini " +
                "kabul etmiş sayılırsınız.";
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
            Button b = new Button { Text = text, Size = new Size(46, 41), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = fore, Font = new Font("Segoe UI", 10F), Cursor = Cursors.Hand, Margin = Padding.Empty, Padding = Padding.Empty };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = hoverBg;
            b.MouseEnter += (s, e) => { SetupSoundSystem.PlayHover(); b.ForeColor = Color.White; };
            b.MouseLeave += (s, e) => b.ForeColor = fore;
            return b;
        }

        private void MakeDraggable(Control c)
        {
            c.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(this.Handle, 0xA1, 0x2, 0); } };
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  UNINSTALLER FORM (Custom Windows Add/Remove Program Wizard)
    // ══════════════════════════════════════════════════════════════════════════
    public class UninstallForm : Form
    {
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private CheckBox chkCleanData;
        private Button btnUninstall, btnCancel;
        private Panel progressBg, progressFill;
        private Label lblStatus;

        public UninstallForm()
        {
            this.DoubleBuffered = true;
            InitializeUninstallUI();
        }

        private void InitializeUninstallUI()
        {
            this.Text            = "LiteNex Client Kaldırma Sihirbazı";
            this.Size            = new Size(580, 360);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.BackColor       = SetupTheme.C_BG;
            this.ForeColor       = SetupTheme.C_TEXT;
            this.Font            = new Font("Segoe UI", 9.5F);
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost         = true;

            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string icoPath = Path.Combine(appDir, "logo.ico");
            if (File.Exists(icoPath)) { try { this.Icon = new Icon(icoPath); } catch {} }

            // ── TITLE BAR ──────────────────────────────────────────────────────
            Panel titleBar = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = SetupTheme.C_TITLEBAR };
            MakeDraggable(titleBar);

            titleBar.Paint += (s, e) =>
            {
                using (Pen p = new Pen(Color.FromArgb(225, 29, 72)))
                    e.Graphics.DrawLine(p, 0, 41, titleBar.Width, 41);
            };

            Label lblTitle = new Label { Text = "🗑️  LiteNex Client Kaldırma Sihirbazı", Location = new Point(14, 0), Size = new Size(400, 42), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent };
            MakeDraggable(lblTitle);

            Button btnClose = new Button { Text = "✕", Size = new Size(46, 41), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = SetupTheme.C_MUTED, Font = new Font("Segoe UI", 10F), Cursor = Cursors.Hand, Location = new Point(titleBar.Width - 46, 0), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(225, 29, 72);
            btnClose.Click += (s, e) => Application.Exit();

            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(btnClose);
            this.Controls.Add(titleBar);

            // ── MAIN CONTENT CARD ──────────────────────────────────────────────
            Panel card = new Panel { Location = new Point(20, 62), Size = new Size(540, 275), BackColor = SetupTheme.C_CARD };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 8))
                {
                    using (SolidBrush sb = new SolidBrush(SetupTheme.C_CARD)) e.Graphics.FillPath(sb, path);
                    using (Pen p = new Pen(SetupTheme.C_BORDER)) e.Graphics.DrawPath(p, path);
                }
            };

            Label lblWarn = new Label
            {
                Text = "LiteNex Client bilgisayarınızdan kaldırılacaktır.\n" +
                       "Masaüstü kısayolları ve bileşenleri temizlenecektir.",
                Location = new Point(20, 20),
                Size = new Size(500, 44),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            chkCleanData = new CheckBox
            {
                Text = "Oyuncu profillerini ve yerel oyun kayıtlarını (.litenex) tamamen sil",
                Location = new Point(20, 75),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Orange,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            lblStatus = new Label { Text = "Kaldırmaya hazır.", Location = new Point(20, 115), Size = new Size(500, 20), Font = new Font("Segoe UI", 8.5F), ForeColor = SetupTheme.C_MUTED, BackColor = Color.Transparent };

            progressBg   = new Panel { Location = new Point(20, 140), Size = new Size(500, 12), BackColor = SetupTheme.C_CARD2 };
            progressFill = new Panel { Location = new Point(0, 0), Size = new Size(0, 12), BackColor = Color.FromArgb(225, 29, 72) };
            progressBg.Controls.Add(progressFill);

            btnUninstall = new Button
            {
                Text = "🗑️   Uygulamayı Kaldır",
                Location = new Point(20, 195),
                Size = new Size(240, 46),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(225, 29, 72),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUninstall.FlatAppearance.BorderSize = 0;
            btnUninstall.Click += (s, e) => PerformUninstall();

            btnCancel = new Button
            {
                Text = "İptal",
                Location = new Point(280, 195),
                Size = new Size(240, 46),
                FlatStyle = FlatStyle.Flat,
                BackColor = SetupTheme.C_CARD2,
                ForeColor = SetupTheme.C_TEXT,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => Application.Exit();

            card.Controls.Add(lblWarn);
            card.Controls.Add(chkCleanData);
            card.Controls.Add(lblStatus);
            card.Controls.Add(progressBg);
            card.Controls.Add(btnUninstall);
            card.Controls.Add(btnCancel);
            this.Controls.Add(card);
        }

        private void PerformUninstall()
        {
            btnUninstall.Enabled = false;
            btnCancel.Enabled = false;
            SetupSoundSystem.PlayClick();

            ThreadPool.QueueUserWorkItem((_) =>
            {
                try
                {
                    UpdateProgress(20, "Kısayollar temizleniyor...");
                    string desktopLink = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "LiteNex Launcher.lnk");
                    if (File.Exists(desktopLink)) { try { File.Delete(desktopLink); } catch {} }

                    string startMenuFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "LiteNex Client");
                    if (Directory.Exists(startMenuFolder)) { try { Directory.Delete(startMenuFolder, true); } catch {} }
                    Thread.Sleep(300);

                    UpdateProgress(50, "Windows Kayıt Defteri temizleniyor...");
                    try
                    {
                        Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\LiteNexClient", false);
                    }
                    catch { }
                    Thread.Sleep(300);

                    if (chkCleanData.Checked)
                    {
                        UpdateProgress(75, "Oyuncu verileri (.litenex) siliniyor...");
                        string gameDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".litenex");
                        if (Directory.Exists(gameDataDir)) { try { Directory.Delete(gameDataDir, true); } catch {} }
                    }

                    UpdateProgress(90, "Klasör ve dosyalar temizleniyor...");
                    string targetDir = AppDomain.CurrentDomain.BaseDirectory;
                    Thread.Sleep(300);

                    UpdateProgress(100, "LiteNex Client kaldırıldı!");
                    SetupSoundSystem.PlaySuccess();

                    // Self delete directory via temp cmd script
                    string cleanBat = Path.Combine(Path.GetTempPath(), "litenex_uninstaller_cleanup.bat");
                    string batContent =
                        "@echo off\r\n" +
                        "timeout /t 1 /nobreak > nul\r\n" +
                        "rmdir /s /q \"" + targetDir.TrimEnd('\\') + "\"\r\n" +
                        "del /f /q \"%~f0\"\r\n";
                    File.WriteAllText(cleanBat, batContent);

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c \"" + cleanBat + "\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(psi);

                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("LiteNex Client bilgisayarınızdan başarıyla kaldırıldı.", "LiteNex Client Kaldırma");
                        Application.Exit();
                    }));
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("Kaldırma hatası: " + ex.Message, "Kaldırma Hatası");
                        Application.Exit();
                    }));
                }
            });
        }

        private void UpdateProgress(int pct, string status)
        {
            if (this.IsDisposed) return;
            this.Invoke(new Action(() =>
            {
                lblStatus.Text = status;
                progressFill.Width = (int)(progressBg.Width * (pct / 100.0));
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

        private void MakeDraggable(Control c)
        {
            c.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(this.Handle, 0xA1, 0x2, 0); } };
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  MAIN ENTRY POINT
    // ══════════════════════════════════════════════════════════════════════════
    public static class Program
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool IsIconic(IntPtr hWnd);

        [STAThread]
        static void Main(string[] args)
        {
            // ── Tek Örnek Koruması (Single Instance Mutex) ──────────────────
            bool isUninstall = args != null && args.Length > 0 &&
                (args[0].Equals("/uninstall", StringComparison.OrdinalIgnoreCase) ||
                 args[0].Equals("-uninstall", StringComparison.OrdinalIgnoreCase));

            string mutexName = isUninstall ? "LiteNexUninstallMutex_v6" : "LiteNexSetupMutex_v6";

            bool createdNew;
            using (System.Threading.Mutex mutex = new System.Threading.Mutex(true, mutexName, out createdNew))
            {
                if (!createdNew)
                {
                    // Zaten çalışıyor — mevcut pencereyi öne getir
                    System.Diagnostics.Process current = System.Diagnostics.Process.GetCurrentProcess();
                    foreach (System.Diagnostics.Process proc in System.Diagnostics.Process.GetProcessesByName(current.ProcessName))
                    {
                        if (proc.Id != current.Id && proc.MainWindowHandle != IntPtr.Zero)
                        {
                            IntPtr hWnd = proc.MainWindowHandle;
                            if (IsIconic(hWnd)) ShowWindow(hWnd, 9); // SW_RESTORE
                            SetForegroundWindow(hWnd);
                            break;
                        }
                    }
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (isUninstall)
                {
                    Application.Run(new UninstallForm());
                }
                else
                {
                    Application.Run(new SetupForm());
                }
            }
        }
    }
}
