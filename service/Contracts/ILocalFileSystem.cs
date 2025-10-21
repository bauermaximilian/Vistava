// SPDX-License-Identifier: GPL-3.0-or-later

using Vistava.Service.Common;

namespace Vistava.Service.Contracts;

public interface ILocalFileSystem
{
    Task<FileListEntryHandle> GetFileAsync(string path, CancellationToken token);
    Task<FileListEntryHandle> GetThumbnailAsync(string path, CancellationToken token);
    Task<IList<FileListEntry>> ListDirectoryAsync(string path, Uri baseHostUri, int start, int limit, 
        CancellationToken token);
    Task<IList<FileListEntry>> ListPlaylistAsync(string path, Uri baseHostUri, int start, int limit,
        CancellationToken token);
}
