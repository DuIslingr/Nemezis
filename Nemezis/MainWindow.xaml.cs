using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using System.Text.RegularExpressions;


namespace Nemizisv2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public bool _CanRun = true;

        private bool lldbGDB1WasClicked = false;

        private string _FilesFolderPath = AppDomain.CurrentDomain.BaseDirectory + "files";
        private string _PathLLVMMCexe { get { return Path.Combine(_FilesFolderPath, "llvm-mc.exe"); } }
        private string _PathInputFile { get { return Path.Combine(_FilesFolderPath, "tmpFile"); } }

        private string _LlvmExecuteCommand = " \"{0}\" {1} -show-encoding";

        private delegate void ReceivedConsoleResponseDelegate(string output);

        private string consoleResponse = "";

        public MainWindow()
        {
            InitializeComponent();


            InitializeComponent();

            string localUserPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            CheckFiles();
        }

        private void CheckFiles()
        {
            string pathToLLvmExe = Path.Combine(_FilesFolderPath, _PathLLVMMCexe);
            bool bNoLLVMMCExe = !File.Exists(pathToLLvmExe);
            if (bNoLLVMMCExe)
            {
                string sMsg = string.Format(format: "Required file not found! ({0})", arg0: pathToLLvmExe);
                this.Hide();
                MessageBox.Show(this, sMsg, caption: "Error", button: MessageBoxButton.OK);
                this._CanRun = false;
                Environment.Exit(1);
            }
        }

        private void convertAsmToHex_Click_1(object sender, RoutedEventArgs e)
        {
            lldbGDB1WasClicked = true;
            string input = ASM2HEX_ASMBX.Text;
            if (writeToInputFile(input))
            {
                executeCommand(false, new ReceivedConsoleResponseDelegate(
                    (output) => {
                        List<string> converted = getHexValuesFromConsoleOutput(output);
                        string finalString = string.Join(" ", converted);
                        SetTextboxText(ASM2HEX_HEXBX, finalString);
                    }));
            }
            else
            {
                // ErrorHandling
            }
        }

        private void convertHexToAsm_Click_1(object sender, RoutedEventArgs e)
        {
            string input = HEX2ASM_HEXBX.Text;
            string[] hexvals = input.Split(' ');
            if (hexvals.Length <= 0)
            {
                return;
            }
            string fileString = "";
            foreach (string val in hexvals)
            {
                string fixedVal = val.Replace("0x", "");
                fixedVal = "0x" + fixedVal.ToUpper();
                fileString += fixedVal + " ";
            }
            fileString = fileString.Trim();

            if (writeToInputFile(fileString))
            {
                executeCommand(true, new ReceivedConsoleResponseDelegate(
                    (output) => {
                        string command = getAsmCommandFromConsoleOutput(output);
                        SetTextboxText(HEX2ASM_ASMBX, command);
                    }));
            }
            else
            {
                // ErrorHandling
            }
        }

        private string getAsmCommandFromConsoleOutput(string output)
        {
            string returnVal = "";
            string[] lines = output.Split('\n');
            string encodingEnd = "encoding:";
            Regex reg = new Regex("\t.* encoding:");
            Match match = reg.Match(output);
            if (match.Success)
            {
                string line = match.Value.Replace(encodingEnd, "");

                string cmd = line.Trim();
                cmd = cmd.Substring(0, cmd.Length - 1); // drop last char (is # or @ or something)
                cmd = cmd.Replace("\t", " ");
                cmd = cmd.Trim();
                returnVal = cmd;
            }

            return returnVal;
        }
        private List<string> getHexValuesFromConsoleOutput(string output)
        {
            List<string> values = new List<string>();
            Regex reg = new Regex("\\[[0-9a-fx, ]+\\]");
            Match match = reg.Match(output);
            if (match.Success)
            {
                char[] charsToTrim = { '[', ']' };
                string fixedMatch = match.Value.Trim(charsToTrim);
                string[] parts = fixedMatch.Split(',');
                foreach (string part in parts)
                {
                    string fixedPart = part.ToUpper().Replace("0X", "");
                    values.Add(fixedPart);
                }
            }

            return values;
        }

        private bool writeToInputFile(string inputText)
        {
            try
            {
                StreamWriter sw = new StreamWriter(_PathInputFile);
                sw.Write(inputText);
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }
        private void executeCommand(bool isDisassemble, ReceivedConsoleResponseDelegate del)
        {
            Process p = new Process();
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = _PathLLVMMCexe;
            p.StartInfo.Arguments = string.Format(_LlvmExecuteCommand, _PathInputFile, getSelectedParam());

            if (isDisassemble)
            {
                p.StartInfo.Arguments += " -disassemble";
            }
            consoleResponse = "";
            p.OutputDataReceived += new DataReceivedEventHandler(
                (s, e) =>
                {
                    if (e.Data == null)
                    {
                        del(consoleResponse);
                    }
                    else
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

        public void SetTextboxText(TextBox textBox, string text)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(new Action<TextBox, string>(SetTextboxText), new object[] { textBox, text });
                return;
            }
            textBox.Text = text;
        }

        private string getSelectedParam()
        {
            string param = "";
            if (radioArm.IsChecked == true)
            {
            if (toggleButton.IsChecked == true && lldbGDB1WasClicked)
                    param = "-triple=armv7eb";
                    lldbGDB1WasClicked = false;
            else
                    param = "-triple=armv7";
                    lldbGDB1WasClicked = false;
            }
            else if (radioThumb.IsChecked == true)
            {
            if (toggleButton.IsChecked == true && lldbGDB1WasClicked)
                    param = "-triple=thumbv7eb";
                    lldbGDB1WasClicked = false;
            else
                    param = "-triple=thumbv7";
                    lldbGDB1WasClicked = false;
            }
            else if (radioAarch64.IsChecked == true)
            {
            if (toggleButton.IsChecked == true && lldbGDB1WasClicked)
                    param = "-triple=aarch64_be";
                    lldbGDB1WasClicked = false;
                    MessageBox.Show("Supposedly Big Endian is supported, but it seems to be the same hex. So just disable LLDB/GDB for arm64");
            else
                    param = "-triple=aarch64";
                    lldbGDB1WasClicked = false;
            }
            else if (radioX86.IsChecked == true)
            {
            if (toggleButton.IsChecked == true && lldbGDB1WasClicked)
                    MessageBox.Show("Big Endian aka GDB/LLDB Mode is not supported with this Architecture");
                    lldbGDB1WasClicked = false;
            else
                    param = "-arch=x86";
                    lldbGDB1WasClicked = false;
            }
            else if (radioX86_64.IsChecked == true)
            {
            if (toggleButton.IsChecked == true && lldbGDB1WasClicked)
                    MessageBox.Show("Big Endian aka GDB/LLDB Mode is not supported with this Architecture");
                    lldbGDB1WasClicked = false;
            else
                    param = "-arch=x86-64";
                    lldbGDB1WasClicked = false;
            }
            return param;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Nemezis v1.0.0!\nCoded by Blizzard\nCoded and Designed by Du'Islingr", "About");
        }

    }
}
