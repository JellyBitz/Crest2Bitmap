using Crest2Bitmap.Properties;

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Crest2Bitmap
{
    internal class Program
    {
        /// <summary>
        /// Entry point
        /// </summary>
        static void Main(string[] args)
        {
            // Files not being specified
            if (args.Length == 0)
                return;

            // Get file paths
            List<string> files = new List<string>();
            foreach (var arg in args)
            {
                // Add file
                if (File.Exists(arg))
                    files.Add(arg);
                // Add files from folder
                else if (Directory.Exists(arg))
                    files.AddRange(Directory.GetFiles(arg));
            }

            // Convert each file with 256 bytes as data (16x16 as 8bpp indexed)
            int iconWidth = 16, iconHeight = 16;
            var imgDataSize = iconWidth * iconHeight;
            foreach (string path in files)
            {
                // Check file size
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    if (fs.Length == imgDataSize)
                    {
                        // Create bitmap
                        Bitmap bmp = new Bitmap(iconWidth, iconHeight, PixelFormat.Format8bppIndexed);

                        // Set up palette - System (Windows)
                        var palette = bmp.Palette;
                        var paletteBytes = (byte[])Resources.ResourceManager.GetObject("Palette_System_Windows.ACT");
                        for (int i = 0; i < palette.Entries.Length; i++)
                            palette.Entries[i] = Color.FromArgb(paletteBytes[i * 3], paletteBytes[i * 3 + 1], paletteBytes[i * 3 + 2]);
                        UpdatePalette(bmp, palette);

                        // Set indexed data into bitmap
                        for (int y = 15; y >= 0; y--)
                            for (int x = 0; x < 16; x++)
                                SetPixelIndex(bmp, x, y, (byte)fs.ReadByte());

                        // Save it
                        bmp.Save(path + ".bmp", ImageFormat.Bmp);
                    }
                }
            }
        }
        /// <summary>
        /// Set palette from bitmap
        /// </summary>
        private static void UpdatePalette(Bitmap bmp, ColorPalette palette)
        {
            BitmapData imgData = bmp.LockBits(new Rectangle(new Point(0, 0), new Size(1, 1)), ImageLockMode.WriteOnly, bmp.PixelFormat);
            bmp.Palette = palette;
            bmp.UnlockBits(imgData);
        }
        /// <summary>
        /// Set index from pixel
        /// </summary>
        private static void SetPixelIndex(Bitmap bmp, int x, int y, byte index)
        {
            BitmapData data = bmp.LockBits(new Rectangle(new Point(x, y), new Size(1, 1)), ImageLockMode.WriteOnly, bmp.PixelFormat);
            Marshal.WriteByte(data.Scan0, index);
            bmp.UnlockBits(data);
        }
    }
}
