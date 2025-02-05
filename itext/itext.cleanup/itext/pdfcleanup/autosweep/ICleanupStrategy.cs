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
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace iText.PdfCleanup.Autosweep {
    /// <summary>
    /// This class represents a generic cleanup strategy to be used with
    /// <see cref="iText.PdfCleanup.PdfCleaner"/>
    /// or
    /// <see cref="PdfAutoSweepTools"/>
    /// ICleanupStrategy must implement Cloneable to ensure a strategy can be reset after having handled a page.
    /// </summary>
    public interface ICleanupStrategy : ILocationExtractionStrategy {
        /// <summary>Get the color in which redaction is to take place</summary>
        /// <param name="location">where to get the redaction color from</param>
        /// <returns>
        /// a
        /// <see cref="iText.Kernel.Colors.Color"/>
        /// </returns>
        Color GetRedactionColor(IPdfTextLocation location);

        /// <summary>
        /// ICleanupStrategy objects have to be reset at times
        /// <c>PdfAutoSweep</c>
        /// will use the same strategy for all pages,
        /// and expects to receive only the rectangles from the last page as output.
        /// </summary>
        /// <remarks>
        /// ICleanupStrategy objects have to be reset at times
        /// <c>PdfAutoSweep</c>
        /// will use the same strategy for all pages,
        /// and expects to receive only the rectangles from the last page as output.
        /// Hence the reset method.
        /// </remarks>
        /// <returns>a clone of this Object</returns>
        ICleanupStrategy Reset();
    }
}
