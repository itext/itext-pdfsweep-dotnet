/*
This file is part of the iText (R) project.
Copyright (c) 1998-2021 iText Group NV
Authors: iText Software.

This program is offered under a commercial and under the AGPL license.
For commercial licensing, contact us at https://itextpdf.com/sales.  For AGPL licensing, see below.

AGPL licensing:
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using iText.Kernel;
using Rectangle = iText.Kernel.Geom.Rectangle;

namespace iText.PdfCleanup.Util {
    /// <summary>
    /// Utility class providing methods to handle images and work with graphics.
    /// </summary>
    public class CleanUpImageUtil {
        private static readonly Color? CLEANED_AREA_FILL_COLOR = Color.White;

        private const String UnsupportedImageFormat = "The given image format is not supported by pdfSweep.";

        private CleanUpImageUtil() {
        }

        /// <summary>Clean up an image using a List of Rectangles that need to be redacted</summary>
        /// <param name="imageBytes">the image to be cleaned up</param>
        /// <param name="areasToBeCleaned">the List of Rectangles that need to be redacted out of the image</param>
        public static byte[] CleanUpImage(byte[] imageBytes, IList<Rectangle> areasToBeCleaned) {
            if (areasToBeCleaned.IsEmpty()) {
                return imageBytes;
            }

            using (Stream imageStream = new MemoryStream(imageBytes)) {
                Image image = Image.FromStream(imageStream);

                ImageFormat formatToSave = image.RawFormat;
                PixelFormat pixelFormat = image.PixelFormat;
                switch (pixelFormat) {
                    case PixelFormat.Format8bppIndexed:
                        image = Clean8bppImage(image, areasToBeCleaned);
                        break;
                    default:
                        CleanImage(image, areasToBeCleaned);
                        break;
                }

                using (MemoryStream outStream = new MemoryStream()) {
                    EncoderParameters encParams = null;
                    if (Equals(formatToSave, ImageFormat.Jpeg)) {
                        encParams = new EncoderParameters(1);
                        encParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

                        // We want to preserve the original format, but in case of 8bpp indexed pixel format
                        // we can not save JPEG format.
                        if (image.PixelFormat == PixelFormat.Format8bppIndexed) {
                            formatToSave = ImageFormat.Png;
                        }
                    }

                    image.Save(outStream, GetEncoderInfo(formatToSave), encParams);

                    return outStream.ToArray();
                }
            }
        }

        private static Image Clean8bppImage(Image image, IList<Rectangle> areasToBeCleaned) {
            // We need to create a new empty Bitmap and redraw an original image on it to clean it in case of 8bpp
            Bitmap tempBitMap = new Bitmap(image.Width, image.Height);
            tempBitMap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (Graphics g = Graphics.FromImage(tempBitMap)) {
                g.DrawImage(image, 0, 0);
            }

            CleanImage(tempBitMap, areasToBeCleaned);

            // The result shall be with the same bpp as the original image
            return To8bppIndexed(tempBitMap, image.Palette);
        }

        private static Bitmap To8bppIndexed(Bitmap toConvert, ColorPalette palette) {
            Color[] paletteEntries = palette.Entries;
            Dictionary<Color, byte> colorToIndex = new Dictionary<Color, byte>(paletteEntries.Length);
            for (int i = 0; i < paletteEntries.Length; ++i) {
                colorToIndex.Put(paletteEntries[i], (byte) i);
            }

            Bitmap result = new Bitmap(toConvert.Width, toConvert.Height, PixelFormat.Format8bppIndexed);
            result.SetResolution(toConvert.HorizontalResolution, toConvert.VerticalResolution);
            result.Palette = palette;

            BitmapData data = result.LockBits(new System.Drawing.Rectangle(0, 0, result.Width, result.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            byte[] bytes = new byte[data.Height * data.Stride];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

            for (int x = 0; x < toConvert.Width; x++) {
                for (int y = 0; y < toConvert.Height; y++) {
                    Color pixelColor = toConvert.GetPixel(x, y);
                    if (!colorToIndex.ContainsKey(pixelColor)) {
                        throw new PdfException(UnsupportedImageFormat);
                    }

                    byte index = colorToIndex.Get(pixelColor);
                    bytes[y * data.Stride + x] = index;
                }
            }

            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);

            result.UnlockBits(data);
            return result;
        }

        /// <summary>Clean up a BufferedImage using a List of Rectangles that need to be redacted</summary>
        /// <param name="image">the image to be cleaned up</param>
        /// <param name="areasToBeCleaned">the List of Rectangles that need to be redacted out of the image</param>
        private static void CleanImage(Image image, IList<Rectangle> areasToBeCleaned) {
            using (Graphics g = Graphics.FromImage(image)) {
                // A rectangle in the areasToBeCleaned list is treated to be in standard [0,1]x[0,1] image space
                // (y varies from bottom to top and x from left to right), so we should scale the rectangle and also
                // invert and shear the y axe.
                foreach (Rectangle rect in areasToBeCleaned) {
                    int imgHeight = image.Height;
                    int imgWidth = image.Width;
                    int[] scaledRectToClean = CleanUpHelperUtil.GetImageRectToClean(rect, imgWidth, imgHeight);

                    g.FillRectangle(new SolidBrush(CLEANED_AREA_FILL_COLOR.Value), scaledRectToClean[0],
                        scaledRectToClean[1], scaledRectToClean[2], scaledRectToClean[3]);
                }
            }
        }

        private static ImageCodecInfo GetEncoderInfo(ImageFormat format) {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

            for (int j = 0; j < encoders.Length; ++j) {
                if (encoders[j].FormatID == format.Guid)
                    return encoders[j];
            }

            return null;
        }
    }
}
