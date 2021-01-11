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
using iText.IO.Util;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.PdfCleanup.Util;
using iText.Test;
using iText.Test.Attributes;

namespace iText.PdfCleanup {
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
        [LogMessage(iText.IO.LogMessageConstant.CREATED_ROOT_TAG_HAS_MAPPING)]
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
            iText.PdfCleanup.PdfCleanUpTool cleaner = new iText.PdfCleanup.PdfCleanUpTool(pdfDocument, cleanUpLocations
                );
            cleaner.CleanUp();
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
