using SharpShell.Attributes;
using SharpShell.SharpThumbnailHandler;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace ModernArchiveThumbnail.Handlers
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".cbz", ".cbr", ".zip", ".rar", ".7z", ".cb7")]
    public class ModernArchiveThumbnailHandler : SharpThumbnailHandler
    {
        private static readonly HashSet<string> ValidFormats = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".jpe", ".jfif", ".png", ".bmp",
            ".webp", ".heic", ".heif", ".avif", ".gif", ".tiff", ".tif", ".ico"
        };

        private static readonly string[] PriorityKeys = { "cover", "front", "folder", "001", "p00" };

        [ThreadStatic]
        private static byte[] _buffer;

        static ModernArchiveThumbnailHandler()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                try
                {
                    var name = new AssemblyName(args.Name).Name;
                    var folder = Path.GetDirectoryName(typeof(ModernArchiveThumbnailHandler).Assembly.Location);
                    var path = Path.Combine(folder, name + ".dll");
                    if (File.Exists(path)) return Assembly.LoadFrom(path);
                }
                catch { }
                return null;
            };
        }

        protected override Bitmap GetThumbnailImage(uint width)
        {
            var timeout = Environment.TickCount + 2000;

            try
            {
                if (SelectedItemStream == null) return null;

                if (!IsValidArchiveHeader(SelectedItemStream)) return null;

                using (var reader = ReaderFactory.Open(SelectedItemStream))
                {
                    int scanCount = 0;

                    while (reader.MoveToNextEntry())
                    {
                        if (Environment.TickCount > timeout) break;
                        if (scanCount++ >= 25) break;

                        var entry = reader.Entry;
                        if (entry.IsDirectory || entry.Size <= 0 || entry.Size > 20971520) continue;

                        string ext = Path.GetExtension(entry.Key);
                        if (string.IsNullOrEmpty(ext) || !ValidFormats.Contains(ext)) continue;

                        string fileName = Path.GetFileNameWithoutExtension(entry.Key).ToLower();
                        bool isPriority = PriorityKeys.Any(k => fileName.Contains(k)) || char.IsDigit(fileName[0]);

                        try
                        {
                            long size = entry.Size;
                            if (size > int.MaxValue) continue;

                            int sz = (int)size;
                            if (_buffer == null || _buffer.Length < sz)
                                _buffer = new byte[sz];

                            using (var ms = new MemoryStream(_buffer, 0, sz, true, true))
                            {
                                reader.WriteEntryTo(ms);
                                ms.Position = 0;

                                var result = DecodeGdi(ms, width);
                                if (result != null) return result;

                                if (Environment.TickCount > timeout - 500) continue;
                                ms.Position = 0;
                                result = DecodeWpf(ms, width);
                                if (result != null) return result;
                            }

                            if (isPriority) break;
                        }
                        catch { continue; }
                    }
                }
            }
            catch { }

            return null;
        }

        private bool IsValidArchiveHeader(Stream stream)
        {
            try
            {
                if (stream.Length < 4) return false;
                byte[] header = new byte[4];
                long pos = stream.Position;
                stream.Read(header, 0, 4);
                stream.Position = pos;

                if (header[0] == 0x50 && header[1] == 0x4B) return true;
                if (header[0] == 0x52 && header[1] == 0x61 && header[2] == 0x72 && header[3] == 0x21) return true;
                if (header[0] == 0x37 && header[1] == 0x7A && header[2] == 0xBC && header[3] == 0xAF) return true;

                return true;
            }
            catch { return true; }
        }

        private Bitmap DecodeGdi(MemoryStream ms, uint width)
        {
            try
            {
                using (var img = System.Drawing.Image.FromStream(ms, false, false))
                {
                    if (img.Width > 16384 || img.Height > 16384) return null;
                    int h = Math.Max(1, (int)(img.Height * ((float)width / img.Width)));
                    var bmp = new Bitmap((int)width, h, PixelFormat.Format24bppRgb);

                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                        g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                        g.DrawImage(img, 0, 0, (int)width, h);
                    }
                    return bmp;
                }
            }
            catch { return null; }
        }

        private Bitmap DecodeWpf(MemoryStream ms, uint width)
        {
            try
            {
                ms.Position = 0;
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = ms;
                bmp.DecodePixelWidth = (int)width;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();

                using (var stream = new MemoryStream())
                {
                    var enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bmp));
                    enc.Save(stream);
                    stream.Position = 0;
                    return new Bitmap(stream);
                }
            }
            catch { return null; }
        }
    }
}