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

using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using iText.PdfCleanup.Util;
using iText.Test;
using NUnit.Framework;

namespace iText.PdfCleanup {
    public class CleanUpCsCompareUtilTest : ExtendedITextTest {
        [NUnit.Framework.Test]
        public virtual void ImageNotCsCompatibleTest() {
            PdfImageXObject image1 = CreateMockedPdfImageXObject(PdfName.DeviceGray, 8);
            PdfImageXObject image2 = CreateMockedPdfImageXObject(PdfName.DeviceRGB, 8);

            Assert.False(CleanUpCsCompareUtil.IsOriginalCsCompatible(image1, image2));
        }

        [NUnit.Framework.Test]
        public virtual void ImageCsCompatibleTest() {
            PdfImageXObject image1 = CreateMockedPdfImageXObject(PdfName.DeviceRGB, 8);
            PdfImageXObject image2 = CreateMockedPdfImageXObject(PdfName.DeviceRGB, 8);

            Assert.True(CleanUpCsCompareUtil.IsOriginalCsCompatible(image1, image2));
        }

        private PdfImageXObject CreateMockedPdfImageXObject(PdfName colorSpace, int bitsPerComponent) {
            PdfStream stream1 = new PdfStream();
            stream1.Put(PdfName.BitsPerComponent, new PdfNumber(bitsPerComponent));
            stream1.Put(PdfName.ColorSpace, colorSpace);
            stream1.Put(PdfName.Width, new PdfNumber(1));
            stream1.Put(PdfName.Height, new PdfNumber(1));
            return new PdfImageXObject(stream1);
        }
    }
}
