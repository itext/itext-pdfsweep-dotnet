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
using System.IO;
using iText.Kernel.Pdf;
using iText.Test;

namespace iText.PdfCleanup {
    public class CleanUpInvalidPdfTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/CleanUpInvalidPdfTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/CleanUpInvalidPdfTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        [NUnit.Framework.Ignore("DEVSIX-3608: this test currently throws StackOverflowError, which cannot be caught in .NET"
            )]
        public virtual void CleanCircularReferencesInResourcesTest() {
            NUnit.Framework.Assert.That(() =>  {
                String input = inputPath + "circularReferencesInResources.pdf";
                PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(new MemoryStream()));
                IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                    >();
                cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(1, pdfDocument.GetPage(1).GetPageSize(), null
                    ));
                new iText.PdfCleanup.PdfCleanUpTool(pdfDocument, cleanUpLocations).CleanUp();
                pdfDocument.Close();
            }
            , NUnit.Framework.Throws.InstanceOf<OutOfMemoryException>())
;
        }
    }
}
