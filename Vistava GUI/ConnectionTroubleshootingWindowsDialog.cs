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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;
using Vistava.Library;

namespace Vistava.GUI
{
    public partial class ConnectionTroubleshootingWindowsDialog : Form
    {
        private readonly Settings settings;

        public ConnectionTroubleshootingWindowsDialog()
        {
            InitializeComponent();

            if (!Settings.TryLoad(out settings))
            {
                MessageBox.Show("The current configuration is invalid. " +
                    "Please review your configuration and try again.",
                    "Invalid configuration", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                Close();
            }

            registerPrefixField.Text = "netsh http add urlacl url=" + 
                settings.Prefix + " user=\"" +
                WindowsIdentity.GetCurrent().Name + "\"";
            unregisterPrefixField.Text = "netsh http delete urlacl url=" +
                settings.Prefix;

            firewallLabel.Text = firewallLabel.Text.Replace("%PORT%",
                settings.Port.ToString());
            firewallRuleBox.Text = "netsh advfirewall firewall add rule " +
                "name=\"Vistava\" dir=in action=allow protocol=TCP " +
                "localport=" + settings.Port;

            firewallRemoveRuleBox.Text = "netsh advfirewall firewall delete " +
                "rule name=\"Vistava\" protocol=TCP " +
                "localport=" + settings.Port;
        }

        private void runRegisterPrefixCommandButton_Click(object sender, 
            EventArgs e)
        {
            RunCommand(registerPrefixField.Text);
        }

        private void RunCommand(string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/C " + command;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Verb = "runas";
            process.Start();
            process.WaitForExit();

            WindowState = FormWindowState.Minimized;
            WindowState = FormWindowState.Normal;
            BringToFront();
            if (process.ExitCode == 0)
            {
                MessageBox.Show("The command was executed successfully.",
                    "Information", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("The command execution failed (code " +
                    process.ExitCode + ").", "Error", MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
            }            
        }

        private void runUnregisterPrefixCommandButton_Click(object sender, 
            EventArgs e)
        {
            RunCommand(unregisterPrefixField.Text);
        }

        private void runAddFirewallExceptionCommandButton_Click(object sender, 
            EventArgs e)
        {
            RunCommand(firewallRuleBox.Text);
        }

        private void runRemoveFirewallExceptionCommandButton_Click(
            object sender, EventArgs e)
        {
            RunCommand(firewallRemoveRuleBox.Text);
        }
    }
}
