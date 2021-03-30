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
using System.Collections.Generic;
using iText.IO.Util;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Test;

namespace iText.PdfCleanup {
    public class PdfCleanUpProcessorUnitTest : ExtendedITextTest {
        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForJavaNullValuesTest() {
            NUnit.Framework.Assert.IsFalse(CreateAndCompareImages(null, null));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForPdfNullAndJavaNullValuesTest() {
            NUnit.Framework.Assert.IsTrue(CreateAndCompareImages(new PdfNull(), null));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForPdfNullValuesTest() {
            NUnit.Framework.Assert.IsFalse(CreateAndCompareImages(new PdfNull(), new PdfNull()));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForNameAndJavaNullValuesTest() {
            NUnit.Framework.Assert.IsTrue(CreateAndCompareImages(PdfName.DeviceRGB, null));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForNameAndPdfNullValuesTest() {
            NUnit.Framework.Assert.IsTrue(CreateAndCompareImages(PdfName.DeviceRGB, new PdfNull()));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForArrayAndJavaNullValuesTest() {
            PdfArray pdfArray = CreatePdfArray(PdfName.Separation, new PdfNumber(1), new PdfStream());
            NUnit.Framework.Assert.IsTrue(CreateAndCompareImages(pdfArray, null));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForArrayAndPdfNullValuesTest() {
            PdfArray pdfArray = CreatePdfArray(PdfName.Separation, new PdfNumber(1), new PdfStream());
            NUnit.Framework.Assert.IsTrue(CreateAndCompareImages(pdfArray, new PdfNull()));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForEqualPdfNameValuesTest() {
            NUnit.Framework.Assert.IsFalse(CreateAndCompareImages(new PdfName("DeviceGray"), PdfName.DeviceGray));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForDifferentPdfNameValuesTest() {
            NUnit.Framework.Assert.IsTrue(CreateAndCompareImages(new PdfName("DeviceGray"), new PdfName("DeviceRGB")));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForTheSamePdfArraysValuesTest() {
            PdfArray pdfFirstArray = CreatePdfArray(PdfName.Separation, new PdfNumber(1), new PdfStream());
            NUnit.Framework.Assert.IsFalse(CreateAndCompareImages(pdfFirstArray, pdfFirstArray));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForPdfArraysWithStreamValuesTest() {
            PdfArray pdfFirstArray = CreatePdfArray(PdfName.Separation, new PdfNumber(1), new PdfStream());
            PdfArray pdfSecondArray = CreatePdfArray(PdfName.Separation, new PdfNumber(1), new PdfStream());
            NUnit.Framework.Assert.IsTrue(CreateAndCompareImages(pdfFirstArray, pdfSecondArray));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForEqualPdfArraysValuesTest() {
            PdfArray pdfFirstArray = CreatePdfArray(PdfName.Separation, new PdfNumber(1), new PdfBoolean(true));
            PdfArray pdfSecondArray = CreatePdfArray(PdfName.Separation, new PdfNumber(1), new PdfBoolean(true));
            NUnit.Framework.Assert.IsFalse(CreateAndCompareImages(pdfFirstArray, pdfSecondArray));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForEqualPdfArraysWithNullsValuesTest() {
            PdfArray pdfFirstArray = CreatePdfArray(PdfName.Separation, new PdfNull());
            PdfArray pdfSecondArray = CreatePdfArray(PdfName.Separation, new PdfNull());
            NUnit.Framework.Assert.IsFalse(CreateAndCompareImages(pdfFirstArray, pdfSecondArray));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForPdfArraysWithDifferentSizeValuesTest() {
            PdfArray pdfFirstArray = CreatePdfArray(PdfName.Separation, new PdfNumber(1), new PdfBoolean(true));
            PdfArray pdfSecondArray = CreatePdfArray(PdfName.Separation, new PdfNumber(1));
            NUnit.Framework.Assert.IsTrue(CreateAndCompareImages(pdfFirstArray, pdfSecondArray));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForPdfNameAndPdfArrayValuesTest() {
            PdfArray pdfArray = CreatePdfArray(PdfName.Separation);
            NUnit.Framework.Assert.IsTrue(CreateAndCompareImages(PdfName.Separation, pdfArray));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForPdfNullAndPdfArrayValuesTest() {
            PdfArray pdfArray = CreatePdfArray(PdfName.Separation);
            NUnit.Framework.Assert.IsTrue(CreateAndCompareImages(new PdfNull(), pdfArray));
        }

        [NUnit.Framework.Test]
        public virtual void AreColorSpacesDifferentForJavaNullAndPdfArrayValuesTest() {
            PdfArray pdfArray = CreatePdfArray(PdfName.Separation);
            NUnit.Framework.Assert.IsTrue(CreateAndCompareImages(null, pdfArray));
        }

        [NUnit.Framework.Test]
        public virtual void OpenNotWrittenTagsUsualTest() {
            LinkedList<CanvasTag> tags = new LinkedList<CanvasTag>(JavaUtil.ArraysAsList(new CanvasTag(new PdfName("tag name1"
                )), new CanvasTag(new PdfName("tag name2")), new CanvasTag(new PdfName("tag name3"))));
            TestOpenNotWrittenTags(tags);
        }

        [NUnit.Framework.Test]
        public virtual void OpenNotWrittenTagsEmptyTest() {
            TestOpenNotWrittenTags(new LinkedList<CanvasTag>());
        }

        private void TestOpenNotWrittenTags(LinkedList<CanvasTag> tags) {
            PdfCleanUpProcessor processor = new _PdfCleanUpProcessor_161(tags, null, null);
            foreach (CanvasTag tag in tags) {
                processor.AddNotWrittenTag(tag);
            }
            processor.OpenNotWrittenTags();
        }

        private sealed class _PdfCleanUpProcessor_161 : PdfCleanUpProcessor {
            public _PdfCleanUpProcessor_161(LinkedList<CanvasTag> tags, IList<Rectangle> baseArg1, PdfDocument baseArg2
                )
                : base(baseArg1, baseArg2) {
                this.tags = tags;
            }

            internal override PdfCanvas GetCanvas() {
                return new _PdfCanvas_164(tags, new PdfStream(), null, null);
            }

            private sealed class _PdfCanvas_164 : PdfCanvas {
                public _PdfCanvas_164(LinkedList<CanvasTag> tags, PdfStream baseArg1, PdfResources baseArg2, PdfDocument baseArg3
                    )
                    : base(baseArg1, baseArg2, baseArg3) {
                    this.tags = tags;
                    this.tagsToCompare = tags;
                }

                internal readonly LinkedList<CanvasTag> tagsToCompare;

                public override PdfCanvas OpenTag(CanvasTag tag) {
                    NUnit.Framework.Assert.AreEqual(this.tagsToCompare.JRemoveFirst(), tag);
                    return null;
                }

                private readonly LinkedList<CanvasTag> tags;
            }

            private readonly LinkedList<CanvasTag> tags;
        }

        private static PdfArray CreatePdfArray(params PdfObject[] objects) {
            return new PdfArray(JavaUtil.ArraysAsList(objects));
        }

        private static bool CreateAndCompareImages(PdfObject firstCs, PdfObject secondCs) {
            PdfImageXObject firstImage = CreateImageWithCs(firstCs);
            PdfImageXObject secondImage = CreateImageWithCs(secondCs);
            bool compareFirstToSecondResult = PdfCleanUpProcessor.AreColorSpacesDifferent(firstImage, secondImage);
            bool compareSecondToFirstResult = PdfCleanUpProcessor.AreColorSpacesDifferent(secondImage, firstImage);
            if (compareFirstToSecondResult != compareSecondToFirstResult) {
                throw new InvalidOperationException("The comparing of CS shall be a commutative operation.");
            }
            return compareFirstToSecondResult;
        }

        private static PdfImageXObject CreateImageWithCs(PdfObject cs) {
            PdfStream stream = new PdfStream();
            stream.Put(PdfName.Type, PdfName.XObject);
            stream.Put(PdfName.Subtype, PdfName.Image);
            if (cs != null) {
                stream.Put(PdfName.ColorSpace, cs);
            }
            return new PdfImageXObject(stream);
        }
    }
}
