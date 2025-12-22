using SharpShell.Attributes;
using SharpShell.SharpThumbnailHandler;
using SharpCompress.Archives;
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
    [Guid("9E8F7ABC-1234-4D22-9AA0-123456789111")]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".cbz", ".cbr", ".zip", ".rar", ".7z", ".cb7", ".alz", ".egg")]
    public class ModernArchiveThumbnailHandler : SharpThumbnailHandler
    {
        private static readonly HashSet<string> ValidFormats = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".jpe", ".jfif", ".png", ".bmp", ".webp", ".heic", ".heif", ".avif", ".gif", ".tiff", ".tif", ".ico"
        };

        private static readonly string[] PriorityKeys = new string[] { "cover", "front", "folder", "001", "000" };

        [ThreadStatic]
        private static byte[] _buffer;

        static ModernArchiveThumbnailHandler()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                try
                {
                    string name = new AssemblyName(args.Name).Name;
                    string folder = Path.GetDirectoryName(typeof(ModernArchiveThumbnailHandler).Assembly.Location);
                    string path = Path.Combine(folder, name + ".dll");
                    if (File.Exists(path)) return Assembly.LoadFrom(path);
                }
                catch { }
                return null;
            };
        }

        protected override Bitmap GetThumbnailImage(uint width)
        {
            int timeout = Environment.TickCount + 2500;
            try
            {
                if (SelectedItemStream == null) return null;

                using (IArchive archive = ArchiveFactory.Open(SelectedItemStream))
                {
                    int scanCount = 0;
                    foreach (IArchiveEntry entry in archive.Entries)
                    {
                        if (Environment.TickCount > timeout || scanCount++ >= 40) break;
                        if (entry.IsDirectory || entry.Size <= 0 || entry.Size > 31457280) continue;

                        string fullPath = entry.Key;
                        if (fullPath.Contains("__MACOSX")) continue;

                        string ext = Path.GetExtension(fullPath).ToLower();
                        if (string.IsNullOrEmpty(ext) || !ValidFormats.Contains(ext)) continue;

                        string fileName = Path.GetFileNameWithoutExtension(fullPath).ToLower();
                        bool isPriority = false;
                        foreach (string k in PriorityKeys)
                        {
                            if (fileName.StartsWith(k))
                            {
                                isPriority = true;
                                break;
                            }
                        }

                        try
                        {
                            int sz = (int)entry.Size;
                            if (_buffer == null || _buffer.Length < sz) _buffer = new byte[sz];

                            using (MemoryStream ms = new MemoryStream(_buffer, 0, sz, true, true))
                            {
                                entry.WriteTo(ms);
                                ms.Position = 0;

                                Bitmap bitmap = null;
                                if (sz < 4000000 && ext != ".heic" && ext != ".avif" && ext != ".webp")
                                {
                                    bitmap = DecodeGdi(ms, width);
                                }

                                if (bitmap == null)
                                {
                                    ms.Position = 0;
                                    bitmap = DecodeWpf(ms, width);
                                }

                                if (bitmap != null) return bitmap;
                            }
                            if (isPriority) break;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                if (_buffer != null && _buffer.Length > 8000000) _buffer = null;
            }
            return null;
        }

        private Bitmap DecodeGdi(MemoryStream ms, uint width)
        {
            try
            {
                using (Image img = Image.FromStream(ms, false, false))
                {
                    float ratio = (float)width / img.Width;
                    int h = (int)(img.Height * ratio);
                    if (h < 1) h = 1;
                    Bitmap bmp = new Bitmap((int)width, h, PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
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
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = ms;
                bmp.DecodePixelWidth = (int)width;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();

                using (MemoryStream stream = new MemoryStream())
                {
                    BmpBitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bmp));
                    enc.Save(stream);
                    return new Bitmap(stream);
                }
            }
            catch { return null; }
        }
    }
}