// SPDX-License-Identifier: GPL-3.0-or-later

namespace Vistava.Service.Common;

public class FileListEntryHandle(Stream stream, string contentType) : IDisposable
{
	public Stream Stream { get; } = stream;

	public string ContentType { get; } = contentType;

	public void Dispose()
	{
		((IDisposable)Stream).Dispose();
		GC.SuppressFinalize(this);
	}
}
