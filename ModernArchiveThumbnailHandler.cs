using SharpShell.Attributes;
using SharpShell.SharpThumbnailHandler;
using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace ModernArchiveThumbnail.Handlers
{
    [ComVisible(true)]
    [Guid("9E8F7ABC-1234-4D22-9AA0-123456789111")]
    [COMServerAssociation(AssociationType.ClassOfExtension,
        ".cbz", ".cbr", ".zip", ".rar", ".7z", ".cb7", ".alz", ".egg")]
    public class ModernArchiveThumbnailHandler : SharpThumbnailHandler
    {
        private static readonly HashSet<string> ValidFormats = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp", ".tif", ".tiff", ".ico", ".heic", ".avif"
        };

        private static readonly string[] PriorityKeys =
        {
            "cover", "front", "folder", "000", "001"
        };

        [ThreadStatic]
        private static byte[] _buffer;

        static ModernArchiveThumbnailHandler()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                try
                {
                    var name = new AssemblyName(e.Name).Name;
                    var baseDir = Path.GetDirectoryName(typeof(ModernArchiveThumbnailHandler).Assembly.Location);
                    var path = Path.Combine(baseDir, name + ".dll");
                    if (File.Exists(path))
                        return Assembly.LoadFrom(path);
                }
                catch { }
                return null;
            };
        }

        protected override Bitmap GetThumbnailImage(uint width)
        {
            if (SelectedItemStream == null)
                return null;

            int deadline = Environment.TickCount + 2000;

            try
            {
                using (var archive = ArchiveFactory.Open(SelectedItemStream))
                {
                    int scanned = 0;

                    foreach (var entry in archive.Entries)
                    {
                        if (Environment.TickCount > deadline || scanned++ >= 50)
                            break;

                        if (entry.IsDirectory || entry.Size <= 0 || entry.Size > 50_000_000)
                            continue;

                        var path = entry.Key;
                        if (path.IndexOf("__MACOSX", StringComparison.OrdinalIgnoreCase) >= 0)
                            continue;

                        var ext = Path.GetExtension(path);
                        if (!ValidFormats.Contains(ext))
                            continue;

                        bool priority = false;
                        var name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                        foreach (var k in PriorityKeys)
                        {
                            if (name.StartsWith(k))
                            {
                                priority = true;
                                break;
                            }
                        }

                        int size = (int)entry.Size;
                        if (_buffer == null || _buffer.Length < size)
                            _buffer = new byte[Math.Min(size, 8_000_000)];

                        using (var ms = new MemoryStream(_buffer, 0, size, true, true))
                        {
                            entry.WriteTo(ms);
                            ms.Position = 0;

                            Bitmap bmp = DecodeGdi(ms, width);
                            if (bmp != null)
                                return bmp;

                            ms.Position = 0;
                            bmp = DecodeWpf(ms, width);
                            if (bmp != null)
                                return bmp;
                        }

                        if (priority)
                            break;
                    }
                }
            }
            catch { }
            finally
            {
                if (_buffer != null && _buffer.Length > 6_000_000)
                    _buffer = null;
            }

            return null;
        }

        private Bitmap DecodeGdi(Stream s, uint width)
        {
            try
            {
                using (var img = Image.FromStream(s, false, false))
                {
                    float r = (float)width / img.Width;
                    int h = Math.Max(1, (int)(img.Height * r));

                    var bmp = new Bitmap((int)width, h, PixelFormat.Format32bppArgb);
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                        g.DrawImage(img, 0, 0, (int)width, h);
                    }
                    return bmp;
                }
            }
            catch { return null; }
        }

        private Bitmap DecodeWpf(Stream s, uint width)
        {
            try
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = s;
                bi.DecodePixelWidth = (int)width;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                bi.Freeze();

                using (var ms = new MemoryStream())
                {
                    var enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bi));
                    enc.Save(ms);
                    return new Bitmap(ms);
                }
            }
            catch { return null; }
        }
    }
}
