using SharpShell.Attributes;
using SharpShell.SharpThumbnailHandler;
using SharpCompress.Archives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;
using Microsoft.Win32;
using System;
using System.Drawing;
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
                if (SelectedItemStream == null) return null;

                int mode = 2;
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(@"Software\ModernArchiveThumbnail"))
                    {
                        if (key != null) mode = (int)key.GetValue("Mode", 2);
                    }
                }
                catch { }

                using (var archive = ArchiveFactory.Open(SelectedItemStream))
                {
                    var validImages = archive.Entries
                        .Where(e => !e.IsDirectory)
                        .Where(e =>
                        {
                            var name = e.Key.ToLower();
                            return name.EndsWith(".jpg") || name.EndsWith(".jpeg") || 
                                   name.EndsWith(".jpe") || name.EndsWith(".jfif") ||
                                   name.EndsWith(".png") || name.EndsWith(".webp") ||
                                   name.EndsWith(".bmp") || name.EndsWith(".gif") ||
                                   name.EndsWith(".tiff") || name.EndsWith(".tif") ||
                                   name.EndsWith(".tga") || 
                                   name.EndsWith(".heic") || name.EndsWith(".heif") ||
                                   name.EndsWith(".avif") || name.EndsWith(".vif") ||
                                   name.EndsWith(".ico");
                        })
                        .Take(2);

                    foreach (var targetEntry in validImages)
                    {
                        try
                        {
                            using (var ms = new MemoryStream())
                            {
                                targetEntry.WriteTo(ms);
                                ms.Position = 0;

                                if (mode == 2)
                                {
                                    var result = TryWpfDecodeTempFile(ms, width, targetEntry.Key);
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
                                    var sampler = (mode == 0) ? KnownResamplers.Triangle : KnownResamplers.NearestNeighbor;

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

        private Bitmap TryWpfDecodeTempFile(MemoryStream ms, uint width, string fileName)
        {
            string tempFile = null;
            try
            {
                string tempFolder = Path.GetTempPath();
                string rawExt = Path.GetExtension(fileName).ToLower();
                string extension = (rawExt.Length > 0 && rawExt.Length < 6) ? rawExt : ".tmp";
                tempFile = Path.Combine(tempFolder, Guid.NewGuid().ToString() + extension);

                File.WriteAllBytes(tempFile, ms.ToArray());

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(tempFile);
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
            finally { try { if (tempFile != null && File.Exists(tempFile)) File.Delete(tempFile); } catch { } }
        }
    }
}