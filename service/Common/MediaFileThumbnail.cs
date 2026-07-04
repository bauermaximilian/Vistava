// SPDX-License-Identifier: GPL-3.0-or-later

using LiteDB;

namespace Vistava.Service.Common;

public record MediaFileThumbnail
{
    [BsonId]
    public required string Path { get; init; }
    
    public required DateTime LastModified { get; init; }
    
    public byte[]? ThumbnailData { get; init; }
}