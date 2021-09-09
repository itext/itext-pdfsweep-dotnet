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

namespace iText.PdfCleanup {
    public class PdfAutoSweepToolsTest : ExtendedITextTest {
        private static readonly String INPUT_PATH = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/PdfAutoSweepTest/";

        private static readonly String OUTPUT_PATH = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/PdfAutoSweepTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(OUTPUT_PATH);
        }

        [NUnit.Framework.Test]
        public virtual void TentativeCleanUpTest() {
            String input = INPUT_PATH + "Lipsum.pdf";
            String output = OUTPUT_PATH + "tentativeCleanUp.pdf";
            String cmp = INPUT_PATH + "cmp_tentativeCleanUp.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(D|d)olor").SetRedactionColor(ColorConstants.GREEN));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(output).SetCompressionLevel(0));
            // sweep
            PdfAutoSweepTools autoSweep = new PdfAutoSweepTools(strategy);
            autoSweep.TentativeCleanUp(pdf);
            pdf.Close();
            // compare
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_tentativeCleanUp_");
        }

        [NUnit.Framework.Test]
        public virtual void HighlightTest() {
            String input = INPUT_PATH + "Lipsum.pdf";
            String output = OUTPUT_PATH + "highlightTest.pdf";
            String cmp = INPUT_PATH + "cmp_highlightTest.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(D|d)olor").SetRedactionColor(ColorConstants.GREEN));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(output).SetCompressionLevel(CompressionConstants
                .NO_COMPRESSION));
            // sweep
            PdfAutoSweepTools autoSweep = new PdfAutoSweepTools(strategy);
            autoSweep.Highlight(pdf);
            pdf.Close();
            // compare
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_highlightTest_");
        }

        [NUnit.Framework.Test]
        public virtual void GetPdfCleanUpLocationsTest() {
            String input = INPUT_PATH + "Lipsum.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(D|d)olor"));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(new MemoryStream()));
            // sweep
            IList cleanUpLocations = (IList)new PdfAutoSweepTools(strategy).GetPdfCleanUpLocations(pdf.GetPage(1));
            pdf.Close();
            // compare
            NUnit.Framework.Assert.AreEqual(2, cleanUpLocations.Count);
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
