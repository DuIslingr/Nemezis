using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Nemezis
{
    public partial class RadForm1 : RadForm
    {
        public bool _CanRun = true;

        private string _FilesFolderPath = Application.StartupPath + "/files/";
        private string _PathLLVMMCexe { get { return Path.Combine(_FilesFolderPath, "llvm-mc.exe"); } }
        private string _PathInputFile { get { return Path.Combine(_FilesFolderPath, "tmpFile"); } }

        private string _LlvmExecuteCommand = " \"{0}\" {1} -show-encoding";

        private delegate void receivedConsoleResponseDelegate(string output);

        private string consoleResponse = "";

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
            string pathToLLvmExe = Path.Combine(_FilesFolderPath, _PathLLVMMCexe);
            bool bNoLLVMMCExe = !File.Exists(pathToLLvmExe);
            if (bNoLLVMMCExe)
            {
                string sMsg = string.Format(format: "Required file not found! ({0})", arg0: pathToLLvmExe);
                this.Hide();
                RadMessageBox.Show(this, sMsg, caption: "Error", buttons: MessageBoxButtons.OK);
                this._CanRun = false;
            }
        }

        private void convertToAsm_Click(object sender, EventArgs e)
        {
            string input = asmToHexAsmTextbox.Text;
            if (writeToInputFile(input)) {
                executeCommand(false, new receivedConsoleResponseDelegate(
                    (output) => {
                        string converted = getHexValuesFromConsoleOutput(output);
                        SetTextboxText(asmToHexHexTextBox, converted);
                }));
            } else {
                // ErrorHandling
            }
        }
        private void convertToHex_Click(object sender, EventArgs e)
        {
            string input = hexToAsmHexTextbox.Text;
            if (writeToInputFile(input))
            {
                executeCommand(true, new receivedConsoleResponseDelegate(
                    (output) => {
                        string converted = getHexValuesFromConsoleOutput(output);
                        SetTextboxText(asmToHexHexTextBox, converted);
                    }));
            }
            else {
                // ErrorHandling
            }
        }

        private string getHexValuesFromConsoleOutput(string output)
        {
            string converted = "";

            int idx = output.IndexOf("[");
            if (idx > 0){
                string tmp = output.Substring(idx + 1);
                idx = tmp.IndexOf("]");
                if (idx > 0)
                {
                    tmp = tmp.Substring(0, idx);
                    string tmpValue = "";
                    int val = -1;
                    while (tmp.Length > 0)
                    {
                        int commaIdx = tmp.IndexOf(",");
                        if (commaIdx > 0) {
                            tmpValue = tmp.Substring(0, commaIdx);
                        }
                        // parse hex to int
                        val = Convert.ToInt32(tmpValue, 16);
                        converted += val.ToString("X2") + " ";

                        if (commaIdx > 0) {
                            tmp = tmp.Substring(commaIdx + 1);
                        } else {
                            tmp = "";
                        }
                    }
                }
            }

            return converted.Trim();
        }

        private bool writeToInputFile(string inputText)
        {
            try {
                StreamWriter sw = new StreamWriter(_PathInputFile);
                sw.Write(inputText);
                sw.Flush();
                sw.Close();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }
        private void executeCommand(bool isDisassemble, receivedConsoleResponseDelegate del)
        {
            Process p = new Process();
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = _PathLLVMMCexe;
            p.StartInfo.Arguments = string.Format(_LlvmExecuteCommand, _PathInputFile, getSelectedParam());
            if (isDisassemble) {
                p.StartInfo.Arguments += " -disassemble";
            }
            consoleResponse = "";
            p.OutputDataReceived += new DataReceivedEventHandler(
                (s, e) =>
                {
                    if (e.Data == null)
                    {
                        del(consoleResponse);
                    } else
                    {
                        consoleResponse += e.Data + "\n";
                    }
                }
            );
            p.ErrorDataReceived += new DataReceivedEventHandler((s, e) => {
                del(e.Data);
                Console.WriteLine(e.Data);
            });

            p.Start();
            p.BeginOutputReadLine();
        }

        public void SetTextboxText(RadTextBox textBox, string text)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<RadTextBox,string>(SetTextboxText), new object[] { textBox, text });
                return;
            }
            textBox.Text = text;
        }
        private string getSelectedParam()
        {
            string param = "";
            if (radioArm.IsChecked)
            {
                param = "-arch=arm";
            }
            else if (radioThumb.IsChecked)
            {
                param = "-arch=thumb";
            }
            else if (radioAarch64.IsChecked)
            {
                param = "-arch=aarch64";
            }
            else if (radioX86.IsChecked)
            {
                param = "-arch=x86";
            }
            else if (radioX86_64.IsChecked)
            {
                param = "-arch=x86-64";
            }
            return param;
        }

        private void radButton3_Click(object sender, EventArgs e)
        {
            RadMessageBox.Show("Nemezis v1.0.0!\nDesigned by Du'Islingr", "About");
        }
    }
}
