/* 
 * Vistava - A media file server with a responsive web browser interface.
 * Copyright (C) 2020 Maximilian Bauer
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using Vistava.Library;

namespace Vistava.GUI
{
    public partial class ConfigurationDialog : Form
    {
        private readonly Settings settings;

        private bool unsavedChanges;

        public ConfigurationDialog()
        {
            InitializeComponent();

            if (!Settings.SettingsFileExists) 
                settings = new Settings();
            else if (!Settings.TryLoad(out settings))
                MessageBox.Show("The settings file was invalid or " +
                    "couldn't be loaded! The program is initialized with " +
                    "default settings instead - the new configuration will " +
                    "be written to disk after confirming the next " +
                    "configuration with \"OK\".", "Configuration failure",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

            portBox.Text = settings.Port.ToString();
            autoStartServerCheckbox.Checked = settings.AutoStartServer;
            showBalloonTipsCheckbox.Checked = settings.ShowBalloonTips;

            unsavedChanges = false;

            RefreshAccountView();
            RefreshPrefix();
        }

        private void RefreshAccountView()
        {
            accountView.Items.Clear();

            foreach (Account account in settings)
            {
                ListViewItem accountItem = new ListViewItem(new string[]
                {
                    account.Username,
                    account.RootDirectory
                });

                if (!account.HasExistingRootDirectory)
                    accountItem.ForeColor = Color.LightGray;

                accountView.Items.Add(accountItem);
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (SaveChanges()) Close();
        }

        private bool SaveChanges()
        {
            if (settings.AccountCount == 0)
            {
                if (MessageBox.Show("The current configuration doesn't " +
                    "contain any user accounts. While the server will start " +
                    "and run normally, no user will be able to log in and " +
                    "actually use it. Are you sure you want to continue?",
                    "No user accounts configured",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                    return false;
            }
            
            if (!HasValidPortValue(out _))
            {
                MessageBox.Show("The entered port value is no valid numeric " +
                    "value between 1 and 65535. Please add a valid value " +
                    "and try again.", "Invalid port", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }
            else
            {
                while (!settings.TrySave())
                {
                    if (DialogResult.Retry !=
                        MessageBox.Show("The settings file couldn't be " +
                        "written. Please make sure the file isn't " +
                        "write-protected or used by another application, " +
                        "you have enough free disk space and sufficient " +
                        "privileges to write into the application data " +
                        "folder and try again.", "File error", 
                        MessageBoxButtons.RetryCancel, MessageBoxIcon.Error))
                        return false;
                }

                unsavedChanges = false;
                return true;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void addAccountButton_Click(object sender, EventArgs e)
        {
            UserEditDialog newUserDialog = new UserEditDialog();

            if (newUserDialog.ShowDialog() == DialogResult.OK)
            {
                if (settings.ContainsAccount(newUserDialog.Account.Username))
                    MessageBox.Show("The specified username is already " +
                        "taken by another account and can't be added.",
                        "Invalid username", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                else
                {
                    settings.AddUser(newUserDialog.Account);
                    unsavedChanges = true;
                    RefreshAccountView();
                }
            }
        }

        private void accountView_SelectedIndexChanged(object sender, 
            EventArgs e)
        {
            if (accountView.SelectedItems.Count > 0)
            {
                editToolStripMenuItem.Enabled =
                    editAccountButton.Enabled =
                    accountView.SelectedItems.Count == 1;
                removeToolStripMenuItem.Enabled =
                    removeAccountButton.Enabled = true;
            }
            else
            {
                editToolStripMenuItem.Enabled =
                    editAccountButton.Enabled = false;
                removeToolStripMenuItem.Enabled =
                    removeAccountButton.Enabled = false;
            }
        }

        private void removeAccountButton_Click(object sender, EventArgs e)
        {
            if (accountView.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("Are you sure you want to remove the " +
                    "selected accounts?", "Confirmation",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Information) ==
                    DialogResult.OK)
                {

                    foreach (ListViewItem account in accountView.SelectedItems)
                        settings.DeleteUser(account.Text);

                    unsavedChanges = true;

                    RefreshAccountView();
                }
            }
        }

        private void editAccountButton_Click(object sender, EventArgs e)
        {
            if (accountView.SelectedItems.Count > 0)
            {
                string selectedAccountUsername = 
                    accountView.SelectedItems[0].Text;
                UserEditDialog editUserDialog = new UserEditDialog(
                    settings.GetUser(selectedAccountUsername));

                if (editUserDialog.ShowDialog() == DialogResult.OK)
                {
                    unsavedChanges = true;
                    RefreshAccountView();
                }
            }
        }

        private void ConfigurationWindow_FormClosing(object sender, 
            FormClosingEventArgs e)
        {
            if (unsavedChanges)
            {
                DialogResult unsavedChangesChoice =
                    MessageBox.Show("The configuration was modified. " +
                    "Do you want to save your changes?",
                    "Confirmation", MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Information);

                if (unsavedChangesChoice == DialogResult.Cancel)
                    e.Cancel = true;
                else if (unsavedChangesChoice == DialogResult.Yes)
                    e.Cancel = !SaveChanges();
            }
        }

        private void accountView_MouseDoubleClick(object sender, 
            MouseEventArgs e)
        {
            editAccountButton_Click(this, EventArgs.Empty);
        }

        private void firewallButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This application uses the kernel driver " +
                "for providing the HTTP service. If the server doesn't " +
                "start or can't be reached from the outside, try the " +
                "following:\n\n" +
                "1) Run the application as administrator.\n" +
                "For some ports (especially if the HTTP listener prefixes," +
                "which are generated with that port, haven't been " +
                "reserved for the current user), administrator " +
                "privileges are required to register these prefixes to " +
                "start the HTTP server at the configured port.\n\n" +
                "2) Add a firewall exception for the port of your context.\n" +
                "As this application builds upon the systems' HTTP service, " +
                "you will not get a firewall blocking notification - and " +
                "just adding a firewall exception for this servers' " +
                "executable won't work, as the operating system manages the " +
                "HTTP connection. So an exception needs to be added to the " +
                "firewall manually for incoming connections with protocol " +
                "TCP and the port you entered as part of the HTTP context. " +
                "Under Windows 10, this can be done in " +
                "\"Windows Defender Firewall with Advanced Security\" " +
                "(with command \"wf.msc\").", "Connection troubleshooting",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool HasValidPortValue(out int port)
        {
            string portBoxValue = portBox.Text.Trim().ToLowerInvariant();
            port = 0;
            return portBoxValue.Length > 0 &&
                int.TryParse(portBoxValue, out port) &&
                port > 0 && port < 65535;
        }

        private void portBox_TextChanged(object sender, EventArgs e)
        {
            if (HasValidPortValue(out int port))
            {
                portBox.BackColor = SystemColors.Window;
                settings.Port = port;
                unsavedChanges = true;
                RefreshPrefix();
            }
            else portBox.BackColor = Color.Red;
        }

        private void httpsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            settings.UseSSL = httpsCheckbox.Checked;
            RefreshPrefix();
        }

        private void RefreshPrefix()
        {
            prefixLabel.Text = "Prefix: " + settings.Prefix;
        }

        private void autoStartServerCheckbox_CheckedChanged(object sender,
            EventArgs e)
        {
            settings.AutoStartServer = autoStartServerCheckbox.Checked;
            unsavedChanges = true;
        }

        private void showBalloonTipsCheckbox_CheckedChanged(object sender, 
            EventArgs e)
        {
            settings.ShowBalloonTips = showBalloonTipsCheckbox.Checked;
            unsavedChanges = true;
        }
    }
}
