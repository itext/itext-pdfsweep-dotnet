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
using iText.Kernel.Geom;

namespace iText.PdfCleanup.Util {
    /// <summary>Utility class providing clean up helping methods.</summary>
    public sealed class CleanUpHelperUtil {
        private const float EPS = 1e-4F;

        private CleanUpHelperUtil() {
        }

        /// <summary>
        /// Calculates the coordinates of the image rectangle to clean by the passed
        /// <see cref="iText.Kernel.Geom.Rectangle"/>
        /// ,
        /// specifying the area to clean.
        /// </summary>
        /// <param name="rect">
        /// the
        /// <see cref="iText.Kernel.Geom.Rectangle"/>
        /// specifying the area to clean.
        /// </param>
        /// <param name="imgWidth">width of the image to clean</param>
        /// <param name="imgHeight">height of the image to clean</param>
        /// <returns>an array of the resultant rectangle coordinates</returns>
        public static int[] GetImageRectToClean(Rectangle rect, int imgWidth, int imgHeight) {
            double bottom = (double)rect.GetBottom() * imgHeight;
            int scaledBottomY = (int)Math.Ceiling(bottom - EPS);
            double top = (double)rect.GetTop() * imgHeight;
            int scaledTopY = (int)Math.Floor(top + EPS);
            double left = (double)rect.GetLeft() * imgWidth;
            int x = (int)Math.Ceiling(left - EPS);
            int y = imgHeight - scaledTopY;
            double right = (double)rect.GetRight() * imgWidth;
            int w = (int)Math.Floor(right + EPS) - x;
            int h = scaledTopY - scaledBottomY;
            return new int[] { x, y, w, h };
        }
    }
}
