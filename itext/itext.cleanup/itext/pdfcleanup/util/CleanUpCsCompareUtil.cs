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
using System.Drawing;
using System.IO;
using iText.Kernel.Pdf.Xobject;

namespace iText.PdfCleanup.Util {
    /// <summary>
    /// Utility class providing methods to check images compatibility.
    /// </summary>
    public class CleanUpCsCompareUtil {
        private CleanUpCsCompareUtil() {
        }
        
        /// <summary>
        /// Check whether the image info of the passed original image and the image info of the cleared
        /// image are the same.
        /// </summary>
        /// <param name="originalImage"><see cref="iText.Kernel.Pdf.Xobject.PdfImageXObject"/> of the original image</param>
        /// <param name="clearedImage"><see cref="iText.Kernel.Pdf.Xobject.PdfImageXObject"/> of the cleared image</param>
        /// <returns>true if the image infos are the same</returns>
        public static bool IsOriginalCsCompatible(PdfImageXObject originalImage, PdfImageXObject clearedImage) {
            using (Stream cmpStream = new MemoryStream(originalImage.GetImageBytes()))
            using (Stream toCompareStream = new MemoryStream(clearedImage.GetImageBytes())) {
                Image cmpImage = Image.FromStream(cmpStream);
                Image toCompareImage = Image.FromStream(toCompareStream);
                return cmpImage.PixelFormat == toCompareImage.PixelFormat;
            }
        }
    }
}
