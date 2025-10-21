// SPDX-License-Identifier: GPL-3.0-or-later

namespace Vistava.Service.Common;

public class FileListEntry
{
    public string? Label { get; init; }

    public string? QueryTarget { get; init; }

    public string? IconName { get; init; }

    public string? MediaUrl { get; init; }

    public double? MediaDuration { get; init; }

    public string? MediaType { get; init; }

    public string? ThumbnailUrl { get; init; }

    public string? ThumbnailType { get; init; }

    public string? FileSystemPath { get; init; }
}
