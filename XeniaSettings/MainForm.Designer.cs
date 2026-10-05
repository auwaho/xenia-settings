namespace XeniaSettings
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadPatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.saveConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.savePatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.label2 = new System.Windows.Forms.Label();
            this.configSearchTextBox = new System.Windows.Forms.TextBox();
            this.configTreeView = new System.Windows.Forms.TreeView();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.valueTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.setValueButton = new System.Windows.Forms.Button();
            this.configDescRichTextBox = new System.Windows.Forms.RichTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.patchesSearchTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.patchesTreeView = new XeniaSettings.Utilities.PatchTreeView();
            this.patchDescRichTextBox = new System.Windows.Forms.RichTextBox();
            this.menuStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.Window;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(464, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.reloadConfigToolStripMenuItem,
            this.reloadPatchesToolStripMenuItem,
            this.toolStripSeparator2,
            this.saveConfigToolStripMenuItem,
            this.savePatchesToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.settingsToolStripMenuItem.Text = "File";
            // 
            // reloadConfigToolStripMenuItem
            // 
            this.reloadConfigToolStripMenuItem.Name = "reloadConfigToolStripMenuItem";
            this.reloadConfigToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.reloadConfigToolStripMenuItem.Text = "Reload config";
            this.reloadConfigToolStripMenuItem.Click += new System.EventHandler(this.reloadConfigToolStripMenuItem_Click);
            // 
            // reloadPatchesToolStripMenuItem
            // 
            this.reloadPatchesToolStripMenuItem.Name = "reloadPatchesToolStripMenuItem";
            this.reloadPatchesToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.reloadPatchesToolStripMenuItem.Text = "Reload patches";
            this.reloadPatchesToolStripMenuItem.Click += new System.EventHandler(this.reloadPatchesToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(151, 6);
            // 
            // saveConfigToolStripMenuItem
            // 
            this.saveConfigToolStripMenuItem.Name = "saveConfigToolStripMenuItem";
            this.saveConfigToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.saveConfigToolStripMenuItem.Text = "Save config";
            this.saveConfigToolStripMenuItem.Click += new System.EventHandler(this.saveConfigToolStripMenuItem_Click);
            // 
            // savePatchesToolStripMenuItem
            // 
            this.savePatchesToolStripMenuItem.Name = "savePatchesToolStripMenuItem";
            this.savePatchesToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.savePatchesToolStripMenuItem.Text = "Save patches";
            this.savePatchesToolStripMenuItem.Click += new System.EventHandler(this.savePatchesToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(151, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.aboutToolStripMenuItem.Text = "About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(0, 24);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(464, 657);
            this.tabControl1.TabIndex = 2;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer1);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage1.Size = new System.Drawing.Size(456, 629);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Config";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(5, 5);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Window;
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer4);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(446, 619);
            this.splitContainer1.SplitterDistance = 380;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer4.IsSplitterFixed = true;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.label2);
            this.splitContainer4.Panel1.Controls.Add(this.configSearchTextBox);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.configTreeView);
            this.splitContainer4.Size = new System.Drawing.Size(444, 378);
            this.splitContainer4.SplitterDistance = 29;
            this.splitContainer4.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Search setting:";
            // 
            // configSearchTextBox
            // 
            this.configSearchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.configSearchTextBox.Location = new System.Drawing.Point(93, 3);
            this.configSearchTextBox.Name = "configSearchTextBox";
            this.configSearchTextBox.Size = new System.Drawing.Size(348, 23);
            this.configSearchTextBox.TabIndex = 0;
            this.configSearchTextBox.TextChanged += new System.EventHandler(this.configSearchTextBox_TextChanged);
            // 
            // configTreeView
            // 
            this.configTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.configTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configTreeView.Location = new System.Drawing.Point(0, 0);
            this.configTreeView.Name = "configTreeView";
            this.configTreeView.Size = new System.Drawing.Size(444, 345);
            this.configTreeView.TabIndex = 0;
            this.configTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.configTreeView_AfterSelect);
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.valueTextBox);
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            this.splitContainer2.Panel1.Controls.Add(this.setValueButton);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer2.Panel2.Controls.Add(this.configDescRichTextBox);
            this.splitContainer2.Size = new System.Drawing.Size(446, 235);
            this.splitContainer2.SplitterDistance = 31;
            this.splitContainer2.TabIndex = 0;
            // 
            // valueTextBox
            // 
            this.valueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.valueTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.valueTextBox.Location = new System.Drawing.Point(93, 3);
            this.valueTextBox.Name = "valueTextBox";
            this.valueTextBox.Size = new System.Drawing.Size(348, 22);
            this.valueTextBox.TabIndex = 2;
            this.valueTextBox.TextChanged += new System.EventHandler(this.valueTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Current value:";
            // 
            // setValueButton
            // 
            this.setValueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.setValueButton.Location = new System.Drawing.Point(350, 3);
            this.setValueButton.Name = "setValueButton";
            this.setValueButton.Size = new System.Drawing.Size(91, 23);
            this.setValueButton.TabIndex = 0;
            this.setValueButton.Text = "Set value";
            this.setValueButton.UseVisualStyleBackColor = true;
            this.setValueButton.Visible = false;
            this.setValueButton.Click += new System.EventHandler(this.setValueButton_Click);
            // 
            // configDescRichTextBox
            // 
            this.configDescRichTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.configDescRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.configDescRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configDescRichTextBox.Location = new System.Drawing.Point(0, 0);
            this.configDescRichTextBox.Name = "configDescRichTextBox";
            this.configDescRichTextBox.ReadOnly = true;
            this.configDescRichTextBox.Size = new System.Drawing.Size(444, 198);
            this.configDescRichTextBox.TabIndex = 0;
            this.configDescRichTextBox.Text = "";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.splitContainer3);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage2.Size = new System.Drawing.Size(456, 629);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Patches";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            this.splitContainer3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(5, 5);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.BackColor = System.Drawing.SystemColors.Window;
            this.splitContainer3.Panel1.Controls.Add(this.splitContainer5);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer3.Panel2.Controls.Add(this.patchDescRichTextBox);
            this.splitContainer3.Size = new System.Drawing.Size(446, 619);
            this.splitContainer3.SplitterDistance = 415;
            this.splitContainer3.TabIndex = 0;
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer5.IsSplitterFixed = true;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.patchesSearchTextBox);
            this.splitContainer5.Panel1.Controls.Add(this.label3);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.patchesTreeView);
            this.splitContainer5.Size = new System.Drawing.Size(444, 413);
            this.splitContainer5.SplitterDistance = 29;
            this.splitContainer5.TabIndex = 0;
            // 
            // patchesSearchTextBox
            // 
            this.patchesSearchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.patchesSearchTextBox.Location = new System.Drawing.Point(122, 3);
            this.patchesSearchTextBox.Name = "patchesSearchTextBox";
            this.patchesSearchTextBox.Size = new System.Drawing.Size(319, 23);
            this.patchesSearchTextBox.TabIndex = 1;
            this.patchesSearchTextBox.TextChanged += new System.EventHandler(this.patchesSearchTextBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "Search patch/game:";
            // 
            // patchesTreeView
            // 
            this.patchesTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.patchesTreeView.CheckBoxes = true;
            this.patchesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.patchesTreeView.Location = new System.Drawing.Point(0, 0);
            this.patchesTreeView.Name = "patchesTreeView";
            this.patchesTreeView.HideSelection = false;
            this.patchesTreeView.Size = new System.Drawing.Size(444, 380);
            this.patchesTreeView.TabIndex = 0;
            this.patchesTreeView.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.patchesTreeView_BeforeCheck);
            this.patchesTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.patchesTreeView_AfterCheck);
            this.patchesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.patchesTreeView_AfterSelect);
            // 
            // patchDescRichTextBox
            // 
            this.patchDescRichTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.patchDescRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.patchDescRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.patchDescRichTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.patchDescRichTextBox.Location = new System.Drawing.Point(0, 0);
            this.patchDescRichTextBox.Name = "patchDescRichTextBox";
            this.patchDescRichTextBox.ReadOnly = true;
            this.patchDescRichTextBox.Size = new System.Drawing.Size(444, 198);
            this.patchDescRichTextBox.TabIndex = 0;
            this.patchDescRichTextBox.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(464, 681);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(300, 39);
            this.Name = "MainForm";
            this.Text = "Xenia Settings";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel1.PerformLayout();
            this.splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reloadConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reloadPatchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem saveConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem savePatchesToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TextBox valueTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button setValueButton;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox configSearchTextBox;
        private System.Windows.Forms.RichTextBox configDescRichTextBox;
        private System.Windows.Forms.TreeView configTreeView;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox patchesSearchTextBox;
        private System.Windows.Forms.TreeView patchesTreeView;
        private System.Windows.Forms.RichTextBox patchDescRichTextBox;
    }
}

