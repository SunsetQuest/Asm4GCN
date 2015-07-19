namespace Asm4GcnGUI
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();

                InfoStyle.Dispose();
                WarnStyle.Dispose();
                ErrorStyle.Dispose();
                S_InstNamesStyle.Dispose();
                TTTStyle.Dispose();
                V_InstNamesStyle.Dispose();
                constantStyle.Dispose();
                V_OppStyle.Dispose();
                S_OppStyle.Dispose();
                O_InstNamesStyle.Dispose();
                GreenStyle.Dispose();
                TTTStyle.Dispose();
                CommandsStyle.Dispose();
                RegistersStyle.Dispose();
                LabelStyle.Dispose();
                DefineStyle.Dispose();
                SameWordsStyle.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsNew = new System.Windows.Forms.ToolStripButton();
            this.tsOpen = new System.Windows.Forms.ToolStripButton();
            this.tsSave = new System.Windows.Forms.ToolStripButton();
            this.saveAsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.tsPrint = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.cutToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.copyToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.pasteToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsRun = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.codeProjectToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButtonMore = new System.Windows.Forms.ToolStripDropDownButton();
            this.openOutputFolderInExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableVisualStudioDebugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kernelExportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveKernelToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayKernelsBinaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportDummyKernelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveDummyBinToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDummyBinaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showSpecsInNotepadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.creditsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButtonEditor = new System.Windows.Forms.ToolStripDropDownButton();
            this.undoCtrlZToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoCtrlRToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.findCtrlFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findNextF3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceCtrlHToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gotoNextCharAltFcharToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.forwardNavigationShiftCtrlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backwardNavigationCtrlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gotoLineCtrlGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.goToFirstCharOfWordCtrlHomeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.goToLastCharOfWordCtrlEndToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllCtrlAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.increaseIndentShiftTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutCurrentLineShiftDelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.recordStopMacroCtrlMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.executeMacroCtrlEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.zoomInCtrlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zoomOutCtrlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zoom100Ctrl0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zoomCtrlWheelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.addBookmarkCtrlBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeBookmarkCtrlShiftBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nextBookmarkCtrlNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.prevBookmarkCtrlShiftNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtOutput = new FastColoredTextBoxNS.FastColoredTextBox();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabHost = new System.Windows.Forms.TabPage();
            this.txtHost = new FastColoredTextBoxNS.FastColoredTextBox();
            this.tabGCN = new System.Windows.Forms.TabPage();
            this.txtAsm = new FastColoredTextBoxNS.FastColoredTextBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.autocompleteMenu1 = new AutocompleteMenuNS.AutocompleteMenu();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtOutput)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabHost.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtHost)).BeginInit();
            this.tabGCN.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtAsm)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 910);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(723, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsNew,
            this.tsOpen,
            this.tsSave,
            this.saveAsToolStripButton,
            this.tsPrint,
            this.toolStripSeparator,
            this.cutToolStripButton,
            this.copyToolStripButton,
            this.pasteToolStripButton,
            this.toolStripSeparator2,
            this.tsRun,
            this.toolStripSeparator1,
            this.codeProjectToolStripButton,
            this.toolStripSeparator3,
            this.toolStripDropDownButtonMore,
            this.toolStripDropDownButtonEditor});
            this.toolStrip1.Location = new System.Drawing.Point(108, -2);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(383, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsNew
            // 
            this.tsNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsNew.Image = ((System.Drawing.Image)(resources.GetObject("tsNew.Image")));
            this.tsNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsNew.Name = "tsNew";
            this.tsNew.Size = new System.Drawing.Size(23, 22);
            this.tsNew.Text = "&New";
            this.tsNew.Click += new System.EventHandler(this.newToolStripButton_Click);
            // 
            // tsOpen
            // 
            this.tsOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsOpen.Image = ((System.Drawing.Image)(resources.GetObject("tsOpen.Image")));
            this.tsOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsOpen.Name = "tsOpen";
            this.tsOpen.Size = new System.Drawing.Size(23, 22);
            this.tsOpen.Text = "&Open";
            this.tsOpen.Click += new System.EventHandler(this.tsOpen_Click);
            // 
            // tsSave
            // 
            this.tsSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsSave.Image = ((System.Drawing.Image)(resources.GetObject("tsSave.Image")));
            this.tsSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsSave.Name = "tsSave";
            this.tsSave.Size = new System.Drawing.Size(23, 22);
            this.tsSave.Text = "&Save";
            this.tsSave.Click += new System.EventHandler(this.tsSave_Click);
            // 
            // saveAsToolStripButton
            // 
            this.saveAsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveAsToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("saveAsToolStripButton.Image")));
            this.saveAsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveAsToolStripButton.Name = "saveAsToolStripButton";
            this.saveAsToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.saveAsToolStripButton.Text = "Save As...";
            this.saveAsToolStripButton.Click += new System.EventHandler(this.saveAsToolStripButton_Click);
            // 
            // tsPrint
            // 
            this.tsPrint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsPrint.Image = ((System.Drawing.Image)(resources.GetObject("tsPrint.Image")));
            this.tsPrint.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsPrint.Name = "tsPrint";
            this.tsPrint.Size = new System.Drawing.Size(23, 22);
            this.tsPrint.Text = "&Print";
            this.tsPrint.Click += new System.EventHandler(this.tsPrint_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // cutToolStripButton
            // 
            this.cutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.cutToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("cutToolStripButton.Image")));
            this.cutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cutToolStripButton.Name = "cutToolStripButton";
            this.cutToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.cutToolStripButton.Text = "C&ut";
            this.cutToolStripButton.Click += new System.EventHandler(this.cutToolStripButton_Click);
            // 
            // copyToolStripButton
            // 
            this.copyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.copyToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("copyToolStripButton.Image")));
            this.copyToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyToolStripButton.Name = "copyToolStripButton";
            this.copyToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.copyToolStripButton.Text = "&Copy";
            this.copyToolStripButton.Click += new System.EventHandler(this.copyToolStripButton_Click);
            // 
            // pasteToolStripButton
            // 
            this.pasteToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.pasteToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripButton.Image")));
            this.pasteToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pasteToolStripButton.Name = "pasteToolStripButton";
            this.pasteToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.pasteToolStripButton.Text = "&Paste";
            this.pasteToolStripButton.Click += new System.EventHandler(this.pasteToolStripButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsRun
            // 
            this.tsRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsRun.Image = ((System.Drawing.Image)(resources.GetObject("tsRun.Image")));
            this.tsRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsRun.Name = "tsRun";
            this.tsRun.Size = new System.Drawing.Size(23, 22);
            this.tsRun.Text = "Compile and Run";
            this.tsRun.Click += new System.EventHandler(this.CompileAndRun);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // codeProjectToolStripButton
            // 
            this.codeProjectToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.codeProjectToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("codeProjectToolStripButton.Image")));
            this.codeProjectToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.codeProjectToolStripButton.Name = "codeProjectToolStripButton";
            this.codeProjectToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.codeProjectToolStripButton.Text = "CodeProject Site";
            this.codeProjectToolStripButton.Click += new System.EventHandler(this.codeProjectToolStripButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripDropDownButtonMore
            // 
            this.toolStripDropDownButtonMore.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButtonMore.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openOutputFolderInExplorerToolStripMenuItem,
            this.enableVisualStudioDebugToolStripMenuItem,
            this.kernelExportToolStripMenuItem,
            this.exportDummyKernelToolStripMenuItem,
            this.showSpecsInNotepadToolStripMenuItem,
            this.creditsToolStripMenuItem});
            this.toolStripDropDownButtonMore.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButtonMore.Image")));
            this.toolStripDropDownButtonMore.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButtonMore.Name = "toolStripDropDownButtonMore";
            this.toolStripDropDownButtonMore.Size = new System.Drawing.Size(57, 22);
            this.toolStripDropDownButtonMore.Text = "More...";
            // 
            // openOutputFolderInExplorerToolStripMenuItem
            // 
            this.openOutputFolderInExplorerToolStripMenuItem.Name = "openOutputFolderInExplorerToolStripMenuItem";
            this.openOutputFolderInExplorerToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.openOutputFolderInExplorerToolStripMenuItem.Text = "Open Output Folder in Explorer";
            this.openOutputFolderInExplorerToolStripMenuItem.Click += new System.EventHandler(this.openOutputFolderInExplorerToolStripMenuItem_Click);
            // 
            // enableVisualStudioDebugToolStripMenuItem
            // 
            this.enableVisualStudioDebugToolStripMenuItem.CheckOnClick = true;
            this.enableVisualStudioDebugToolStripMenuItem.Name = "enableVisualStudioDebugToolStripMenuItem";
            this.enableVisualStudioDebugToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.enableVisualStudioDebugToolStripMenuItem.Text = "Enable Visual Studio Debug";
            this.enableVisualStudioDebugToolStripMenuItem.ToolTipText = "If Enabled, the host code is built with debug information for an external debugge" +
    "r.  Warning, this leaves temp files behind.";
            // 
            // kernelExportToolStripMenuItem
            // 
            this.kernelExportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveKernelToFileToolStripMenuItem,
            this.displayKernelsBinaryToolStripMenuItem});
            this.kernelExportToolStripMenuItem.Name = "kernelExportToolStripMenuItem";
            this.kernelExportToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.kernelExportToolStripMenuItem.Text = "Export Kernel";
            // 
            // saveKernelToFileToolStripMenuItem
            // 
            this.saveKernelToFileToolStripMenuItem.Name = "saveKernelToFileToolStripMenuItem";
            this.saveKernelToFileToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.saveKernelToFileToolStripMenuItem.Text = "Save to \'KernelBinary.bin\'";
            this.saveKernelToFileToolStripMenuItem.Click += new System.EventHandler(this.saveKernelToFileToolStripMenuItem_Click);
            // 
            // displayKernelsBinaryToolStripMenuItem
            // 
            this.displayKernelsBinaryToolStripMenuItem.Name = "displayKernelsBinaryToolStripMenuItem";
            this.displayKernelsBinaryToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.displayKernelsBinaryToolStripMenuItem.Text = "Show Binary in Notepad";
            this.displayKernelsBinaryToolStripMenuItem.Click += new System.EventHandler(this.displayKernelsBinaryToolStripMenuItem_Click);
            // 
            // exportDummyKernelToolStripMenuItem
            // 
            this.exportDummyKernelToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveDummyBinToFileToolStripMenuItem,
            this.showDummyBinaryToolStripMenuItem});
            this.exportDummyKernelToolStripMenuItem.Name = "exportDummyKernelToolStripMenuItem";
            this.exportDummyKernelToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.exportDummyKernelToolStripMenuItem.Text = "Export Dummy Kernel";
            // 
            // saveDummyBinToFileToolStripMenuItem
            // 
            this.saveDummyBinToFileToolStripMenuItem.Name = "saveDummyBinToFileToolStripMenuItem";
            this.saveDummyBinToFileToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.saveDummyBinToFileToolStripMenuItem.Text = "Save to \'DummyBinary.bin\'";
            this.saveDummyBinToFileToolStripMenuItem.Click += new System.EventHandler(this.saveDummyBinToFileToolStripMenuItem_Click);
            // 
            // showDummyBinaryToolStripMenuItem
            // 
            this.showDummyBinaryToolStripMenuItem.Name = "showDummyBinaryToolStripMenuItem";
            this.showDummyBinaryToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.showDummyBinaryToolStripMenuItem.Text = "Show Binary in Notepad";
            this.showDummyBinaryToolStripMenuItem.Click += new System.EventHandler(this.showDummyBinaryToolStripMenuItem_Click);
            // 
            // showSpecsInNotepadToolStripMenuItem
            // 
            this.showSpecsInNotepadToolStripMenuItem.Name = "showSpecsInNotepadToolStripMenuItem";
            this.showSpecsInNotepadToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.showSpecsInNotepadToolStripMenuItem.Text = "Show GPU Specs in Notepad";
            this.showSpecsInNotepadToolStripMenuItem.ToolTipText = "Outputs some general GPU information.";
            this.showSpecsInNotepadToolStripMenuItem.Click += new System.EventHandler(this.showSpecsInNotepadToolStripMenuItem_Click);
            // 
            // creditsToolStripMenuItem
            // 
            this.creditsToolStripMenuItem.Name = "creditsToolStripMenuItem";
            this.creditsToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.creditsToolStripMenuItem.Text = "Credits...";
            this.creditsToolStripMenuItem.Click += new System.EventHandler(this.creditsToolStripMenuItem_Click);
            // 
            // toolStripDropDownButtonEditor
            // 
            this.toolStripDropDownButtonEditor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButtonEditor.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoCtrlZToolStripMenuItem,
            this.redoCtrlRToolStripMenuItem,
            this.toolStripSeparator4,
            this.findCtrlFToolStripMenuItem,
            this.findNextF3ToolStripMenuItem,
            this.replaceCtrlHToolStripMenuItem,
            this.gotoNextCharAltFcharToolStripMenuItem,
            this.toolStripSeparator5,
            this.forwardNavigationShiftCtrlToolStripMenuItem,
            this.backwardNavigationCtrlToolStripMenuItem,
            this.gotoLineCtrlGToolStripMenuItem,
            this.goToFirstCharOfWordCtrlHomeToolStripMenuItem,
            this.goToLastCharOfWordCtrlEndToolStripMenuItem,
            this.toolStripSeparator6,
            this.selectAllCtrlAToolStripMenuItem,
            this.increaseIndentShiftTabToolStripMenuItem,
            this.cutCurrentLineShiftDelToolStripMenuItem,
            this.toolStripSeparator7,
            this.recordStopMacroCtrlMToolStripMenuItem,
            this.executeMacroCtrlEToolStripMenuItem,
            this.toolStripSeparator8,
            this.zoomInCtrlToolStripMenuItem,
            this.zoomOutCtrlToolStripMenuItem,
            this.zoom100Ctrl0ToolStripMenuItem,
            this.zoomCtrlWheelToolStripMenuItem,
            this.toolStripSeparator9,
            this.addBookmarkCtrlBToolStripMenuItem,
            this.removeBookmarkCtrlShiftBToolStripMenuItem,
            this.nextBookmarkCtrlNToolStripMenuItem,
            this.prevBookmarkCtrlShiftNToolStripMenuItem});
            this.toolStripDropDownButtonEditor.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButtonEditor.Image")));
            this.toolStripDropDownButtonEditor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButtonEditor.Name = "toolStripDropDownButtonEditor";
            this.toolStripDropDownButtonEditor.Size = new System.Drawing.Size(60, 22);
            this.toolStripDropDownButtonEditor.Text = "Editor...";
            // 
            // undoCtrlZToolStripMenuItem
            // 
            this.undoCtrlZToolStripMenuItem.Name = "undoCtrlZToolStripMenuItem";
            this.undoCtrlZToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.undoCtrlZToolStripMenuItem.Tag = "^z";
            this.undoCtrlZToolStripMenuItem.Text = "Undo (Ctrl-Z)";
            this.undoCtrlZToolStripMenuItem.Click += new System.EventHandler(this.undoCtrlZToolStripMenuItem_Click);
            // 
            // redoCtrlRToolStripMenuItem
            // 
            this.redoCtrlRToolStripMenuItem.Name = "redoCtrlRToolStripMenuItem";
            this.redoCtrlRToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.redoCtrlRToolStripMenuItem.Tag = "^r";
            this.redoCtrlRToolStripMenuItem.Text = "Redo (Ctrl-R)";
            this.redoCtrlRToolStripMenuItem.Click += new System.EventHandler(this.redoCtrlRToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(268, 6);
            // 
            // findCtrlFToolStripMenuItem
            // 
            this.findCtrlFToolStripMenuItem.Name = "findCtrlFToolStripMenuItem";
            this.findCtrlFToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.findCtrlFToolStripMenuItem.Tag = "^f";
            this.findCtrlFToolStripMenuItem.Text = "Find (Ctrl-F)";
            this.findCtrlFToolStripMenuItem.Click += new System.EventHandler(this.findCtrlFToolStripMenuItem_Click);
            // 
            // findNextF3ToolStripMenuItem
            // 
            this.findNextF3ToolStripMenuItem.Name = "findNextF3ToolStripMenuItem";
            this.findNextF3ToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.findNextF3ToolStripMenuItem.Tag = "{F3}";
            this.findNextF3ToolStripMenuItem.Text = "Find Next (F3)";
            this.findNextF3ToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // replaceCtrlHToolStripMenuItem
            // 
            this.replaceCtrlHToolStripMenuItem.Name = "replaceCtrlHToolStripMenuItem";
            this.replaceCtrlHToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.replaceCtrlHToolStripMenuItem.Tag = "^h";
            this.replaceCtrlHToolStripMenuItem.Text = "Replace (Ctrl-H)";
            this.replaceCtrlHToolStripMenuItem.Click += new System.EventHandler(this.replaceCtrlHToolStripMenuItem_Click);
            // 
            // gotoNextCharAltFcharToolStripMenuItem
            // 
            this.gotoNextCharAltFcharToolStripMenuItem.Name = "gotoNextCharAltFcharToolStripMenuItem";
            this.gotoNextCharAltFcharToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.gotoNextCharAltFcharToolStripMenuItem.Text = "Find Next Char (Alt-F, [char])";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(268, 6);
            // 
            // forwardNavigationShiftCtrlToolStripMenuItem
            // 
            this.forwardNavigationShiftCtrlToolStripMenuItem.Name = "forwardNavigationShiftCtrlToolStripMenuItem";
            this.forwardNavigationShiftCtrlToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.forwardNavigationShiftCtrlToolStripMenuItem.Tag = "^+-";
            this.forwardNavigationShiftCtrlToolStripMenuItem.Text = "Forward Navigation (Shift+Ctrl-";
            this.forwardNavigationShiftCtrlToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // backwardNavigationCtrlToolStripMenuItem
            // 
            this.backwardNavigationCtrlToolStripMenuItem.Name = "backwardNavigationCtrlToolStripMenuItem";
            this.backwardNavigationCtrlToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.backwardNavigationCtrlToolStripMenuItem.Tag = "^-";
            this.backwardNavigationCtrlToolStripMenuItem.Text = "Backward Navigation (Ctrl-)";
            this.backwardNavigationCtrlToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // gotoLineCtrlGToolStripMenuItem
            // 
            this.gotoLineCtrlGToolStripMenuItem.Name = "gotoLineCtrlGToolStripMenuItem";
            this.gotoLineCtrlGToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.gotoLineCtrlGToolStripMenuItem.Tag = "^g";
            this.gotoLineCtrlGToolStripMenuItem.Text = "Goto Line (Ctrl-G)";
            this.gotoLineCtrlGToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // goToFirstCharOfWordCtrlHomeToolStripMenuItem
            // 
            this.goToFirstCharOfWordCtrlHomeToolStripMenuItem.Name = "goToFirstCharOfWordCtrlHomeToolStripMenuItem";
            this.goToFirstCharOfWordCtrlHomeToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.goToFirstCharOfWordCtrlHomeToolStripMenuItem.Tag = "^{HOME}";
            this.goToFirstCharOfWordCtrlHomeToolStripMenuItem.Text = "Go To First Char of word (Ctrl-Home)";
            this.goToFirstCharOfWordCtrlHomeToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // goToLastCharOfWordCtrlEndToolStripMenuItem
            // 
            this.goToLastCharOfWordCtrlEndToolStripMenuItem.Name = "goToLastCharOfWordCtrlEndToolStripMenuItem";
            this.goToLastCharOfWordCtrlEndToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.goToLastCharOfWordCtrlEndToolStripMenuItem.Tag = "^{END}";
            this.goToLastCharOfWordCtrlEndToolStripMenuItem.Text = "Go To Last Char of word (Ctrl-End)";
            this.goToLastCharOfWordCtrlEndToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(268, 6);
            // 
            // selectAllCtrlAToolStripMenuItem
            // 
            this.selectAllCtrlAToolStripMenuItem.Name = "selectAllCtrlAToolStripMenuItem";
            this.selectAllCtrlAToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.selectAllCtrlAToolStripMenuItem.Tag = "^a";
            this.selectAllCtrlAToolStripMenuItem.Text = "Select All (Ctrl-A)";
            this.selectAllCtrlAToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // increaseIndentShiftTabToolStripMenuItem
            // 
            this.increaseIndentShiftTabToolStripMenuItem.Name = "increaseIndentShiftTabToolStripMenuItem";
            this.increaseIndentShiftTabToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.increaseIndentShiftTabToolStripMenuItem.Tag = "+{TAB}";
            this.increaseIndentShiftTabToolStripMenuItem.Text = "Increase Indent (Shift-Tab)";
            this.increaseIndentShiftTabToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // cutCurrentLineShiftDelToolStripMenuItem
            // 
            this.cutCurrentLineShiftDelToolStripMenuItem.Name = "cutCurrentLineShiftDelToolStripMenuItem";
            this.cutCurrentLineShiftDelToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.cutCurrentLineShiftDelToolStripMenuItem.Tag = "+{DEL}";
            this.cutCurrentLineShiftDelToolStripMenuItem.Text = "Cut current line (Shift-Del)";
            this.cutCurrentLineShiftDelToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(268, 6);
            // 
            // recordStopMacroCtrlMToolStripMenuItem
            // 
            this.recordStopMacroCtrlMToolStripMenuItem.Name = "recordStopMacroCtrlMToolStripMenuItem";
            this.recordStopMacroCtrlMToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.recordStopMacroCtrlMToolStripMenuItem.Tag = "m";
            this.recordStopMacroCtrlMToolStripMenuItem.Text = "Record/Stop Macro (Ctrl-M)";
            this.recordStopMacroCtrlMToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // executeMacroCtrlEToolStripMenuItem
            // 
            this.executeMacroCtrlEToolStripMenuItem.Name = "executeMacroCtrlEToolStripMenuItem";
            this.executeMacroCtrlEToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.executeMacroCtrlEToolStripMenuItem.Tag = "^e";
            this.executeMacroCtrlEToolStripMenuItem.Text = "Execute Macro (Ctrl-E)";
            this.executeMacroCtrlEToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(268, 6);
            // 
            // zoomInCtrlToolStripMenuItem
            // 
            this.zoomInCtrlToolStripMenuItem.Name = "zoomInCtrlToolStripMenuItem";
            this.zoomInCtrlToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.zoomInCtrlToolStripMenuItem.Tag = "^+";
            this.zoomInCtrlToolStripMenuItem.Text = "Zoom-In (Ctrl+)";
            this.zoomInCtrlToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // zoomOutCtrlToolStripMenuItem
            // 
            this.zoomOutCtrlToolStripMenuItem.Name = "zoomOutCtrlToolStripMenuItem";
            this.zoomOutCtrlToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.zoomOutCtrlToolStripMenuItem.Tag = "^-";
            this.zoomOutCtrlToolStripMenuItem.Text = "Zoom-Out (Ctrl-)";
            this.zoomOutCtrlToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // zoom100Ctrl0ToolStripMenuItem
            // 
            this.zoom100Ctrl0ToolStripMenuItem.Name = "zoom100Ctrl0ToolStripMenuItem";
            this.zoom100Ctrl0ToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.zoom100Ctrl0ToolStripMenuItem.Tag = "^0";
            this.zoom100Ctrl0ToolStripMenuItem.Text = "Zoom 100% (Ctrl-0)";
            this.zoom100Ctrl0ToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // zoomCtrlWheelToolStripMenuItem
            // 
            this.zoomCtrlWheelToolStripMenuItem.Name = "zoomCtrlWheelToolStripMenuItem";
            this.zoomCtrlWheelToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.zoomCtrlWheelToolStripMenuItem.Text = "Zoom (Ctrl-MouseWheel)";
            this.zoomCtrlWheelToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(268, 6);
            // 
            // addBookmarkCtrlBToolStripMenuItem
            // 
            this.addBookmarkCtrlBToolStripMenuItem.Name = "addBookmarkCtrlBToolStripMenuItem";
            this.addBookmarkCtrlBToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.addBookmarkCtrlBToolStripMenuItem.Tag = "^b";
            this.addBookmarkCtrlBToolStripMenuItem.Text = "Add Bookmark (Ctrl-B)";
            this.addBookmarkCtrlBToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // removeBookmarkCtrlShiftBToolStripMenuItem
            // 
            this.removeBookmarkCtrlShiftBToolStripMenuItem.Name = "removeBookmarkCtrlShiftBToolStripMenuItem";
            this.removeBookmarkCtrlShiftBToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.removeBookmarkCtrlShiftBToolStripMenuItem.Tag = "^+b";
            this.removeBookmarkCtrlShiftBToolStripMenuItem.Text = "Remove Bookmark (Ctrl-Shift-B)";
            this.removeBookmarkCtrlShiftBToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // nextBookmarkCtrlNToolStripMenuItem
            // 
            this.nextBookmarkCtrlNToolStripMenuItem.Name = "nextBookmarkCtrlNToolStripMenuItem";
            this.nextBookmarkCtrlNToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.nextBookmarkCtrlNToolStripMenuItem.Tag = "^n";
            this.nextBookmarkCtrlNToolStripMenuItem.Text = "Next Bookmark (Ctrl-N)";
            this.nextBookmarkCtrlNToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // prevBookmarkCtrlShiftNToolStripMenuItem
            // 
            this.prevBookmarkCtrlShiftNToolStripMenuItem.Name = "prevBookmarkCtrlShiftNToolStripMenuItem";
            this.prevBookmarkCtrlShiftNToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.prevBookmarkCtrlShiftNToolStripMenuItem.Tag = "^+n";
            this.prevBookmarkCtrlShiftNToolStripMenuItem.Text = "Prev Bookmark (Ctrl-Shift-N)";
            this.prevBookmarkCtrlShiftNToolStripMenuItem.Click += new System.EventHandler(this.SendKeysToControl);
            // 
            // txtOutput
            // 
            this.txtOutput.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.autocompleteMenu1.SetAutocompleteMenu(this.txtOutput, null);
            this.txtOutput.AutoScrollMinSize = new System.Drawing.Size(0, 14);
            this.txtOutput.BackBrush = null;
            this.txtOutput.CharHeight = 14;
            this.txtOutput.CharWidth = 8;
            this.txtOutput.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtOutput.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.txtOutput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtOutput.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.txtOutput.IsReplaceMode = false;
            this.txtOutput.Location = new System.Drawing.Point(0, 830);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.Paddings = new System.Windows.Forms.Padding(0);
            this.txtOutput.ReadOnly = true;
            this.txtOutput.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.txtOutput.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("txtOutput.ServiceColors")));
            this.txtOutput.ShowLineNumbers = false;
            this.txtOutput.Size = new System.Drawing.Size(723, 80);
            this.txtOutput.TabIndex = 3;
            this.txtOutput.WordWrap = true;
            this.txtOutput.Zoom = 100;
            this.txtOutput.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.txtOutput_TextChanged);
            this.txtOutput.Enter += new System.EventHandler(this.setUserControlAsCurrent);
            // 
            // splitter1
            // 
            this.splitter1.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 820);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(723, 10);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl1.Controls.Add(this.tabHost);
            this.tabControl1.Controls.Add(this.tabGCN);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(6, 1);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(723, 820);
            this.tabControl1.TabIndex = 5;
            // 
            // tabHost
            // 
            this.tabHost.Controls.Add(this.txtHost);
            this.tabHost.Location = new System.Drawing.Point(4, 23);
            this.tabHost.Name = "tabHost";
            this.tabHost.Padding = new System.Windows.Forms.Padding(3);
            this.tabHost.Size = new System.Drawing.Size(715, 793);
            this.tabHost.TabIndex = 1;
            this.tabHost.Text = "C# Host";
            this.tabHost.UseVisualStyleBackColor = true;
            // 
            // txtHost
            // 
            this.txtHost.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.autocompleteMenu1.SetAutocompleteMenu(this.txtHost, null);
            this.txtHost.AutoIndentCharsPatterns = "\n^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);\n";
            this.txtHost.AutoScrollMinSize = new System.Drawing.Size(627, 840);
            this.txtHost.BackBrush = null;
            this.txtHost.BracketsHighlightStrategy = FastColoredTextBoxNS.BracketsHighlightStrategy.Strategy2;
            this.txtHost.ChangedLineColor = System.Drawing.Color.Cornsilk;
            this.txtHost.CharHeight = 14;
            this.txtHost.CharWidth = 8;
            this.txtHost.CurrentLineColor = System.Drawing.Color.LemonChiffon;
            this.txtHost.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtHost.DelayedEventsInterval = 500;
            this.txtHost.DelayedTextChangedInterval = 1000;
            this.txtHost.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.txtHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtHost.HighlightingRangeType = FastColoredTextBoxNS.HighlightingRangeType.VisibleRange;
            this.txtHost.IsReplaceMode = false;
            this.txtHost.Language = FastColoredTextBoxNS.Language.CSharp;
            this.txtHost.LeftBracket = '(';
            this.txtHost.LeftBracket2 = '{';
            this.txtHost.Location = new System.Drawing.Point(3, 3);
            this.txtHost.Name = "txtHost";
            this.txtHost.Paddings = new System.Windows.Forms.Padding(0);
            this.txtHost.RightBracket = ')';
            this.txtHost.RightBracket2 = '}';
            this.txtHost.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.txtHost.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("txtHost.ServiceColors")));
            this.txtHost.ShowFoldingLines = true;
            this.txtHost.Size = new System.Drawing.Size(709, 787);
            this.txtHost.TabIndex = 2;
            this.txtHost.Text = resources.GetString("txtHost.Text");
            this.txtHost.Zoom = 100;
            this.txtHost.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.txtHost_TextChanged);
            this.txtHost.Enter += new System.EventHandler(this.setUserControlAsCurrent);
            // 
            // tabGCN
            // 
            this.tabGCN.Controls.Add(this.txtAsm);
            this.tabGCN.Location = new System.Drawing.Point(4, 23);
            this.tabGCN.Name = "tabGCN";
            this.tabGCN.Padding = new System.Windows.Forms.Padding(3);
            this.tabGCN.Size = new System.Drawing.Size(715, 793);
            this.tabGCN.TabIndex = 0;
            this.tabGCN.Text = "GCN";
            this.tabGCN.UseVisualStyleBackColor = true;
            // 
            // txtAsm
            // 
            this.txtAsm.AutoCompleteBrackets = true;
            this.txtAsm.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.autocompleteMenu1.SetAutocompleteMenu(this.txtAsm, this.autocompleteMenu1);
            this.txtAsm.AutoScrollMinSize = new System.Drawing.Size(715, 658);
            this.txtAsm.BackBrush = null;
            this.txtAsm.ChangedLineColor = System.Drawing.Color.Cornsilk;
            this.txtAsm.CharHeight = 14;
            this.txtAsm.CharWidth = 8;
            this.txtAsm.CurrentLineColor = System.Drawing.Color.LemonChiffon;
            this.txtAsm.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtAsm.DelayedEventsInterval = 500;
            this.txtAsm.DelayedTextChangedInterval = 1000;
            this.txtAsm.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.txtAsm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAsm.HighlightingRangeType = FastColoredTextBoxNS.HighlightingRangeType.VisibleRange;
            this.txtAsm.IsReplaceMode = false;
            this.txtAsm.LeftBracket = '{';
            this.txtAsm.LeftBracket2 = '[';
            this.txtAsm.Location = new System.Drawing.Point(3, 3);
            this.txtAsm.Name = "txtAsm";
            this.txtAsm.Paddings = new System.Windows.Forms.Padding(0);
            this.txtAsm.RightBracket = '}';
            this.txtAsm.RightBracket2 = ']';
            this.txtAsm.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.txtAsm.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("txtAsm.ServiceColors")));
            this.txtAsm.ShowFoldingLines = true;
            this.txtAsm.Size = new System.Drawing.Size(709, 787);
            this.txtAsm.TabIndex = 1;
            this.txtAsm.Text = resources.GetString("txtAsm.Text");
            this.txtAsm.Zoom = 100;
            this.txtAsm.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.txtAsm_TextChanged);
            this.txtAsm.SelectionChangedDelayed += new System.EventHandler(this.txtAsm_SelectionChangedDelayed);
            this.txtAsm.Enter += new System.EventHandler(this.setUserControlAsCurrent);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "cs";
            this.saveFileDialog1.Filter = "C# file (*.cs)|*.cs|All files (*.*)|*.*";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "cs";
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "C# file (*.cs)|*.cs|All files (*.*)|*.*";
            // 
            // printDialog1
            // 
            this.printDialog1.UseEXDialog = true;
            // 
            // autocompleteMenu1
            // 
            this.autocompleteMenu1.Colors = ((AutocompleteMenuNS.Colors)(resources.GetObject("autocompleteMenu1.Colors")));
            this.autocompleteMenu1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.autocompleteMenu1.ImageList = this.imageList1;
            this.autocompleteMenu1.Items = new string[0];
            this.autocompleteMenu1.MaximumSize = new System.Drawing.Size(270, 400);
            this.autocompleteMenu1.MinFragmentLength = 3;
            this.autocompleteMenu1.TargetControlWrapper = null;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "s.png");
            this.imageList1.Images.SetKeyName(1, "v.png");
            this.imageList1.Images.SetKeyName(2, "DS.png");
            this.imageList1.Images.SetKeyName(3, "Bu.png");
            this.imageList1.Images.SetKeyName(4, "Clear.png");
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.FileName = "openFileDialog2";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(723, 932);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtOutput)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabHost.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtHost)).EndInit();
            this.tabGCN.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtAsm)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsNew;
        private System.Windows.Forms.ToolStripButton tsOpen;
        private System.Windows.Forms.ToolStripButton tsSave;
        private System.Windows.Forms.ToolStripButton tsPrint;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripButton cutToolStripButton;
        private System.Windows.Forms.ToolStripButton copyToolStripButton;
        private System.Windows.Forms.ToolStripButton pasteToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton codeProjectToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsRun;
        private FastColoredTextBoxNS.FastColoredTextBox txtOutput;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGCN;
        private FastColoredTextBoxNS.FastColoredTextBox txtAsm;
        private System.Windows.Forms.TabPage tabHost;
        private FastColoredTextBoxNS.FastColoredTextBox txtHost;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Windows.Forms.ToolStripButton saveAsToolStripButton;
        private AutocompleteMenuNS.AutocompleteMenu autocompleteMenu1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButtonMore;
        private System.Windows.Forms.ToolStripMenuItem openOutputFolderInExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem kernelExportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveKernelToFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayKernelsBinaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showSpecsInNotepadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportDummyKernelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveDummyBinToFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showDummyBinaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableVisualStudioDebugToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private System.Windows.Forms.ToolStripMenuItem creditsToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButtonEditor;
        private System.Windows.Forms.ToolStripMenuItem undoCtrlZToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoCtrlRToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem findCtrlFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findNextF3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceCtrlHToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gotoLineCtrlGToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem selectAllCtrlAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem increaseIndentShiftTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem goToFirstCharOfWordCtrlHomeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem goToLastCharOfWordCtrlEndToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backwardNavigationCtrlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem forwardNavigationShiftCtrlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zoomCtrlWheelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutCurrentLineShiftDelToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem recordStopMacroCtrlMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem executeMacroCtrlEToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem zoomInCtrlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zoomOutCtrlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zoom100Ctrl0ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gotoNextCharAltFcharToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem addBookmarkCtrlBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeBookmarkCtrlShiftBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nextBookmarkCtrlNToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem prevBookmarkCtrlShiftNToolStripMenuItem;
    }
}

