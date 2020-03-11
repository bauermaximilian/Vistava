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
using System.Windows.Forms;
using Vistava.Library;

namespace Vistava
{
    public partial class UserEditDialog : Form
    {
        private const string PasswordPlaceholder = "●●●●●●●●";

        public Account Account { get; private set; } = null;

        public UserEditDialog()
        {
            InitializeComponent();
        }

        public UserEditDialog(Account account)
        {
            InitializeComponent();

            Account = account ?? 
                throw new ArgumentNullException(nameof(account));

            usernameBox.Text = account.Username;
            usernameBox.ReadOnly = true;
            passwordBox.Text = PasswordPlaceholder; 
            rootDirectoryBox.Text = Account.RootDirectory;
        }        

        private void passwordBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (passwordBox.Text == PasswordPlaceholder)
                passwordBox.Text = "";
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Account == null)
                {
                    Account = new Account(usernameBox.Text,
                        passwordBox.Text,
                        rootDirectoryBox.Text);
                }
                else
                {
                    if (passwordBox.Text != PasswordPlaceholder)
                        Account.SetPassword(passwordBox.Text);
                    Account.SetRootDirectory(rootDirectoryBox.Text);
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exc)
            {
                MessageBox.Show("The specified parameters were invalid. "
                    + exc.Message, "Invalid parameters", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void selectDirectoryButton_Click(object sender, EventArgs e)
        {
            if (rootDirectoryBrowserDialog.ShowDialog() == DialogResult.OK)
                rootDirectoryBox.Text = 
                    rootDirectoryBrowserDialog.SelectedPath;
        }
    }
}
