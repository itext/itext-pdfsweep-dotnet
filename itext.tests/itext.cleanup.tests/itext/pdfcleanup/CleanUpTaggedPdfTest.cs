/*
This file is part of the iText (R) project.
Copyright (c) 1998-2021 iText Group NV
Authors: iText Software.

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License version 3
as published by the Free Software Foundation with the addition of the
following permission added to Section 15 as permitted in Section 7(a):
FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
OF THIRD PARTY RIGHTS

This program is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License
along with this program; if not, see http://www.gnu.org/licenses or write to
the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
Boston, MA, 02110-1301 USA, or download the license from the following URL:
http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions
of this program must display Appropriate Legal Notices, as required under
Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License,
a covered work must retain the producer line in every PDF that is created
or manipulated using iText.

You can be released from the requirements of the license by purchasing
a commercial license. Buying such a license is mandatory as soon as you
develop commercial activities involving the iText software without
disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP,
serving PDFs on the fly in a web application, shipping iText with a closed
source product.

For more information, please contact iText Software Corp. at this
address: sales@itextpdf.com
*/
using System;
using System.Collections.Generic;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Test;

namespace iText.PdfCleanup {
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
            iText.PdfCleanup.PdfCleanUpTool cleaner = (cleanUpLocations == null) ? new iText.PdfCleanup.PdfCleanUpTool
                (pdfDocument, true) : new iText.PdfCleanup.PdfCleanUpTool(pdfDocument, cleanUpLocations);
            cleaner.CleanUp();
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
