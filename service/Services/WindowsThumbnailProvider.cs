// SPDX-License-Identifier: GPL-3.0-or-later

// This code is based upon the work from Daniel Peñalba (https://stackoverflow.com/a/21752100) 
// and Snicker (https://stackoverflow.com/a/42178963).

using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Vistava.Service.Contracts;

namespace Vistava.Service.Services;

[Flags]
public enum ThumbnailOptions
{
    None = 0x00,
    BiggerSizeOk = 0x01,
    InMemoryOnly = 0x02,
    IconOnly = 0x04,
    ThumbnailOnly = 0x08,
    InCacheOnly = 0x10,
}

[SupportedOSPlatform("Windows")]
public class WindowsThumbnailProvider : IThumbnailProvider
{
    private const string IShellItem2Guid = "7E9FB0D3-919F-4307-AB2E-9B1860310C93";

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int SHCreateItemFromParsingName(
        [MarshalAs(UnmanagedType.LPWStr)] string path,
        // The following parameter is not used - binding context.
        nint pbc,
        ref Guid riid,
        [MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(nint hObject);

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
    private interface IShellItem
    {
        void BindToHandler(nint pbc,
            [MarshalAs(UnmanagedType.LPStruct)] Guid bhid,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            out nint ppv);

        void GetParent(out IShellItem ppsi);
        void GetDisplayName(SIGDN sigdnName, out nint ppszName);
        void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
        void Compare(IShellItem psi, uint hint, out int piOrder);
    };

    private enum SIGDN : uint
    {
        NORMALDISPLAY = 0,
        PARENTRELATIVEPARSING = 0x80018001,
        PARENTRELATIVEFORADDRESSBAR = 0x8001c001,
        DESKTOPABSOLUTEPARSING = 0x80028000,
        PARENTRELATIVEEDITING = 0x80031001,
        DESKTOPABSOLUTEEDITING = 0x8004c000,
        FILESYSPATH = 0x80058000,
        URL = 0x80068000
    }

    private enum HResult
    {
        Ok = 0x0000,
        False = 0x0001,
        InvalidArguments = unchecked((int)0x80070057),
        OutOfMemory = unchecked((int)0x8007000E),
        NoInterface = unchecked((int)0x80004002),
        Fail = unchecked((int)0x80004005),
        ElementNotFound = unchecked((int)0x80070490),
        TypeElementNotFound = unchecked((int)0x8002802B),
        NoObject = unchecked((int)0x800401E5),
        Win32ErrorCanceled = 1223,
        Canceled = unchecked((int)0x800704C7),
        ResourceInUse = unchecked((int)0x800700AA),
        AccessDenied = unchecked((int)0x80030005)
    }

    [ComImport()]
    [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItemImageFactory
    {
        [PreserveSig]
        HResult GetImage(
        [In, MarshalAs(UnmanagedType.Struct)] NativeSize size,
        [In] ThumbnailOptions flags,
        [Out] out nint phbm);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSize
    {
        private int width;
        private int height;

        public int Width { set { width = value; } }
        public int Height { set { height = value; } }
    };

    public string ThumbnailMimeType { get; } = "image/jpeg";

    public Task<Stream> GetThumbnailAsync(string filePath, CancellationToken token)
    {
        MemoryStream memoryStream = new();
        try
        {
            using Bitmap thumbnailBitmap = GetThumbnail(filePath, 200, 200, ThumbnailOptions.BiggerSizeOk);
            thumbnailBitmap.Save(memoryStream, ImageFormat.Jpeg);
            memoryStream.Position = 0;
            return Task.FromResult((Stream)memoryStream);
        }
        catch
        {
            memoryStream.Dispose();
            throw new FileNotFoundException();
        }
    }

    private static Bitmap GetThumbnail(string path, int width, int height, ThumbnailOptions options)
    {
        nint hBitmap = GetHBitmap(path, width, height, options);

        try
        {
            // return a System.Drawing.Bitmap from the hBitmap
            return Image.FromHbitmap(hBitmap);
        }
        finally
        {
            // delete HBitmap to avoid memory leaks
            DeleteObject(hBitmap);
        }
    }

    private static nint GetHBitmap(string fileName, int width, int height,
        ThumbnailOptions options)
    {
        IShellItem nativeShellItem;
        Guid shellItem2Guid = new Guid(IShellItem2Guid);
        int retCode = SHCreateItemFromParsingName(fileName, nint.Zero, ref shellItem2Guid,
            out nativeShellItem);

        if (retCode != 0)
            throw Marshal.GetExceptionForHR(retCode) ?? new Exception();

        NativeSize nativeSize = new NativeSize();
        nativeSize.Width = width;
        nativeSize.Height = height;

        nint hBitmap;
        HResult hr = ((IShellItemImageFactory)nativeShellItem).GetImage(nativeSize, options,
            out hBitmap);

        Marshal.ReleaseComObject(nativeShellItem);

        if (hr == HResult.Ok) return hBitmap;

        throw Marshal.GetExceptionForHR((int)hr) ?? new Exception();
    }
}
