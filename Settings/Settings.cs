using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace ModernArchiveThumbnailSettings
{
    public partial class SettingsForm : Form
    {
        [DllImport("shell32.dll")]
        static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);
        
        private const int SHCNE_ASSOCCHANGED = 0x08000000;
        private const int SHCNF_IDLIST = 0x0000;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SettingsForm());
        }

        public SettingsForm()
        {
            this.Text = "Modern Archive Thumbnail Manager";
            this.Size = new Size(450, 260);
            this.Font = new Font("Segoe UI", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblInfo = new Label { 
                Text = "✅ Instant Stream Engine Active\n(고속 스트리밍 엔진이 정상 작동 중입니다)\n\nIf thumbnails don't appear, click the button below.\n(썸네일이 보이지 않으면 아래 버튼을 눌러주세요)", 
                Location = new Point(20, 20), 
                Size = new Size(400, 80), 
                TextAlign = ContentAlignment.MiddleCenter 
            };

            Button btnClean = new Button { 
                Text = "Clear Cache & Restart Explorer\n(캐시 초기화 및 탐색기 재시작)", 
                Location = new Point(30, 110), 
                Size = new Size(370, 60), 
                BackColor = Color.DodgerBlue, 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            
            btnClean.FlatAppearance.BorderSize = 0;
            btnClean.Click += (s, e) => {
                btnClean.Enabled = false;
                btnClean.Text = "Processing... (처리 중...)";
                Application.DoEvents();
                
                ClearAndRestart();
                
                btnClean.Enabled = true;
                btnClean.Text = "Clear Cache & Restart Explorer\n(캐시 초기화 및 탐색기 재시작)";
                MessageBox.Show("Complete. (완료되었습니다.)", "Complete");
            };

            this.Controls.Add(lblInfo);
            this.Controls.Add(btnClean);
        }

        private void ClearAndRestart()
        {
            try
            {
                string cachePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                    @"Microsoft\Windows\Explorer"
                );
                
                var psi = new ProcessStartInfo { 
                    FileName = "cmd.exe", 
                    Arguments = "/c taskkill /f /im explorer.exe & timeout /t 1 /nobreak > nul & del /f /q \"" + cachePath + "\\thumbcache_*.db\" & start explorer.exe", 
                    CreateNoWindow = true, 
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                
                Process.Start(psi);
                Thread.Sleep(2000);
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
            }
            catch { }
        }
    }
}