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
using NUnit.Framework;
using iText.IO.Util;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.PdfCleanup.Util;
using iText.Test;
using iText.Test.Attributes;

namespace iText.PdfCleanup {
    public class PdfCleanUpToolWithInlineImagesTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/PdfCleanUpToolWithInlineImagesTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/PdfCleanUpToolWithInlineImagesTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        [LogMessage(iText.IO.LogMessageConstant.IMAGE_SIZE_CANNOT_BE_MORE_4KB)]
        [Ignore("DEVSIX-1617: System.Drawing.Image creates a Bitmap image object with fixed pixel format. If you try to get Graphics from such an image you'll get an exception." )]
        public virtual void CleanUpTest28() {
            String input = inputPath + "inlineImages.pdf";
            String output = outputPath + "inlineImages_partial.pdf";
            String cmp = inputPath + "cmp_inlineImages_partial.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(62, 100, 20, 800), null));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_28");
        }

        [NUnit.Framework.Test]
        [Ignore("DEVSIX-1617: System.Drawing.Image creates a Bitmap image object with fixed pixel format. If you try to get Graphics from such an image you'll get an exception." )]
        public virtual void CleanUpTest29() {
            String input = inputPath + "inlineImages.pdf";
            String output = outputPath + "inlineImages_partial2.pdf";
            String cmp = inputPath + "cmp_inlineImages_partial2.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(10, 100, 70, 599), null));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_29");
        }

        [NUnit.Framework.Test]
        [LogMessage(iText.IO.LogMessageConstant.IMAGE_SIZE_CANNOT_BE_MORE_4KB)]
        public virtual void CleanUpTest31() {
            String input = inputPath + "inlineImageCleanup.pdf";
            String output = outputPath + "inlineImageCleanup.pdf";
            String cmp = inputPath + "cmp_inlineImageCleanup.pdf";
            CleanUp(input, output, null);
            CleanUpImagesCompareTool cmpTool = new CleanUpImagesCompareTool();
            String errorMessage = cmpTool.ExtractAndCompareImages(output, cmp, outputPath, "1");
            String compareByContentResult = cmpTool.CompareByContent(output, cmp, outputPath);
            if (compareByContentResult != null) {
                errorMessage += compareByContentResult;
            }
            if (!errorMessage.Equals("")) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
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
