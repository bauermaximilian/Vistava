namespace Vistava.GUI
{
    partial class ConnectionTroubleshootingWindowsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionTroubleshootingWindowsDialog));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.runUnregisterPrefixCommandButton = new System.Windows.Forms.Button();
            this.unregisterPrefixField = new System.Windows.Forms.TextBox();
            this.runRegisterPrefixCommandButton = new System.Windows.Forms.Button();
            this.registerPrefixField = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.okButton = new System.Windows.Forms.Button();
            this.firewallLabel = new System.Windows.Forms.Label();
            this.firewallRuleBox = new System.Windows.Forms.TextBox();
            this.runAddFirewallExceptionCommandButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.runRemoveFirewallExceptionCommandButton = new System.Windows.Forms.Button();
            this.firewallRemoveRuleBox = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(740, 285);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.runUnregisterPrefixCommandButton);
            this.tabPage1.Controls.Add(this.unregisterPrefixField);
            this.tabPage1.Controls.Add(this.runRegisterPrefixCommandButton);
            this.tabPage1.Controls.Add(this.registerPrefixField);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(732, 259);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Server initialisation";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(6, 153);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(720, 18);
            this.label2.TabIndex = 6;
            this.label2.Text = "To remove the registration again, use the following command:";
            // 
            // runUnregisterPrefixCommandButton
            // 
            this.runUnregisterPrefixCommandButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runUnregisterPrefixCommandButton.Location = new System.Drawing.Point(536, 201);
            this.runUnregisterPrefixCommandButton.Name = "runUnregisterPrefixCommandButton";
            this.runUnregisterPrefixCommandButton.Size = new System.Drawing.Size(185, 23);
            this.runUnregisterPrefixCommandButton.TabIndex = 5;
            this.runUnregisterPrefixCommandButton.Text = "&Run command as administrator";
            this.runUnregisterPrefixCommandButton.UseVisualStyleBackColor = true;
            this.runUnregisterPrefixCommandButton.Click += new System.EventHandler(this.runUnregisterPrefixCommandButton_Click);
            // 
            // unregisterPrefixField
            // 
            this.unregisterPrefixField.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.unregisterPrefixField.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.unregisterPrefixField.Location = new System.Drawing.Point(8, 174);
            this.unregisterPrefixField.Name = "unregisterPrefixField";
            this.unregisterPrefixField.ReadOnly = true;
            this.unregisterPrefixField.Size = new System.Drawing.Size(713, 21);
            this.unregisterPrefixField.TabIndex = 4;
            // 
            // runRegisterPrefixCommandButton
            // 
            this.runRegisterPrefixCommandButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runRegisterPrefixCommandButton.Location = new System.Drawing.Point(536, 114);
            this.runRegisterPrefixCommandButton.Name = "runRegisterPrefixCommandButton";
            this.runRegisterPrefixCommandButton.Size = new System.Drawing.Size(185, 23);
            this.runRegisterPrefixCommandButton.TabIndex = 3;
            this.runRegisterPrefixCommandButton.Text = "&Run command as administrator";
            this.runRegisterPrefixCommandButton.UseVisualStyleBackColor = true;
            this.runRegisterPrefixCommandButton.Click += new System.EventHandler(this.runRegisterPrefixCommandButton_Click);
            // 
            // registerPrefixField
            // 
            this.registerPrefixField.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.registerPrefixField.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.registerPrefixField.Location = new System.Drawing.Point(8, 87);
            this.registerPrefixField.Name = "registerPrefixField";
            this.registerPrefixField.ReadOnly = true;
            this.registerPrefixField.Size = new System.Drawing.Size(713, 21);
            this.registerPrefixField.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(6, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(720, 75);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.runRemoveFirewallExceptionCommandButton);
            this.tabPage2.Controls.Add(this.firewallRemoveRuleBox);
            this.tabPage2.Controls.Add(this.runAddFirewallExceptionCommandButton);
            this.tabPage2.Controls.Add(this.firewallRuleBox);
            this.tabPage2.Controls.Add(this.firewallLabel);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(732, 259);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Connection and firewall";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(588, 291);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(138, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // firewallLabel
            // 
            this.firewallLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.firewallLabel.Location = new System.Drawing.Point(6, 9);
            this.firewallLabel.Name = "firewallLabel";
            this.firewallLabel.Size = new System.Drawing.Size(716, 72);
            this.firewallLabel.TabIndex = 1;
            this.firewallLabel.Text = resources.GetString("firewallLabel.Text");
            // 
            // firewallRuleBox
            // 
            this.firewallRuleBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.firewallRuleBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.firewallRuleBox.Location = new System.Drawing.Point(8, 87);
            this.firewallRuleBox.Name = "firewallRuleBox";
            this.firewallRuleBox.ReadOnly = true;
            this.firewallRuleBox.Size = new System.Drawing.Size(713, 21);
            this.firewallRuleBox.TabIndex = 8;
            // 
            // runAddFirewallExceptionCommandButton
            // 
            this.runAddFirewallExceptionCommandButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runAddFirewallExceptionCommandButton.Location = new System.Drawing.Point(536, 114);
            this.runAddFirewallExceptionCommandButton.Name = "runAddFirewallExceptionCommandButton";
            this.runAddFirewallExceptionCommandButton.Size = new System.Drawing.Size(185, 23);
            this.runAddFirewallExceptionCommandButton.TabIndex = 11;
            this.runAddFirewallExceptionCommandButton.Text = "&Run command as administrator";
            this.runAddFirewallExceptionCommandButton.UseVisualStyleBackColor = true;
            this.runAddFirewallExceptionCommandButton.Click += new System.EventHandler(this.runAddFirewallExceptionCommandButton_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.Location = new System.Drawing.Point(6, 153);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(720, 17);
            this.label3.TabIndex = 14;
            this.label3.Text = "To remove the firewall exception again, use the following command:";
            // 
            // runRemoveFirewallExceptionCommandButton
            // 
            this.runRemoveFirewallExceptionCommandButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runRemoveFirewallExceptionCommandButton.Location = new System.Drawing.Point(536, 201);
            this.runRemoveFirewallExceptionCommandButton.Name = "runRemoveFirewallExceptionCommandButton";
            this.runRemoveFirewallExceptionCommandButton.Size = new System.Drawing.Size(185, 22);
            this.runRemoveFirewallExceptionCommandButton.TabIndex = 13;
            this.runRemoveFirewallExceptionCommandButton.Text = "&Run command as administrator";
            this.runRemoveFirewallExceptionCommandButton.UseVisualStyleBackColor = true;
            this.runRemoveFirewallExceptionCommandButton.Click += new System.EventHandler(this.runRemoveFirewallExceptionCommandButton_Click);
            // 
            // firewallRemoveRuleBox
            // 
            this.firewallRemoveRuleBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.firewallRemoveRuleBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.firewallRemoveRuleBox.Location = new System.Drawing.Point(8, 174);
            this.firewallRemoveRuleBox.Name = "firewallRemoveRuleBox";
            this.firewallRemoveRuleBox.ReadOnly = true;
            this.firewallRemoveRuleBox.Size = new System.Drawing.Size(713, 21);
            this.firewallRemoveRuleBox.TabIndex = 12;
            // 
            // ConnectionTroubleshootingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(738, 323);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConnectionTroubleshootingDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Vistava - Connection Troubleshooting";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox registerPrefixField;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button runRegisterPrefixCommandButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button runUnregisterPrefixCommandButton;
        private System.Windows.Forms.TextBox unregisterPrefixField;
        private System.Windows.Forms.Label firewallLabel;
        private System.Windows.Forms.TextBox firewallRuleBox;
        private System.Windows.Forms.Button runAddFirewallExceptionCommandButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button runRemoveFirewallExceptionCommandButton;
        private System.Windows.Forms.TextBox firewallRemoveRuleBox;
    }
}