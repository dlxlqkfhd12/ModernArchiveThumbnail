using SharpShell.Attributes;
using SharpShell.SharpThumbnailHandler;
using SharpCompress.Archives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace ModernArchiveThumbnail.Handlers
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".cbz", ".cbr", ".zip", ".rar", ".7z", ".cb7")]
    public class ModernArchiveThumbnailHandler : SharpThumbnailHandler
    {
        private static readonly HashSet<string> FastFormats = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".jpe", ".jfif", ".png", ".bmp"
        };

        private static readonly HashSet<string> ModernFormats = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".webp", ".heic", ".heif", ".avif", ".jxl", ".gif", ".tiff", ".tif", ".ico"
        };

        private static readonly string[] SkipPatterns = new[]
        {
            "credit", "recruit", "logo", "ad", "advertisement", "scanlation"
        };

        private static readonly string[] PreferPatterns = new[]
        {
            "cover", "front", "00", "01"
        };

        private const int MAX_FILE_SIZE = 10 * 1024 * 1024;

        [ThreadStatic]
        private static byte[] _sharedBuffer;

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
            var sw = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                int active = 1, mode = 2;
                bool skipScanlation = false, preferCover = false;
                
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\ModernArchiveThumbnail"))
                {
                    if (key != null)
                    {
                        active = (int)key.GetValue("HandlerRegistered", 1);
                        mode = (int)key.GetValue("ThumbnailMode", 2);
                        skipScanlation = (int)key.GetValue("SkipScanlation", 0) == 1;
                        preferCover = (int)key.GetValue("PreferCover", 0) == 1;
                    }
                }

                if (active == 0) return null;
                if (SelectedItemStream == null) return null;

                int timeout = (mode == 1) ? 600 : (mode == 3) ? 350 : 450;
                int maxTries = (mode == 1) ? 4 : (mode == 3) ? 2 : 3;

                using (var archive = ArchiveFactory.Open(SelectedItemStream))
                {
                    var candidates = archive.Entries
                        .Where(e => !e.IsDirectory && e.Size > 0 && e.Size < MAX_FILE_SIZE)
                        .Where(e => {
                            string ext = Path.GetExtension(e.Key);
                            return ext.Length > 0 && (FastFormats.Contains(ext) || ModernFormats.Contains(ext));
                        })
                        .Select(e => new {
                            Entry = e,
                            Name = Path.GetFileNameWithoutExtension(e.Key).ToLower(),
                            Ext = Path.GetExtension(e.Key),
                            Priority = CalculatePriority(e.Key, skipScanlation, preferCover)
                        })
                        .Where(x => x.Priority < 1000)
                        .OrderBy(x => x.Priority)
                        .ThenBy(x => x.Entry.Key)
                        .Take(maxTries * 2)
                        .Select(x => x.Entry);

                    int tried = 0;
                    foreach (var targetEntry in candidates)
                    {
                        if (sw.ElapsedMilliseconds > timeout || tried >= maxTries) break;
                        tried++;
                        
                        try
                        {
                            long entrySize = targetEntry.Size;
                            if (entrySize > int.MaxValue) continue;

                            int size = (int)entrySize;
                            if (_sharedBuffer == null || _sharedBuffer.Length < size)
                                _sharedBuffer = new byte[size];

                            using (var ms = new MemoryStream(_sharedBuffer, 0, size, true, true))
                            {
                                targetEntry.WriteTo(ms);
                                ms.Position = 0;

                                string ext = Path.GetExtension(targetEntry.Key);
                                bool isFast = FastFormats.Contains(ext);

                                if (mode == 3 && isFast)
                                {
                                    var result = TryGdiDecode(ms, width);
                                    if (result != null) return result;
                                    continue;
                                }

                                if (mode == 3 || (mode == 2 && isFast))
                                {
                                    var result = TryGdiDecode(ms, width);
                                    if (result != null) return result;
                                    
                                    if (sw.ElapsedMilliseconds < timeout * 0.6)
                                    {
                                        ms.Position = 0;
                                        result = TryWpfDecode(ms, width);
                                        if (result != null) return result;
                                    }
                                    continue;
                                }

                                if (mode == 1 || mode == 2)
                                {
                                    var result = TryGdiDecode(ms, width);
                                    if (result != null) return result;
                                    
                                    if (sw.ElapsedMilliseconds > timeout * 0.5) break;
                                    
                                    ms.Position = 0;
                                    result = TryWpfDecode(ms, width);
                                    if (result != null) return result;
                                    
                                    if (sw.ElapsedMilliseconds > timeout * 0.7) break;
                                    
                                    ms.Position = 0;
                                }

                                var imgInfo = SixLabors.ImageSharp.Image.Identify(ms);
                                if (imgInfo == null || imgInfo.Width > 16384 || imgInfo.Height > 16384)
                                    continue;

                                ms.Position = 0;
                                using (var img = SixLabors.ImageSharp.Image.Load(ms))
                                {
                                    var sampler = (mode == 1) ? KnownResamplers.Lanczos3 : 
                                                  (mode == 3) ? KnownResamplers.NearestNeighbor : 
                                                  KnownResamplers.Box;

                                    img.Mutate(x => x.Resize(new ResizeOptions
                                    {
                                        Size = new SixLabors.ImageSharp.Size((int)width, 0),
                                        Mode = ResizeMode.Max,
                                        Sampler = sampler
                                    }));

                                    var bmpStream = new MemoryStream();
                                    img.SaveAsBmp(bmpStream);
                                    return new Bitmap(bmpStream);
                                }
                            }
                        }
                        catch
                        {
                            if (sw.ElapsedMilliseconds > timeout * 0.8) break;
                            continue;
                        }
                    }
                }
            }
            catch { }
            finally
            {
                sw.Stop();
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CalculatePriority(string filename, bool skipScanlation, bool preferCover)
        {
            string lower = filename.ToLower();

            if (skipScanlation)
            {
                foreach (var pattern in SkipPatterns)
                {
                    if (lower.Contains(pattern))
                        return 1000;
                }
            }

            if (preferCover)
            {
                foreach (var pattern in PreferPatterns)
                {
                    if (lower.Contains(pattern))
                        return 0;
                }
            }

            return 10;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Bitmap TryGdiDecode(MemoryStream ms, uint width)
        {
            try
            {
                using (var img = System.Drawing.Image.FromStream(ms, false, false))
                {
                    if (img.Width > 16384 || img.Height > 16384) return null;

                    int newHeight = Math.Max(1, (int)(img.Height * ((float)width / img.Width)));
                    var thumbnail = new Bitmap((int)width, newHeight, PixelFormat.Format24bppRgb);
                    
                    using (var g = Graphics.FromImage(thumbnail))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                        g.DrawImage(img, 0, 0, (int)width, newHeight);
                    }
                    return thumbnail;
                }
            }
            catch { return null; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Bitmap TryWpfDecode(MemoryStream ms, uint width)
        {
            try
            {
                ms.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.DecodePixelWidth = (int)width;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                using (var outStream = new MemoryStream())
                {
                    var encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    encoder.Save(outStream);
                    outStream.Position = 0;
                    return new Bitmap(outStream);
                }
            }
            catch { return null; }
        }
    }
}