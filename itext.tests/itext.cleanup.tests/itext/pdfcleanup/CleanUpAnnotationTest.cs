/*
This file is part of the iText (R) project.
Copyright (c) 1998-2018 iText Group NV
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
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Test;

namespace iText.PdfCleanup {
    public class CleanUpAnnotationTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/CleanUpAnnotationTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/CleanUpAnnotationTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanFull01() {
            String input = inputPath + "cleanAnnotation.pdf";
            String output = outputPath + "cleanAnnotation_full01.pdf";
            String cmp = inputPath + "cmp_cleanAnnotation_full01.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = new List<PdfCleanUpLocation>();
            cleanUpLocations.Add(new PdfCleanUpLocation(1, PageSize.A4, ColorConstants.WHITE));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_Annotation_full");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanLinkAnnotation01() {
            String input = inputPath + "cleanAnnotation.pdf";
            String output = outputPath + "cleanAnnotation_Link01.pdf";
            String cmp = inputPath + "cmp_cleanAnnotation_Link01.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = new List<PdfCleanUpLocation>();
            PdfCleanUpLocation linkLoc = new PdfCleanUpLocation(1, new Rectangle(235, 740, 30, 16), ColorConstants.BLUE
                );
            cleanUpLocations.Add(linkLoc);
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_Annotation_link01");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanTextAnnotation01() {
            String input = inputPath + "cleanAnnotation.pdf";
            String output = outputPath + "cleanAnnotation_Text01.pdf";
            String cmp = inputPath + "cmp_cleanAnnotation_Text01.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = new List<PdfCleanUpLocation>();
            PdfCleanUpLocation textLoc = new PdfCleanUpLocation(1, new Rectangle(150, 650, 0, 0), ColorConstants.RED);
            cleanUpLocations.Add(textLoc);
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_Annotation_text01");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanLineAnnotation01() {
            String input = inputPath + "cleanAnnotation.pdf";
            String output = outputPath + "cleanAnnotation_Line01.pdf";
            String cmp = inputPath + "cmp_cleanAnnotation_Line01.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = new List<PdfCleanUpLocation>();
            PdfCleanUpLocation lineLoc = new PdfCleanUpLocation(1, new Rectangle(20, 20, 555, 0), ColorConstants.GREEN
                );
            cleanUpLocations.Add(lineLoc);
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_Annotation_line01");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanHighlightAnnotation01() {
            String input = inputPath + "cleanAnnotation.pdf";
            String output = outputPath + "cleanAnnotation_highlight01.pdf";
            String cmp = inputPath + "cmp_cleanAnnotation_highlight01.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = new List<PdfCleanUpLocation>();
            PdfCleanUpLocation highLightLoc = new PdfCleanUpLocation(1, new Rectangle(105, 500, 70, 10), ColorConstants
                .BLACK);
            cleanUpLocations.Add(highLightLoc);
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_text_highlight01");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanFormAnnotations01() {
            String input = inputPath + "formAnnotation.pdf";
            String output = outputPath + "formAnnotation01.pdf";
            String cmp = inputPath + "cmp_formAnnotation01.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = new List<PdfCleanUpLocation>();
            PdfCleanUpLocation highLightLoc = new PdfCleanUpLocation(1, new Rectangle(20, 600, 500, 170), ColorConstants
                .YELLOW);
            cleanUpLocations.Add(highLightLoc);
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_form01");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanFormAnnotations02() {
            String input = inputPath + "formAnnotation.pdf";
            String output = outputPath + "formAnnotation02.pdf";
            String cmp = inputPath + "cmp_formAnnotation02.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = new List<PdfCleanUpLocation>();
            PdfCleanUpLocation highLightLoc = new PdfCleanUpLocation(1, new Rectangle(20, 600, 300, 100), ColorConstants
                .YELLOW);
            cleanUpLocations.Add(highLightLoc);
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_form01");
        }

        /// <exception cref="System.IO.IOException"/>
        private void CleanUp(String input, String output, IList<PdfCleanUpLocation> cleanUpLocations) {
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            PdfCleanUpTool cleaner = (cleanUpLocations == null) ? new PdfCleanUpTool(pdfDocument, true) : new PdfCleanUpTool
                (pdfDocument, cleanUpLocations);
            cleaner.CleanUp();
            pdfDocument.Close();
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
