// SPDX-License-Identifier: GPL-3.0-or-later

namespace Vistava.Service.Contracts;

public interface IThumbnailProvider
{
    string ThumbnailMimeType { get; }

    Task<Stream> GetThumbnailAsync(string filePath, CancellationToken token);
}
