using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;

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

        private const string RegKeyPath = @"Software\ModernArchiveThumbnail";
        private TabControl tabControl;

        public SettingsForm()
        {
            this.Text = "Modern Archive Thumbnail Settings";
            this.Size = new Size(580, 500);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            InitCustomUI();
            LoadSettings();
        }

        private void InitCustomUI()
        {
            foreach (Control c in this.Controls) c.Visible = false;

            tabControl = new TabControl { Dock = DockStyle.Fill, Padding = new Point(10, 6) };

            TabPage tabGeneral = new TabPage("설정 (General)");
            tabGeneral.BackColor = Color.White;
            SetupGeneralTab(tabGeneral);
            tabControl.TabPages.Add(tabGeneral);

            TabPage tabTools = new TabPage("관리 (Tools)");
            tabTools.BackColor = Color.White;
            SetupToolsTab(tabTools);
            tabControl.TabPages.Add(tabTools);

            this.Controls.Add(tabControl);
        }

        private void SetupGeneralTab(TabPage tab)
        {
            GroupBox grpMode = new GroupBox { Text = "모드 선택 (Select Mode)", Location = new Point(20, 20), Size = new Size(520, 250) };
            
            RadioButton rb1 = new RadioButton { Name = "rbMode1", Text = "1. 호환성 모드 (최고 화질 - High Quality / Compatibility)", Location = new Point(30, 40), AutoSize = true };
            RadioButton rb2 = new RadioButton { Name = "rbMode2", Text = "2. 일반 모드 (보통 속도 - Normal Speed / Balanced)", Location = new Point(30, 75), AutoSize = true };
            RadioButton rb3 = new RadioButton { Name = "rbMode3", Text = "3. 고속 모드 (빠름 - High Speed / Fast Streaming)", Location = new Point(30, 110), AutoSize = true, ForeColor = Color.DarkBlue };
            RadioButton rb4 = new RadioButton { Name = "rbMode0", Text = "4. 썸네일 기능 끄기 (Disable Thumbnails & Clear Cache)", Location = new Point(30, 145), AutoSize = true, ForeColor = Color.DarkRed };
            
            Label lblInfo = new Label { Text = "※ 모드 3은 최적화된 디코더를 사용합니다 (메모리 재사용, 백업 디코더 포함).\n   Mode 3 uses optimized decoder with fallback support.", Location = new Point(30, 190), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 8F) };
            
            grpMode.Controls.Add(rb1); 
            grpMode.Controls.Add(rb2); 
            grpMode.Controls.Add(rb3); 
            grpMode.Controls.Add(rb4);
            grpMode.Controls.Add(lblInfo);

            Button btnApply = new Button { Name = "btnApply", Text = "설정 적용 (Apply Settings)", Location = new Point(20, 290), Size = new Size(520, 50), FlatStyle = FlatStyle.Flat, BackColor = Color.DodgerBlue, ForeColor = Color.White, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
            btnApply.Click += BtnApply_Click;

            Label lblStatus = new Label { Name = "lblStatus", Text = "현재 상태 (Status):", Location = new Point(25, 360), AutoSize = true, ForeColor = Color.Gray };

            tab.Controls.Add(grpMode);
            tab.Controls.Add(btnApply);
            tab.Controls.Add(lblStatus);
        }

        private void SetupToolsTab(TabPage tab)
        {
            GroupBox grpCache = new GroupBox { Text = "문제 해결 (Troubleshooting)", Location = new Point(20, 20), Size = new Size(520, 180) };
            
            Label lblCache = new Label { Text = "썸네일이 갱신되지 않거나, 기능을 껐는데도 이미지가 계속 보일 때 사용하세요.\n(Use this if thumbnails are not updating or still showing after disabling.)", Location = new Point(30, 40), Size = new Size(460, 50) };
            
            Button btnClean = new Button { Name = "btnClean", Text = "썸네일 캐시 초기화 (Clear Cache)", Location = new Point(30, 110), Size = new Size(460, 50), BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnClean.FlatAppearance.BorderSize = 0;
            btnClean.Click += BtnClean_Click;

            grpCache.Controls.Add(lblCache);
            grpCache.Controls.Add(btnClean);
            tab.Controls.Add(grpCache);
        }

        private void LoadSettings()
        {
            var key = Registry.CurrentUser.OpenSubKey(RegKeyPath);
            
            int mode = 2;
            int active = 1;

            if (key != null) {
                mode = (int)key.GetValue("ThumbnailMode", 2);
                active = (int)key.GetValue("HandlerRegistered", 1);
            }
            
            var grp = tabControl.TabPages[0].Controls[0] as GroupBox;
            
            if (active == 0) ((RadioButton)grp.Controls["rbMode0"]).Checked = true;
            else if(mode == 1) ((RadioButton)grp.Controls["rbMode1"]).Checked = true;
            else if(mode == 2) ((RadioButton)grp.Controls["rbMode2"]).Checked = true;
            else if(mode == 3) ((RadioButton)grp.Controls["rbMode3"]).Checked = true;
            else ((RadioButton)grp.Controls["rbMode2"]).Checked = true;
            
            UpdateStatusLabel(active == 1);
            if (key != null) key.Close();
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            var grp = tabControl.TabPages[0].Controls[0] as GroupBox;
            int modeToSave = 2;
            int activeToSave = 1;

            int prevActive = 1;
            int prevMode = 2;
            var keyRead = Registry.CurrentUser.OpenSubKey(RegKeyPath);
            if (keyRead != null)
            {
                prevActive = (int)keyRead.GetValue("HandlerRegistered", 1);
                prevMode = (int)keyRead.GetValue("ThumbnailMode", 2);
                keyRead.Close();
            }

            if (((RadioButton)grp.Controls["rbMode0"]).Checked)
            {
                activeToSave = 0;
            }
            else
            {
                if (((RadioButton)grp.Controls["rbMode1"]).Checked) modeToSave = 1;
                else if (((RadioButton)grp.Controls["rbMode2"]).Checked) modeToSave = 2;
                else if (((RadioButton)grp.Controls["rbMode3"]).Checked) modeToSave = 3;
            }

            var key = Registry.CurrentUser.CreateSubKey(RegKeyPath);
            key.SetValue("ThumbnailMode", modeToSave);
            key.SetValue("HandlerRegistered", activeToSave);
            key.Close();

            bool needRestart = false;
            bool needCacheClear = false;

            if (activeToSave == 0)
            {
                needRestart = true;
                needCacheClear = true;
            }
            else if (prevActive == 0 && activeToSave == 1)
            {
                needRestart = true;
                needCacheClear = true;
            }
            else if (activeToSave == 1 && prevMode != modeToSave)
            {
                needRestart = true;
                needCacheClear = false;
            }

            if (needRestart)
            {
                btn.Enabled = false;
                btn.Text = "적용 중... (Applying...)";
                btn.BackColor = Color.Gray;
                Application.DoEvents();

                FastRestartExplorer(needCacheClear);

                btn.Text = "설정 적용 (Apply Settings)";
                btn.BackColor = Color.DodgerBlue;
                btn.Enabled = true;
            }

            UpdateStatusLabel(activeToSave == 1);
            
            if (activeToSave == 0)
                MessageBox.Show("기능이 비활성화 되었으며, 썸네일 캐시가 삭제되었습니다.\nThumbnail Disabled & Cache Cleared.", "Complete");
            else if (prevActive == 0 && activeToSave == 1)
                MessageBox.Show("설정이 적용되었으며, 썸네일 캐시가 새로고침 되었습니다.\nSettings Applied & Cache Refreshed.", "Complete");
            else if (prevMode != modeToSave)
                MessageBox.Show("모드가 변경되었습니다. 탐색기가 새로고침 되었습니다.\nMode Changed & Explorer Refreshed.", "Complete");
            else
                MessageBox.Show("설정이 적용되었습니다.\nSettings Applied.", "Complete");
        }

        private void BtnClean_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("탐색기를 재시작하고 썸네일 캐시를 삭제합니다.\n모든 썸네일이 새로고침 됩니다.\n계속하시겠습니까?\n\nRestart Explorer and clear thumbnail cache?\nAll thumbnails will be refreshed.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var btn = sender as Button;
                btn.Enabled = false;
                btn.Text = "처리 중... (Processing...)";
                btn.BackColor = Color.DarkRed;
                Application.DoEvents();

                FastRestartExplorer(true);

                btn.Text = "썸네일 캐시 초기화 (Clear Cache)";
                btn.BackColor = Color.IndianRed;
                btn.Enabled = true;

                MessageBox.Show("캐시 삭제 및 새로고침 완료.\nCache Cleared & Refreshed.", "Success");
            }
        }

        private void FastRestartExplorer(bool clearCache)
        {
            try
            {
                if (clearCache)
                {
                    string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string cachePath = Path.Combine(localApp, @"Microsoft\Windows\Explorer");
                    
                    try
                    {
                        var files = Directory.GetFiles(cachePath, "thumbcache_*.db");
                        foreach (var f in files)
                        {
                            try { File.Delete(f); } catch { }
                        }
                    }
                    catch { }
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c taskkill /f /im explorer.exe & timeout /t 1 /nobreak > nul & start explorer.exe",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);

                Thread.Sleep(1500);
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
            }
            catch { }
        }

        private void UpdateStatusLabel(bool active)
        {
            var tab = tabControl.TabPages[0];
            Label lbl = (Label)tab.Controls["lblStatus"];

            if (active) {
                lbl.Text = "현재 상태 (Status): ✅ 작동 중 (Active)";
                lbl.ForeColor = Color.Green;
            } else {
                lbl.Text = "현재 상태 (Status): ⛔ 중지됨 (Disabled)";
                lbl.ForeColor = Color.Red;
            }
        }
    }
}