/*
This file is part of the iText (R) project.
Copyright (c) 1998-2025 Apryse Group NV
Authors: Apryse Software.

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
using iText.Kernel.Colors;
using iText.Kernel.Geom;

namespace iText.PdfCleanup {
    /// <summary>Defines the region to be erased in a PDF document.</summary>
    public class PdfCleanUpLocation {
        private int page;

        private Rectangle region;

        private Color cleanUpColor;

        /// <summary>
        /// Constructs a
        /// <see cref="PdfCleanUpLocation"/>
        /// object.
        /// </summary>
        /// <param name="page">specifies the number of the page which the region belongs to.</param>
        /// <param name="region">represents the boundaries of the area to be erased.</param>
        public PdfCleanUpLocation(int page, Rectangle region) {
            this.page = page;
            this.region = region;
        }

        /// <summary>
        /// Constructs a
        /// <see cref="PdfCleanUpLocation"/>
        /// object.
        /// </summary>
        /// <param name="page">specifies the number of the page which the region belongs to.</param>
        /// <param name="region">represents the boundaries of the area to be erased.</param>
        /// <param name="cleanUpColor">
        /// a color used to fill the area after erasing it. If
        /// <see langword="null"/>
        /// the erased area left uncolored.
        /// </param>
        public PdfCleanUpLocation(int page, Rectangle region, Color cleanUpColor)
            : this(page, region) {
            this.cleanUpColor = cleanUpColor;
        }

        /// <returns>the number of the page which the region belongs to.</returns>
        public virtual int GetPage() {
            return page;
        }

        /// <returns>
        /// A
        /// <see cref="iText.Kernel.Geom.Rectangle"/>
        /// representing the boundaries of the area to be erased.
        /// </returns>
        public virtual Rectangle GetRegion() {
            return region;
        }

        /// <summary>Returns a color used to fill the area after erasing it.</summary>
        /// <remarks>
        /// Returns a color used to fill the area after erasing it. If
        /// <see langword="null"/>
        /// the erased area left uncolored.
        /// </remarks>
        /// <returns>a color used to fill the area after erasing it.</returns>
        public virtual Color GetCleanUpColor() {
            return cleanUpColor;
        }
    }
}
