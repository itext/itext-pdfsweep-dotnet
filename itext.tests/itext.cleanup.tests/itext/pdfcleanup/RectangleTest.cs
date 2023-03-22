/*
This file is part of the iText (R) project.
Copyright (c) 1998-2023 Apryse Group NV
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
using System;
using System.IO;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Utils;
using iText.Test;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("IntegrationTest")]
    public class RectangleTest : ExtendedITextTest {
        private static readonly String SOURCE_FOLDER = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/RectangleTest/";

        private static readonly String DESTINATION_FOLDER = NUnit.Framework.TestContext.CurrentContext.TestDirectory
             + "/test/itext/pdfcleanup/RectangleTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void BeforeClass() {
            CreateDestinationFolder(DESTINATION_FOLDER);
        }

        [NUnit.Framework.Test]
        public virtual void ZeroWidthLineTest() {
            // TODO DEVSIX-7136 Rectangles drawn with zero-width line disappear on sweeping
            String outPdf = DESTINATION_FOLDER + "zeroWidthLine.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_zeroWidthLine.pdf";
            MemoryStream outDocBaos = new MemoryStream();
            using (PdfDocument pdfDocument = new PdfDocument(new PdfWriter(outDocBaos))) {
                PdfPage page = pdfDocument.AddNewPage();
                PdfCanvas canvas = new PdfCanvas(page);
                canvas.SetStrokeColor(DeviceRgb.BLUE).SetLineWidth(0).Rectangle(new Rectangle(350, 400, 100, 100)).Stroke(
                    );
                canvas.SetStrokeColor(DeviceRgb.RED).SetLineWidth(0).MoveTo(100, 100).LineTo(100, 200).LineTo(200, 200).LineTo
                    (200, 100).ClosePath().Stroke();
            }
            PdfDocument pdfDocument_1 = new PdfDocument(new PdfReader(new MemoryStream(outDocBaos.ToArray())), new PdfWriter
                (outPdf));
            iText.PdfCleanup.PdfCleanUpTool workingTool = new iText.PdfCleanup.PdfCleanUpTool(pdfDocument_1);
            Rectangle area = new Rectangle(0, 50, 150, 150);
            workingTool.AddCleanupLocation(new iText.PdfCleanup.PdfCleanUpLocation(1, area, ColorConstants.RED));
            workingTool.CleanUp();
            pdfDocument_1.Close();
            NUnit.Framework.Assert.IsNull(new CompareTool().CompareByContent(outPdf, cmpPdf, DESTINATION_FOLDER, "diff_"
                ));
        }
    }
}
