/*
This file is part of the iText (R) project.
Copyright (c) 1998-2021 iText Group NV
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
using System.IO;
using iText.IO.Util;
using iText.Kernel;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Utils;
using iText.PdfCleanup.Util;
using iText.Test;
using iText.Test.Attributes;

namespace iText.PdfCleanup {
    public class PdfCleanUpToolTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/PdfCleanUpToolTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/PdfCleanUpToolTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest01() {
            String input = inputPath + "page229.pdf";
            String output = outputPath + "page229_01.pdf";
            String cmp = inputPath + "cmp_page229_01.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(240.0f, 602.3f, 275.7f - 240.0f, 614.8f - 602.3f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(171.3f, 550.3f, 208.4f - 171.3f, 562.8f - 550.3f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(270.7f, 459.2f, 313.1f - 270.7f, 471.7f - 459.2f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(249.9f, 329.3f, 279.6f - 249.9f, 341.8f - 329.3f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(216.2f, 303.3f, 273.0f - 216.2f, 315.8f - 303.3f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_01");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest02() {
            String input = inputPath + "page229-modified-Tc-Tw.pdf";
            String output = outputPath + "page229-modified-Tc-Tw.pdf";
            String cmp = inputPath + "cmp_page229-modified-Tc-Tw.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(240.0f, 602.3f, 275.7f - 240.0f, 614.8f - 602.3f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(171.3f, 550.3f, 208.4f - 171.3f, 562.8f - 550.3f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(270.7f, 459.2f, 313.1f - 270.7f, 471.7f - 459.2f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(249.9f, 329.3f, 279.6f - 249.9f, 341.8f - 329.3f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(216.2f, 303.3f, 273.0f - 216.2f, 315.8f - 303.3f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_02");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest03() {
            String input = inputPath + "page166_03.pdf";
            String output = outputPath + "page166_03.pdf";
            String cmp = inputPath + "cmp_page166_03.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_03");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTestSvg() {
            // TODO: update cmp file after DEVSIX-3185 fixed
            String input = inputPath + "line_chart.pdf";
            String output = outputPath + "line_chart.pdf";
            String cmp = inputPath + "cmp_line_chart.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(60f, 
                780f, 60f, 45f), ColorConstants.GRAY)));
            CompareByContent(cmp, output, outputPath, "diff_Svg");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest04() {
            String input = inputPath + "hello_05.pdf";
            String output = outputPath + "hello_05.pdf";
            String cmp = inputPath + "cmp_hello_05.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_04");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest05() {
            String input = inputPath + "BigImage-jpg.pdf";
            String output = outputPath + "BigImage-jpg.pdf";
            String cmp = inputPath + "cmp_BigImage-jpg.pdf";
            CleanUp(input, output, null);
            CleanUpImagesCompareTool cmpTool = new CleanUpImagesCompareTool();
            String errorMessage = cmpTool.ExtractAndCompareImages(output, cmp, outputPath, "1");
            String compareByContentResult = cmpTool.CompareByContent(output, cmp, outputPath);
            if (compareByContentResult != null) {
                errorMessage += compareByContentResult;
            }
            if (!errorMessage.Equals("")) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest06() {
            String input = inputPath + "BigImage-png.pdf";
            String output = outputPath + "BigImage-png.pdf";
            String cmp = inputPath + "cmp_BigImage-png.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_06");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest07() {
            String input = inputPath + "BigImage-tif.pdf";
            String output = outputPath + "BigImage-tif.pdf";
            String cmp = inputPath + "cmp_BigImage-tif.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_07");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest08() {
            String input = inputPath + "BigImage-tif-lzw.pdf";
            String output = outputPath + "BigImage-tif-lzw.pdf";
            String cmp = inputPath + "cmp_BigImage-tif-lzw.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_08");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest09() {
            String input = inputPath + "simpleImmediate.pdf";
            String output = outputPath + "simpleImmediate.pdf";
            String cmp = inputPath + "cmp_simpleImmediate.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(97f, 405f, 480f - 97f, 445f - 405f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_09");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest10() {
            String input = inputPath + "simpleImmediate-tm.pdf";
            String output = outputPath + "simpleImmediate-tm.pdf";
            String cmp = inputPath + "cmp_simpleImmediate-tm.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(97f, 405f, 480f - 97f, 445f - 405f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_10");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest11() {
            String input = inputPath + "multiUseIndirect.pdf";
            String output = outputPath + "multiUseIndirect.pdf";
            String cmp = inputPath + "cmp_multiUseIndirect.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(97f, 605f, 480f - 97f, 645f - 605f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_11");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest12() {
            String input = inputPath + "multiUseImage.pdf";
            String output = outputPath + "multiUseImage.pdf";
            String cmp = inputPath + "cmp_multiUseImage.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(97f, 405f, 480f - 97f, 445f - 405f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_12");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest13() {
            String input = inputPath + "maskImage.pdf";
            String output = outputPath + "maskImage.pdf";
            String cmp = inputPath + "cmp_maskImage.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(97f, 405f, 480f - 97f, 445f - 405f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_13");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest14() {
            String input = inputPath + "rotatedImg.pdf";
            String output = outputPath + "rotatedImg.pdf";
            String cmp = inputPath + "cmp_rotatedImg.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(97f, 405f, 480f - 97f, 445f - 405f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_14");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest15() {
            String input = inputPath + "lineArtsCompletely.pdf";
            String output = outputPath + "lineArtsCompletely.pdf";
            String cmp = inputPath + "cmp_lineArtsCompletely.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_15");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest16() {
            String input = inputPath + "lineArtsPartially.pdf";
            String output = outputPath + "lineArtsPartially.pdf";
            String cmp = inputPath + "cmp_lineArtsPartially.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_16");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest17() {
            String input = inputPath + "dashedStyledClosedBezier.pdf";
            String output = outputPath + "dashedStyledClosedBezier.pdf";
            String cmp = inputPath + "cmp_dashedStyledClosedBezier.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_17");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest18() {
            String input = inputPath + "styledLineArts.pdf";
            String output = outputPath + "styledLineArts.pdf";
            String cmp = inputPath + "cmp_styledLineArts.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_18");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest19() {
            String input = inputPath + "dashedBezier.pdf";
            String output = outputPath + "dashedBezier.pdf";
            String cmp = inputPath + "cmp_dashedBezier.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_19");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest20() {
            String input = inputPath + "closedBezier.pdf";
            String output = outputPath + "closedBezier.pdf";
            String cmp = inputPath + "cmp_closedBezier.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_20");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest21() {
            String input = inputPath + "clippingNWRule.pdf";
            String output = outputPath + "clippingNWRule.pdf";
            String cmp = inputPath + "cmp_clippingNWRule.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_21");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest22() {
            String input = inputPath + "dashedClosedRotatedTriangles.pdf";
            String output = outputPath + "dashedClosedRotatedTriangles.pdf";
            String cmp = inputPath + "cmp_dashedClosedRotatedTriangles.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_22");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest23() {
            String input = inputPath + "miterTest.pdf";
            String output = outputPath + "miterTest.pdf";
            String cmp = inputPath + "cmp_miterTest.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_23");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest24() {
            String input = inputPath + "degenerateCases.pdf";
            String output = outputPath + "degenerateCases.pdf";
            String cmp = inputPath + "cmp_degenerateCases.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_24");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest25() {
            String input = inputPath + "absentICentry.pdf";
            String output = outputPath + "absentICentry.pdf";
            String cmp = inputPath + "cmp_absentICentry.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_25");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest26() {
            String input = inputPath + "lotOfDashes.pdf";
            String output = outputPath + "lotOfDashes.pdf";
            String cmp = inputPath + "cmp_lotOfDashes.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, outputPath, "diff_26");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest27() {
            String input = inputPath + "clipPathReduction.pdf";
            String output = outputPath + "clipPathReduction.pdf";
            String cmp = inputPath + "cmp_clipPathReduction.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(212, 394, 186, 170), null));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_27");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest30() {
            String input = inputPath + "inlineImages.pdf";
            String output = outputPath + "inlineImages_full.pdf";
            String cmp = inputPath + "cmp_inlineImages_full.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(10, 100, 400, 600), null));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_30");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest32() {
            String input = inputPath + "page229.pdf";
            String output = outputPath + "wholePageCleanUp.pdf";
            String cmp = inputPath + "cmp_wholePageCleanUp.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(1, 1
                , PageSize.A4.GetWidth() - 1, PageSize.A4.GetHeight() - 1))));
            CompareByContent(cmp, output, outputPath, "diff_32");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest33() {
            String input = inputPath + "viewer_prefs_dict_table.pdf";
            String output = outputPath + "complexTextPositioning.pdf";
            String cmp = inputPath + "cmp_complexTextPositioning.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(300f
                , 370f, 215f, 270f))));
            CompareByContent(cmp, output, outputPath, "diff_33");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest34() {
            String input = inputPath + "new_york_times.pdf";
            String output = outputPath + "textAndImages.pdf";
            String cmp = inputPath + "cmp_textAndImages.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(150f
                , 235f, 230f, 445f))));
            CleanUpImagesCompareTool cmpTool = new CleanUpImagesCompareTool();
            String errorMessage = cmpTool.ExtractAndCompareImages(output, cmp, outputPath, "1.2");
            String compareByContentResult = cmpTool.CompareByContent(output, cmp, outputPath);
            if (compareByContentResult != null) {
                errorMessage += compareByContentResult;
            }
            if (!errorMessage.Equals("")) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest35() {
            String input = inputPath + "lineArtsSimple.pdf";
            String output = outputPath + "lineArtsSimple.pdf";
            String cmp = inputPath + "cmp_lineArtsSimple.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(60f, 
                80f, 460f, 65f), ColorConstants.GRAY)));
            CompareByContent(cmp, output, outputPath, "diff_35");
        }

        /// <summary>In this test, glyph "1" got removed by the clean up area that on first sight is not covering the glyph.
        ///     </summary>
        /// <remarks>
        /// In this test, glyph "1" got removed by the clean up area that on first sight is not covering the glyph.
        /// However, we can't get the particular glyphs height and instead we have the same height for all glyphs.
        /// Because of this, in case of the big font sizes such situations might occur, that even though visually glyph is
        /// rather away from the cleanup location we still get it removed because it's bbox intersects with cleanup area rectangle.
        /// </remarks>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest36() {
            String input = inputPath + "bigOne.pdf";
            String output = outputPath + "bigOne.pdf";
            String cmp = inputPath + "cmp_bigOne.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(300f
                , 370f, 215f, 270f), ColorConstants.GRAY)));
            CompareByContent(cmp, output, outputPath, "diff_36");
        }

        /// <summary>In this test we check that line style operators (such as 'w') are processed correctly</summary>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest37() {
            String input = inputPath + "helloHelvetica.pdf";
            String output = outputPath + "helloHelvetica.pdf";
            String cmp = inputPath + "cmp_helloHelvetica.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(0f, 0f, 595f, 680f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_37");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest38() {
            String input = inputPath + "helloHelvetica02.pdf";
            String output = outputPath + "helloHelvetica02.pdf";
            String cmp = inputPath + "cmp_helloHelvetica02.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(0f, 0f, 0f, 0f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_38");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest39() {
            String input = inputPath + "corruptJpeg.pdf";
            String output = outputPath + "corruptJpeg.pdf";
            String cmp = inputPath + "cmp_corruptJpeg.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(100, 350, 100, 200), ColorConstants.ORANGE));
            CleanUp(input, output, cleanUpLocations);
            CleanUpImagesCompareTool cmpTool = new CleanUpImagesCompareTool();
            String errorMessage = cmpTool.ExtractAndCompareImages(output, cmp, outputPath, "1.2");
            String compareByContentResult = cmpTool.CompareByContent(output, cmp, outputPath);
            if (compareByContentResult != null) {
                errorMessage += compareByContentResult;
            }
            if (!errorMessage.Equals("")) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest40() {
            String input = inputPath + "emptyTj01.pdf";
            String output = outputPath + "emptyTj01.pdf";
            String cmp = inputPath + "cmp_emptyTj01.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(70f, 555f, 200f, 5f), ColorConstants.ORANGE));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_40");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest41() {
            String input = inputPath + "newLines01.pdf";
            String output = outputPath + "newLines01.pdf";
            String cmp = inputPath + "cmp_newLines01.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(70f, 555f, 200f, 10f), ColorConstants.ORANGE));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_41");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest42() {
            String input = inputPath + "newLines02.pdf";
            String output = outputPath + "newLines02.pdf";
            String cmp = inputPath + "cmp_newLines02.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(36f, 733f, 270f, 5f), ColorConstants.ORANGE));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_42");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest43() {
            String input = inputPath + "newLines03.pdf";
            String output = outputPath + "newLines03.pdf";
            String cmp = inputPath + "cmp_newLines03.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(36f, 733f, 230f, 5f), ColorConstants.ORANGE));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_43");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest44() {
            String input = inputPath + "emptyTj02.pdf";
            String output = outputPath + "emptyTj02.pdf";
            String cmp = inputPath + "cmp_emptyTj02.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(70f, 565f, 200f, 5f), ColorConstants.ORANGE));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_44");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest45() {
            String input = inputPath + "emptyPdf.pdf";
            String output = outputPath + "emptyPdf.pdf";
            String cmp = inputPath + "cmp_emptyPdf.pdf";
            PdfAnnotation redactAnnotation = new PdfRedactAnnotation(new Rectangle(97, 405, 383, 40)).SetOverlayText(new 
                PdfString("OverlayTest")).SetDefaultAppearance(new PdfString("/Helv 0 Tf 0 g"));
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            pdfDocument.GetFirstPage().AddAnnotation(redactAnnotation);
            new iText.PdfCleanup.PdfCleanUpTool(pdfDocument, true).CleanUp();
            pdfDocument.Close();
            CompareByContent(cmp, output, outputPath, "diff_45");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest46() {
            NUnit.Framework.Assert.That(() =>  {
                String input = inputPath + "emptyPdf.pdf";
                String output = outputPath + "emptyPdf.pdf";
                PdfAnnotation redactAnnotation = new PdfRedactAnnotation(new Rectangle(97, 405, 383, 40)).SetOverlayText(new 
                    PdfString("OverlayTest"));
                PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
                pdfDocument.GetFirstPage().AddAnnotation(redactAnnotation);
                new iText.PdfCleanup.PdfCleanUpTool(pdfDocument, true).CleanUp();
                pdfDocument.Close();
            }
            , NUnit.Framework.Throws.InstanceOf<PdfException>().With.Message.EqualTo(PdfException.DefaultAppearanceNotFound))
;
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTestFontColor() {
            String filename = "fontCleanup.pdf";
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(inputPath + filename), new PdfWriter(outputPath + filename
                ));
            new iText.PdfCleanup.PdfCleanUpTool(pdfDoc, true).CleanUp();
            pdfDoc.Close();
            NUnit.Framework.Assert.IsNull(new CompareTool().CompareVisually(outputPath + filename, inputPath + "cmp_" 
                + filename, outputPath, "diff_"));
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpAnnotationSetterTest() {
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(inputPath + "fontCleanup.pdf"), new PdfWriter(new MemoryStream
                ()));
            iText.PdfCleanup.PdfCleanUpTool cleanUpTool = new iText.PdfCleanup.PdfCleanUpTool(pdfDoc);
            NUnit.Framework.Assert.IsTrue(cleanUpTool.IsProcessAnnotations());
            cleanUpTool.SetProcessAnnotations(false);
            NUnit.Framework.Assert.IsFalse(cleanUpTool.IsProcessAnnotations());
        }

        [NUnit.Framework.Test]
        public virtual void DocumentInNonStampingModeTest() {
            NUnit.Framework.Assert.That(() =>  {
                PdfDocument pdfDocument = new PdfDocument(new PdfReader(inputPath + "fontCleanup.pdf"));
                new iText.PdfCleanup.PdfCleanUpTool(pdfDocument);
            }
            , NUnit.Framework.Throws.InstanceOf<PdfException>().With.Message.EqualTo(PdfException.PdfDocumentMustBeOpenedInStampingMode))
;
        }

        [NUnit.Framework.Test]
        public virtual void DocumentWithoutReaderTest() {
            NUnit.Framework.Assert.That(() =>  {
                PdfDocument pdfDocument = new PdfDocument(new PdfWriter(new MemoryStream()));
                new iText.PdfCleanup.PdfCleanUpTool(pdfDocument);
            }
            , NUnit.Framework.Throws.InstanceOf<PdfException>().With.Message.EqualTo(PdfException.PdfDocumentMustBeOpenedInStampingMode))
;
        }

        [NUnit.Framework.Test]
        [LogMessage(iText.IO.LogMessageConstant.PDF_REFERS_TO_NOT_EXISTING_PROPERTY_DICTIONARY)]
        public virtual void NoPropertiesInResourcesTest() {
            String fileName = "noPropertiesInResourcesTest";
            String input = inputPath + fileName + ".pdf";
            String output = outputPath + fileName + ".pdf";
            String cmp = inputPath + "cmp_" + fileName + ".pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(0, 0, 595, 842), ColorConstants.RED));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_" + fileName);
        }

        [NUnit.Framework.Test]
        [LogMessage(iText.IO.LogMessageConstant.PDF_REFERS_TO_NOT_EXISTING_PROPERTY_DICTIONARY)]
        public virtual void IncorrectBDCToBMCTest() {
            String fileName = "incorrectBDCToBMCTest";
            String input = inputPath + fileName + ".pdf";
            String output = outputPath + fileName + ".pdf";
            String cmp = inputPath + "cmp_" + fileName + ".pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(0, 0, 10, 10), ColorConstants.RED));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, outputPath, "diff_" + fileName);
        }

        [NUnit.Framework.Test]
        [LogMessage(CleanUpLogMessageConstant.FAILED_TO_PROCESS_A_TRANSFORMATION_MATRIX)]
        public virtual void NoninvertibleMatrixRemoveAllTest() {
            String fileName = "noninvertibleMatrixRemoveAllTest";
            String input = inputPath + "noninvertibleMatrix.pdf";
            String output = outputPath + fileName + ".pdf";
            String cmp = inputPath + "cmp_" + fileName + ".pdf";
            iText.PdfCleanup.PdfCleanUpLocation wholePageLocation = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle
                (0, 0, 595, 842), null);
            CleanUp(input, output, JavaUtil.ArraysAsList(wholePageLocation));
            CompareByContent(cmp, output, outputPath, "diff_noninvertibleMatrixRemoveAllTest");
        }

        [NUnit.Framework.Test]
        [LogMessage(CleanUpLogMessageConstant.FAILED_TO_PROCESS_A_TRANSFORMATION_MATRIX)]
        public virtual void NoninvertibleMatrixRemoveAllTest02() {
            String fileName = "noninvertibleMatrixRemoveAllTest02";
            String input = inputPath + "noninvertibleMatrix.pdf";
            String output = outputPath + fileName + ".pdf";
            String cmp = inputPath + "cmp_" + fileName + ".pdf";
            iText.PdfCleanup.PdfCleanUpLocation wholePageLocation = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle
                (-1000, -1000, 2000, 2000), null);
            CleanUp(input, output, JavaUtil.ArraysAsList(wholePageLocation));
            CompareByContent(cmp, output, outputPath, "diff_noninvertibleMatrixRemoveAllTest");
        }

        [NUnit.Framework.Test]
        [LogMessage(CleanUpLogMessageConstant.FAILED_TO_PROCESS_A_TRANSFORMATION_MATRIX)]
        public virtual void NoninvertibleMatrixRemoveNothingTest() {
            String fileName = "noninvertibleMatrixRemoveNothingTest";
            String input = inputPath + "noninvertibleMatrix.pdf";
            String output = outputPath + fileName + ".pdf";
            String cmp = inputPath + "cmp_" + fileName + ".pdf";
            iText.PdfCleanup.PdfCleanUpLocation dummyLocation = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle
                (0, 0, 0, 0), null);
            CleanUp(input, output, JavaUtil.ArraysAsList(dummyLocation));
            CompareByContent(cmp, output, outputPath, "diff_noninvertibleMatrixRemoveNothingTest");
        }

        [NUnit.Framework.Test]
        [LogMessage(CleanUpLogMessageConstant.FAILED_TO_PROCESS_A_TRANSFORMATION_MATRIX, Count = 7)]
        public virtual void PathAndIncorrectCMTest() {
            String fileName = "pathAndIncorrectCM";
            String input = inputPath + "pathAndIncorrectCM.pdf";
            String output = outputPath + fileName + ".pdf";
            String cmp = inputPath + "cmp_" + fileName + ".pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> dummyLocationsList = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            for (int i = 0; i < 3; i++) {
                dummyLocationsList.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(0, 0, 0, 0), null));
            }
            CleanUp(input, output, dummyLocationsList);
            CompareByContent(cmp, output, outputPath, "diff_pathAndIncorrectCMTest");
        }

        [NUnit.Framework.Test]
        public virtual void SimpleCleanUpOnRotatedPages() {
            String fileName = "simpleCleanUpOnRotatedPages";
            String input = inputPath + "documentWithRotatedPages.pdf";
            String output = outputPath + fileName + ".pdf";
            String cmp = inputPath + "cmp_" + fileName + ".pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> locationsList = new List<iText.PdfCleanup.PdfCleanUpLocation>();
            for (int i = 0; i < 4; i++) {
                locationsList.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(100, 100, 200, 100), ColorConstants
                    .GREEN));
            }
            CleanUp(input, output, locationsList);
            CompareByContent(cmp, output, outputPath, "diff_pathAndIncorrectCMTest");
        }

        [NUnit.Framework.Test]
        public virtual void SimpleCleanUpOnRotatedPagesIgnoreRotation() {
            String fileName = "simpleCleanUpOnRotatedPagesIgnoreRotation";
            String input = inputPath + "documentWithRotatedPages.pdf";
            String output = outputPath + fileName + ".pdf";
            String cmp = inputPath + "cmp_" + fileName + ".pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            IList<iText.PdfCleanup.PdfCleanUpLocation> locationsList = new List<iText.PdfCleanup.PdfCleanUpLocation>();
            for (int i = 0; i < 4; i++) {
                locationsList.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, Rectangle.GetRectangleOnRotatedPage(new Rectangle
                    (100, 100, 200, 100), pdfDocument.GetPage(i + 1)), ColorConstants.GREEN));
            }
            iText.PdfCleanup.PdfCleanUpTool cleaner = new iText.PdfCleanup.PdfCleanUpTool(pdfDocument, locationsList);
            cleaner.CleanUp();
            pdfDocument.Close();
            CompareByContent(cmp, output, outputPath, "diff_pathAndIncorrectCMTest");
        }

        private void CleanUp(String input, String output, IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations
            ) {
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            iText.PdfCleanup.PdfCleanUpTool cleaner = (cleanUpLocations == null) ? new iText.PdfCleanup.PdfCleanUpTool
                (pdfDocument, true) : new iText.PdfCleanup.PdfCleanUpTool(pdfDocument, cleanUpLocations);
            cleaner.CleanUp();
            pdfDocument.Close();
        }

        private void CompareByContent(String cmp, String output, String targetDir, String diffPrefix) {
            CompareTool cmpTool = new CompareTool();
            String errorMessage = cmpTool.CompareByContent(output, cmp, targetDir, diffPrefix + "_");
            if (errorMessage != null) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }
    }
}
