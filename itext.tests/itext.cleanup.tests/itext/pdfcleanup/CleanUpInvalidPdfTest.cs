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
using System.Collections.Generic;
using System.IO;
using iText.Commons.Utils;
using iText.Kernel.Exceptions;
using iText.Kernel.Pdf;
using iText.Test;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("IntegrationTest")]
    public class CleanUpInvalidPdfTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/CleanUpInvalidPdfTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/CleanUpInvalidPdfTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void CleanCircularReferencesInResourcesTest() {
            String input = inputPath + "circularReferencesInResources.pdf";
            using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(input), new PdfWriter(new MemoryStream()))) {
                IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                    >();
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(1, pdfDoc.GetPage(1).GetPageSize(), null));
                Exception exception = NUnit.Framework.Assert.Catch(typeof(PdfException), () => PdfCleaner.CleanUp(pdfDoc, 
                    cleanUpLocations));
                NUnit.Framework.Assert.AreEqual(MessageFormatUtil.Format(KernelExceptionMessageConstant.FORM_XOBJECT_HAS_CIRCULAR_REFERENCES
                    , 12, 0), exception.Message);
            }
        }
    }
}
