// SPDX-License-Identifier: GPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Vistava.Service.Common;

public static partial class PlaylistImporter
{
    [GeneratedRegex(@"[^,]+$")]
    private static partial Regex M3UDirectiveLabelSegment();

    public static string[] SupportedFileExtensions => new string[]
    {
        ".m3u", ".m3u8", ".xspf"
    };

    public static async Task<IEnumerable<PlaylistItem>> ImportPlaylistAsync(string filePath,
        CancellationToken cancellationToken)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".m3u" => await ImportM3uPlaylistAsync(filePath, cancellationToken),
            ".m3u8" => await ImportM3u8PlaylistAsync(filePath, cancellationToken),
            ".xspf" => ImportXspfPlaylist(filePath),
            _ => throw new NotSupportedException("The specified playlist format is " +
                "not supported."),
        };
    }

    public static IEnumerable<PlaylistItem> ImportXspfPlaylist(string filePath)
    {
        XmlDocument document = new();
        document.Load(filePath);

        List<PlaylistItem> playlistItems = new();

        var trackNodes = document.GetElementsByTagName("track");
        foreach (XmlNode trackNode in trackNodes)
        {
            var locationUriString = trackNode["location"]?.InnerText ?? null;
            if (locationUriString == null)
            {
                continue;
            }
            var locationUri = new Uri(locationUriString);
            if (!locationUri.IsFile)
            {
                continue;
            }

            var locationPath = Uri.UnescapeDataString(locationUri.AbsolutePath)
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            var title = trackNode["title"]?.InnerText ?? Path.GetFileName(locationPath);

            playlistItems.Add(new PlaylistItem(title, locationPath));
        }

        return playlistItems;
    }

    public static async Task<IEnumerable<PlaylistItem>> ImportM3u8PlaylistAsync(
        string filePath, CancellationToken cancellationToken)
    {
        return await ImportM3uPlaylistAsync(filePath, Encoding.UTF8, cancellationToken);
    }

    public static async Task<IEnumerable<PlaylistItem>> ImportM3uPlaylistAsync(string filePath, 
        CancellationToken cancellationToken)
    {
        return await ImportM3uPlaylistAsync(filePath, Encoding.Default, cancellationToken);
    }

    private static async Task<IEnumerable<PlaylistItem>> ImportM3uPlaylistAsync(
        string filePath, Encoding encoding, CancellationToken cancellationToken)
    {
        const string M3UFileHeader = "#EXTM3U";

        using StreamReader fileReader = new(filePath, encoding);
        string rootPath;
        if (Path.IsPathRooted(filePath))
        {
            rootPath = Path.GetDirectoryName(filePath) ??
                AppDomain.CurrentDomain.BaseDirectory;
        }
        else
        {
            rootPath = AppDomain.CurrentDomain.BaseDirectory;
        }

        string? header = await fileReader.ReadLineAsync(cancellationToken);
        if (header != M3UFileHeader)
        {
            throw new FormatException("The file header was missing.");
        }

        List<PlaylistItem> playlistItems = new();

        List<string> lastDirectives = new();
        while (true)
        {
            string? line = await fileReader.ReadLineAsync(cancellationToken);
            if (line == null)
            {
                break;
            }
            else if (line.StartsWith('#'))
            {
                lastDirectives.Add(line);
            }
            else if (line.Contains("://"))
            {
                // Skips any web resources
                continue;
            }
            else
            {
                string path;
                string fileName;
                try
                {
                    line = Uri.UnescapeDataString(line);
                    path = Path.IsPathRooted(line) ? line : Path.Combine(rootPath, line);
                    path = path.Replace(Path.AltDirectorySeparatorChar, 
                        Path.DirectorySeparatorChar);
                    fileName = Path.GetFileName(path);
                    if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                    {
                        throw new FormatException("The file extension of the playlist item " +
                            "is missing.");
                    }
                } 
                catch (Exception exc)
                {
                    throw new FormatException("One or more playlist items were invalid.", exc);
                }

                if (TryParseM3UDirectives(lastDirectives, out var label))
                {
                    playlistItems.Add(new PlaylistItem(label, path));
                } 
                else
                {
                    playlistItems.Add(new PlaylistItem(fileName, path));
                }
                lastDirectives.Clear();                    
            }
        }

        return playlistItems;
    }

    private static bool TryParseM3UDirectives(IEnumerable<string> directives,
        [MaybeNullWhen(false)] out string label)
    {
        const string ExtinfDirective = "#EXTINF:";

        label = null;

        foreach (string directive in directives)
        {
            if (directive.StartsWith(ExtinfDirective))
            {
                var labelMatch = M3UDirectiveLabelSegment().Match(directive);

                if (labelMatch.Success)
                {
                    label = labelMatch.Value;
                }
            }
        }

        return label != null;
    }
}
