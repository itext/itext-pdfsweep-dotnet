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
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.PdfCleanup.Logs;
using iText.Test;
using iText.Test.Attributes;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("IntegrationTest")]
    public class CleanUpAnnotationTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/CleanUpAnnotationTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/CleanUpAnnotationTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void CleanFull01() {
            String input = inputPath + "cleanAnnotation.pdf";
            String output = outputPath + "cleanAnnotation_full01.pdf";
            String cmp = inputPath + "cmp_cleanAnnotation_full01.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(1, PageSize.A4, ColorConstants.WHITE));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_Annotation_full");
        }

        [NUnit.Framework.Test]
        public virtual void CleanLinkAnnotation01() {
            String input = inputPath + "cleanAnnotation.pdf";
            String output = outputPath + "cleanAnnotation_Link01.pdf";
            String cmp = inputPath + "cmp_cleanAnnotation_Link01.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            iText.PdfCleanup.PdfCleanUpLocation linkLoc = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(235
                , 740, 30, 16), ColorConstants.BLUE);
            cleanUpLocations.Add(linkLoc);
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_Annotation_link01");
        }

        [NUnit.Framework.Test]
        public virtual void CleanTextAnnotation01() {
            String input = inputPath + "cleanAnnotation.pdf";
            String output = outputPath + "cleanAnnotation_Text01.pdf";
            String cmp = inputPath + "cmp_cleanAnnotation_Text01.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            iText.PdfCleanup.PdfCleanUpLocation textLoc = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(150
                , 650, 0, 0), ColorConstants.RED);
            cleanUpLocations.Add(textLoc);
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_Annotation_text01");
        }

        [NUnit.Framework.Test]
        public virtual void CleanLineAnnotation01() {
            String input = inputPath + "cleanAnnotation.pdf";
            String output = outputPath + "cleanAnnotation_Line01.pdf";
            String cmp = inputPath + "cmp_cleanAnnotation_Line01.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            iText.PdfCleanup.PdfCleanUpLocation lineLoc = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(20, 
                20, 555, 0), ColorConstants.GREEN);
            cleanUpLocations.Add(lineLoc);
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_Annotation_line01");
        }

        [NUnit.Framework.Test]
        public virtual void CleanLineAnnotation02() {
            String input = inputPath + "lineAnnotationLeaders.pdf";
            String output = outputPath + "cleanLineAnnotation02.pdf";
            String cmp = inputPath + "cmp_cleanLineAnnotation02.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            iText.PdfCleanup.PdfCleanUpLocation lineLoc = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(100
                , 560, 200, 30), ColorConstants.GREEN);
            cleanUpLocations.Add(lineLoc);
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void CleanHighlightAnnotation01() {
            String input = inputPath + "cleanAnnotation.pdf";
            String output = outputPath + "cleanAnnotation_highlight01.pdf";
            String cmp = inputPath + "cmp_cleanAnnotation_highlight01.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            iText.PdfCleanup.PdfCleanUpLocation highLightLoc = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle
                (105, 500, 70, 10), ColorConstants.BLACK);
            cleanUpLocations.Add(highLightLoc);
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_text_highlight01");
        }

        [NUnit.Framework.Test]
        public virtual void CleanStrikeOutAnnotation01() {
            String input = inputPath + "strikeOutAnnotQuadOutsideRect.pdf";
            String output = outputPath + "cleanStrikeOutAnnotation01.pdf";
            String cmp = inputPath + "cmp_cleanStrikeOutAnnotation01.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(10, 490, 10, 30), ColorConstants
                .BLACK));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void CleanStrikeOutAnnotation02() {
            String input = inputPath + "strikeOutAnnotQuadOutsideRect.pdf";
            String output = outputPath + "cleanStrikeOutAnnotation02.pdf";
            String cmp = inputPath + "cmp_cleanStrikeOutAnnotation02.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(0, 0, 200, 200), ColorConstants
                .BLACK));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void CleanFreeTextAnnotation01() {
            String input = inputPath + "freeTextAnnotation.pdf";
            String output = outputPath + "cleanFreeTextAnnotation01.pdf";
            String cmp = inputPath + "cmp_cleanFreeTextAnnotation01.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(100, 560, 200, 30), ColorConstants
                .BLACK));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void CleanFormAnnotations01() {
            String input = inputPath + "formAnnotation.pdf";
            String output = outputPath + "formAnnotation01.pdf";
            String cmp = inputPath + "cmp_formAnnotation01.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            iText.PdfCleanup.PdfCleanUpLocation highLightLoc = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle
                (20, 600, 500, 170), ColorConstants.YELLOW);
            cleanUpLocations.Add(highLightLoc);
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_form01");
        }

        [NUnit.Framework.Test]
        public virtual void CleanFormAnnotations02() {
            String input = inputPath + "formAnnotation.pdf";
            String output = outputPath + "formAnnotation02.pdf";
            String cmp = inputPath + "cmp_formAnnotation02.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            iText.PdfCleanup.PdfCleanUpLocation highLightLoc = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle
                (20, 600, 300, 100), ColorConstants.YELLOW);
            cleanUpLocations.Add(highLightLoc);
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_form02");
        }

        [NUnit.Framework.Test]
        [LogMessage(CleanUpLogMessageConstant.REDACTION_OF_ANNOTATION_TYPE_WATERMARK_IS_NOT_SUPPORTED)]
        public virtual void CleanWatermarkAnnotation() {
            // TODO: update cmp file after DEVSIX-2471 fix
            String input = inputPath + "watermarkAnnotation.pdf";
            String output = outputPath + "watermarkAnnotation.pdf";
            String cmp = inputPath + "cmp_watermarkAnnotation.pdf";
            CleanUp(input, output, JavaCollectionsUtil.SingletonList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle
                (410, 410, 50, 50), ColorConstants.YELLOW)));
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
            CompareByContent(cmp, output, targetDir, null);
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
