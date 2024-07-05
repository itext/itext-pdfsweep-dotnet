/*
This file is part of the iText (R) project.
Copyright (c) 1998-2024 Apryse Group NV
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
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.PdfCleanup.Exceptions;
using iText.Test;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("IntegrationTest")]
    public class UnsupportedImageTypeTest : ExtendedITextTest {
        private static readonly String INPUT_PATH = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/UnsupportedImageTypeTest/";

        private static readonly String OUTPUT_PATH = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/UnsupportedImageTypeTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(OUTPUT_PATH);
        }

        [NUnit.Framework.Test]
        public virtual void CheckUnSupportedImageTypeTest() {
            String input = INPUT_PATH + "JpegCmykImage.pdf";
            String output = OUTPUT_PATH + "JpegCmykImage.pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output, new WriterProperties
                ()));
            iText.PdfCleanup.PdfCleanUpTool workingTool = new iText.PdfCleanup.PdfCleanUpTool(pdfDocument);
            int pageIndex = 1;
            Rectangle area = pdfDocument.GetPage(pageIndex).GetPageSize();
            workingTool.AddCleanupLocation(new iText.PdfCleanup.PdfCleanUpLocation(pageIndex, area));
            Exception e = NUnit.Framework.Assert.Catch(typeof(Exception), () => workingTool.CleanUp());
            NUnit.Framework.Assert.IsTrue(CleanupExceptionMessageConstant.UNSUPPORTED_IMAGE_TYPE.ToLowerInvariant().Equals
                (e.Message.ToLowerInvariant()) || "incompatible color conversion".Equals(e.Message.ToLowerInvariant())
                );
            pdfDocument.Close();
        }
    }
}
