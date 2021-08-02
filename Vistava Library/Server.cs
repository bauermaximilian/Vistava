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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Web;
using System.Web.Script.Serialization;

namespace Vistava.Library
{
    public class Server : IDisposable
    {
        private class Session
        {
            public static TimeSpan MaximumSessionAge { get; }
                = TimeSpan.FromMinutes(30);

            public DateTime LastActionTime { get; private set; } 
                = DateTime.Now;

            public bool IsValid => (DateTime.Now - LastActionTime) 
                < MaximumSessionAge;

            public string RootDirectory { get; }

            public Session(string rootDirectory)
            {
                RootDirectory = rootDirectory ??
                    throw new ArgumentNullException(nameof(rootDirectory));
            }

            public void RefreshLastActionTime()
            {
                LastActionTime = DateTime.Now;
            }
        }

        private const string WebRoot = "public_html";

        private const string ApiPathRoot = "/api/";
        private const string ApiPathAuthenticate =
            ApiPathRoot + "authenticate";
        private const string ApiPathDirectories = 
            ApiPathRoot + "directories/";
        private const string ApiPathFiles = 
            ApiPathRoot + "files/";
        private const string ApiPathThumbnails = 
            ApiPathRoot + "thumbnails/";
        private const string ApiPathMetadata = 
            ApiPathRoot + "metadata/";
        private const string MimeTypeJson = "application/json";
        private HttpListener httpListener = null;

        private Settings settings;

        private readonly Dictionary<Guid, Session> sessions =
            new Dictionary<Guid, Session>();

        private DateTime lastSessionCacheCleanup = DateTime.Now;

        private static readonly JavaScriptSerializer jsonSerializer = 
            new JavaScriptSerializer();

        private static readonly TimeSpan sessionCacheCleanupTreshold =
            TimeSpan.FromMinutes(15);

        public bool IsRunning => httpListener != null &&
            httpListener.IsListening;

        public Server() { }

        public void Start()
        {
            if (IsRunning)
                throw new InvalidOperationException("The server is already " +
                    "running and can't be started again.");

            if (!Settings.SettingsFileExists)
                throw new InvalidOperationException("The configuration " +
                    "file for the server was missing.");

            if (!Settings.TryLoad(out settings))
                throw new InvalidOperationException("The configuration " +
                    "file was invalid and couldn't be loaded.");

            httpListener = new HttpListener();

            httpListener.Prefixes.Clear();
            try { httpListener.Prefixes.Add(settings.Prefix); }
            catch (Exception exc)
            {
                throw new ApplicationException("The generated prefix " +
                    "was invalid. " + exc.Message, exc);
            }

            try { httpListener.Start(); }
            catch (Exception exc)
            {
                throw new UnauthorizedAccessException("Starting a server " +
                    "at the specified port/with the specified prefix " +
                    "failed (" + exc.Message + ").", exc);
            }

            httpListener.BeginGetContext(OnRequest, null);
        }

        public void Stop()
        {
            if (IsRunning) httpListener.Stop();
        }

        private void CleanupSessionCache()
        {
            if ((DateTime.Now - lastSessionCacheCleanup) >
                sessionCacheCleanupTreshold)
            {
                List<Guid> outdatedSessionGuids = new List<Guid>();

                foreach (var session in sessions)
                {
                    if (!session.Value.IsValid)
                        outdatedSessionGuids.Add(session.Key);
                }

                foreach (Guid sessionGuid in outdatedSessionGuids)
                    sessions.Remove(sessionGuid);

                lastSessionCacheCleanup = DateTime.Now;

                Debug.WriteLine("Removed " + outdatedSessionGuids.Count +
                    " outdated sessions from cache.");
            }
        }

        public bool TryGetAddress(out string address)
        {
            address = null;
            if (settings == null) return false;

            string ipAddress = null;
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                string ipString = ip.ToString();
                if (ip.AddressFamily == AddressFamily.InterNetwork
                    && !ipString.EndsWith(".1"))
                {
                    ipAddress = ipString;
                    break;
                }
            }
            if (ipAddress == null) return false;

            address = (settings.UseSSL ? "https" : "http") + "://" + ipAddress
                + (settings.Port != 80 ? (":" + settings.Port) : "") + "/";
            return true;
        }

        private static bool TryCreateAbsolutePath(string absoluteRootPath,
            string relativeUrl, out string absolutePath)
        {
            try
            {
                absolutePath = Path.Combine(absoluteRootPath,
                    relativeUrl.Replace('/', Path.DirectorySeparatorChar));
                return absolutePath.Contains(absoluteRootPath);
            }
            catch
            {
                absolutePath = null;
                return false;
            }
        }

        private bool TryAuthenticate(HttpListenerRequest request,
            out string rootDirectory)
        {
            Cookie sessionCookie = request.Cookies["sessionId"];
            if (sessionCookie != null)
            {
                if (Guid.TryParse(sessionCookie.Value, out Guid sessionId)
                    && sessions.TryGetValue(sessionId, out Session session))
                {
                    rootDirectory = session.RootDirectory;
                    session.RefreshLastActionTime();
                    return true;
                }
            }
            rootDirectory = null;
            return false;
        }

        private static void WriteJsonResponse(HttpListenerResponse response, 
            int statusCode, object data)
        {
            response.ContentType = MimeTypeJson;

            string jsonString;
            try
            {
                jsonString = jsonSerializer.Serialize(data);
                response.StatusCode = statusCode;
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Failure while creating JSON response. " +
                    exc.Message);
                response.StatusCode = 500;
                jsonString = "{\"status\":\"failure\"}";
            }

            using (StreamWriter writer = new StreamWriter(
                response.OutputStream))
                writer.WriteLine(jsonString);
        }

        private static void WriteHtmlResponse(HttpListenerResponse response,
            int statusCode, string mainMessage, string subMessage = null)
        {
            response.ContentType = "text/html";
            response.StatusCode = statusCode;

            using (StreamWriter writer = new StreamWriter(
                response.OutputStream))
            {
                writer.WriteLine("<!doctype html>" +
                    "<html><head><meta charset=\"utf - 8\">" +
                    "<title>" + mainMessage + "</title>" +
                    "<style>" +
                    "*{text-align: center;" +
                    "font-family: Arial, Helvetica, sans-serif;}" +
                    "</style></head>" +
                    "<body><h1>" + mainMessage + "</h1>");

                if (!string.IsNullOrEmpty(subMessage))
                {
                    writer.Write("<p>" + subMessage.Replace("\n", "<br>")
                        + "</p>");
                }

                writer.Write("<hr></hr><p>");

                AssemblyName assemblyName =
                    Assembly.GetExecutingAssembly().GetName();
                writer.Write(assemblyName.Name + " (" +
                    assemblyName.Version.ToString() + ")");

                writer.WriteLine("</p></body></html>");
            }
        }

        private void HandleFileTransfer(HttpListenerContext context,
            string requestedAbsoluteFilePath)
        {
            FileInfo requestedFile;
            try { requestedFile = new FileInfo(requestedAbsoluteFilePath); }
            catch
            {
                WriteHtmlResponse(context.Response, 404, "Error 404",
                    "File not found.");
                context.Response.Close();
                return;
            }

            string mimeType = MimeMapping.GetMimeMapping(requestedFile.Name);

            string rangeHeader = context.Request.Headers["Range"];
            if (!string.IsNullOrWhiteSpace(rangeHeader))
            {
                string[] rangeHeaderParts = rangeHeader.Split('=', '-');
                long fileSize = requestedFile.Length;
                long rangeEndMax = requestedFile.Length - 1;
                const int DefaultRangeSize = 1024 * 1024 * 8;

                if (rangeHeaderParts.Length == 3 &&
                    rangeHeaderParts[0] == "bytes" &&
                    long.TryParse(rangeHeaderParts[1], out long rangeStart))
                {
                    if (string.IsNullOrWhiteSpace(rangeHeaderParts[2]) ||
                        !long.TryParse(rangeHeaderParts[2], out long rangeEnd))
                        rangeEnd = Math.Min(rangeEndMax, 
                            rangeStart + DefaultRangeSize);

                    if (rangeStart >= 0 && rangeStart < rangeEndMax
                        && rangeEnd >= 0 && rangeEnd > rangeStart &&
                        rangeEnd <= rangeEndMax)
                    {
                        try
                        {
                            using (FileStream fileStream =
                                requestedFile.OpenRead())
                            {
                                context.Response.StatusCode = 206;
                                context.Response.AddHeader("Content-Range",
                                    "bytes " + rangeStart + "-" +
                                    rangeEnd + "/" + fileSize);
                                context.Response.ContentLength64 =
                                    (rangeEnd + 1) - rangeStart;
                                context.Response.ContentType = mimeType;

                                byte[] copyBuffer = new byte[1024];

                                fileStream.Position = rangeStart;
                                while (true)
                                {
                                    int targetReadBytesCount = (int)Math.Min(
                                        1024, (rangeEnd + 1) - 
                                        fileStream.Position);
                                    if (targetReadBytesCount <= 0) break;

                                    int readBytesCount = fileStream.Read(
                                        copyBuffer, 0, targetReadBytesCount);
                                    context.Response.OutputStream.Write(
                                        copyBuffer, 0, readBytesCount);
                                }

                                context.Response.Close();
                                return;
                            }
                        }
                        catch (Exception exc)
                        {
                            if (exc is HttpListenerException httpExc)
                            {
                                context.Response.Close();
                                return;
                            }
                            else
                            {
                                WriteHtmlResponse(context.Response, 500,
                                    "Error 500", "Resource access error.");
                                context.Response.Close();
                                return;
                            }
                        }
                    }
                    else
                    {
                        WriteHtmlResponse(context.Response, 416, "Error 416",
                            "Range Not Satisfiable.");
                        context.Response.Close();
                        return;
                    }
                }
                else
                {
                    WriteHtmlResponse(context.Response, 400, "Error 400",
                        "Invalid range header.");
                    context.Response.Close();
                    return;
                }
            }
            else
            {
                try
                {
                    using (FileStream fileStream = requestedFile.OpenRead())
                    {
                        context.Response.StatusCode = 200;
                        context.Response.ContentLength64 =
                            requestedFile.Length;
                        context.Response.ContentType = mimeType;
                        fileStream.CopyTo(context.Response.OutputStream);
                    }
                    context.Response.Close();
                    return;
                }
                catch
                {
                    WriteHtmlResponse(context.Response, 500, "Error 500",
                        "Resource access error.");
                    context.Response.Close();
                    return;
                }
            }
        }

        private void OnRequest(IAsyncResult result)
        {
            if (!httpListener.IsListening) return;

            HttpListenerContext context = httpListener.EndGetContext(result);
            httpListener.BeginGetContext(OnRequest, null);

            string localUrl = context.Request.Url.LocalPath;

            try
            {
                if (localUrl.StartsWith(ApiPathRoot))
                    HandleApiRequest(context);
                else
                    HandleNormalRequest(context);
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Unhandled exception: " + exc.Message);

                try
                {
                    WriteHtmlResponse(context.Response, 500, "Error 500",
                        "Internal server error.");
                }
                finally
                {
                    context.Response.Close();
                }
            }
        }

        private void HandleNormalRequest(HttpListenerContext context)
        {
            string url = context.Request.Url.LocalPath.TrimStart('/').Trim();
            if (url == "") url = "index.html";

            string absoluteWebRootPath = Path.Combine(Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location),
                WebRoot);

            if (!Directory.Exists(absoluteWebRootPath))
            {
                WriteHtmlResponse(context.Response, 500, "Error 500",
                    "The " + WebRoot + " folder was missing in the " +
                    "directory of the server library.");
                context.Response.Close();
            }
            else if (!TryCreateAbsolutePath(absoluteWebRootPath, url,
                out string absolutePath) || !File.Exists(absolutePath))
            {
                WriteHtmlResponse(context.Response, 404, "Error 404",
                    "The requested resource wasn't found.");
                context.Response.Close();
            }
            else HandleFileTransfer(context, absolutePath);
        }

        private void HandleApiRequestDirectory(HttpListenerContext context,
            string url, string userRootPath)
        {
            string directoryUrl = url.Substring(ApiPathDirectories.Length,
                url.Length - ApiPathDirectories.Length);

            if (TryCreateAbsolutePath(userRootPath, directoryUrl,
                out string absoluteDirectoryPath) &&
                Directory.Exists(absoluteDirectoryPath))
            {
                string ConvertPathToApiUrl(string absolutePath, 
                    bool isFilePath)
                {
                    string relativeDirectoryPath =
                        absolutePath.Substring(userRootPath.Length,
                        absolutePath.Length - userRootPath.Length);
                    string relativeDirectoryUrl =
                        relativeDirectoryPath.TrimStart(
                            Path.DirectorySeparatorChar)
                        .Replace(Path.DirectorySeparatorChar, '/');
                    string absoluteDirectoryUrl =
                        (isFilePath ? ApiPathFiles : ApiPathDirectories) +
                        relativeDirectoryUrl;
                    return absoluteDirectoryUrl;
                }

                try
                {
                    List<string> directories = new List<string>();
                    List<string> files = new List<string>();

                    foreach (string directory in 
                        Directory.EnumerateDirectories(absoluteDirectoryPath))
                        directories.Add(ConvertPathToApiUrl(directory, false));
                    foreach (string file in
                        Directory.EnumerateFiles(absoluteDirectoryPath))
                        files.Add(ConvertPathToApiUrl(file, true));

                    Dictionary<string, object> responseData = 
                        new Dictionary<string, object>
                    {
                        ["directories"] = directories,
                        ["files"] = files
                    };

                    WriteJsonResponse(context.Response, 200,
                        new Dictionary<string, object>
                        {
                            ["status"] = "success",
                            ["data"] = responseData
                        });
                }
                catch (Exception exc)
                {
                    Debug.WriteLine("Failure while enumerating directory. " +
                        exc.Message);
                    WriteJsonResponse(context.Response, 500,
                        new Dictionary<string, object>
                        {
                            ["status"] = "directory access failed"
                        });
                }
            }
            else
            {
                WriteJsonResponse(context.Response, 404,
                    new Dictionary<string, object>
                    {
                        ["status"] = "directory not found"
                    });
            }

            context.Response.Close();
        }

        private void HandleApiRequestFile(HttpListenerContext context,
            string url, string userRootPath)
        {
            string fileUrl = url.Substring(ApiPathFiles.Length,
                url.Length - ApiPathFiles.Length);

            if (TryCreateAbsolutePath(userRootPath, fileUrl,
                out string absoluteFileUrl) && File.Exists(absoluteFileUrl))
            {
                HandleFileTransfer(context, absoluteFileUrl);
            }
            else
            {
                WriteHtmlResponse(context.Response, 404, "Error 404",
                    "File not found");
            }

            context.Response.Close();
        }

        private void HandleApiRequestThumbnails(HttpListenerContext context,
            string url, string userRootPath)
        {
            string fileUrl = url.Substring(ApiPathThumbnails.Length,
                url.Length - ApiPathThumbnails.Length);

            WriteJsonResponse(context.Response, 501,
            new Dictionary<string, string>()
            {
                { "status", "API function not implemented." }
            });
            context.Response.Close();

            //https://stackoverflow.com/questions/1439719/c-sharp-get-thumbnail-from-file-via-windows-api
        }

        private void HandleApiRequestMetadata(HttpListenerContext context,
            string url, string userRootPath)
        {
            string fileUrl = url.Substring(ApiPathMetadata.Length,
                url.Length - ApiPathMetadata.Length);

            WriteJsonResponse(context.Response, 501,
            new Dictionary<string, string>()
            {
                { "status", "API function not implemented." }
            });
            context.Response.Close();
        }

        private void HandleApiRequestAuthentication(
            HttpListenerContext context)
        {
            if (context.Request.HttpMethod != "POST")
            {
                WriteJsonResponse(context.Response, 405,
                    new Dictionary<string, string>()
                    {
                        { "status", "Invalid HTTP method." }
                    });
                context.Response.Close();
                return;
            }

            string username, password;
            try
            {
                Dictionary<string, string> authentication;
                using (StreamReader reader = new StreamReader(
                    context.Request.InputStream))
                {
                    string authenticationString = reader.ReadToEnd();
                    authentication =
                        jsonSerializer.Deserialize<Dictionary<string, string>>(
                            authenticationString);
                    username = authentication["username"];
                    password = authentication["password"];
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Failed parsing client authentication " +
                    "request: " + exc.Message);

                WriteJsonResponse(context.Response, 400,
                    new Dictionary<string, string>()
                    {
                        { "status", "Invalid request." }
                    });
                context.Response.Close();
                return;
            }

            if (settings.TryAuthenticateUser(username, password,
                out string rootDirectory))
            {
                CleanupSessionCache();

                Guid sessionId = Guid.NewGuid();
                Session session = new Session(rootDirectory);
                sessions.Add(sessionId, session);

                context.Response.Cookies.Add(new Cookie("sessionId",
                    sessionId.ToString()));

                WriteJsonResponse(context.Response, 200,
                    new Dictionary<string, string>()
                    {
                        { "status", "Authentication successful - " +
                        "have a cookie." }
                    });
                context.Response.Close();
                return;
            }
            else
            {
                WriteJsonResponse(context.Response, 403,
                    new Dictionary<string, string>()
                    {
                        { "status", "Invalid combination of " +
                        "username and password." }
                    });
                context.Response.Close();
                return;
            }
        }

        private void HandleApiRequest(HttpListenerContext context)
        {
            string url = context.Request.Url.LocalPath.Trim();
            if (url == ApiPathRoot)
            {
                WriteHtmlResponse(context.Response, 200, "Vistava API",
                    "Coming soon");
            }
            else if (url == ApiPathAuthenticate)
                HandleApiRequestAuthentication(context);
            else
            {
                if (!TryAuthenticate(context.Request,
                    out string userRootPath))
                {
                    WriteJsonResponse(context.Response, 401,
                        new Dictionary<string, string>()
                        {
                            { "status", "No valid session provided." }
                        });
                    context.Response.Close();
                }
                else if (url.StartsWith(ApiPathDirectories))
                    HandleApiRequestDirectory(context, url, userRootPath);
                else if (url.StartsWith(ApiPathFiles))
                    HandleApiRequestFile(context, url, userRootPath);
                else if (url.StartsWith(ApiPathThumbnails))
                    HandleApiRequestThumbnails(context, url, userRootPath);
                else if (url.StartsWith(ApiPathMetadata))
                    HandleApiRequestMetadata(context, url, userRootPath);
                else
                {
                    WriteJsonResponse(context.Response, 404,
                    new Dictionary<string, string>()
                    {
                        { "status", "Unknown API function." }
                    });
                    context.Response.Close();
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
