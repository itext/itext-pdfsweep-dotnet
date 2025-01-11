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
using System.Collections.Generic;
using iText.Commons.Utils;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.PdfCleanup;
using iText.Test;
using iText.Test.Attributes;

namespace iText.PdfCleanup.Text {
    [NUnit.Framework.Category("IntegrationTest")]
    public class CleanUpTextTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/text/CleanUpTextTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/text/CleanUpTextTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        [LogMessage(iText.IO.Logs.IoLogMessageConstant.FONT_DICTIONARY_WITH_NO_FONT_DESCRIPTOR)]
        [LogMessage(iText.IO.Logs.IoLogMessageConstant.FONT_DICTIONARY_WITH_NO_WIDTHS)]
        public virtual void CleanZeroWidthTextInvalidFont() {
            String input = inputPath + "cleanZeroWidthTextInvalidFont.pdf";
            String output = outputPath + "cleanZeroWidthTextInvalidFont.pdf";
            String cmp = inputPath + "cmp_cleanZeroWidthTextInvalidFont.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(50, 
                50, 500, 500))));
            CompareByContent(cmp, output, outputPath);
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

        private void CompareByContent(String cmp, String output, String targetDir) {
            CompareTool cmpTool = new CompareTool();
            String errorMessage = cmpTool.CompareByContent(output, cmp, targetDir);
            if (errorMessage != null) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }
    }
}
