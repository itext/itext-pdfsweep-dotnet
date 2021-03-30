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
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using iText.PdfCleanup.Util;
using iText.Test;

namespace iText.PdfCleanup {
    public class FilteredImagesCacheTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/FilteredImagesCacheTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/FilteredImagesCacheTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void FilteredImagesCacheTest01() {
            // basic test with reusing of xobjects
            String input = inputPath + "multipleImageXObjectOccurrences.pdf";
            String output = outputPath + "filteredImagesCacheTest01.pdf";
            String cmp = inputPath + "cmp_filteredImagesCacheTest01.pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            for (int i = 0; i < pdfDocument.GetNumberOfPages(); ++i) {
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(150, 300, 300, 150)));
            }
            CleanUp(pdfDocument, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "1.2");
            AssertNumberXObjects(output, 1);
        }

        [NUnit.Framework.Test]
        public virtual void FilteredImagesCacheTest02() {
            // reusing when several clean areas (different on different pages)
            String input = inputPath + "multipleImageXObjectOccurrences.pdf";
            String output = outputPath + "filteredImagesCacheTest02.pdf";
            String cmp = inputPath + "cmp_filteredImagesCacheTest02.pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            for (int i = 0; i < pdfDocument.GetNumberOfPages(); i += 5) {
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(350, 450, 300, 40)));
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(300, 400, 50, 150)));
            }
            for (int i = 1; i < pdfDocument.GetNumberOfPages(); i += 5) {
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(350, 450, 300, 20)));
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(350, 490, 300, 20)));
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(350, 530, 300, 20)));
            }
            for (int i = 3; i < pdfDocument.GetNumberOfPages(); i += 5) {
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(300, 400, 50, 150)));
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(350, 400, 50, 150)));
            }
            for (int i = 4; i < pdfDocument.GetNumberOfPages(); i += 5) {
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(350, 450, 300, 20)));
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(350, 450, 300, 20)));
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(350, 450, 300, 20)));
            }
            CleanUp(pdfDocument, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "1.2");
            AssertNumberXObjects(output, 5);
        }

        [NUnit.Framework.Test]
        public virtual void FilteredImagesCacheTest03() {
            // same areas, different src images
            String input = inputPath + "multipleDifferentImageXObjectOccurrences.pdf";
            String output = outputPath + "filteredImagesCacheTest03.pdf";
            String cmp = inputPath + "cmp_filteredImagesCacheTest03.pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            for (int i = 0; i < pdfDocument.GetNumberOfPages(); ++i) {
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(150, 300, 300, 150)));
            }
            CleanUp(pdfDocument, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "1.2");
            AssertNumberXObjects(output, 2);
        }

        [NUnit.Framework.Test]
        public virtual void FilteredImagesCacheTest04() {
            // same image with different scaling and the same resultant image area
            String input = inputPath + "multipleScaledImageXObjectOccurrences.pdf";
            String output = outputPath + "filteredImagesCacheTest04.pdf";
            String cmp = inputPath + "cmp_filteredImagesCacheTest04.pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            float x = 559;
            float y = 600.2f;
            float width = 100;
            float height = 100;
            Rectangle region1 = new Rectangle(x - width, y - height, width, height);
            float scaleFactor = 1.2f;
            width *= scaleFactor;
            height *= scaleFactor;
            Rectangle region2 = new Rectangle(x - width, y - height, width, height);
            for (int i = 0; i < pdfDocument.GetNumberOfPages(); i += 2) {
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, region1));
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 2, region2));
            }
            CleanUp(pdfDocument, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "1.2");
            AssertNumberXObjects(output, 1);
        }

        [NUnit.Framework.Test]
        public virtual void FilteredImagesCacheFlushingTest01() {
            String input = inputPath + "severalImageXObjectOccurrences.pdf";
            String output = outputPath + "filteredImagesCacheFlushingTest01.pdf";
            String cmp = inputPath + "cmp_filteredImagesCacheFlushingTest01.pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            iText.PdfCleanup.PdfCleanUpTool cleanUpTool = new iText.PdfCleanup.PdfCleanUpTool(pdfDocument);
            cleanUpTool.AddCleanupLocation(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(150, 300, 300, 150
                )));
            cleanUpTool.CleanUp();
            PdfImageXObject img = pdfDocument.GetPage(2).GetResources().GetImage(new PdfName("Im1"));
            img.GetPdfObject().Release();
            cleanUpTool.AddCleanupLocation(new iText.PdfCleanup.PdfCleanUpLocation(2, new Rectangle(150, 300, 300, 150
                )));
            cleanUpTool.CleanUp();
            cleanUpTool.AddCleanupLocation(new iText.PdfCleanup.PdfCleanUpLocation(3, new Rectangle(150, 300, 300, 150
                )));
            cleanUpTool.CleanUp();
            pdfDocument.Close();
            CompareByContent(cmp, output, outputPath, "1.2");
            AssertNumberXObjects(output, 1);
        }

        [NUnit.Framework.Test]
        public virtual void FilteredImagesCacheFlushingTest02() {
            String input = inputPath + "severalImageXObjectOccurrences.pdf";
            String output = outputPath + "filteredImagesCacheFlushingTest02.pdf";
            String cmp = inputPath + "cmp_filteredImagesCacheFlushingTest02.pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            iText.PdfCleanup.PdfCleanUpTool cleanUpTool = new iText.PdfCleanup.PdfCleanUpTool(pdfDocument);
            cleanUpTool.AddCleanupLocation(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(150, 300, 300, 150
                )));
            cleanUpTool.CleanUp();
            PdfImageXObject img = pdfDocument.GetPage(1).GetResources().GetImage(new PdfName("Im1"));
            img.MakeIndirect(pdfDocument).Flush();
            cleanUpTool.AddCleanupLocation(new iText.PdfCleanup.PdfCleanUpLocation(2, new Rectangle(150, 300, 300, 150
                )));
            cleanUpTool.CleanUp();
            cleanUpTool.AddCleanupLocation(new iText.PdfCleanup.PdfCleanUpLocation(3, new Rectangle(150, 300, 300, 150
                )));
            cleanUpTool.CleanUp();
            pdfDocument.Close();
            CompareByContent(cmp, output, outputPath, "1.2");
            AssertNumberXObjects(output, 1);
        }

        private void CleanUp(PdfDocument pdfDocument, IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations) {
            new iText.PdfCleanup.PdfCleanUpTool(pdfDocument, cleanUpLocations).CleanUp();
            pdfDocument.Close();
        }

        private void AssertNumberXObjects(String output, int n) {
            PdfDocument doc = new PdfDocument(new PdfReader(output));
            int xObjCount = 0;
            for (int i = 0; i < doc.GetNumberOfPdfObjects(); ++i) {
                PdfObject pdfObject = doc.GetPdfObject(i);
                if (pdfObject != null && pdfObject.IsStream()) {
                    PdfDictionary dict = (PdfDictionary)pdfObject;
                    if (PdfName.Image.Equals(dict.GetAsName(PdfName.Subtype)) && dict.ContainsKey(PdfName.Width) && dict.ContainsKey
                        (PdfName.Height)) {
                        ++xObjCount;
                    }
                }
            }
            NUnit.Framework.Assert.AreEqual(n, xObjCount);
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
