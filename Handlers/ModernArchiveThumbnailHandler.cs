using SharpShell.Attributes;
using SharpShell.SharpThumbnailHandler;
using SharpCompress.Archives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using Microsoft.Win32;
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
    [COMServerAssociation(AssociationType.ClassOfExtension, ".cbz", ".cbr", ".zip", ".rar", ".7z")]
    public class ModernArchiveThumbnailHandler : SharpThumbnailHandler
    {
        private static readonly HashSet<string> ValidExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".jpe", ".jfif", ".png", ".webp", ".bmp", ".gif",
            ".tiff", ".tif", ".tga", ".heic", ".heif", ".avif", ".vif", ".ico"
        };

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
            try
            {
                int active = 1;
                int mode = 2;
                
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\ModernArchiveThumbnail"))
                {
                    if (key != null)
                    {
                        active = (int)key.GetValue("HandlerRegistered", 1);
                        mode = (int)key.GetValue("ThumbnailMode", 2);
                    }
                }

                if (active == 0) return null;
                if (SelectedItemStream == null) return null;

                using (var archive = ArchiveFactory.Open(SelectedItemStream))
                {
                    int maxTries = (mode == 1) ? 5 : (mode == 3) ? 4 : 2;
                    
                    var validImages = archive.Entries
                        .Where(e => !e.IsDirectory)
                        .Where(e => {
                            string ext = Path.GetExtension(e.Key);
                            return ext.Length > 0 && ValidExtensions.Contains(ext);
                        })
                        .OrderBy(e => e.Key)
                        .Take(maxTries);

                    byte[] buffer = null;

                    foreach (var targetEntry in validImages)
                    {
                        try
                        {
                            long entrySize = targetEntry.Size;
                            if (entrySize > int.MaxValue || entrySize <= 0)
                                continue;

                            int size = (int)entrySize;
                            if (buffer == null || buffer.Length < size)
                                buffer = new byte[Math.Max(size, 1024 * 1024)];

                            using (var ms = new MemoryStream(buffer, 0, size, true, true))
                            {
                                targetEntry.WriteTo(ms);
                                ms.Position = 0;

                                if (mode == 3)
                                {
                                    var result = TryStreamingDecode(ms, width);
                                    if (result != null) return result;
                                    
                                    ms.Position = 0;
                                    result = TryWpfDecodeFromStream(ms, width);
                                    if (result != null) return result;
                                    
                                    continue;
                                }

                                if (mode == 1 || mode == 2)
                                {
                                    var result = TryWpfDecodeFromStream(ms, width);
                                    if (result != null) return result;
                                    ms.Position = 0;
                                }

                                var imgInfo = SixLabors.ImageSharp.Image.Identify(ms);
                                if (imgInfo != null && (imgInfo.Width > 16384 || imgInfo.Height > 16384))
                                {
                                    continue;
                                }

                                ms.Position = 0;
                                using (var img = SixLabors.ImageSharp.Image.Load(ms))
                                {
                                    var sampler = (mode == 1) ? KnownResamplers.Triangle : KnownResamplers.NearestNeighbor;

                                    img.Mutate(x => x.Resize(new ResizeOptions
                                    {
                                        Size = new SixLabors.ImageSharp.Size((int)width, 0),
                                        Mode = ResizeMode.Max,
                                        Sampler = sampler
                                    }).BackgroundColor(SixLabors.ImageSharp.Color.White));

                                    var bmpStream = new MemoryStream();
                                    img.SaveAsBmp(bmpStream);
                                    return new Bitmap(bmpStream);
                                }
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        private Bitmap TryStreamingDecode(MemoryStream ms, uint width)
        {
            try
            {
                using (var img = System.Drawing.Image.FromStream(ms, false, false))
                {
                    if (img.Width > 16384 || img.Height > 16384) return null;

                    int newHeight = (int)(img.Height * ((float)width / img.Width));
                    if (newHeight == 0) newHeight = 1;

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
            catch
            {
                return null;
            }
        }

        private Bitmap TryWpfDecodeFromStream(MemoryStream ms, uint width)
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
            catch
            {
                return null;
            }
        }
    }
}