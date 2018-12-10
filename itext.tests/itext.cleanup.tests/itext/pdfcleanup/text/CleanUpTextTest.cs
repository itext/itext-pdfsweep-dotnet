using System;
using System.Collections.Generic;
using iText.IO.Util;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.PdfCleanup;
using iText.Test;
using iText.Test.Attributes;

namespace iText.PdfCleanup.Text {
    public class CleanUpTextTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/text/CleanUpTextTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/text/CleanUpTextTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        [LogMessage(iText.IO.LogMessageConstant.FONT_DICTIONARY_WITH_NO_FONT_DESCRIPTOR)]
        [LogMessage(iText.IO.LogMessageConstant.FONT_DICTIONARY_WITH_NO_WIDTHS)]
        public virtual void CleanZeroWidthTextInvalidFont() {
            String input = inputPath + "cleanZeroWidthTextInvalidFont.pdf";
            String output = outputPath + "cleanZeroWidthTextInvalidFont.pdf";
            String cmp = inputPath + "cmp_cleanZeroWidthTextInvalidFont.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new PdfCleanUpLocation(1, new Rectangle(50, 50, 500, 500))));
            CompareByContent(cmp, output, outputPath);
        }

        /// <exception cref="System.IO.IOException"/>
        private void CleanUp(String input, String output, IList<PdfCleanUpLocation> cleanUpLocations) {
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            PdfCleanUpTool cleaner = (cleanUpLocations == null) ? new PdfCleanUpTool(pdfDocument, true) : new PdfCleanUpTool
                (pdfDocument, cleanUpLocations);
            cleaner.CleanUp();
            pdfDocument.Close();
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        private void CompareByContent(String cmp, String output, String targetDir) {
            CompareTool cmpTool = new CompareTool();
            String errorMessage = cmpTool.CompareByContent(output, cmp, targetDir);
            if (errorMessage != null) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }
    }
}
