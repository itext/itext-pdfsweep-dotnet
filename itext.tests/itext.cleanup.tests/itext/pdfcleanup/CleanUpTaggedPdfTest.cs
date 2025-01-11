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
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Test;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("IntegrationTest")]
    public class CleanUpTaggedPdfTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/CleanUpTaggedPdfTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/CleanUpTaggedPdfTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void CleanTextFull() {
            String input = inputPath + "cleanText_full.pdf";
            String output = outputPath + "cleanText_full.pdf";
            String cmp = inputPath + "cmp_cleanText_full.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_text_full");
        }

        [NUnit.Framework.Test]
        public virtual void CleanTextPartial() {
            String input = inputPath + "cleanText_partial.pdf";
            String output = outputPath + "cleanText_partial.pdf";
            String cmp = inputPath + "cmp_cleanText_partial.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_text_partial");
        }

        [NUnit.Framework.Test]
        public virtual void CleanImageFull() {
            String input = inputPath + "cleanImage_full.pdf";
            String output = outputPath + "cleanImage_full.pdf";
            String cmp = inputPath + "cmp_cleanImage_full.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_image_full");
        }

        [NUnit.Framework.Test]
        public virtual void CleanImagePartial() {
            String input = inputPath + "cleanImage_partial.pdf";
            String output = outputPath + "cleanImage_partial.pdf";
            String cmp = inputPath + "cmp_cleanImage_partial.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_image_partial");
        }

        [NUnit.Framework.Test]
        public virtual void CleanPathFull() {
            String input = inputPath + "cleanPath_full.pdf";
            String output = outputPath + "cleanPath_full.pdf";
            String cmp = inputPath + "cmp_cleanPath_full.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_path_full");
        }

        [NUnit.Framework.Test]
        public virtual void CleanPathPartial() {
            String input = inputPath + "cleanPath_partial.pdf";
            String output = outputPath + "cleanPath_partial.pdf";
            String cmp = inputPath + "cmp_cleanPath_partial.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_path_partial");
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
