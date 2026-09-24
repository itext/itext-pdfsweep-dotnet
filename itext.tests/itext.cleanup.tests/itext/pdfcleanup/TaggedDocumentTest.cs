/*
This file is part of the iText (R) project.
Copyright (c) 1998-2026 Apryse Group NV
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
using iText.Commons.Internal.Runtime;
using iText.Commons.Utils;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Test;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("IntegrationTest")]
    public class TaggedDocumentTest : ExtendedITextTest {
        private static readonly String SOURCE_FOLDER = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/TaggedDocumentTest/";

        private static readonly String DESTINATION_FOLDER = NUnit.Framework.TestContext.CurrentContext.TestDirectory
             + "/test/itext/pdfcleanup/TaggedDocumentTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void BeforeClass() {
            CreateOrClearDestinationFolder(DESTINATION_FOLDER);
        }

        [NUnit.Framework.Test]
        public virtual void CanvasSimpleMcTreeRedactChildTest() {
            String inPdf = SOURCE_FOLDER + "canvasSimpleMcTree.pdf";
            String outPdf = DESTINATION_FOLDER + "canvasSimpleMcTreeRedactChild.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_canvasSimpleMcTreeRedactChild.pdf";
            Cleanup(inPdf, outPdf, cmpPdf, new Rectangle(300, 300, 100, 300));
        }

        [NUnit.Framework.Test]
        public virtual void CanvasSimpleMcTreeRedactParentTest() {
            String inPdf = SOURCE_FOLDER + "canvasSimpleMcTree.pdf";
            String outPdf = DESTINATION_FOLDER + "canvasSimpleMcTreeRedactParent.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_canvasSimpleMcTreeRedactParent.pdf";
            Cleanup(inPdf, outPdf, cmpPdf, new Rectangle(100, 300, 100, 300));
        }

        [NUnit.Framework.Test]
        public virtual void CanvasImagesMcTreeRedactImageTest() {
            String inPdf = SOURCE_FOLDER + "canvasImagesMcTree.pdf";
            String outPdf = DESTINATION_FOLDER + "canvasImagesMcTreeRedactImage.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_canvasImagesMcTreeRedactImage.pdf";
            Cleanup(inPdf, outPdf, cmpPdf, new Rectangle(40, 435, 5, 5));
        }

        [NUnit.Framework.Test]
        public virtual void CanvasImagesMcTreeRedactFullImageTest() {
            String inPdf = SOURCE_FOLDER + "canvasImagesMcTree.pdf";
            String outPdf = DESTINATION_FOLDER + "canvasImagesMcTreeRedactFullImage.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_canvasImagesMcTreeRedactFullImage.pdf";
            Cleanup(inPdf, outPdf, cmpPdf, new Rectangle(32, 425, 50, 50));
        }

        [NUnit.Framework.Test]
        public virtual void CanvasImagesMcTreeRedactInlineImageTest() {
            String inPdf = SOURCE_FOLDER + "canvasImagesMcTree.pdf";
            String outPdf = DESTINATION_FOLDER + "canvasImagesMcTreeRedactInlineImage.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_canvasImagesMcTreeRedactInlineImage.pdf";
            Cleanup(inPdf, outPdf, cmpPdf, new Rectangle(100, 435, 5, 5));
        }

        [NUnit.Framework.Test]
        public virtual void FormXobjectImagesMcTreeTest() {
            String inPdf = SOURCE_FOLDER + "formXobjectImagesMcTree.pdf";
            String outPdf = DESTINATION_FOLDER + "formXobjectImagesMcTree.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_formXobjectImagesMcTree.pdf";
            Cleanup(inPdf, outPdf, cmpPdf, new Rectangle(40, 435, 5, 5));
        }

        [NUnit.Framework.Test]
        public virtual void CanvasTagFormXobjectImagesMcTreeTest() {
            String inPdf = SOURCE_FOLDER + "canvasTagFormXobjectImagesMcTree.pdf";
            String outPdf = DESTINATION_FOLDER + "canvasTagFormXobjectImagesMcTree.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_canvasTagFormXobjectImagesMcTree.pdf";
            Cleanup(inPdf, outPdf, cmpPdf, new Rectangle(100, 435, 5, 5));
        }

        [NUnit.Framework.Test]
        public virtual void CanvasMcTreeRedactFillPathTest() {
            String inPdf = SOURCE_FOLDER + "canvasPathsMcTree.pdf";
            String outPdf = DESTINATION_FOLDER + "canvasMcTreeRedactFillPath.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_canvasMcTreeRedactFillPath.pdf";
            Cleanup(inPdf, outPdf, cmpPdf, new Rectangle(160, 435, 5, 5));
        }

        [NUnit.Framework.Test]
        public virtual void CanvasMcTreeRedactStrokePathTest() {
            String inPdf = SOURCE_FOLDER + "canvasPathsMcTree.pdf";
            String outPdf = DESTINATION_FOLDER + "canvasMcTreeRedactStrokePath.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_canvasMcTreeRedactStrokePath.pdf";
            Cleanup(inPdf, outPdf, cmpPdf, new Rectangle(220, 435, 5, 40));
        }

        [NUnit.Framework.Test]
        public virtual void CanvasMcTreeRedactClipPathTest() {
            String inPdf = SOURCE_FOLDER + "canvasPathsMcTree.pdf";
            String outPdf = DESTINATION_FOLDER + "canvasMcTreeRedactClipPath.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_canvasMcTreeRedactClipPath.pdf";
            Cleanup(inPdf, outPdf, cmpPdf, new Rectangle(284, 430, 2, 2));
        }

        [NUnit.Framework.Test]
        public virtual void CanvasMcTreeRedactClipPath2Test() {
            String inPdf = SOURCE_FOLDER + "canvasPathsMcTree.pdf";
            String outPdf = DESTINATION_FOLDER + "canvasMcTreeRedactClipPath2.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_canvasMcTreeRedactClipPath2.pdf";
            Cleanup(inPdf, outPdf, cmpPdf, new Rectangle(380, 515, 5, 5));
        }

        [NUnit.Framework.Test]
        public virtual void StructTreeRootTest() {
            String inPdf = SOURCE_FOLDER + "structTreeRoot.pdf";
            String outPdf = DESTINATION_FOLDER + "structTreeRoot.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_structTreeRoot.pdf";
            Cleanup(inPdf, outPdf, cmpPdf, new Rectangle(36, 735, 180, 40));
        }

        private static void Cleanup(String input, String output, String cmp, Rectangle cleanupArea) {
            using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(input), new PdfWriter(output))) {
                iText.PdfCleanup.PdfCleanUpTool workingTool = new iText.PdfCleanup.PdfCleanUpTool(pdfDoc);
                workingTool.AddCleanupLocation(new iText.PdfCleanup.PdfCleanUpLocation(1, cleanupArea, ColorConstants.BLACK
                    ));
                workingTool.CleanUp();
            }
            String diff = new CompareTool().CompareByContent(output, cmp, DESTINATION_FOLDER);
            if (diff != null) {
                String cmp2;
                int lastDot = cmp.LastIndexOf('.');
                if (lastDot <= 0) {
                    cmp2 = cmp + "_2";
                }
                else {
                    String @base = cmp.JSubstring(0, lastDot);
                    String ext = cmp.Substring(lastDot);
                    cmp2 = @base + "_2" + ext;
                }
                if (FileUtil.FileExists(cmp2)) {
                    // Second cmp is required on .NET because cleanup if inline images differs per system
                    diff = new CompareTool().CompareByContent(output, cmp2, DESTINATION_FOLDER);
                }
            }
            NUnit.Framework.Assert.IsNull(diff);
        }
    }
}
