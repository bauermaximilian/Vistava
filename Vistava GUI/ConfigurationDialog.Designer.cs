namespace Vistava.GUI
{
    partial class ConfigurationDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigurationDialog));
            this.accountView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.accountViewContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.editAccountButton = new System.Windows.Forms.Button();
            this.removeAccountButton = new System.Windows.Forms.Button();
            this.addAccountButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.prefixLabel = new System.Windows.Forms.Label();
            this.httpsCheckbox = new System.Windows.Forms.CheckBox();
            this.portBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.prefixTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.autoStartServerCheckbox = new System.Windows.Forms.CheckBox();
            this.showBalloonTipsCheckbox = new System.Windows.Forms.CheckBox();
            this.accountViewContextMenu.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // accountView
            // 
            this.accountView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.accountView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.accountView.ContextMenuStrip = this.accountViewContextMenu;
            this.accountView.FullRowSelect = true;
            this.accountView.GridLines = true;
            this.accountView.HideSelection = false;
            this.accountView.Location = new System.Drawing.Point(6, 19);
            this.accountView.Name = "accountView";
            this.accountView.Size = new System.Drawing.Size(469, 160);
            this.accountView.TabIndex = 0;
            this.accountView.UseCompatibleStateImageBehavior = false;
            this.accountView.View = System.Windows.Forms.View.Details;
            this.accountView.SelectedIndexChanged += new System.EventHandler(this.accountView_SelectedIndexChanged);
            this.accountView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.accountView_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "User";
            this.columnHeader1.Width = 94;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Root directory";
            this.columnHeader2.Width = 326;
            // 
            // accountViewContextMenu
            // 
            this.accountViewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNewToolStripMenuItem,
            this.removeToolStripMenuItem,
            this.editToolStripMenuItem});
            this.accountViewContextMenu.Name = "accountViewContextMenu";
            this.accountViewContextMenu.Size = new System.Drawing.Size(131, 70);
            // 
            // addNewToolStripMenuItem
            // 
            this.addNewToolStripMenuItem.Name = "addNewToolStripMenuItem";
            this.addNewToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.addNewToolStripMenuItem.Text = "&Add new...";
            this.addNewToolStripMenuItem.Click += new System.EventHandler(this.addAccountButton_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Enabled = false;
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.removeToolStripMenuItem.Text = "&Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeAccountButton_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Enabled = false;
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.editToolStripMenuItem.Text = "&Edit...";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editAccountButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.editAccountButton);
            this.groupBox1.Controls.Add(this.removeAccountButton);
            this.groupBox1.Controls.Add(this.addAccountButton);
            this.groupBox1.Controls.Add(this.accountView);
            this.groupBox1.Location = new System.Drawing.Point(12, 68);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(481, 214);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "User management";
            // 
            // editAccountButton
            // 
            this.editAccountButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.editAccountButton.Enabled = false;
            this.editAccountButton.Location = new System.Drawing.Point(6, 185);
            this.editAccountButton.Name = "editAccountButton";
            this.editAccountButton.Size = new System.Drawing.Size(100, 23);
            this.editAccountButton.TabIndex = 3;
            this.editAccountButton.Text = "&Edit...";
            this.editAccountButton.UseVisualStyleBackColor = true;
            this.editAccountButton.Click += new System.EventHandler(this.editAccountButton_Click);
            // 
            // removeAccountButton
            // 
            this.removeAccountButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.removeAccountButton.Enabled = false;
            this.removeAccountButton.Location = new System.Drawing.Point(229, 185);
            this.removeAccountButton.Name = "removeAccountButton";
            this.removeAccountButton.Size = new System.Drawing.Size(100, 23);
            this.removeAccountButton.TabIndex = 2;
            this.removeAccountButton.Text = "&Remove";
            this.removeAccountButton.UseVisualStyleBackColor = true;
            this.removeAccountButton.Click += new System.EventHandler(this.removeAccountButton_Click);
            // 
            // addAccountButton
            // 
            this.addAccountButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.addAccountButton.Location = new System.Drawing.Point(335, 185);
            this.addAccountButton.Name = "addAccountButton";
            this.addAccountButton.Size = new System.Drawing.Size(140, 23);
            this.addAccountButton.TabIndex = 1;
            this.addAccountButton.Text = "Add &new...";
            this.addAccountButton.UseVisualStyleBackColor = true;
            this.addAccountButton.Click += new System.EventHandler(this.addAccountButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.prefixLabel);
            this.groupBox2.Controls.Add(this.httpsCheckbox);
            this.groupBox2.Controls.Add(this.portBox);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(481, 50);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Connection settings";
            // 
            // prefixLabel
            // 
            this.prefixLabel.Cursor = System.Windows.Forms.Cursors.Help;
            this.prefixLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.prefixLabel.Location = new System.Drawing.Point(322, 17);
            this.prefixLabel.Name = "prefixLabel";
            this.prefixLabel.Size = new System.Drawing.Size(153, 23);
            this.prefixLabel.TabIndex = 9;
            this.prefixLabel.Text = "Prefix: https://*:65535/";
            this.prefixLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.prefixTooltip.SetToolTip(this.prefixLabel, resources.GetString("prefixLabel.ToolTip"));
            // 
            // httpsCheckbox
            // 
            this.httpsCheckbox.AutoSize = true;
            this.httpsCheckbox.Enabled = false;
            this.httpsCheckbox.Location = new System.Drawing.Point(198, 21);
            this.httpsCheckbox.Name = "httpsCheckbox";
            this.httpsCheckbox.Size = new System.Drawing.Size(84, 17);
            this.httpsCheckbox.TabIndex = 8;
            this.httpsCheckbox.Text = "Use HTTPS";
            this.httpsCheckbox.UseVisualStyleBackColor = true;
            this.httpsCheckbox.CheckedChanged += new System.EventHandler(this.httpsCheckbox_CheckedChanged);
            // 
            // portBox
            // 
            this.portBox.Location = new System.Drawing.Point(125, 18);
            this.portBox.Name = "portBox";
            this.portBox.Size = new System.Drawing.Size(67, 20);
            this.portBox.TabIndex = 7;
            this.portBox.Text = "65535";
            this.portBox.TextChanged += new System.EventHandler(this.portBox_TextChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(9, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 20);
            this.label1.TabIndex = 6;
            this.label1.Text = "Web server port: ";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(383, 342);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(110, 23);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(289, 342);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(88, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // prefixTooltip
            // 
            this.prefixTooltip.AutoPopDelay = 15000;
            this.prefixTooltip.InitialDelay = 500;
            this.prefixTooltip.ReshowDelay = 100;
            this.prefixTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.prefixTooltip.ToolTipTitle = "Information";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.showBalloonTipsCheckbox);
            this.groupBox3.Controls.Add(this.autoStartServerCheckbox);
            this.groupBox3.Location = new System.Drawing.Point(12, 288);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(481, 48);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Program settings";
            // 
            // autoStartServerCheckbox
            // 
            this.autoStartServerCheckbox.Location = new System.Drawing.Point(6, 19);
            this.autoStartServerCheckbox.Name = "autoStartServerCheckbox";
            this.autoStartServerCheckbox.Size = new System.Drawing.Size(164, 23);
            this.autoStartServerCheckbox.TabIndex = 0;
            this.autoStartServerCheckbox.Text = "Start server with application";
            this.prefixTooltip.SetToolTip(this.autoStartServerCheckbox, "If this checkbox is checked, the server is started automatically when the Vistava" +
        " application is started.");
            this.autoStartServerCheckbox.UseVisualStyleBackColor = true;
            this.autoStartServerCheckbox.CheckedChanged += new System.EventHandler(this.autoStartServerCheckbox_CheckedChanged);
            // 
            // showBalloonTipsCheckbox
            // 
            this.showBalloonTipsCheckbox.Location = new System.Drawing.Point(176, 19);
            this.showBalloonTipsCheckbox.Name = "showBalloonTipsCheckbox";
            this.showBalloonTipsCheckbox.Size = new System.Drawing.Size(226, 23);
            this.showBalloonTipsCheckbox.TabIndex = 1;
            this.showBalloonTipsCheckbox.Text = "Show balloon tips on application start";
            this.prefixTooltip.SetToolTip(this.showBalloonTipsCheckbox, "If this checkbox is checked, there will be a balloon tips when the program is. Un" +
        "check this to not display these balloon tips.");
            this.showBalloonTipsCheckbox.UseVisualStyleBackColor = true;
            this.showBalloonTipsCheckbox.CheckedChanged += new System.EventHandler(this.showBalloonTipsCheckbox_CheckedChanged);
            // 
            // ConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(505, 377);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "ConfigurationDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "vistava  - Configuration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigurationWindow_FormClosing);
            this.accountViewContextMenu.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView accountView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button editAccountButton;
        private System.Windows.Forms.Button removeAccountButton;
        private System.Windows.Forms.Button addAccountButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ContextMenuStrip accountViewContextMenu;
        private System.Windows.Forms.ToolStripMenuItem addNewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox portBox;
        private System.Windows.Forms.CheckBox httpsCheckbox;
        private System.Windows.Forms.Label prefixLabel;
        private System.Windows.Forms.ToolTip prefixTooltip;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox showBalloonTipsCheckbox;
        private System.Windows.Forms.CheckBox autoStartServerCheckbox;
    }
}

