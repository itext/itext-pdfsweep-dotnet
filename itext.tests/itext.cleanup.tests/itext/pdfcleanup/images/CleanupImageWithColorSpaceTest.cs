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
using System.Collections.Generic;
using iText.Commons.Utils;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.PdfCleanup;
using iText.PdfCleanup.Util;
using iText.Test;

namespace iText.PdfCleanup.Images {
    [NUnit.Framework.Category("IntegrationTest")]
    public class CleanupImageWithColorSpaceTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/images/CleanupImageWithColorSpaceTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/images/CleanupImageWithColorSpaceTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTestColorSpace() {
            String input = inputPath + "imgSeparationCs.pdf";
            String output = outputPath + "imgSeparationCs.pdf";
            String cmp = inputPath + "cmp_imgSeparationCs.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(60f, 
                780f, 60f, 45f), ColorConstants.GREEN)));
            CompareByContent(cmp, output, outputPath, "9");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTestColorSpaceJpegBaselineEncoded() {
            // cleanup jpeg image with baseline encoded data
            String input = inputPath + "imgSeparationCsJpegBaselineEncoded.pdf";
            String output = outputPath + "imgSeparationCsJpegBaselineEncoded.pdf";
            String cmp = inputPath + "cmp_imgSeparationCsJpegBaselineEncoded.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(60f, 
                600f, 100f, 50f), ColorConstants.GREEN)));
            CompareByContent(cmp, output, outputPath, "11");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTestColorSpaceJpegBaselineEncodedWithApp14Segment() {
            // cleanup jpeg image with baseline encoded data and app14 segment with unknown color type
            // Adobe Photoshop will always add an APP14 segment into the resulting jpeg file.
            // To make Unknown color type we have set the quality of an image to maximum during the "save as" operation
            String input = inputPath + "imgSeparationCsJpegBaselineEncodedWithApp14Segment.pdf";
            String output = outputPath + "imgSeparationCsJpegBaselineEncodedWithApp14Segment.pdf";
            String cmp = inputPath + "cmp_imgSeparationCsJpegBaselineEncodedWithApp14Segment.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(60f, 
                600f, 100f, 50f), ColorConstants.GREEN)));
            CompareByContent(cmp, output, outputPath, "10");
        }

        private void CleanUp(String input, String output, IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations
            ) {
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            if (cleanUpLocations == null) {
                PdfCleaner.CleanUpRedactAnnotations(pdfDocument);
            }
            else {
                PdfCleaner.CleanUp(pdfDocument, cleanUpLocations);
            }
            pdfDocument.Close();
        }

        private void CompareByContent(String cmp, String output, String targetDir, String fuzzValue) {
            CleanUpImagesCompareTool cmpTool = new CleanUpImagesCompareTool();
            cmpTool.UseGsImageExtracting(true);
            String errorMessage = cmpTool.ExtractAndCompareImages(output, cmp, targetDir, fuzzValue);
            String compareByContentResult = cmpTool.CompareByContent(output, cmp, targetDir);
            if (compareByContentResult != null) {
                errorMessage += compareByContentResult;
            }
            if (!errorMessage.Equals("")) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }
    }
}
