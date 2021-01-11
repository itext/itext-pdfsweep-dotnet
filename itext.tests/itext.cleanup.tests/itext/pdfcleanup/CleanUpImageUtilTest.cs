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
using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Xobject;
using iText.PdfCleanup.Util;
using iText.Test;

namespace iText.PdfCleanup {
    public class CleanUpImageUtilTest : ExtendedITextTest {
        private static readonly String SOURCE_PATH = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/CleanupImageHandlingUtilTest/";

        [NUnit.Framework.Test]
        public virtual void CleanUpImageNullImageBytesTest() {
            IList<Rectangle> areasToBeCleaned = new List<Rectangle>();
            areasToBeCleaned.Add(new Rectangle(100, 100));
            NUnit.Framework.Assert.That(() =>  {
                CleanUpImageUtil.CleanUpImage(null, areasToBeCleaned);
            }
            , NUnit.Framework.Throws.InstanceOf<Exception>())
;
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpImageEmptyAreasToCleanTest() {
            ImageData data = ImageDataFactory.Create(SOURCE_PATH + "cleanUpImageEmptyAreasToClean.png");
            PdfImageXObject imageXObject = new PdfImageXObject(data);
            byte[] sourceImageBytes = imageXObject.GetImageBytes();
            byte[] resultImageBytes = CleanUpImageUtil.CleanUpImage(new PdfImageXObject(data).GetImageBytes(), new List
                <Rectangle>());
            NUnit.Framework.Assert.AreEqual(sourceImageBytes, resultImageBytes);
        }
    }
}
