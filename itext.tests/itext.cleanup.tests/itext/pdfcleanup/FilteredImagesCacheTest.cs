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
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using iText.PdfCleanup.Util;
using iText.Test;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("IntegrationTest")]
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
            PdfCleaner.CleanUp(pdfDocument, cleanUpLocations);
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
