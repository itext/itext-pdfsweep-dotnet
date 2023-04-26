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
using System.Collections;
using System.IO;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.PdfCleanup.Autosweep;
using iText.Test;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("IntegrationTest")]
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
