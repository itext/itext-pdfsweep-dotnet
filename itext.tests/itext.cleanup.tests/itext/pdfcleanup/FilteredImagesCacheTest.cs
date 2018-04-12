using System;
using System.Collections.Generic;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Utils;
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

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void FilteredImagesCacheTest01() {
            // basic test with reusing of xobjects
            String input = inputPath + "multipleImageXObjectOccurrences.pdf";
            String output = outputPath + "filteredImagesCacheTest01.pdf";
            String cmp = inputPath + "cmp_filteredImagesCacheTest01.pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            IList<PdfCleanUpLocation> cleanUpLocations = new List<PdfCleanUpLocation>();
            for (int i = 0; i < pdfDocument.GetNumberOfPages(); ++i) {
                cleanUpLocations.Add(new PdfCleanUpLocation(i + 1, new Rectangle(150, 300, 300, 150)));
            }
            CleanUp(pdfDocument, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff");
            AssertNumberXObjects(output, 1);
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void FilteredImagesCacheTest02() {
            // reusing when several clean areas (different on different pages)
            String input = inputPath + "multipleImageXObjectOccurrences.pdf";
            String output = outputPath + "filteredImagesCacheTest02.pdf";
            String cmp = inputPath + "cmp_filteredImagesCacheTest02.pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            IList<PdfCleanUpLocation> cleanUpLocations = new List<PdfCleanUpLocation>();
            for (int i = 0; i < pdfDocument.GetNumberOfPages(); i += 5) {
                cleanUpLocations.Add(new PdfCleanUpLocation(i + 1, new Rectangle(350, 450, 300, 40)));
                cleanUpLocations.Add(new PdfCleanUpLocation(i + 1, new Rectangle(300, 400, 50, 150)));
            }
            for (int i = 1; i < pdfDocument.GetNumberOfPages(); i += 5) {
                cleanUpLocations.Add(new PdfCleanUpLocation(i + 1, new Rectangle(350, 450, 300, 20)));
                cleanUpLocations.Add(new PdfCleanUpLocation(i + 1, new Rectangle(350, 490, 300, 20)));
                cleanUpLocations.Add(new PdfCleanUpLocation(i + 1, new Rectangle(350, 530, 300, 20)));
            }
            for (int i = 3; i < pdfDocument.GetNumberOfPages(); i += 5) {
                cleanUpLocations.Add(new PdfCleanUpLocation(i + 1, new Rectangle(300, 400, 50, 150)));
                cleanUpLocations.Add(new PdfCleanUpLocation(i + 1, new Rectangle(350, 400, 50, 150)));
            }
            for (int i = 4; i < pdfDocument.GetNumberOfPages(); i += 5) {
                cleanUpLocations.Add(new PdfCleanUpLocation(i + 1, new Rectangle(350, 450, 300, 20)));
                cleanUpLocations.Add(new PdfCleanUpLocation(i + 1, new Rectangle(350, 450, 300, 20)));
                cleanUpLocations.Add(new PdfCleanUpLocation(i + 1, new Rectangle(350, 450, 300, 20)));
            }
            CleanUp(pdfDocument, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff");
            AssertNumberXObjects(output, 5);
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void FilteredImagesCacheTest03() {
            // same areas, different src images
            String input = inputPath + "multipleDifferentImageXObjectOccurrences.pdf";
            String output = outputPath + "filteredImagesCacheTest03.pdf";
            String cmp = inputPath + "cmp_filteredImagesCacheTest03.pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            IList<PdfCleanUpLocation> cleanUpLocations = new List<PdfCleanUpLocation>();
            for (int i = 0; i < pdfDocument.GetNumberOfPages(); ++i) {
                cleanUpLocations.Add(new PdfCleanUpLocation(i + 1, new Rectangle(150, 300, 300, 150)));
            }
            CleanUp(pdfDocument, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff");
            AssertNumberXObjects(output, 2);
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void FilteredImagesCacheTest04() {
            // same image with different scaling and the same resultant image area
            String input = inputPath + "multipleScaledImageXObjectOccurrences.pdf";
            String output = outputPath + "filteredImagesCacheTest04.pdf";
            String cmp = inputPath + "cmp_filteredImagesCacheTest04.pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            IList<PdfCleanUpLocation> cleanUpLocations = new List<PdfCleanUpLocation>();
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
                cleanUpLocations.Add(new PdfCleanUpLocation(i + 1, region1));
                cleanUpLocations.Add(new PdfCleanUpLocation(i + 2, region2));
            }
            CleanUp(pdfDocument, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff");
            AssertNumberXObjects(output, 1);
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void FilteredImagesCacheFlushingTest01() {
            String input = inputPath + "severalImageXObjectOccurrences.pdf";
            String output = outputPath + "filteredImagesCacheFlushingTest01.pdf";
            String cmp = inputPath + "cmp_filteredImagesCacheFlushingTest01.pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            PdfCleanUpTool cleanUpTool = new PdfCleanUpTool(pdfDocument);
            cleanUpTool.AddCleanupLocation(new PdfCleanUpLocation(1, new Rectangle(150, 300, 300, 150)));
            cleanUpTool.CleanUp();
            PdfImageXObject img = pdfDocument.GetPage(2).GetResources().GetImage(new PdfName("Im1"));
            img.GetPdfObject().Release();
            cleanUpTool.AddCleanupLocation(new PdfCleanUpLocation(2, new Rectangle(150, 300, 300, 150)));
            cleanUpTool.CleanUp();
            cleanUpTool.AddCleanupLocation(new PdfCleanUpLocation(3, new Rectangle(150, 300, 300, 150)));
            cleanUpTool.CleanUp();
            pdfDocument.Close();
            CompareByContent(cmp, output, outputPath, "diff");
            AssertNumberXObjects(output, 1);
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void FilteredImagesCacheFlushingTest02() {
            String input = inputPath + "severalImageXObjectOccurrences.pdf";
            String output = outputPath + "filteredImagesCacheFlushingTest02.pdf";
            String cmp = inputPath + "cmp_filteredImagesCacheFlushingTest02.pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            PdfCleanUpTool cleanUpTool = new PdfCleanUpTool(pdfDocument);
            cleanUpTool.AddCleanupLocation(new PdfCleanUpLocation(1, new Rectangle(150, 300, 300, 150)));
            cleanUpTool.CleanUp();
            PdfImageXObject img = pdfDocument.GetPage(1).GetResources().GetImage(new PdfName("Im1"));
            img.MakeIndirect(pdfDocument).Flush();
            cleanUpTool.AddCleanupLocation(new PdfCleanUpLocation(2, new Rectangle(150, 300, 300, 150)));
            cleanUpTool.CleanUp();
            cleanUpTool.AddCleanupLocation(new PdfCleanUpLocation(3, new Rectangle(150, 300, 300, 150)));
            cleanUpTool.CleanUp();
            pdfDocument.Close();
            CompareByContent(cmp, output, outputPath, "diff");
            AssertNumberXObjects(output, 1);
        }

        /// <exception cref="System.IO.IOException"/>
        private void CleanUp(PdfDocument pdfDocument, IList<PdfCleanUpLocation> cleanUpLocations) {
            new PdfCleanUpTool(pdfDocument, cleanUpLocations).CleanUp();
            pdfDocument.Close();
        }

        /// <exception cref="System.IO.IOException"/>
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

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        private void CompareByContent(String cmp, String output, String targetDir, String diffPrefix) {
            CompareTool cmpTool = new CompareTool();
            String errorMessage = cmpTool.CompareByContent(output, cmp, targetDir, diffPrefix + "_");
            if (errorMessage != null) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }
    }
}
