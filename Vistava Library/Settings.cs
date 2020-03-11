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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Vistava.Library
{
    public class Settings : IEnumerable<Account>, ICloneable
    {
        public const string XmlNodeRoot = "vistava";
        private const string XmlNodeVersion = "version";
        private const string XmlNodePort = "port";
        private const string XmlNodeSsl = "useSSL";
        private const string XmlNodeAutoStartServer = "autoStartServer";
        private const string XmlNodeShowBalloonTips = "showBalloonTips";
        private const string XmlNodeAccounts = "accounts";

        public static bool SettingsFileExists =>
            File.Exists(Common.ConfigurationFilePath);

        private readonly Dictionary<string, Account> accounts
            = new Dictionary<string, Account>();

        public int Port { get; set; } = 80;

        public bool UseSSL { get; set; } = false;

        public bool AutoStartServer { get; set; } = false;

        public bool ShowBalloonTips { get; set; } = true;

        public int AccountCount => accounts.Count;

        public string Prefix => 
            (UseSSL ? "https" : "http") + "://*:" + Port + "/";
        
        public Settings() { }

        public bool TryAuthenticateUser(string username, string password, 
            out string rootDirectory)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (accounts.TryGetValue(username, out Account account))
            {
                if (account.TryAuthenticate(password))
                {
                    rootDirectory = account.RootDirectory;
                    return true;
                }
            }

            rootDirectory = null;
            return false;
        }

        public void AddUser(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            if (accounts.ContainsKey(account.Username))
                throw new ArgumentException("An account with the same " +
                    "username already exists.");
            accounts.Add(account.Username, account);
        }

        public Account GetUser(string username)
        {
            if (TryGetUser(username, out Account account))
                return account;
            else throw new ArgumentException("No account with the specified " +
                "account exists.");
        }

        public bool TryGetUser(string username, out Account account)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            return accounts.TryGetValue(username, out account);
        }

        public bool DeleteUser(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            return accounts.Remove(account.Username);
        }

        public bool DeleteUser(string username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            return accounts.Remove(username);
        }

        public bool ContainsAccount(string username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            return accounts.ContainsKey(username);
        }

        public static Settings FromXml(XmlDocument document)
        {
            Settings configuration = new Settings();

            XmlNode root = document.DocumentElement;
            if (root.Name == XmlNodeRoot)
            {
                if (int.TryParse(root[XmlNodePort]?.InnerText, out int port))
                    configuration.Port = port;
                else throw new XmlException("The " + XmlNodePort +
                    " node was missing.");

                if (bool.TryParse(root[XmlNodeSsl]?.InnerText, 
                    out bool useSSL)) configuration.UseSSL = useSSL;
                else throw new XmlException("The " + XmlNodeSsl +
                    " node was missing.");

                if (bool.TryParse(root[XmlNodeAutoStartServer]?.InnerText,
                    out bool autoStartServer)) 
                    configuration.AutoStartServer = autoStartServer;
                else throw new XmlException("The " + XmlNodeAutoStartServer +
                    " node was missing.");

                if (bool.TryParse(root[XmlNodeShowBalloonTips]?.InnerText,
                    out bool showBalloonTips)) 
                    configuration.ShowBalloonTips = showBalloonTips;
                else throw new XmlException("The " + XmlNodeShowBalloonTips +
                    " node was missing.");

                XmlNode accountsNode = root[XmlNodeAccounts] ??
                    throw new XmlException("The " + XmlNodeAccounts +
                    " node was missing.");

                foreach (XmlNode accountNode in accountsNode)
                    if (accountNode.Name == Account.XmlNodeRoot)
                        configuration.AddUser(Account.FromXml(accountNode));
            }
            else throw new XmlException("The root node is invalid.");

            return configuration;
        }

        public void ToXml(XmlWriter writer)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement(XmlNodeRoot);
            writer.WriteAttributeString(XmlNodeVersion,
                Assembly.GetExecutingAssembly().GetName().Version.ToString(2));
            {
                writer.WriteElementString(XmlNodeAutoStartServer, 
                    AutoStartServer.ToString());
                writer.WriteElementString(XmlNodeShowBalloonTips,
                    ShowBalloonTips.ToString());
                writer.WriteElementString(XmlNodePort, Port.ToString());
                writer.WriteElementString(XmlNodeSsl, UseSSL.ToString());
                writer.WriteStartElement(XmlNodeAccounts);

                foreach (Account account in accounts.Values)
                    account.ToXml(writer);

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        public bool TrySave()
        {
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Indent = true,
                OmitXmlDeclaration = false
            };

            try
            {
                string configurationFilePath = Common.ConfigurationFilePath;
                string configurationFileFolder =
                    Path.GetDirectoryName(configurationFilePath);

                Directory.CreateDirectory(configurationFileFolder);
                using (XmlWriter writer = XmlWriter.Create(
                    configurationFilePath, settings))
                    ToXml(writer);

                return true;
            } 
            catch { return false; }
        }

        public static bool TryLoad(out Settings serverConfiguration)
        {
            try
            {
                if (!File.Exists(Common.ConfigurationFilePath))
                    throw new FileNotFoundException("The settings file " +
                        "doesn't exist.");

                XmlDocument document = new XmlDocument();
                document.Load(Common.ConfigurationFilePath);
                serverConfiguration = FromXml(document);

                return true;

            }
            catch (Exception exc)
            {
                serverConfiguration = new Settings();

                Debug.WriteLine("Error while loading settings file.\n" +
                    exc.Message);
                return false;
            }
        }

        public IEnumerator<Account> GetEnumerator()
        {
            return ((IEnumerable<Account>)accounts.Values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Account>)accounts.Values).GetEnumerator();
        }

        public object Clone()
        {
            Settings clone = new Settings
            {
                Port = Port
            };

            foreach (KeyValuePair<string, Account> account in accounts)
                clone.AddUser((Account)account.Value.Clone());

            return clone;
        }
    }
}
