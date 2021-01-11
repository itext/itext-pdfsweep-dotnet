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
using System.Collections;
using System.IO;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.PdfCleanup.Autosweep;
using iText.Test;
using iText.Test.Attributes;

namespace iText.PdfCleanup {
    public class PdfAutoSweepTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/PdfAutoSweepTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/PdfAutoSweepTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void RedactLipsum() {
            String input = inputPath + "Lipsum.pdf";
            String output = outputPath + "cleanUpDocument.pdf";
            String cmp = inputPath + "cmp_cleanUpDocument.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(D|d)olor").SetRedactionColor(ColorConstants.GREEN));
            PdfWriter writer = new PdfWriter(output);
            writer.SetCompressionLevel(0);
            PdfDocument pdf = new PdfDocument(new PdfReader(input), writer);
            // sweep
            PdfAutoSweep autoSweep = new PdfAutoSweep(strategy);
            autoSweep.CleanUp(pdf);
            pdf.Close();
            // compare
            CompareByContent(cmp, output, outputPath, "diff_cleanUpDocument_");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpPageTest() {
            String input = inputPath + "Lipsum.pdf";
            String output = outputPath + "cleanUpPage.pdf";
            String cmp = inputPath + "cmp_cleanUpPage.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(D|d)olor").SetRedactionColor(ColorConstants.GREEN));
            PdfWriter writer = new PdfWriter(output);
            writer.SetCompressionLevel(0);
            PdfDocument pdf = new PdfDocument(new PdfReader(input), writer);
            // sweep
            PdfAutoSweep autoSweep = new PdfAutoSweep(strategy);
            autoSweep.CleanUp(pdf.GetPage(1));
            pdf.Close();
            // compare
            CompareByContent(cmp, output, outputPath, "diff_cleanUpPage_");
        }

        [NUnit.Framework.Test]
        public virtual void TentativeCleanUpTest() {
            String input = inputPath + "Lipsum.pdf";
            String output = outputPath + "tentativeCleanUp.pdf";
            String cmp = inputPath + "cmp_tentativeCleanUp.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(D|d)olor").SetRedactionColor(ColorConstants.GREEN));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(output).SetCompressionLevel(0));
            // sweep
            PdfAutoSweep autoSweep = new PdfAutoSweep(strategy);
            autoSweep.TentativeCleanUp(pdf);
            pdf.Close();
            // compare
            CompareByContent(cmp, output, outputPath, "diff_tentativeCleanUp_");
        }

        [NUnit.Framework.Test]
        public virtual void GetPdfCleanUpLocationsTest() {
            String input = inputPath + "Lipsum.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(D|d)olor"));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(new MemoryStream()));
            // sweep
            PdfAutoSweep autoSweep = new PdfAutoSweep(strategy);
            IList cleanUpLocations = (IList)autoSweep.GetPdfCleanUpLocations(pdf.GetPage(1));
            pdf.Close();
            // compare
            NUnit.Framework.Assert.AreEqual(2, cleanUpLocations.Count);
        }

        [NUnit.Framework.Test]
        public virtual void HighlightTest() {
            String input = inputPath + "Lipsum.pdf";
            String output = outputPath + "highlightTest.pdf";
            String cmp = inputPath + "cmp_highlightTest.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(D|d)olor").SetRedactionColor(ColorConstants.GREEN));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(output).SetCompressionLevel(CompressionConstants
                .NO_COMPRESSION));
            // sweep
            PdfAutoSweep autoSweep = new PdfAutoSweep(strategy);
            autoSweep.Highlight(pdf);
            pdf.Close();
            // compare
            CompareByContent(cmp, output, outputPath, "diff_highlightTest_");
        }

        [NUnit.Framework.Test]
        public virtual void RedactLipsumPatternStartsWithWhiteSpace() {
            String input = inputPath + "Lipsum.pdf";
            String output = outputPath + "redactLipsumPatternStartsWithWhitespace.pdf";
            String cmp = inputPath + "cmp_redactLipsumPatternStartsWithWhitespace.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("\\s(D|d)olor").SetRedactionColor(ColorConstants.GREEN));
            PdfWriter writer = new PdfWriter(output);
            writer.SetCompressionLevel(0);
            PdfDocument pdf = new PdfDocument(new PdfReader(input), writer);
            // sweep
            PdfAutoSweep autoSweep = new PdfAutoSweep(strategy);
            autoSweep.CleanUp(pdf);
            pdf.Close();
            // compare
            CompareByContent(cmp, output, outputPath, "diff_redactLipsumPatternStartsWithWhitespace_");
        }

        [NUnit.Framework.Test]
        [LogMessage(CleanUpLogMessageConstant.FAILED_TO_PROCESS_A_TRANSFORMATION_MATRIX, Count = 2)]
        public virtual void RedactPdfWithNoninvertibleMatrix() {
            String input = inputPath + "noninvertibleMatrix.pdf";
            String output = outputPath + "redactPdfWithNoninvertibleMatrix.pdf";
            String cmp = inputPath + "cmp_redactPdfWithNoninvertibleMatrix.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("Hello World!").SetRedactionColor(ColorConstants.GREEN));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            // sweep
            PdfAutoSweep autoSweep = new PdfAutoSweep(strategy);
            autoSweep.CleanUp(pdf);
            pdf.Close();
            // compare
            CompareByContent(cmp, output, outputPath, "diff_redactPdfWithNoninvertibleMatrix_");
        }

        [NUnit.Framework.Test]
        [NUnit.Framework.Ignore("DEVSIX-4047")]
        public virtual void LineArtsDrawingOnCanvasTest() {
            String input = inputPath + "lineArtsDrawingOnCanvas.pdf";
            String output = outputPath + "lineArtsDrawingOnCanvas.pdf";
            String cmp = inputPath + "cmp_lineArtsDrawingOnCanvas.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(iphone)|(iPhone)"));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            PdfAutoSweep autoSweep = new PdfAutoSweep(strategy);
            autoSweep.CleanUp(pdf);
            pdf.Close();
            CompareByContent(cmp, output, outputPath, "diff_lineArtsDrawingOnCanvasTest_");
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
