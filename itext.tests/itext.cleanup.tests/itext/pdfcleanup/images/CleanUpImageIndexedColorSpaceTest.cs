/*
This file is part of the iText (R) project.
Copyright (c) 1998-2021 iText Group NV
Authors: iText Software.

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
using iText.IO.Util;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Test;

namespace iText.PdfCleanup.Images {
    public class CleanUpImageIndexedColorSpaceTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/images/CleanUpImageIndexedColorSpaceTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/images/CleanUpImageIndexedColorSpaceTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void NoWhiteColorTest() {
            String input = inputPath + "indexedImageNoWhite.pdf";
            String output = outputPath + "indexedImageNoWhite.pdf";
            String cmp = inputPath + "cmp_indexedImageNoWhite.pdf";
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output))) {
                new iText.PdfCleanup.PdfCleanUpTool(pdfDocument, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                    (1, new Rectangle(150, 250, 100, 100)))).CleanUp();
            }
            /*
            Result in Java and .NET is different.
            
            Java is able to process images with indexed colorspace same as others and
            doesn't preserve indexed colorspace. .NET requires special processing for
            indexed colorspace images, but preserves indexed colorspace.
            
            In .NET color of cleaned area is the first color of indexed color palette.
            In Java color of cleaned area is white.
            */
            NUnit.Framework.Assert.IsNull(new CompareTool().CompareByContent(output, cmp, outputPath));
        }
    }
}
