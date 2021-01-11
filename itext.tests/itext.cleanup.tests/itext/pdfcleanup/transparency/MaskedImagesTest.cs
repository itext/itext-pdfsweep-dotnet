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
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Utils;
using iText.PdfCleanup.Util;
using iText.Test;

namespace iText.PdfCleanup.Transparency {
    public class MaskedImagesTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/transparency/MaskedImagesTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/transparency/MaskedImagesTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void ImageTransparencyImageMask() {
            RunTest("imageIsMask", "0");
        }

        [NUnit.Framework.Test]
        public virtual void ImageTransparencyMask() {
            RunTest("imageMask", "1");
        }

        [NUnit.Framework.Test]
        public virtual void ImageTransparencySMask() {
            RunTest("imageSMask", "1");
        }

        [NUnit.Framework.Test]
        public virtual void ImageTransparencySMaskAIS() {
            RunTest("imageSMaskAIS", "1");
        }

        [NUnit.Framework.Test]
        public virtual void ImageTransparencyColorKeyMaskArray() {
            RunTest("imageColorKeyMaskArray", "1");
        }

        [NUnit.Framework.Test]
        public virtual void ImageTransparencyTextOnTransparentField() {
            String fileName = "textOnTransparentField";
            String input = inputPath + fileName + ".pdf";
            String output = outputPath + fileName + "_cleaned.pdf";
            String cmp = inputPath + "cmp_" + fileName + ".pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaCollectionsUtil.SingletonList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(280, 360, 200, 75)));
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            iText.PdfCleanup.PdfCleanUpTool cleaner = new iText.PdfCleanup.PdfCleanUpTool(pdfDocument, cleanUpLocations
                );
            cleaner.CleanUp();
            new PdfCanvas(pdfDocument.GetFirstPage().NewContentStreamBefore(), pdfDocument.GetFirstPage().GetResources
                (), pdfDocument).SetColor(ColorConstants.LIGHT_GRAY, true).Rectangle(0, 0, 1000, 1000).Fill().SetColor
                (ColorConstants.BLACK, true);
            pdfDocument.Close();
            NUnit.Framework.Assert.IsNull(new CompareTool().CompareByContent(output, cmp, outputPath));
        }

        private static void RunTest(String fileName, String fuzzValue) {
            String input = inputPath + fileName + ".pdf";
            String output = outputPath + fileName + "_cleaned.pdf";
            String cmp = inputPath + "cmp_" + fileName + ".pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaCollectionsUtil.SingletonList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(308, 520, 200, 75)));
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            iText.PdfCleanup.PdfCleanUpTool cleaner = new iText.PdfCleanup.PdfCleanUpTool(pdfDocument, cleanUpLocations
                );
            cleaner.CleanUp();
            pdfDocument.Close();
            CleanUpImagesCompareTool cmpTool = new CleanUpImagesCompareTool();
            String errorMessage = cmpTool.ExtractAndCompareImages(output, cmp, outputPath, fuzzValue);
            String compareByContentResult = cmpTool.CompareByContent(output, cmp, outputPath);
            if (compareByContentResult != null) {
                errorMessage += compareByContentResult;
            }
            if (!errorMessage.Equals("")) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }
    }
}
