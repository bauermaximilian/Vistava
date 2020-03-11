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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Vistava.Library
{
    public class Account : ICloneable
    {
        public const string XmlNodeRoot = "account";
        private const string XmlAttributeUsername = "name";
        private const string XmlNodePasswordHash = "passwordHash";
        private const string XmlNodeRootDirectory = "rootDirectory";

        private static readonly SHA256Managed hashProvider =
            new SHA256Managed();

        public bool HasExistingRootDirectory
        {
            get
            {
                return Directory.Exists(RootDirectory);
            }
        }

        public string Username { get; }

        public string PasswordHash { get; private set; }

        public string RootDirectory { get; private set; }

        public void ToXml(XmlWriter writer)
        {
            writer.WriteStartElement(XmlNodeRoot);
            writer.WriteAttributeString(XmlAttributeUsername,
                Username);
            writer.WriteElementString(XmlNodePasswordHash,
                PasswordHash);
            writer.WriteElementString(XmlNodeRootDirectory,
                RootDirectory);
            writer.WriteEndElement();
        }

        public static Account FromXml(XmlNode xmlNode)
        {
            if (xmlNode == null)
                throw new ArgumentNullException(nameof(xmlNode));

            string username, passwordHash, rootDirectory;

            username =
                xmlNode.Attributes[XmlAttributeUsername]?.Value
                ?? throw new XmlException("The " + XmlAttributeUsername
                + " attribute of the " + XmlNodeRoot
                + " node was missing.");
            passwordHash =
                xmlNode[XmlNodePasswordHash]?.InnerText
                ?? throw new XmlException("The " + XmlNodePasswordHash
                + " node was missing.");
            rootDirectory =
                xmlNode[XmlNodeRootDirectory]?.InnerText
                ?? throw new XmlException("The " + XmlNodeRootDirectory
                + " node was missing.");

            try
            {
                return new Account(username, rootDirectory)
                {
                    PasswordHash = passwordHash
                };
            }
            catch (Exception exc)
            {
                throw new XmlException("The " + XmlNodeRoot + " node " +
                    "contained invalid values. " + exc.Message, exc);
            }
        }

        private Account(string username, string rootDirectory)
        {
            Username = username ??
                throw new ArgumentNullException(nameof(username));
            if (username.Trim().Length == 0)
                throw new ArgumentException("The specified username is " +
                    "empty or consist of whitespaces only.");

            SetRootDirectory(rootDirectory);
        }

        public Account(string username, string password, string rootDirectory)
            : this(username, rootDirectory)
        {
            SetPassword(password);   
        }

        public void SetRootDirectory(string rootDirectory)
        {
            if (rootDirectory == null)
                throw new ArgumentNullException(nameof(rootDirectory));
            if (rootDirectory.Trim().Length == 0)
                throw new ArgumentException("The specified root path is " +
                    "empty or consist of whitespaces only.");

            RootDirectory = rootDirectory;
        }

        public void SetPassword(string password)
        {
            try { PasswordHash = GeneratePasswordHash(password); }
            catch (ArgumentNullException) { throw; }
            catch (ArgumentException) { throw; }            
        }

        public bool TryAuthenticate(string password)
        {
            try { return GeneratePasswordHash(password) == PasswordHash; }
            catch (ArgumentNullException) { throw; }
            catch (ArgumentException) { return false; }
        }

        private static string GeneratePasswordHash(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            else if (password.Trim().Length == 0)
                throw new ArgumentException("The password " +
                    "must not be empty or consist of whitespaces only.");

            byte[] passwordBytes = Encoding.Default.GetBytes(password);
            byte[] passwordHash = hashProvider.ComputeHash(passwordBytes);
            return BitConverter.ToString(passwordHash).Replace("-", "");
        }

        public object Clone()
        {
            return new Account(Username, RootDirectory)
            {
                PasswordHash = PasswordHash
            };
        }
    }
}
