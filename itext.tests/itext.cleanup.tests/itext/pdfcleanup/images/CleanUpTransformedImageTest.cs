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
using System;
using iText.Commons.Utils;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.PdfCleanup;
using iText.PdfCleanup.Util;
using iText.Test;

namespace iText.PdfCleanup.Images {
    [NUnit.Framework.Category("IntegrationTest")]
    public class CleanUpTransformedImageTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/images/CleanUpTransformedImageTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/images/CleanUpTransformedImageTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void SkewedGrayscaleImageBBoxCleanUpTest() {
            // TODO DEVSIX-5089 skewed images cleanup is not supported
            String input = inputPath + "skewedGrayImage.pdf";
            String output = outputPath + "skewedGrayImage.pdf";
            String cmp = inputPath + "cmp_skewedGrayImage.pdf";
            Rectangle cleanupRegion = new Rectangle(150, 250, 100, 100);
            NUnit.Framework.Assert.Catch(typeof(IndexOutOfRangeException), () => {
                CleanFirstPageAndDrawCleanupRegion(cleanupRegion, input, output);
                NUnit.Framework.Assert.IsNull(FindDifferencesBetweenOutputAndCmp(output, cmp));
            }
            );
        }

        [NUnit.Framework.Test]
        public virtual void SkewedRgbImageBBoxCleanUpTest() {
            // TODO DEVSIX-5089 skewed images cleanup is not supported
            String input = inputPath + "skewedRgbImage.pdf";
            String output = outputPath + "skewedRgbImage.pdf";
            String cmp = inputPath + "cmp_skewedRgbImage.pdf";
            Rectangle cleanupRegion = new Rectangle(150, 250, 100, 100);
            CleanFirstPageAndDrawCleanupRegion(cleanupRegion, input, output);
            NUnit.Framework.Assert.IsNull(FindDifferencesBetweenOutputAndCmp(output, cmp));
        }

        private static void CleanFirstPageAndDrawCleanupRegion(Rectangle cleanupRegion, String input, String output
            ) {
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output))) {
                PdfCleaner.CleanUp(pdfDocument, JavaCollectionsUtil.SingletonList(new iText.PdfCleanup.PdfCleanUpLocation(
                    1, cleanupRegion)));
                DrawCleanupRegionOnPage(pdfDocument, cleanupRegion);
            }
        }

        private static void DrawCleanupRegionOnPage(PdfDocument pdfDocument, Rectangle cleanupRegion) {
            new PdfCanvas(pdfDocument.GetFirstPage()).SetLineDash(3, 3).SetStrokeColor(ColorConstants.CYAN).Rectangle(
                cleanupRegion).Stroke();
        }

        private static String FindDifferencesBetweenOutputAndCmp(String output, String cmp) {
            CleanUpImagesCompareTool compareTool = new CleanUpImagesCompareTool();
            String imgCompare = compareTool.ExtractAndCompareImages(output, cmp, outputPath);
            String contentCompare = compareTool.CompareByContent(output, cmp, outputPath);
            return String.IsNullOrEmpty(imgCompare) ? contentCompare : String.Join("\n", imgCompare, contentCompare);
        }
    }
}
