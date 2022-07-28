/*
This file is part of the iText (R) project.
Copyright (c) 1998-2022 iText Group NV
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
using System.IO;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Test;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("Unit test")]
    public class TextPositioningTest : ExtendedITextTest {
        public const float EPS = 0.0001F;

        [NUnit.Framework.Test]
        public virtual void CheckNoNpeThrowsInWritePositioningOperatorTest() {
            PdfCanvas canvasForTest = CreateTestCanvas(1.0F);
            TextPositioning textPositioning = new TextPositioning();
            try {
                textPositioning.AppendPositioningOperator("T*", new List<PdfObject>());
                textPositioning.WritePositionedText("T*", new List<PdfObject>(), new PdfArray(), canvasForTest);
            }
            catch (NullReferenceException) {
                NUnit.Framework.Assert.Fail("We don't expect, that NPE will be thrown in this test!");
            }
            NUnit.Framework.Assert.AreEqual(0.0, canvasForTest.GetGraphicsState().GetLeading(), EPS);
        }

        [NUnit.Framework.Test]
        public virtual void CheckNoNpeThrowsInWritePositioningTextTest() {
            PdfCanvas canvasForTest = CreateTestCanvas(2.0F);
            TextPositioning textPositioning = new TextPositioning();
            try {
                textPositioning.AppendPositioningOperator("'", new List<PdfObject>());
                textPositioning.WritePositionedText("'", new List<PdfObject>(), new PdfArray(), canvasForTest);
            }
            catch (NullReferenceException) {
                NUnit.Framework.Assert.Fail("We don't expect, that NPE will be thrown in this test!");
            }
            NUnit.Framework.Assert.AreEqual(0.0, canvasForTest.GetGraphicsState().GetLeading(), EPS);
        }

        private static PdfCanvas CreateTestCanvas(float canvasLeading) {
            PdfDocument document = new PdfDocument(new PdfWriter(new MemoryStream()));
            PdfPage documentPage = document.AddNewPage();
            PdfCanvas canvas = new PdfCanvas(documentPage);
            canvas.SetLeading(canvasLeading);
            canvas.SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.COURIER), 14);
            return canvas;
        }
    }
}
