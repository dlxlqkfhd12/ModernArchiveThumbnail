using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace ModernArchiveSettings
{
    public class SettingsForm : Form
    {
        private GroupBox gbLanguage;
        private RadioButton rbKorean;
        private RadioButton rbEnglish;
        
        private GroupBox gbMode;
        private RadioButton rbModeCompatHigh;
        private RadioButton rbModeCompatFast;
        private RadioButton rbModeNative;
        
        private Button btnApply;
        private Label lblInfo;
        private bool isEnglish = false; 

        public SettingsForm()
        {
            this.Size = new Size(450, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            InitializeUI();
            LoadCurrentSetting();
        }

        private void InitializeUI()
        {
            gbLanguage = new GroupBox();
            gbLanguage.Location = new Point(20, 10);
            gbLanguage.Size = new Size(390, 60);

            rbKorean = new RadioButton();
            rbKorean.Text = "한국어 (Korean)";
            rbKorean.Location = new Point(20, 25);
            rbKorean.AutoSize = true;
            rbKorean.CheckedChanged += Language_Changed;

            rbEnglish = new RadioButton();
            rbEnglish.Text = "English";
            rbEnglish.Location = new Point(200, 25);
            rbEnglish.AutoSize = true;
            rbEnglish.CheckedChanged += Language_Changed;

            gbLanguage.Controls.Add(rbKorean);
            gbLanguage.Controls.Add(rbEnglish);

            gbMode = new GroupBox();
            gbMode.Location = new Point(20, 85);
            gbMode.Size = new Size(390, 240);

            rbModeCompatHigh = new RadioButton();
            rbModeCompatHigh.Location = new Point(20, 30);
            rbModeCompatHigh.AutoSize = true;
            
            rbModeCompatFast = new RadioButton();
            rbModeCompatFast.Location = new Point(20, 95);
            rbModeCompatFast.AutoSize = true;

            rbModeNative = new RadioButton();
            rbModeNative.Location = new Point(20, 160);
            rbModeNative.AutoSize = true;

            gbMode.Controls.Add(rbModeCompatHigh);
            gbMode.Controls.Add(rbModeCompatFast);
            gbMode.Controls.Add(rbModeNative);

            btnApply = new Button();
            btnApply.Location = new Point(20, 340);
            btnApply.Size = new Size(390, 60);
            btnApply.Click += BtnApply_Click;

            lblInfo = new Label();
            lblInfo.Location = new Point(20, 420);
            lblInfo.AutoSize = true;
            lblInfo.ForeColor = Color.Gray;

            this.Controls.Add(gbLanguage);
            this.Controls.Add(gbMode);
            this.Controls.Add(btnApply);
            this.Controls.Add(lblInfo);
        }

        private void Language_Changed(object sender, EventArgs e)
        {
            isEnglish = rbEnglish.Checked;
            UpdateUIText();
        }

        private void UpdateUIText()
        {
            if (isEnglish)
            {
                this.Text = "Modern Archive Thumbnail Settings";
                gbLanguage.Text = "Language";
                gbMode.Text = "Performance Engine";
                
                rbModeCompatHigh.Text = "1. Compatibility Mode 1 (High Quality)\n- Uses pure C# (Triangle).\n- High quality but slowest.";
                
                rbModeCompatFast.Text = "2. Compatibility Mode 2 (Optimized)\n- Uses pure C# (NearestNeighbor).\n- Faster than Mode 1, slight quality drop.";
                
                rbModeNative.Text = "3. High Speed Mode (Recommended)\n- Uses Windows Native Codec.\n- The FASTEST and most efficient.";
                
                btnApply.Text = "Apply & Restart Explorer";
                lblInfo.Text = "v1.0.1 - Made by dlxlqkfhd12";
            }
            else
            {
                this.Text = "Modern Archive Thumbnail 설정";
                gbLanguage.Text = "언어 설정 (Language)";
                gbMode.Text = "엔진 선택 (Performance)";
                
                rbModeCompatHigh.Text = "1. 호환성 모드 1 (정밀 / 고화질)\n- 부드러운 화질(Triangle)로 변환합니다.\n- 화질이 가장 좋지만 속도는 가장 느립니다.";
                
                rbModeCompatFast.Text = "2. 호환성 모드 2 (가속 / 밸런스)\n- 빠른 속도(NearestNeighbor)로 변환합니다.\n- 1번보다 빠르지만 화질이 약간 거칩니다.";
                
                rbModeNative.Text = "3. 고속 모드 (권장 / 저사양 PC 최적)\n- 윈도우 가속 엔진을 사용하여 가장 빠릅니다.\n- 저사양 PC에서도 끊김 없는 성능을 제공합니다.";
                
                btnApply.Text = "설정 적용 및 탐색기 재시작";
                lblInfo.Text = "v1.0.1 - 제작: dlxlqkfhd12";
            }
        }

        private void LoadCurrentSetting()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\ModernArchiveThumbnail"))
                {
                    if (key != null)
                    {
                        int mode = (int)key.GetValue("Mode", 2);
                        
                        if (mode == 0) rbModeCompatHigh.Checked = true;
                        else if (mode == 1) rbModeCompatFast.Checked = true;
                        else rbModeNative.Checked = true;

                        string lang = (string)key.GetValue("Language", "KO");
                        if (lang == "EN") rbEnglish.Checked = true;
                        else rbKorean.Checked = true;
                    }
                    else
                    {
                        rbModeNative.Checked = true;
                        rbKorean.Checked = true; 
                    }
                }
                UpdateUIText();
            }
            catch 
            {
                rbModeNative.Checked = true;
                rbKorean.Checked = true;
                UpdateUIText();
            }
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\ModernArchiveThumbnail"))
                {
                    int mode = 2; 
                    if (rbModeCompatHigh.Checked) mode = 0;
                    if (rbModeCompatFast.Checked) mode = 1;
                    if (rbModeNative.Checked) mode = 2;

                    key.SetValue("Mode", mode);
                    key.SetValue("Language", rbEnglish.Checked ? "EN" : "KO");
                }

                string msg = isEnglish 
                    ? "Settings saved! Restarting Explorer..." 
                    : "저장 완료! 탐색기를 재시작합니다...";
                
                MessageBox.Show(msg, isEnglish ? "Success" : "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);

                PerformSmartRestart();

                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void PerformSmartRestart()
        {
            try
            {
                ProcessStartInfo killInfo = new ProcessStartInfo("taskkill", "/F /IM explorer.exe");
                killInfo.CreateNoWindow = true;
                killInfo.UseShellExecute = false;
                Process.Start(killInfo).WaitForExit();

                int safetyGuard = 0;
                while (Process.GetProcessesByName("explorer").Length > 0 && safetyGuard < 50)
                {
                    System.Threading.Thread.Sleep(50);
                    safetyGuard++;
                }

                string explorerPath = Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), "explorer.exe");
                Process.Start(explorerPath);
            }
            catch
            {
                try
                {
                    ProcessStartInfo cmd = new ProcessStartInfo("cmd", "/c start explorer.exe");
                    cmd.CreateNoWindow = true;
                    cmd.UseShellExecute = false;
                    Process.Start(cmd);
                }
                catch { }
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new SettingsForm());
        }
    }
}