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
using System.Collections.Generic;
using iText.Commons.Utils;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.PdfCleanup.Util;
using iText.Test;
using iText.Test.Attributes;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("IntegrationTest")]
    public class BigDocumentCleanUpTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/BigDocumentCleanUpTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/BigDocumentCleanUpTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void BigUntaggedDocument() {
            String input = inputPath + "iphone_user_guide_untagged.pdf";
            String output = outputPath + "bigUntaggedDocument.pdf";
            String cmp = inputPath + "cmp_bigUntaggedDocument.pdf";
            IList<Rectangle> rects = JavaUtil.ArraysAsList(new Rectangle(60f, 80f, 460f, 65f), new Rectangle(300f, 370f
                , 215f, 260f));
            CleanUp(input, output, InitLocations(rects, 130));
            CompareByContent(cmp, output, outputPath, "4");
        }

        [NUnit.Framework.Test]
        [LogMessage(iText.IO.Logs.IoLogMessageConstant.CREATED_ROOT_TAG_HAS_MAPPING)]
        public virtual void BigTaggedDocument() {
            String input = inputPath + "chapter8_Interactive_features.pdf";
            String output = outputPath + "bigTaggedDocument.pdf";
            String cmp = inputPath + "cmp_bigTaggedDocument.pdf";
            IList<Rectangle> rects = JavaUtil.ArraysAsList(new Rectangle(60f, 80f, 460f, 65f), new Rectangle(300f, 370f
                , 215f, 270f));
            CleanUp(input, output, InitLocations(rects, 131));
            CompareByContent(cmp, output, outputPath, "4");
        }

        [NUnit.Framework.Test]
        public virtual void TextPositioning() {
            String input = inputPath + "textPositioning.pdf";
            String output = outputPath + "textPositioning.pdf";
            String cmp = inputPath + "cmp_textPositioning.pdf";
            IList<Rectangle> rects = JavaUtil.ArraysAsList(new Rectangle(0f, 0f, 1f, 1f));
            // just to enable cleanup processing of the pages
            CleanUp(input, output, InitLocations(rects, 163));
            CompareByContent(cmp, output, outputPath, "4");
        }

        private void CleanUp(String input, String output, IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations
            ) {
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            PdfCleaner.CleanUp(pdfDocument, cleanUpLocations);
            pdfDocument.Close();
        }

        private IList<iText.PdfCleanup.PdfCleanUpLocation> InitLocations(IList<Rectangle> rects, int pagesNum) {
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            for (int i = 0; i < pagesNum; ++i) {
                for (int j = 0; j < rects.Count; ++j) {
                    cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, rects[j]));
                }
            }
            return cleanUpLocations;
        }

        private void CompareByContent(String cmp, String output, String targetDir, String fuzzValue) {
            CleanUpImagesCompareTool cmpTool = new CleanUpImagesCompareTool();
            String errorMessage = cmpTool.ExtractAndCompareImages(output, cmp, targetDir, fuzzValue);
            String compareByContentResult = cmpTool.CompareByContent(output, cmp, targetDir);
            if (compareByContentResult != null) {
                errorMessage += compareByContentResult;
            }
            if (!errorMessage.Equals("")) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }
    }
}
