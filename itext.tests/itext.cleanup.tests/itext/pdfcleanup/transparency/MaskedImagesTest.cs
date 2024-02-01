/*
This file is part of the iText (R) project.
Copyright (c) 1998-2024 Apryse Group NV
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
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Utils;
using iText.PdfCleanup;
using iText.PdfCleanup.Util;
using iText.Test;

namespace iText.PdfCleanup.Transparency {
    [NUnit.Framework.Category("IntegrationTest")]
    public class MaskedImagesTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/transparency/MaskedImagesTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/transparency/MaskedImagesTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void ImageTransparencyImageMask() {
            RunTest("imageIsMask", "0");
        }

        [NUnit.Framework.Test]
        public virtual void ImageTransparencyMask() {
            RunTest("imageMask", "1");
        }

        [NUnit.Framework.Test]
        public virtual void ImageTransparencySMask() {
            RunTest("imageSMask", "1");
        }

        [NUnit.Framework.Test]
        public virtual void ImageTransparencySMaskAIS() {
            RunTest("imageSMaskAIS", "1");
        }

        [NUnit.Framework.Test]
        public virtual void ImageTransparencyColorKeyMaskArray() {
            RunTest("imageColorKeyMaskArray", "1");
        }

        [NUnit.Framework.Test]
        public virtual void ImageTransparencyTextOnTransparentField() {
            String fileName = "textOnTransparentField";
            String input = inputPath + fileName + ".pdf";
            String output = outputPath + fileName + "_cleaned.pdf";
            String cmp = inputPath + "cmp_" + fileName + ".pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaCollectionsUtil.SingletonList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(280, 360, 200, 75)));
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            PdfCleaner.CleanUp(pdfDocument, cleanUpLocations);
            new PdfCanvas(pdfDocument.GetFirstPage().NewContentStreamBefore(), pdfDocument.GetFirstPage().GetResources
                (), pdfDocument).SetColor(ColorConstants.LIGHT_GRAY, true).Rectangle(0, 0, 1000, 1000).Fill().SetColor
                (ColorConstants.BLACK, true);
            pdfDocument.Close();
            NUnit.Framework.Assert.IsNull(new CompareTool().CompareByContent(output, cmp, outputPath));
        }

        private static void RunTest(String fileName, String fuzzValue) {
            String input = inputPath + fileName + ".pdf";
            String output = outputPath + fileName + "_cleaned.pdf";
            String cmp = inputPath + "cmp_" + fileName + ".pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaCollectionsUtil.SingletonList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(308, 520, 200, 75)));
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            PdfCleaner.CleanUp(pdfDocument, cleanUpLocations);
            pdfDocument.Close();
            CleanUpImagesCompareTool cmpTool = new CleanUpImagesCompareTool();
            String errorMessage = cmpTool.ExtractAndCompareImages(output, cmp, outputPath, fuzzValue);
            String compareByContentResult = cmpTool.CompareByContent(output, cmp, outputPath);
            if (compareByContentResult != null) {
                errorMessage += compareByContentResult;
            }
            if (!errorMessage.Equals("")) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }
    }
}
