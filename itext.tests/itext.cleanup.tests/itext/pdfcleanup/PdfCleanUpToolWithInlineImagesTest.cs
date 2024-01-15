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
using NUnit.Framework;
using iText.Commons.Utils;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.PdfCleanup.Util;
using iText.Test;
using iText.Test.Attributes;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("IntegrationTest")]
    public class PdfCleanUpToolWithInlineImagesTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/PdfCleanUpToolWithInlineImagesTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/PdfCleanUpToolWithInlineImagesTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        [LogMessage(iText.IO.Logs.IoLogMessageConstant.IMAGE_SIZE_CANNOT_BE_MORE_4KB)]
        [Ignore("DEVSIX-1617: System.Drawing.Image creates a Bitmap image object with fixed pixel format. If you try to get Graphics from such an image you'll get an exception." )]
        public virtual void CleanUpTest28() {
            String input = inputPath + "inlineImages.pdf";
            String output = outputPath + "inlineImages_partial.pdf";
            String cmp = inputPath + "cmp_inlineImages_partial.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(62, 100, 20, 800), null));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_28");
        }

        [NUnit.Framework.Test]
        [Ignore("DEVSIX-1617: System.Drawing.Image creates a Bitmap image object with fixed pixel format. If you try to get Graphics from such an image you'll get an exception." )]
        public virtual void CleanUpTest29() {
            String input = inputPath + "inlineImages.pdf";
            String output = outputPath + "inlineImages_partial2.pdf";
            String cmp = inputPath + "cmp_inlineImages_partial2.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(10, 100, 70, 599), null));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_29");
        }

        [NUnit.Framework.Test]
        [LogMessage(iText.IO.Logs.IoLogMessageConstant.IMAGE_SIZE_CANNOT_BE_MORE_4KB)]
        public virtual void CleanUpTest31() {
            String input = inputPath + "inlineImageCleanup.pdf";
            String output = outputPath + "inlineImageCleanup.pdf";
            String cmp = inputPath + "cmp_inlineImageCleanup.pdf";
            CleanUp(input, output, null);
            CleanUpImagesCompareTool cmpTool = new CleanUpImagesCompareTool();
            String errorMessage = cmpTool.ExtractAndCompareImages(output, cmp, outputPath, "1");
            String compareByContentResult = cmpTool.CompareByContent(output, cmp, outputPath);
            if (compareByContentResult != null) {
                errorMessage += compareByContentResult;
            }
            if (!errorMessage.Equals("")) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
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

        private void CompareByContent(String cmp, String output, String targetDir, String diffPrefix) {
            CompareTool cmpTool = new CompareTool();
            String errorMessage = cmpTool.CompareByContent(output, cmp, targetDir, diffPrefix + "_");
            if (errorMessage != null) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }
    }
}
