/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
    address: sales@itextpdf.com */
using System;
using System.Collections.Generic;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Versions.Attributes;
using iText.Kernel;
using iText.Test;

namespace iText.PdfCleanup {
    public class PdfCleanUpToolTest : ExtendedITextTest {

        private static readonly String inputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/../../resources/itext/pdfcleanup/PdfCleanUpToolTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/PdfCleanUpToolTest/";

        [NUnit.Framework.TestFixtureSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest01() {
            String input = inputPath + "page229.pdf";
            String output = outputPath + "page229_01.pdf";
            String cmp = inputPath + "cmp_page229_01.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = iText.IO.Util.JavaUtil.ArraysAsList(new PdfCleanUpLocation(1, 
                new Rectangle(240.0f, 602.3f, 275.7f - 240.0f, 614.8f - 602.3f), Color.GRAY), new PdfCleanUpLocation(1
                , new Rectangle(171.3f, 550.3f, 208.4f - 171.3f, 562.8f - 550.3f), Color.GRAY), new PdfCleanUpLocation
                (1, new Rectangle(270.7f, 459.2f, 313.1f - 270.7f, 471.7f - 459.2f), Color.GRAY), new PdfCleanUpLocation
                (1, new Rectangle(249.9f, 329.3f, 279.6f - 249.9f, 341.8f - 329.3f), Color.GRAY), new PdfCleanUpLocation
                (1, new Rectangle(216.2f, 303.3f, 273.0f - 216.2f, 315.8f - 303.3f), Color.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_01");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest02() {
            String input = inputPath + "page229-modified-Tc-Tw.pdf";
            String output = outputPath + "page229-modified-Tc-Tw.pdf";
            String cmp = inputPath + "cmp_page229-modified-Tc-Tw.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = iText.IO.Util.JavaUtil.ArraysAsList(new PdfCleanUpLocation(1, 
                new Rectangle(240.0f, 602.3f, 275.7f - 240.0f, 614.8f - 602.3f), Color.GRAY), new PdfCleanUpLocation(1
                , new Rectangle(171.3f, 550.3f, 208.4f - 171.3f, 562.8f - 550.3f), Color.GRAY), new PdfCleanUpLocation
                (1, new Rectangle(270.7f, 459.2f, 313.1f - 270.7f, 471.7f - 459.2f), Color.GRAY), new PdfCleanUpLocation
                (1, new Rectangle(249.9f, 329.3f, 279.6f - 249.9f, 341.8f - 329.3f), Color.GRAY), new PdfCleanUpLocation
                (1, new Rectangle(216.2f, 303.3f, 273.0f - 216.2f, 315.8f - 303.3f), Color.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_02");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest03() {
            String input = inputPath + "page166_03.pdf";
            String output = outputPath + "page166_03.pdf";
            String cmp = inputPath + "cmp_page166_03.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_03");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest04() {
            String input = inputPath + "hello_05.pdf";
            String output = outputPath + "hello_05.pdf";
            String cmp = inputPath + "cmp_hello_05.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_04");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest05() {
            String input = inputPath + "BigImage-jpg.pdf";
            String output = outputPath + "BigImage-jpg.pdf";
            String cmp = inputPath + "cmp_BigImage-jpg.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_05");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest06() {
            String input = inputPath + "BigImage-png.pdf";
            String output = outputPath + "BigImage-png.pdf";
            String cmp = inputPath + "cmp_BigImage-png.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_06");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest07() {
            String input = inputPath + "BigImage-tif.pdf";
            String output = outputPath + "BigImage-tif.pdf";
            String cmp = inputPath + "cmp_BigImage-tif.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_07");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest08() {
            String input = inputPath + "BigImage-tif-lzw.pdf";
            String output = outputPath + "BigImage-tif-lzw.pdf";
            String cmp = inputPath + "cmp_BigImage-tif-lzw.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_08");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest09() {
            String input = inputPath + "simpleImmediate.pdf";
            String output = outputPath + "simpleImmediate.pdf";
            String cmp = inputPath + "cmp_simpleImmediate.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = iText.IO.Util.JavaUtil.ArraysAsList(new PdfCleanUpLocation(1, 
                new Rectangle(97f, 405f, 480f - 97f, 445f - 405f), Color.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_09");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest10() {
            String input = inputPath + "simpleImmediate-tm.pdf";
            String output = outputPath + "simpleImmediate-tm.pdf";
            String cmp = inputPath + "cmp_simpleImmediate-tm.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = iText.IO.Util.JavaUtil.ArraysAsList(new PdfCleanUpLocation(1, 
                new Rectangle(97f, 405f, 480f - 97f, 445f - 405f), Color.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_10");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest11() {
            String input = inputPath + "multiUseIndirect.pdf";
            String output = outputPath + "multiUseIndirect.pdf";
            String cmp = inputPath + "cmp_multiUseIndirect.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = iText.IO.Util.JavaUtil.ArraysAsList(new PdfCleanUpLocation(1, 
                new Rectangle(97f, 605f, 480f - 97f, 645f - 605f), Color.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_11");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest12() {
            String input = inputPath + "multiUseImage.pdf";
            String output = outputPath + "multiUseImage.pdf";
            String cmp = inputPath + "cmp_multiUseImage.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = iText.IO.Util.JavaUtil.ArraysAsList(new PdfCleanUpLocation(1, 
                new Rectangle(97f, 405f, 480f - 97f, 445f - 405f), Color.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_12");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest13() {
            String input = inputPath + "smaskImage.pdf";
            String output = outputPath + "smaskImage.pdf";
            String cmp = inputPath + "cmp_smaskImage.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = iText.IO.Util.JavaUtil.ArraysAsList(new PdfCleanUpLocation(1, 
                new Rectangle(97f, 405f, 480f - 97f, 445f - 405f), Color.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_13");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest14() {
            String input = inputPath + "rotatedImg.pdf";
            String output = outputPath + "rotatedImg.pdf";
            String cmp = inputPath + "cmp_rotatedImg.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = iText.IO.Util.JavaUtil.ArraysAsList(new PdfCleanUpLocation(1, 
                new Rectangle(97f, 405f, 480f - 97f, 445f - 405f), Color.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_14");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest15() {
            String input = inputPath + "lineArtsCompletely.pdf";
            String output = outputPath + "lineArtsCompletely.pdf";
            String cmp = inputPath + "cmp_lineArtsCompletely.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_15");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest16() {
            String input = inputPath + "lineArtsPartially.pdf";
            String output = outputPath + "lineArtsPartially.pdf";
            String cmp = inputPath + "cmp_lineArtsPartially.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_16");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest17() {
            String input = inputPath + "dashedStyledClosedBezier.pdf";
            String output = outputPath + "dashedStyledClosedBezier.pdf";
            String cmp = inputPath + "cmp_dashedStyledClosedBezier.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_17");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest18() {
            String input = inputPath + "styledLineArts.pdf";
            String output = outputPath + "styledLineArts.pdf";
            String cmp = inputPath + "cmp_styledLineArts.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_18");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest19() {
            String input = inputPath + "dashedBezier.pdf";
            String output = outputPath + "dashedBezier.pdf";
            String cmp = inputPath + "cmp_dashedBezier.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_19");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest20() {
            String input = inputPath + "closedBezier.pdf";
            String output = outputPath + "closedBezier.pdf";
            String cmp = inputPath + "cmp_closedBezier.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_20");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest21() {
            String input = inputPath + "clippingNWRule.pdf";
            String output = outputPath + "clippingNWRule.pdf";
            String cmp = inputPath + "cmp_clippingNWRule.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_21");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest22() {
            String input = inputPath + "dashedClosedRotatedTriangles.pdf";
            String output = outputPath + "dashedClosedRotatedTriangles.pdf";
            String cmp = inputPath + "cmp_dashedClosedRotatedTriangles.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_22");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest23() {
            String input = inputPath + "miterTest.pdf";
            String output = outputPath + "miterTest.pdf";
            String cmp = inputPath + "cmp_miterTest.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_23");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest24() {
            String input = inputPath + "degenerateCases.pdf";
            String output = outputPath + "degenerateCases.pdf";
            String cmp = inputPath + "cmp_degenerateCases.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_24");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest25() {
            String input = inputPath + "absentICentry.pdf";
            String output = outputPath + "absentICentry.pdf";
            String cmp = inputPath + "cmp_absentICentry.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_25");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest26() {
            String input = inputPath + "lotOfDashes.pdf";
            String output = outputPath + "lotOfDashes.pdf";
            String cmp = inputPath + "cmp_lotOfDashes.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_26");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest27() {
            String input = inputPath + "clipPathReduction.pdf";
            String output = outputPath + "clipPathReduction.pdf";
            String cmp = inputPath + "cmp_clipPathReduction.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = iText.IO.Util.JavaUtil.ArraysAsList(new PdfCleanUpLocation(1, 
                new Rectangle(212, 394, 186, 170), null));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_27");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest30() {
            String input = inputPath + "inlineImages.pdf";
            String output = outputPath + "inlineImages_full.pdf";
            String cmp = inputPath + "cmp_inlineImages_full.pdf";
            IList<PdfCleanUpLocation> cleanUpLocations = iText.IO.Util.JavaUtil.ArraysAsList(new PdfCleanUpLocation(1, 
                new Rectangle(10, 100, 400, 600), null));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_30");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest32() {
            String input = inputPath + "page229.pdf";
            String output = outputPath + "wholePageCleanUp.pdf";
            String cmp = inputPath + "cmp_wholePageCleanUp.pdf";
            CleanUp(input, output, iText.IO.Util.JavaUtil.ArraysAsList(new PdfCleanUpLocation(1, new Rectangle(1, 1, PageSize
                .A4.GetWidth() - 1, PageSize.A4.GetHeight() - 1))));
            CompareByContent(cmp, output, outputPath, "diff_32");
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
