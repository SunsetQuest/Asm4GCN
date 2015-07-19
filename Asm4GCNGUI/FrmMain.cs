// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without any warranty.
using OpenClWithGcnNS;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Security.AccessControl;    // for DirectoryHasPermission
using System.Security.Principal;        // for DirectoryHasPermission
using FastColoredTextBoxNS;
using AC = AutocompleteMenuNS;          // for autocompleteMenu1 
using System.Runtime.InteropServices;   // for NotepadHelper

namespace Asm4GcnGUI
{
    public partial class frmMain : Form
    {
        OpenClWithGCN gcn = new OpenClWithGCN();

        bool changeSinceLastSave;
        string curFileName;
        string outputFolder = Path.GetTempPath() + "\\OpenCLwithGCNOutput";
        FastColoredTextBox lastSelectedTextControl;
        Regex S_InstNames, V_InstNames, O_InstNames;
        
        public frmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string gpuInfo = OpenClWithGCN.CheckGPUAndVersion();
            if (gpuInfo.Length > 0)
                txtOutput.AppendText(gpuInfo);

            /////////////////// Copy needed exe/dll files to output folder ///////////////////
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);
            //if (!File.Exists(outputFolder + "\\OpenClWithGCN.dll"))
                File.Copy("OpenClWithGCN.dll", outputFolder + "\\OpenClWithGCN.dll", true);
            //if (!File.Exists(outputFolder + "\\Asm4GCN.exe"))
                File.Copy("Asm4GCN.exe", outputFolder + "\\Asm4GCN.exe", true);
            //if (!File.Exists(outputFolder + "\\NOpenCL.dll"))
                File.Copy("NOpenCL.dll", outputFolder + "\\NOpenCL.dll", true);

            /////////////////// Setup AutoCompletes for GCN asm window  //////////////////////
            autocompleteMenu1.MaximumSize = new System.Drawing.Size(700, 3000);
            var columnWidth = new int[] { 125, 575 };

            foreach (var i in GcnTools.ISA_DATA.ISA_Insts)
            {
                int img = i.name[0]=='s'?0:i.name[0]=='v'?1:i.name[0]=='d'?2:i.name[0]=='b'?3:4;
                autocompleteMenu1.AddItem(new AC.MulticolumnAutocompleteItem(new[] { i.name, i.iSANotes }, 
                    i.name) { ColumnWidth = columnWidth, ImageIndex = img });
            }
            autocompleteMenu1.AddItem(new AC.MulticolumnAutocompleteItem(new[] { 
                "__asm4GCN", "Begins a Asm4GCN Block" }
                , "__asm4GCN") { ColumnWidth = columnWidth, ImageIndex = 4 }
            );
            autocompleteMenu1.AddItem(new AC.MulticolumnAutocompleteItem(new[] { 
                "__kernel", "Begins an OpenCL Block" }, 
                "__kernel") { ColumnWidth = columnWidth, ImageIndex = 4 }
            );

            changeSinceLastSave = false;
            this.Text = "Welcome to " + Application.ProductName;
        }

        /// <summary>
        /// Wraps the text in the Asm4GCN window in a namespace, class, and constant string.
        /// </summary>
        private string GetEncapsulatedAsmText()
        {
            string modifiedAsm = txtAsm.Text.Replace(@"""", @"""""");
            return @"namespace GCN_NS { static class Code { public const string DevCode = @""" + modifiedAsm + @""";}}";
        }

        private void CompileAndRun(object sender, EventArgs args)
        {
            // lets first clear the output window so we can see the progress
            txtOutput.Clear();
            this.Refresh(); //force txtOutput to update
            String exePathAndName = String.Format(@"{0}\{1}.exe", outputFolder, 
                Path.GetFileNameWithoutExtension(curFileName ?? "Output"));

            // Build and if successful then lets tart the program
            if (Build(exePathAndName))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("cmd", @"/C """ + exePathAndName + @""" & pause" ) ;
                try
                {
                    Process.Start(startInfo);
                }
                catch (Exception e)
                {
                    txtOutput.Text += "ERROR: unable to launch " + exePathAndName + ". Details: " + e.Message;
                }
            }
        }

        private bool Build(String exePathAndName)
        {
            bool enableVSDebug = enableVisualStudioDebugToolStripMenuItem.Checked; // WARNING: Leaves temp files behind.
            if (enableVSDebug)
                txtOutput.AppendText("INFO: compiled with debug info - temp files will not be cleaned up.\r\n");

            bool success = gcn.GcnCompile(txtAsm.Text);

            if (String.IsNullOrWhiteSpace(gcn.env.lastMessage))
                txtOutput.AppendText("INFO: (no Errors/Warnings in GCN kernel)\r\n");
            else
                txtOutput.AppendText(gcn.env.lastMessage);

            if (success)
            {
                var results = (new Microsoft.CSharp.CSharpCodeProvider()).CompileAssemblyFromSource(
                new System.CodeDom.Compiler.CompilerParameters()
                {
                    GenerateInMemory = true, // note: this is not really "in memory" 
                    GenerateExecutable = true,
                    CompilerOptions = " /platform:x86",
                    OutputAssembly = exePathAndName,
                    IncludeDebugInformation = enableVSDebug,  // Debug information - not working some reason
                    TempFiles = new TempFileCollection(outputFolder, enableVSDebug/*keep temp files*/), 
                    //TreatWarningsAsErrors = false,
                    WarningLevel = 3,
                    ReferencedAssemblies = { "mscorlib.dll", "System.dll", "System.Core.dll", "System.Data.dll", 
                        "System.Data.Linq.dll", "System.Xml.dll", "System.Xml.Linq.dll", "NOpenCL.dll", 
                        "OpenClWithGCN.dll" }
                }
                , GetEncapsulatedAsmText(), txtHost.Text);

                foreach (CompilerError error in results.Errors)
                    txtOutput.AppendText(String.Format("{0} ({1}) Line:{2}: {3}\r\n", 
                        error.IsWarning ? "WARNING: " : "ERROR: ", error.ErrorNumber, error.Line, error.ErrorText));

                if (results.Errors.HasErrors)
                    success = false;
                else
                    txtOutput.AppendText("INFO: (no C# errors found in host code)\r\n");

                results.TempFiles.Delete();

            }

            return success;
        }

        private void tsOpen_Click(object sender, EventArgs e)
        {
            CheckSaveBeforeClosingCurrent();

            openFileDialog1.Title = "What file do you with to open?";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                LoadFromFile(openFileDialog1.FileName);
        }

        private void CheckSaveBeforeClosingCurrent()
        {
            if (changeSinceLastSave)
                if (MessageBox.Show("Do you want to save?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) 
                    == DialogResult.Yes)
                {
                    if (String.IsNullOrEmpty(curFileName))
                        PromptAndSaveAs();
                    else
                        SaveToFile(curFileName);
                }
        }

        private void SaveToFile(string fileName)
        {
            File.WriteAllText(fileName, GetEncapsulatedAsmText() + 
                "\r\n///////// HOST ///////// (do not modify this line)\r\n" +
                txtHost.Text );
            changeSinceLastSave = false;
            this.Text = Path.GetFileName(fileName) + " - " + Application.ProductName;
        }

        private void LoadFromFile(string fileName)
        {
            string file;
            try
            {
                file = File.ReadAllText(openFileDialog1.FileName);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
                return;
            }

            Match m = Regex.Match( file, @"^" +
            @".*?public const string DevCode = @""(?<1>.*?)"";}}" + // Grab the device code
            @"\r\n\s*///////// HOST /////////\s*\(do not modify this line\)\s*\r\n" + // End marker for device code
            @"(?<2>.*?)$", RegexOptions.Singleline); // Grab the host code
            
            if (m.Groups[1].Success && m.Groups[1].Success)
            {
                txtAsm.Text = m.Groups[1].Value;
                txtHost.Text = m.Groups[2].Value;
            }
            else // if we cannot pull apart the file then just use host window.
            {
                txtHost.Text = file;
                txtAsm.Clear();
            }

            changeSinceLastSave = false;
            curFileName = fileName;
            this.Text = Path.GetFileName(curFileName) + " - " + Application.ProductName;
        }

        private void tsSave_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(curFileName))
                PromptAndSaveAs();
            else
                SaveToFile(curFileName);
        }

        private void PromptAndSaveAs()
        {
            saveFileDialog1.Title = "Please choose a filename to save to...";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                curFileName = saveFileDialog1.FileName;
                SaveToFile(curFileName);
            }
        }

        private void saveAsToolStripButton_Click(object sender, EventArgs e)
        {
            PromptAndSaveAs();
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            // make sure there is nothing to save first
            CheckSaveBeforeClosingCurrent();

            // load default text
            ComponentResourceManager resources = new ComponentResourceManager(typeof(frmMain));
            txtAsm.Text = resources.GetString("txtAsm.Text");
            txtHost.Text = resources.GetString("txtHost.Text");
            changeSinceLastSave = false;
            curFileName = null;
            this.Text = Application.ProductName;
        }

        private void tsPrint_Click(object sender, EventArgs e)
        {
            PrintDialogSettings printSettings = new PrintDialogSettings() { ShowPrintPreviewDialog = true};
            if (tabControl1.SelectedIndex == 0)
                txtHost.Print(printSettings);
            else
                txtAsm.Print(printSettings);
        }

        private void setUserControlAsCurrent(object sender, EventArgs e)
        {
            lastSelectedTextControl = (FastColoredTextBox)sender;
        }

        private void cutToolStripButton_Click(object sender, EventArgs e)
        {
            if (lastSelectedTextControl.SelectedText != "")
                lastSelectedTextControl.Cut();
        }

        private void copyToolStripButton_Click(object sender, EventArgs e)
        {
            if (lastSelectedTextControl.SelectionLength > 0)
                lastSelectedTextControl.Copy();
        }

        private void pasteToolStripButton_Click(object sender, EventArgs e)
        {
            // Source: http://msdn.microsoft.com/en-us/library/system.windows.forms.textboxbase.copy(v=vs.110).aspx
            if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text) == true)
            {
                if (lastSelectedTextControl.SelectionLength > 0)
                        lastSelectedTextControl.SelectionStart = lastSelectedTextControl.SelectionStart 
                            + lastSelectedTextControl.SelectionLength;
                lastSelectedTextControl.Paste();
            }
        }

        Style GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
        Style S_InstNamesStyle = new TextStyle(Brushes.Blue, null, FontStyle.Bold);
        Style V_InstNamesStyle = new TextStyle(Brushes.IndianRed, null, FontStyle.Bold);
        Style O_InstNamesStyle = new TextStyle(Brushes.DarkSlateBlue, null, FontStyle.Bold);
        Style S_OppStyle = new TextStyle(Brushes.Blue, null, FontStyle.Italic);
        Style V_OppStyle = new TextStyle(Brushes.IndianRed, null, FontStyle.Italic);
        Style constantStyle = new TextStyle(Brushes.DarkGreen, null, FontStyle.Bold);
        Style TTTStyle = new TextStyle(Brushes.White, Brushes.Black, FontStyle.Italic);
        Style CommandsStyle = new TextStyle(Brushes.DarkGoldenrod, null, FontStyle.Italic);
        Style RegistersStyle = new TextStyle(Brushes.DarkCyan, null, FontStyle.Bold);
        Style LabelStyle = new TextStyle(Brushes.DarkOrange, null, FontStyle.Underline | FontStyle.Bold);
        Style DefineStyle = new TextStyle(Brushes.Black, null, FontStyle.Italic | FontStyle.Bold);
        
        private void txtAsm_TextChanged(object sender, TextChangedEventArgs e)
        {
            Range range = (sender as FastColoredTextBox).VisibleRange;

            // Text Template Transformations
            e.ChangedRange.ClearStyle(TTTStyle);
            e.ChangedRange.SetStyle(S_OppStyle, @"\[\[.*?\]\]");

            // Comments
            range.ClearStyle(GreenStyle);
            range.SetStyle(GreenStyle, @"//.*$", RegexOptions.Multiline);
            range.SetStyle(GreenStyle, @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline);
            range.SetStyle(GreenStyle, @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft);

            // Commands
            range.ClearStyle(CommandsStyle);
            range.SetStyle(CommandsStyle, @"(?<=(\r\n|;)\s*)([sv](1|2|4|8|16)[iufb]|free|ren|#define)(?=\s)");

            // Registers
            range.ClearStyle(RegistersStyle);
            range.SetStyle(RegistersStyle, @"(?<=[\s,;])[sv](\d+|\[\d+:\d+\])(?=[\s,;])");

            // Labels
            range.ClearStyle(LabelStyle);
            range.SetStyle(LabelStyle, @"(?<=[\s,;])(@[a-zA-Z_][a-zA-Z0-9_]*|[a-zA-Z_][a-zA-Z0-9_]*:)(?=[\s,;])");
            
            // Defines var names  (i.e.  _myDefine_ )
            range.ClearStyle(DefineStyle);
            range.SetStyle(DefineStyle, @"(?<=[\s,;])(_[a-zA-Z0-9_]+_)(?=[\s,;])");

            // initialize S_InstNames, V_InstNames, and O_InstNames
            if (S_InstNames == null)
            {
                StringBuilder s = new StringBuilder();
                StringBuilder v = new StringBuilder();
                StringBuilder o = new StringBuilder();
                s.Append(@"\s(");
                v.Append(@"\s(");
                o.Append(@"\s(");
                foreach (var item in GcnTools.ISA_DATA.ISA_Insts)
                    if (item.name[0] == 's')
                        s.Append(item.name + "|");
                    else if (item.name[0] == 'v')
                        v.Append(item.name + "|");
                    else
                        o.Append(item.name + "|");
                s.Length--;
                v.Length--;
                o.Length--;

                s.Append(@")\s");
                v.Append(@")\s");
                o.Append(@")\s");

                S_InstNames = new Regex(s.ToString());
                V_InstNames = new Regex(v.ToString());
                O_InstNames = new Regex(o.ToString());
            }

            e.ChangedRange.ClearStyle(S_InstNamesStyle);
            e.ChangedRange.ClearStyle(V_InstNamesStyle);
            e.ChangedRange.ClearStyle(O_InstNamesStyle);
            e.ChangedRange.SetStyle(S_InstNamesStyle, S_InstNames);
            e.ChangedRange.SetStyle(V_InstNamesStyle, V_InstNames);
            e.ChangedRange.SetStyle(O_InstNamesStyle, O_InstNames);

            e.ChangedRange.ClearStyle(S_OppStyle);
            e.ChangedRange.ClearStyle(V_OppStyle);
            e.ChangedRange.ClearStyle(constantStyle);
            e.ChangedRange.SetStyle(S_OppStyle, @"[\s,][sS](\bin+|\[\bin+:\bin+\])[\s,]");
            e.ChangedRange.SetStyle(V_OppStyle, @"[\s,][vV](\bin+|\[\bin+:\bin+\])[\s,]");
            e.ChangedRange.SetStyle(constantStyle, @"[\s,](?i:" +
                @"(?:0(?<1>x)(?<2>[0-9a-f]+))|" + // hex values 0x12AB
                @"(?:0(?<1>o)(?<2>[0-9a-f]+))|" + // oct values 0o7564
                @"(?:0(?<1>b)(?<2>[0-9a-f]+))|" + // bin values 0b1101010
                @"(?<2>-?(?<1>\bin)\bin*(?:E[+-]?\bin+)?)" +// exponent int (nnnExx, nnnE+xx, and nnnE-xx)
                @")[\s,]");


            // if we are going from a unsaved state to saved state lets update the title
            if (!changeSinceLastSave && !String.IsNullOrEmpty(curFileName))
                this.Text = Path.GetFileName(curFileName) + "(unsaved) - " + Application.ProductName;
            changeSinceLastSave = true;
        }

        Style InfoStyle = new TextStyle(Brushes.Black, Brushes.LightGreen, FontStyle.Bold);
        Style WarnStyle = new TextStyle(Brushes.Black, Brushes.LightYellow, FontStyle.Bold);
        Style ErrorStyle = new TextStyle(Brushes.Black, Brushes.Salmon, FontStyle.Bold);
        private void txtOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            Range range = (sender as FastColoredTextBox).VisibleRange;

            //range.ClearStyle(GreenStyle);
            e.ChangedRange.SetStyle(InfoStyle, @"^INFO:", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(WarnStyle, @"^WARNING:", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(ErrorStyle, @"^ERROR:", RegexOptions.Multiline);
        }


        MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));
        /// <summary>This function is used to highlight matching words from where the curser is at.</summary>
        private void txtAsm_SelectionChangedDelayed(object sender, EventArgs e)
        {
            var fctb = sender as FastColoredTextBox;

            fctb.VisibleRange.ClearStyle(SameWordsStyle);

            if (!fctb.Selection.IsEmpty)
                return; //user selected diapason

            //get fragment around caret
            var fragment = fctb.Selection.GetFragment(@"\w");
            string text = fragment.Text;
            if (text.Length == 0)
                return;
            //highlight same words
            var ranges =
                fctb.VisibleRange.GetRanges("\\b" + text + "\\b");
            //if (ranges.Length > 1)
            foreach (var r in ranges)
                r.SetStyle(SameWordsStyle);
        }


        private void txtHost_TextChanged(object sender, TextChangedEventArgs e)
        {
            // if we are going from a unsaved state to saved state lets update the title
            if (!changeSinceLastSave && !String.IsNullOrEmpty(curFileName))
                this.Text = Path.GetFileName(curFileName) + "(unsaved) - " + Application.ProductName;
        }

        private void codeProjectToolStripButton_Click(object sender, EventArgs e)
        {
            // source: http://support.microsoft.com/kb/305703 (Jan 2015)
            try
            {
                System.Diagnostics.Process.Start("http://www.codeproject.com/Articles/872477/GCN-Assembler-for-AMD-GPUs");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void showSpecsInNotepadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NotepadHelper.ShowMessage(OpenClWithGCN.GetGPUInfo());
        }

        private void saveKernelToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gcn.GcnCompile(txtAsm.Text))
            {
                foreach (AsmBlock blk in gcn.env.asmBlocks)
                {
                    string filename = "KernelBinary-" + blk.funcName + ".bin";
                    txtOutput.Text = "Saving Bin to " + outputFolder + filename;
                    File.WriteAllBytes(filename, blk.bin); // Requires System.IO
                }
            }
            else
            {
                MessageBox.Show("Unable to export (see output log)");
                txtOutput.Text = gcn.env.lastMessage;
            }
        }

        private void displayKernelsBinaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gcn.GcnCompile(txtAsm.Text))
            {
                txtOutput.Text = "INFO: Open kernel in notepad";
                StringBuilder hex = new StringBuilder();
                foreach (AsmBlock blk in gcn.env.asmBlocks)
                {
                    byte[] bin = blk.bin;
                    hex.AppendFormat("Kernel {0}\r\n", blk.funcName);
                    int i; 
                    for (i = 0; i < bin.Length - 4; i += 4)
                        hex.AppendFormat("{0:x2}{1:x2}{2:x2}{3:x2}\r\n", bin[i], bin[i + 1], bin[i + 2], bin[i + 3]);
                    for (; i < bin.Length; i++)
                        hex.AppendFormat("{0:x2}", bin[i]);
                }
                NotepadHelper.ShowMessage(hex.ToString());
            }
            else
            {
                MessageBox.Show("Unable to export (see output log)");
                txtOutput.Text = gcn.env.lastMessage;
            }
        }

        private void saveDummyBinToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gcn.GcnCompile(txtAsm.Text))
            {
                txtOutput.Text = "INFO: Saving dummy bin to " + outputFolder + "\\DummyBinary.bin.";
                File.WriteAllBytes("DummyBinary.bin", gcn.env.dummyBin); // Requires System.IO
            }
            else
            {
                MessageBox.Show("Unable to export (see output log)");
                txtOutput.Text = gcn.env.lastMessage;
            }
        }

        private void showDummyBinaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gcn.GcnCompile(txtAsm.Text))
            {
                txtOutput.Text = "INFO: Open dummy kernel in notepad.";
                StringBuilder hex = new StringBuilder();
                int i;
                byte[] bin = gcn.env.dummyBin;
                for (i = 0; i < bin.Length - 4; i += 4)
                    hex.AppendFormat("{0:x2}{1:x2}{2:x2}{3:x2}\r\n", bin[i], bin[i + 1], bin[i + 2], bin[i + 3]);
                for (; i < bin.Length; i++)
                    hex.AppendFormat("{0:x2}", bin[i]);
                NotepadHelper.ShowMessage(hex.ToString());
            }
            else
            {
                MessageBox.Show("Unable to export (see output log)");
                txtOutput.Text = gcn.env.lastMessage;
            }
        }

        private void openOutputFolderInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(outputFolder);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            CheckSaveBeforeClosingCurrent();
        }

        private void creditsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this,"Thank you to the following...\n\n"
                + "AMD for making their GCN ISA manuals available. I spend many hours combing through the ISA documentation and referencing their data tables. 90% of the instruction information was taken directly AMD’s ISA manual.\n\n"
                + "Sam Harwell for providing NOpenCL, an excellent OpenCL wrapper for .Net. I like this OpenCL wrapper because it is very .net like and is also close the core c based OpenCL. The Tunnel Vision Laboratories project can be found here: https://github.com/tunnelvisionlabs\n\n"
                + "Daniel Bali for building an excellent, easy to follow, open source GCN assembler. This was my first compiler so I was looking for ideas on how to start.  Daniel’s project gave me ideas on how I could tackle this.\n\n"
                + "Derek Gerstmann for his easy to follow and complete OpenCL example. It is very easy to follow and provides a great example for OpenCL. It has been modified some to fit ASM and C# NOpenCL.\n\n"
                + "Pavel Torgashov for the FastColoredTextBox editor control. This control adds some syntax highlighting richness to the GUI interface. His project can be found on CodeProject here: http://www.codeproject.com/Articles/161871/Fast-Colored-TextBox-for-syntax-highlighting \n\n"
                + "Realhet, who built a fully functional and feature rich assembler for windows called HetPas. Much of my GCN assembly skills came from toying with Realhet’s assembler and from reading his posts.  I have been a Realhet fan for a while.  He has made many insightful posts on AMD forums and on his https://realhet.wordpress.com site.\n\n"
                + "\nFor some reason it feels so old school to put credits in an about box... :-)\n\n"
                ,"Credits...", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

       
        /// <summary>
        /// Opens a notepad and places a string in the body and also gives it an optional title.
        /// Source: kmatyaszek on 1/28/2015  
        /// http://stackoverflow.com/questions/7613576/how-to-open-text-in-notepad-from-net 
        /// </summary>
        public static class NotepadHelper
        {
            [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Unicode)]
            private static extern int SetWindowText(IntPtr hWnd, string text);

            [DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Unicode)]
            private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, 
                string lpszClass, string lpszWindow);

            [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Unicode)]
            private static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

            /// <summary>
            /// Opens a notepad and places a string in the body and also gives it an optional title.
            /// </summary>
            public static void ShowMessage(string message = null, string title = null)
            {
                Process notepad = Process.Start(new ProcessStartInfo("notepad.exe"));
                if (notepad != null)
                {
                    notepad.WaitForInputIdle();

                    if (!string.IsNullOrEmpty(title))
                        SetWindowText(notepad.MainWindowHandle, title);

                    if (!string.IsNullOrEmpty(message))
                    {
                        IntPtr child = FindWindowEx(notepad.MainWindowHandle, new IntPtr(0), "Edit", null);
                        SendMessage(child, 0x000C, 0, message);
                    }
                }
            }
        }

        private void undoCtrlZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lastSelectedTextControl.UndoEnabled)
                lastSelectedTextControl.Undo();
        }

        private void redoCtrlRToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lastSelectedTextControl.RedoEnabled)
                lastSelectedTextControl.Redo();
        }

        private void findCtrlFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lastSelectedTextControl.ShowFindDialog();
        }

        private void replaceCtrlHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lastSelectedTextControl.ShowReplaceDialog();
        }

        private void SendKeysToControl(object sender, EventArgs e)
        {
            lastSelectedTextControl.Focus();
            SendKeys.Send((string)((ToolStripItem)sender).Tag);
        }
    } // end frmMain class
} //end Asm4GcnGUI namespace