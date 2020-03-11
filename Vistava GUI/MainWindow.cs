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
using System.Windows.Forms;
using Vistava.Library;

namespace Vistava.GUI
{
    public partial class MainWindow : Form
    {
        private bool configurationDialogOpen = false;

        private readonly Server server = new Server();

        public MainWindow()
        {
            InitializeComponent();
            Visible = false;

            if (!Settings.SettingsFileExists)
            {
                ShowConfigurationDialog(ShowDefaultBalloonTooltip);
            }
            else if (Settings.TryLoad(out Settings settings))
            {
                if (settings.AutoStartServer)
                    StartServer(settings.ShowBalloonTips);
                else if (settings.ShowBalloonTips)
                    ShowDefaultBalloonTooltip();
            }

            RefreshWindowButtons();

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                trayIcon.Visible = false;
                ShowIcon = true;
            }
        }

        private void ShowDefaultBalloonTooltip()
        {
            ShowBalloonTooltip("The application was minimized to the tray. " +
                "To start or stop the server or change the application " +
                "settings, right-click onto the tray icon.");
        }

        private void ShowBalloonTooltip(string text)
        {
            trayIcon.ShowBalloonTip(3000, "Vistava", text, ToolTipIcon.Info);
        }

        private void ShowConfigurationDialog(Action dialogCallback = null)
        {
            if (!configurationDialogOpen)
            {
                var configurationDialog = new ConfigurationDialog();

                configurationDialogOpen = true;
                configurationDialog.FormClosed += (s, e)
                    => configurationDialogOpen = false;

                if (dialogCallback != null)
                {
                    configurationDialog.FormClosed += (s, e)
                        => dialogCallback();
                }

                configurationDialog.ShowDialog();
            }
        }

        private bool StartServer(bool showBalloonTooltip = true)
        {
            try
            {
                server.Start();
                RefreshWindowButtons();

                if (showBalloonTooltip)
                {
                    string message = "The server was started!";
                    if (server.TryGetAddress(out string address))
                        message += " To open the application in your " +
                            "network, you can use the following URL " +
                            "in your web browser: " + address;
                    ShowBalloonTooltip(message);
                }

                return true;
            }
            catch (Exception exc)
            {
                if (exc is InvalidOperationException)
                {
                    MessageBox.Show("The server couldn't be started.\n" +
                        exc.Message + "\nPlease verify that the " +
                        "configuration is correct, restart the program and " +
                        "try again.", "Server initialisation failure",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else if (exc is UnauthorizedAccessException)
                {
                    MessageBox.Show("The server couldn't be started.\n" +
                        exc.Message + "\nTry to run the program as " +
                        "administrator or use the \"Connection " +
                        "troubleshooting\" from the tray menu for more " +
                        "information.\"", "Server initialisation failure",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    MessageBox.Show("The server couldn't be started.\n" +
                        exc.Message + "\nPlease check if you have the " +
                        "newest version of Vistava and try again.",
                        "Server initialisation failure",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
                return false;
            }
        }

        private void RefreshWindowButtons()
        {
            startServerButton.Enabled =
                !(stopServerButton.Enabled = server.IsRunning);
        }

        private void StopServer(bool showBalloonTooltip)
        {
            server.Stop();
            RefreshWindowButtons();

            if (showBalloonTooltip)
            {
                ShowBalloonTooltip("The server was stopped. To restart " +
                    "the server or change the application settings, " +
                    "right-click onto the tray icon.");
            }
        }

        private void configureToolStripMenuItem_Click(object sender, 
            EventArgs e)
        {
            ShowConfigurationDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopServer(false);
            Close();
        }

        private void trayContextMenu_Opening(object sender, 
            System.ComponentModel.CancelEventArgs e)
        {
            startServerToolStripMenuItem.Enabled = 
                !(stopServerToolStripMenuItem.Enabled = server.IsRunning);

            e.Cancel = configurationDialogOpen;
        }

        private void startServerToolStripMenuItem_Click(object sender, 
            EventArgs e)
        {
            StartServer();
        }

        private void stopServerToolStripMenuItem_Click(object sender, 
            EventArgs e)
        {
            StopServer(true);
        }

        private void connectionTroubleshootingToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var dialog = new ConnectionTroubleshootingUnixDialog();
                dialog.ShowDialog();
            }
            else
            {
                var dialog = new ConnectionTroubleshootingWindowsDialog();
                dialog.ShowDialog();
            }
        }

        private void MainWindow_FormClosing(object sender, 
            FormClosingEventArgs e)
        {
            if (server.IsRunning)
            {
                e.Cancel = (e.CloseReason == CloseReason.UserClosing) &&
                    DialogResult.Yes != MessageBox.Show("The server is " +
                    "currently running. Are you sure you want to exit the " +
                    "application and stop the server with it?", "Confirmation",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                if (!e.Cancel) server.Stop();
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            Process.Start(@"ABOUT.txt");
        }
    }
}
