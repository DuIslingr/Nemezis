using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Nemezis
{
    public partial class RadForm1 : RadForm
    {
        public bool _CanRun = true;

        string aParams = "";

        private string _PathLLVMMCexe = Application.StartupPath + @"/files/llvm-mc.exe";

        public RadForm1()
        {
            InitializeComponent();
            ThemeResolutionService.ApplicationThemeName = "VisualStudio2012Dark";
            string _LocalUserPath = Path.GetDirectoryName(Application.ExecutablePath);

            CheckFiles();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RadTypeResolver.Instance.ResolveTypesInCurrentAssembly = true;
        }

        private void CheckFiles()
        {
            bool bNoLLVMMCExe = !File.Exists(_PathLLVMMCexe);
            if (bNoLLVMMCExe)
            {
                string sFilesNotFound = "";
                if (bNoLLVMMCExe)
                    sFilesNotFound += "/files/llvm-mc.exe ";
                string sMsg = string.Format(format: "Required file not found! ({0})", arg0: sFilesNotFound.Trim());
                this.Hide();
                RadMessageBox.Show(this, sMsg, caption: "Error", buttons: MessageBoxButtons.OK);
                this._CanRun = false;
            }
        }

        private void radButton1_Click(object sender, EventArgs e)
        {
            if (radRadioButton1.IsChecked)
            {
                aParams = "-arch=arm";
            }
            else if (radRadioButton2.IsChecked)
            {
                aParams = "-arch=thumb";
            }
            else if (radRadioButton3.IsChecked)
            {
                aParams = "-arch=aarch64";
            }
            else if (radRadioButton4.IsChecked)
            {
                aParams = "-arch=x86";
            }
            else if (radRadioButton5.IsChecked)
            {
                aParams = "-arch=x86-64";
            }

        }

        private void radButton2_Click(object sender, EventArgs e)
        {
            if (radRadioButton1.IsChecked)
            {
                aParams = "-arch=arm";
            }
            else if (radRadioButton2.IsChecked)
            {
                aParams = "-arch=thumb";
            }
            else if (radRadioButton3.IsChecked)
            {
                aParams = "-arch=aarch64";
            }
            else if (radRadioButton4.IsChecked)
            {
                aParams = "-arch=x86";
            }
            else if (radRadioButton5.IsChecked)
            {
                aParams = "-arch=x86-64";
            }
        }

        private void radButton3_Click(object sender, EventArgs e)
        {
            RadMessageBox.Show("Nemezis v1.0.0!\nDesigned by Du'Islingr", "About");
        }
    }
}
