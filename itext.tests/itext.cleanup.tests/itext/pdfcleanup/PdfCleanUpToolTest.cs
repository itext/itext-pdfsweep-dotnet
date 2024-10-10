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
using System.Collections.Generic;
using System.IO;
using iText.Commons.Utils;
using iText.Kernel.Colors;
using iText.Kernel.Exceptions;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Utils;
using iText.PdfCleanup.Autosweep;
using iText.PdfCleanup.Exceptions;
using iText.PdfCleanup.Logs;
using iText.PdfCleanup.Util;
using iText.Test;
using iText.Test.Attributes;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("IntegrationTest")]
    public class PdfCleanUpToolTest : ExtendedITextTest {
        private static readonly String INPUT_PATH = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/PdfCleanUpToolTest/";

        private static readonly String OUTPUT_PATH = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/PdfCleanUpToolTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(OUTPUT_PATH);
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest01() {
            String input = INPUT_PATH + "page229.pdf";
            String output = OUTPUT_PATH + "page229_01.pdf";
            String cmp = INPUT_PATH + "cmp_page229_01.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(240.0f, 602.3f, 275.7f - 240.0f, 614.8f - 602.3f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(171.3f, 550.3f, 208.4f - 171.3f, 562.8f - 550.3f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(270.7f, 459.2f, 313.1f - 270.7f, 471.7f - 459.2f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(249.9f, 329.3f, 279.6f - 249.9f, 341.8f - 329.3f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(216.2f, 303.3f, 273.0f - 216.2f, 315.8f - 303.3f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_01");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest02() {
            String input = INPUT_PATH + "page229-modified-Tc-Tw.pdf";
            String output = OUTPUT_PATH + "page229-modified-Tc-Tw.pdf";
            String cmp = INPUT_PATH + "cmp_page229-modified-Tc-Tw.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(240.0f, 602.3f, 275.7f - 240.0f, 614.8f - 602.3f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(171.3f, 550.3f, 208.4f - 171.3f, 562.8f - 550.3f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(270.7f, 459.2f, 313.1f - 270.7f, 471.7f - 459.2f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(249.9f, 329.3f, 279.6f - 249.9f, 341.8f - 329.3f), ColorConstants.GRAY), new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(216.2f, 303.3f, 273.0f - 216.2f, 315.8f - 303.3f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_02");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest03() {
            String input = INPUT_PATH + "page166_03.pdf";
            String output = OUTPUT_PATH + "page166_03.pdf";
            String cmp = INPUT_PATH + "cmp_page166_03.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_03");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTestSvg() {
            // TODO: update cmp file after DEVSIX-3185 fixed
            String input = INPUT_PATH + "line_chart.pdf";
            String output = OUTPUT_PATH + "line_chart.pdf";
            String cmp = INPUT_PATH + "cmp_line_chart.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(60f, 
                780f, 60f, 45f), ColorConstants.GRAY)));
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_Svg");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest04() {
            String input = INPUT_PATH + "hello_05.pdf";
            String output = OUTPUT_PATH + "hello_05.pdf";
            String cmp = INPUT_PATH + "cmp_hello_05.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_04");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest05() {
            String input = INPUT_PATH + "BigImage-jpg.pdf";
            String output = OUTPUT_PATH + "BigImage-jpg.pdf";
            String cmp = INPUT_PATH + "cmp_BigImage-jpg.pdf";
            CleanUp(input, output, null);
            CleanUpImagesCompareTool cmpTool = new CleanUpImagesCompareTool();
            String errorMessage = cmpTool.ExtractAndCompareImages(output, cmp, OUTPUT_PATH, "1");
            String compareByContentResult = cmpTool.CompareByContent(output, cmp, OUTPUT_PATH);
            if (compareByContentResult != null) {
                errorMessage += compareByContentResult;
            }
            if (!errorMessage.Equals("")) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest06() {
            String input = INPUT_PATH + "BigImage-png.pdf";
            String output = OUTPUT_PATH + "BigImage-png.pdf";
            String cmp = INPUT_PATH + "cmp_BigImage-png.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_06");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest07() {
            String input = INPUT_PATH + "BigImage-tif.pdf";
            String output = OUTPUT_PATH + "BigImage-tif.pdf";
            String cmp = INPUT_PATH + "cmp_BigImage-tif.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_07");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest08() {
            String input = INPUT_PATH + "BigImage-tif-lzw.pdf";
            String output = OUTPUT_PATH + "BigImage-tif-lzw.pdf";
            String cmp = INPUT_PATH + "cmp_BigImage-tif-lzw.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_08");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest09() {
            String input = INPUT_PATH + "simpleImmediate.pdf";
            String output = OUTPUT_PATH + "simpleImmediate.pdf";
            String cmp = INPUT_PATH + "cmp_simpleImmediate.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(97f, 405f, 480f - 97f, 445f - 405f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_09");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest10() {
            String input = INPUT_PATH + "simpleImmediate-tm.pdf";
            String output = OUTPUT_PATH + "simpleImmediate-tm.pdf";
            String cmp = INPUT_PATH + "cmp_simpleImmediate-tm.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(97f, 405f, 480f - 97f, 445f - 405f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_10");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest11() {
            String input = INPUT_PATH + "multiUseIndirect.pdf";
            String output = OUTPUT_PATH + "multiUseIndirect.pdf";
            String cmp = INPUT_PATH + "cmp_multiUseIndirect.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(97f, 605f, 480f - 97f, 645f - 605f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_11");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest12() {
            String input = INPUT_PATH + "multiUseImage.pdf";
            String output = OUTPUT_PATH + "multiUseImage.pdf";
            String cmp = INPUT_PATH + "cmp_multiUseImage.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(97f, 405f, 480f - 97f, 445f - 405f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_12");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest13() {
            String input = INPUT_PATH + "maskImage.pdf";
            String output = OUTPUT_PATH + "maskImage.pdf";
            String cmp = INPUT_PATH + "cmp_maskImage.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(97f, 405f, 480f - 97f, 445f - 405f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_13");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest14() {
            String input = INPUT_PATH + "rotatedImg.pdf";
            String output = OUTPUT_PATH + "rotatedImg.pdf";
            String cmp = INPUT_PATH + "cmp_rotatedImg.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(97f, 405f, 480f - 97f, 445f - 405f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_14");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest15() {
            String input = INPUT_PATH + "lineArtsCompletely.pdf";
            String output = OUTPUT_PATH + "lineArtsCompletely.pdf";
            String cmp = INPUT_PATH + "cmp_lineArtsCompletely.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_15");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest16() {
            String input = INPUT_PATH + "lineArtsPartially.pdf";
            String output = OUTPUT_PATH + "lineArtsPartially.pdf";
            String cmp = INPUT_PATH + "cmp_lineArtsPartially.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_16");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest17() {
            String input = INPUT_PATH + "dashedStyledClosedBezier.pdf";
            String output = OUTPUT_PATH + "dashedStyledClosedBezier.pdf";
            String cmp = INPUT_PATH + "cmp_dashedStyledClosedBezier.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_17");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest18() {
            String input = INPUT_PATH + "styledLineArts.pdf";
            String output = OUTPUT_PATH + "styledLineArts.pdf";
            String cmp = INPUT_PATH + "cmp_styledLineArts.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_18");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest19() {
            String input = INPUT_PATH + "dashedBezier.pdf";
            String output = OUTPUT_PATH + "dashedBezier.pdf";
            String cmp = INPUT_PATH + "cmp_dashedBezier.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_19");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest20() {
            String input = INPUT_PATH + "closedBezier.pdf";
            String output = OUTPUT_PATH + "closedBezier.pdf";
            String cmp = INPUT_PATH + "cmp_closedBezier.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_20");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest21() {
            String input = INPUT_PATH + "clippingNWRule.pdf";
            String output = OUTPUT_PATH + "clippingNWRule.pdf";
            String cmp = INPUT_PATH + "cmp_clippingNWRule.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_21");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest22() {
            String input = INPUT_PATH + "dashedClosedRotatedTriangles.pdf";
            String output = OUTPUT_PATH + "dashedClosedRotatedTriangles.pdf";
            String cmp = INPUT_PATH + "cmp_dashedClosedRotatedTriangles.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_22");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest23() {
            String input = INPUT_PATH + "miterTest.pdf";
            String output = OUTPUT_PATH + "miterTest.pdf";
            String cmp = INPUT_PATH + "cmp_miterTest.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_23");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest24() {
            String input = INPUT_PATH + "degenerateCases.pdf";
            String output = OUTPUT_PATH + "degenerateCases.pdf";
            String cmp = INPUT_PATH + "cmp_degenerateCases.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_24");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest25() {
            String input = INPUT_PATH + "absentICentry.pdf";
            String output = OUTPUT_PATH + "absentICentry.pdf";
            String cmp = INPUT_PATH + "cmp_absentICentry.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_25");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest26() {
            String input = INPUT_PATH + "lotOfDashes.pdf";
            String output = OUTPUT_PATH + "lotOfDashes.pdf";
            String cmp = INPUT_PATH + "cmp_lotOfDashes.pdf";
            CleanUp(input, output, null);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_26");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest27() {
            String input = INPUT_PATH + "clipPathReduction.pdf";
            String output = OUTPUT_PATH + "clipPathReduction.pdf";
            String cmp = INPUT_PATH + "cmp_clipPathReduction.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(212, 394, 186, 170), null));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_27");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest30() {
            String input = INPUT_PATH + "inlineImages.pdf";
            String output = OUTPUT_PATH + "inlineImages_full.pdf";
            String cmp = INPUT_PATH + "cmp_inlineImages_full.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(10, 100, 400, 600), null));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_30");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest32() {
            String input = INPUT_PATH + "page229.pdf";
            String output = OUTPUT_PATH + "wholePageCleanUp.pdf";
            String cmp = INPUT_PATH + "cmp_wholePageCleanUp.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(1, 1
                , PageSize.A4.GetWidth() - 1, PageSize.A4.GetHeight() - 1))));
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_32");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest33() {
            String input = INPUT_PATH + "viewer_prefs_dict_table.pdf";
            String output = OUTPUT_PATH + "complexTextPositioning.pdf";
            String cmp = INPUT_PATH + "cmp_complexTextPositioning.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(300f
                , 370f, 215f, 270f))));
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_33");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest34() {
            String input = INPUT_PATH + "new_york_times.pdf";
            String output = OUTPUT_PATH + "textAndImages.pdf";
            String cmp = INPUT_PATH + "cmp_textAndImages.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(150f
                , 235f, 230f, 445f))));
            CleanUpImagesCompareTool cmpTool = new CleanUpImagesCompareTool();
            String errorMessage = cmpTool.ExtractAndCompareImages(output, cmp, OUTPUT_PATH, "1.2");
            String compareByContentResult = cmpTool.CompareByContent(output, cmp, OUTPUT_PATH);
            if (compareByContentResult != null) {
                errorMessage += compareByContentResult;
            }
            if (!errorMessage.Equals("")) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest35() {
            String input = INPUT_PATH + "lineArtsSimple.pdf";
            String output = OUTPUT_PATH + "lineArtsSimple.pdf";
            String cmp = INPUT_PATH + "cmp_lineArtsSimple.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(60f, 
                80f, 460f, 65f), ColorConstants.GRAY)));
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_35");
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
            String input = INPUT_PATH + "bigOne.pdf";
            String output = OUTPUT_PATH + "bigOne.pdf";
            String cmp = INPUT_PATH + "cmp_bigOne.pdf";
            CleanUp(input, output, JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(300f
                , 370f, 215f, 270f), ColorConstants.GRAY)));
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_36");
        }

        /// <summary>In this test we check that line style operators (such as 'w') are processed correctly</summary>
        [NUnit.Framework.Test]
        public virtual void CleanUpTest37() {
            String input = INPUT_PATH + "helloHelvetica.pdf";
            String output = OUTPUT_PATH + "helloHelvetica.pdf";
            String cmp = INPUT_PATH + "cmp_helloHelvetica.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(0f, 0f, 595f, 680f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_37");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest38() {
            String input = INPUT_PATH + "helloHelvetica02.pdf";
            String output = OUTPUT_PATH + "helloHelvetica02.pdf";
            String cmp = INPUT_PATH + "cmp_helloHelvetica02.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(0f, 0f, 0f, 0f), ColorConstants.GRAY));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_38");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest39() {
            String input = INPUT_PATH + "corruptJpeg.pdf";
            String output = OUTPUT_PATH + "corruptJpeg.pdf";
            String cmp = INPUT_PATH + "cmp_corruptJpeg.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(100, 350, 100, 200), ColorConstants.ORANGE));
            CleanUp(input, output, cleanUpLocations);
            CleanUpImagesCompareTool cmpTool = new CleanUpImagesCompareTool();
            String errorMessage = cmpTool.ExtractAndCompareImages(output, cmp, OUTPUT_PATH, "1.2");
            String compareByContentResult = cmpTool.CompareByContent(output, cmp, OUTPUT_PATH);
            if (compareByContentResult != null) {
                errorMessage += compareByContentResult;
            }
            if (!errorMessage.Equals("")) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest40() {
            String input = INPUT_PATH + "emptyTj01.pdf";
            String output = OUTPUT_PATH + "emptyTj01.pdf";
            String cmp = INPUT_PATH + "cmp_emptyTj01.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(70f, 555f, 200f, 5f), ColorConstants.ORANGE));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_40");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest41() {
            String input = INPUT_PATH + "newLines01.pdf";
            String output = OUTPUT_PATH + "newLines01.pdf";
            String cmp = INPUT_PATH + "cmp_newLines01.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(70f, 555f, 200f, 10f), ColorConstants.ORANGE));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_41");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest42() {
            String input = INPUT_PATH + "newLines02.pdf";
            String output = OUTPUT_PATH + "newLines02.pdf";
            String cmp = INPUT_PATH + "cmp_newLines02.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(36f, 733f, 270f, 5f), ColorConstants.ORANGE));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_42");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest43() {
            String input = INPUT_PATH + "newLines03.pdf";
            String output = OUTPUT_PATH + "newLines03.pdf";
            String cmp = INPUT_PATH + "cmp_newLines03.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(36f, 733f, 230f, 5f), ColorConstants.ORANGE));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_43");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest44() {
            String input = INPUT_PATH + "emptyTj02.pdf";
            String output = OUTPUT_PATH + "emptyTj02.pdf";
            String cmp = INPUT_PATH + "cmp_emptyTj02.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(70f, 565f, 200f, 5f), ColorConstants.ORANGE));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_44");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest45() {
            String input = INPUT_PATH + "emptyPdf.pdf";
            String output = OUTPUT_PATH + "emptyPdf.pdf";
            String cmp = INPUT_PATH + "cmp_emptyPdf.pdf";
            PdfAnnotation redactAnnotation = new PdfRedactAnnotation(new Rectangle(97, 405, 383, 40)).SetOverlayText(new 
                PdfString("OverlayTest")).SetDefaultAppearance(new PdfString("/Helv 0 Tf 0 g"));
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            pdfDocument.GetFirstPage().AddAnnotation(redactAnnotation);
            PdfCleaner.CleanUpRedactAnnotations(pdfDocument);
            pdfDocument.Close();
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_45");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTest46() {
            String input = INPUT_PATH + "emptyPdf.pdf";
            String output = OUTPUT_PATH + "emptyPdf.pdf";
            PdfAnnotation redactAnnotation = new PdfRedactAnnotation(new Rectangle(97, 405, 383, 40)).SetOverlayText(new 
                PdfString("OverlayTest"));
            using (PdfReader reader = new PdfReader(input)) {
                using (PdfWriter writer = new PdfWriter(output)) {
                    using (PdfDocument pdfDocument = new PdfDocument(reader, writer)) {
                        pdfDocument.GetFirstPage().AddAnnotation(redactAnnotation);
                        Exception e = NUnit.Framework.Assert.Catch(typeof(PdfException), () => PdfCleaner.CleanUpRedactAnnotations
                            (pdfDocument));
                        NUnit.Framework.Assert.AreEqual(CleanupExceptionMessageConstant.DEFAULT_APPEARANCE_NOT_FOUND, e.Message);
                    }
                }
            }
        }

        [NUnit.Framework.Test]
        public virtual void AutoCleanWithLocationAndStreamParamsTest() {
            String input = INPUT_PATH + "fontCleanup.pdf";
            String output = OUTPUT_PATH + "autoCleanWithLocationAndStreamParamsTest.pdf";
            String cmp = INPUT_PATH + "cmp_autoCleanWithLocationAndStreamParamsTest.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("leonard"));
            IList<iText.PdfCleanup.PdfCleanUpLocation> additionalLocation = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(0, 0, 200, 100)));
            PdfCleaner.AutoSweepCleanUp(new FileStream(input, FileMode.Open, FileAccess.Read), new FileStream(output, 
                FileMode.Create), strategy, additionalLocation);
            CompareByContent(cmp, output, OUTPUT_PATH, "autoCleanWithLocationAndStreamParamsTest");
        }

        [NUnit.Framework.Test]
        public virtual void AutoCleanPageTest() {
            String input = INPUT_PATH + "fontCleanup.pdf";
            String output = OUTPUT_PATH + "autoCleanPageTest.pdf";
            String cmp = INPUT_PATH + "cmp_autoCleanPageTest.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("leonard"));
            using (PdfReader reader = new PdfReader(input)) {
                using (PdfWriter writer = new PdfWriter(output)) {
                    using (PdfDocument document = new PdfDocument(reader, writer)) {
                        PdfCleaner.AutoSweepCleanUp(document.GetPage(1), strategy);
                    }
                }
            }
            CompareByContent(cmp, output, OUTPUT_PATH, "autoCleanPageTest");
        }

        [NUnit.Framework.Test]
        public virtual void AutoCleanPageWithAdditionalLocationTest() {
            String input = INPUT_PATH + "fontCleanup.pdf";
            String output = OUTPUT_PATH + "autoCleanPageWithAdditionalLocationTest.pdf";
            String cmp = INPUT_PATH + "cmp_autoCleanPageWithAdditionalLocationTest.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("leonard"));
            IList<iText.PdfCleanup.PdfCleanUpLocation> additionalLocation = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(0, 0, 200, 100), ColorConstants.RED));
            using (PdfReader reader = new PdfReader(input)) {
                using (PdfWriter writer = new PdfWriter(output)) {
                    using (PdfDocument document = new PdfDocument(reader, writer)) {
                        PdfCleaner.AutoSweepCleanUp(document.GetPage(1), strategy, additionalLocation);
                    }
                }
            }
            CompareByContent(cmp, output, OUTPUT_PATH, "autoCleanPageWithAdditionalLocationTest");
        }

        [NUnit.Framework.Test]
        public virtual void AutoCleanPageWithAdditionalLocationAndPropertyTest() {
            String input = INPUT_PATH + "fontCleanup.pdf";
            String output = OUTPUT_PATH + "autoCleanPageWithAdditionalLocationAndPropertyTest.pdf";
            String cmp = INPUT_PATH + "cmp_autoCleanPageWithAdditionalLocationAndPropertyTest.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("leonard"));
            IList<iText.PdfCleanup.PdfCleanUpLocation> additionalLocation = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(0, 0, 200, 100), ColorConstants.RED));
            using (PdfReader reader = new PdfReader(input)) {
                using (PdfWriter writer = new PdfWriter(output)) {
                    using (PdfDocument document = new PdfDocument(reader, writer)) {
                        PdfCleaner.AutoSweepCleanUp(document.GetPage(1), strategy, additionalLocation, new CleanUpProperties());
                    }
                }
            }
            CompareByContent(cmp, output, OUTPUT_PATH, "autoCleanPageWithAdditionalLocationAndPropertyTest");
        }

        [NUnit.Framework.Test]
        public virtual void AutoCleanWithCleaUpPropertiesTest() {
            String input = INPUT_PATH + "absentICentry.pdf";
            String output = OUTPUT_PATH + "autoCleanWithCleaUpPropertiesTest.pdf";
            String cmp = INPUT_PATH + "cmp_autoCleanWithCleaUpPropertiesTest.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("leonard"));
            IList<iText.PdfCleanup.PdfCleanUpLocation> additionalLocation = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            additionalLocation.Add(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(100, 100, 500, 500)));
            PdfCleaner.AutoSweepCleanUp(new FileStream(input, FileMode.Open, FileAccess.Read), new FileStream(output, 
                FileMode.Create), strategy, additionalLocation, new CleanUpProperties());
            CompareByContent(cmp, output, OUTPUT_PATH, "autoCleanWithCleaUpPropertiesTest");
        }

        [NUnit.Framework.Test]
        public virtual void AutoCleanWithFalseProcessAnnotationTest() {
            String input = INPUT_PATH + "absentICentry.pdf";
            String output = OUTPUT_PATH + "autoCleanWithFalseProcessAnnotationTest.pdf";
            String cmp = INPUT_PATH + "cmp_autoCleanWithFalseProcessAnnotationTest.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("leonard"));
            IList<iText.PdfCleanup.PdfCleanUpLocation> additionalLocation = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            additionalLocation.Add(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(100, 560, 200, 30)));
            CleanUpProperties properties = new CleanUpProperties().SetProcessAnnotations(false);
            PdfCleaner.AutoSweepCleanUp(new FileStream(input, FileMode.Open, FileAccess.Read), new FileStream(output, 
                FileMode.Create), strategy, additionalLocation, properties);
            CompareByContent(cmp, output, OUTPUT_PATH, "autoCleanWithFalseProcessAnnotationTest");
        }

        [NUnit.Framework.Test]
        public virtual void DocumentInNonStampingModeTest() {
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(INPUT_PATH + "fontCleanup.pdf"));
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(0, 0, 500, 500)));
            Exception e = NUnit.Framework.Assert.Catch(typeof(PdfException), () => PdfCleaner.CleanUp(pdfDocument, cleanUpLocations
                ));
            NUnit.Framework.Assert.AreEqual(CleanupExceptionMessageConstant.PDF_DOCUMENT_MUST_BE_OPENED_IN_STAMPING_MODE
                , e.Message);
        }

        [NUnit.Framework.Test]
        public virtual void DocumentWithoutReaderTest() {
            PdfDocument pdfDocument = new PdfDocument(new PdfWriter(new MemoryStream()));
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(0, 0, 500, 500)));
            Exception e = NUnit.Framework.Assert.Catch(typeof(PdfException), () => PdfCleaner.CleanUp(pdfDocument, cleanUpLocations
                ));
            NUnit.Framework.Assert.AreEqual(CleanupExceptionMessageConstant.PDF_DOCUMENT_MUST_BE_OPENED_IN_STAMPING_MODE
                , e.Message);
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpTestFontColor() {
            String filename = "fontCleanup.pdf";
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(INPUT_PATH + filename), new PdfWriter(OUTPUT_PATH + filename
                ));
            PdfCleaner.CleanUpRedactAnnotations(pdfDoc);
            pdfDoc.Close();
            NUnit.Framework.Assert.IsNull(new CompareTool().CompareVisually(OUTPUT_PATH + filename, INPUT_PATH + "cmp_"
                 + filename, OUTPUT_PATH, "diff_"));
        }

        [NUnit.Framework.Test]
        [LogMessage(iText.IO.Logs.IoLogMessageConstant.PDF_REFERS_TO_NOT_EXISTING_PROPERTY_DICTIONARY)]
        public virtual void NoPropertiesInResourcesTest() {
            String fileName = "noPropertiesInResourcesTest";
            String input = INPUT_PATH + fileName + ".pdf";
            String output = OUTPUT_PATH + fileName + ".pdf";
            String cmp = INPUT_PATH + "cmp_" + fileName + ".pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(0, 0, 595, 842), ColorConstants.RED));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_" + fileName);
        }

        [NUnit.Framework.Test]
        [LogMessage(iText.IO.Logs.IoLogMessageConstant.PDF_REFERS_TO_NOT_EXISTING_PROPERTY_DICTIONARY)]
        public virtual void IncorrectBDCToBMCTest() {
            String fileName = "incorrectBDCToBMCTest";
            String input = INPUT_PATH + fileName + ".pdf";
            String output = OUTPUT_PATH + fileName + ".pdf";
            String cmp = INPUT_PATH + "cmp_" + fileName + ".pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(0, 0, 10, 10), ColorConstants.RED));
            CleanUp(input, output, cleanUpLocations);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_" + fileName);
        }

        [NUnit.Framework.Test]
        [LogMessage(CleanUpLogMessageConstant.FAILED_TO_PROCESS_A_TRANSFORMATION_MATRIX)]
        public virtual void NoninvertibleMatrixRemoveAllTest() {
            String fileName = "noninvertibleMatrixRemoveAllTest";
            String input = INPUT_PATH + "noninvertibleMatrix.pdf";
            String output = OUTPUT_PATH + fileName + ".pdf";
            String cmp = INPUT_PATH + "cmp_" + fileName + ".pdf";
            iText.PdfCleanup.PdfCleanUpLocation wholePageLocation = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle
                (0, 0, 595, 842), null);
            CleanUp(input, output, JavaUtil.ArraysAsList(wholePageLocation));
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_noninvertibleMatrixRemoveAllTest");
        }

        [NUnit.Framework.Test]
        [LogMessage(CleanUpLogMessageConstant.FAILED_TO_PROCESS_A_TRANSFORMATION_MATRIX)]
        public virtual void NoninvertibleMatrixRemoveAllTest02() {
            String fileName = "noninvertibleMatrixRemoveAllTest02";
            String input = INPUT_PATH + "noninvertibleMatrix.pdf";
            String output = OUTPUT_PATH + fileName + ".pdf";
            String cmp = INPUT_PATH + "cmp_" + fileName + ".pdf";
            iText.PdfCleanup.PdfCleanUpLocation wholePageLocation = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle
                (-1000, -1000, 2000, 2000), null);
            CleanUp(input, output, JavaUtil.ArraysAsList(wholePageLocation));
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_noninvertibleMatrixRemoveAllTest");
        }

        [NUnit.Framework.Test]
        [LogMessage(CleanUpLogMessageConstant.FAILED_TO_PROCESS_A_TRANSFORMATION_MATRIX)]
        public virtual void NoninvertibleMatrixRemoveNothingTest() {
            String fileName = "noninvertibleMatrixRemoveNothingTest";
            String input = INPUT_PATH + "noninvertibleMatrix.pdf";
            String output = OUTPUT_PATH + fileName + ".pdf";
            String cmp = INPUT_PATH + "cmp_" + fileName + ".pdf";
            iText.PdfCleanup.PdfCleanUpLocation dummyLocation = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle
                (0, 0, 0, 0), null);
            CleanUp(input, output, JavaUtil.ArraysAsList(dummyLocation));
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_noninvertibleMatrixRemoveNothingTest");
        }

        [NUnit.Framework.Test]
        [LogMessage(CleanUpLogMessageConstant.FAILED_TO_PROCESS_A_TRANSFORMATION_MATRIX, Count = 7)]
        public virtual void PathAndIncorrectCMTest() {
            String fileName = "pathAndIncorrectCM";
            String input = INPUT_PATH + "pathAndIncorrectCM.pdf";
            String output = OUTPUT_PATH + fileName + ".pdf";
            String cmp = INPUT_PATH + "cmp_" + fileName + ".pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> dummyLocationsList = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            for (int i = 0; i < 3; i++) {
                dummyLocationsList.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(0, 0, 0, 0), null));
            }
            CleanUp(input, output, dummyLocationsList);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_pathAndIncorrectCMTest");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpStreamParamsTest() {
            String @in = INPUT_PATH + "page229.pdf";
            String @out = OUTPUT_PATH + "cleanUpStreamParamsTest.pdf";
            String cmp = INPUT_PATH + "cmp_cleanUpStreamParamsTest.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(100, 560, 200, 30)));
            Stream file = new FileStream(@in, FileMode.Open, FileAccess.Read);
            Stream output = new FileStream(@out, FileMode.Create);
            PdfCleaner.CleanUp(file, output, cleanUpLocations);
            CompareByContent(cmp, @out, OUTPUT_PATH, "diff_cleanUpStreamParamsTest");
        }

        [NUnit.Framework.Test]
        public virtual void AutoSweepCleanUpWithAdditionalLocationTest() {
            String @in = INPUT_PATH + "page229.pdf";
            String @out = OUTPUT_PATH + "autoSweepCleanUpWithAdditionalLocationTest.pdf";
            String cmp = INPUT_PATH + "cmp_autoSweepCleanUpWithAdditionalLocationTest.pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(100, 560, 200, 30)));
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy(" (T|t)o ").SetRedactionColor(ColorConstants.GREEN));
            using (PdfReader reader = new PdfReader(@in)) {
                using (PdfWriter writer = new PdfWriter(@out)) {
                    using (PdfDocument document = new PdfDocument(reader, writer)) {
                        PdfCleaner.AutoSweepCleanUp(document, strategy, cleanUpLocations);
                    }
                }
            }
            CompareTool cmpTool = new CompareTool();
            String errorMessage = cmpTool.CompareVisually(@out, cmp, OUTPUT_PATH, "diff_autoSweepCleanUpWithAdditionalLocationTest_"
                );
            if (errorMessage != null) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }

        [NUnit.Framework.Test]
        public virtual void SimpleCleanUpOnRotatedPages() {
            String fileName = "simpleCleanUpOnRotatedPages";
            String input = INPUT_PATH + "documentWithRotatedPages.pdf";
            String output = OUTPUT_PATH + fileName + ".pdf";
            String cmp = INPUT_PATH + "cmp_" + fileName + ".pdf";
            IList<iText.PdfCleanup.PdfCleanUpLocation> locationsList = new List<iText.PdfCleanup.PdfCleanUpLocation>();
            for (int i = 0; i < 4; i++) {
                locationsList.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, new Rectangle(100, 100, 200, 100), ColorConstants
                    .GREEN));
            }
            CleanUp(input, output, locationsList);
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_pathAndIncorrectCMTest");
        }

        [NUnit.Framework.Test]
        public virtual void SimpleCleanUpOnRotatedPagesIgnoreRotation() {
            String fileName = "simpleCleanUpOnRotatedPagesIgnoreRotation";
            String input = INPUT_PATH + "documentWithRotatedPages.pdf";
            String output = OUTPUT_PATH + fileName + ".pdf";
            String cmp = INPUT_PATH + "cmp_" + fileName + ".pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            IList<iText.PdfCleanup.PdfCleanUpLocation> locationsList = new List<iText.PdfCleanup.PdfCleanUpLocation>();
            for (int i = 0; i < 4; i++) {
                locationsList.Add(new iText.PdfCleanup.PdfCleanUpLocation(i + 1, Rectangle.GetRectangleOnRotatedPage(new Rectangle
                    (100, 100, 200, 100), pdfDocument.GetPage(i + 1)), ColorConstants.GREEN));
            }
            PdfCleaner.CleanUp(pdfDocument, locationsList);
            pdfDocument.Close();
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_pathAndIncorrectCMTest");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpDocWithoutReaderTest() {
            PdfDocument pdfDoc = new PdfDocument(new PdfWriter(new MemoryStream()));
            Exception e = NUnit.Framework.Assert.Catch(typeof(PdfException), () => PdfCleaner.CleanUpRedactAnnotations
                (pdfDoc));
            NUnit.Framework.Assert.AreEqual(CleanupExceptionMessageConstant.PDF_DOCUMENT_MUST_BE_OPENED_IN_STAMPING_MODE
                , e.Message);
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpDocWithoutWriterTest() {
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(INPUT_PATH + "emptyPdf.pdf"));
            Exception e = NUnit.Framework.Assert.Catch(typeof(PdfException), () => PdfCleaner.CleanUpRedactAnnotations
                (pdfDoc));
            NUnit.Framework.Assert.AreEqual(CleanupExceptionMessageConstant.PDF_DOCUMENT_MUST_BE_OPENED_IN_STAMPING_MODE
                , e.Message);
        }

        [NUnit.Framework.Test]
        public virtual void RedactLipsum() {
            String input = INPUT_PATH + "Lipsum.pdf";
            String output = OUTPUT_PATH + "cleanUpDocument.pdf";
            String cmp = INPUT_PATH + "cmp_cleanUpDocument.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(D|d)olor").SetRedactionColor(ColorConstants.GREEN));
            PdfWriter writer = new PdfWriter(output);
            writer.SetCompressionLevel(0);
            PdfDocument pdf = new PdfDocument(new PdfReader(input), writer);
            // sweep
            PdfCleaner.AutoSweepCleanUp(pdf, strategy);
            pdf.Close();
            // compare
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_cleanUpDocument_");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpPageTest() {
            String input = INPUT_PATH + "Lipsum.pdf";
            String output = OUTPUT_PATH + "cleanUpPage.pdf";
            String cmp = INPUT_PATH + "cmp_cleanUpPage.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(D|d)olor").SetRedactionColor(ColorConstants.GREEN));
            PdfWriter writer = new PdfWriter(output);
            writer.SetCompressionLevel(0);
            PdfDocument pdf = new PdfDocument(new PdfReader(input), writer);
            // sweep
            PdfCleaner.AutoSweepCleanUp(pdf.GetPage(1), strategy);
            pdf.Close();
            // compare
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_cleanUpPage_");
        }

        [NUnit.Framework.Test]
        public virtual void RedactLipsumPatternStartsWithWhiteSpace() {
            String input = INPUT_PATH + "Lipsum.pdf";
            String output = OUTPUT_PATH + "redactLipsumPatternStartsWithWhitespace.pdf";
            String cmp = INPUT_PATH + "cmp_redactLipsumPatternStartsWithWhitespace.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("\\s(D|d)olor").SetRedactionColor(ColorConstants.GREEN));
            PdfWriter writer = new PdfWriter(output);
            writer.SetCompressionLevel(0);
            PdfDocument pdf = new PdfDocument(new PdfReader(input), writer);
            // sweep
            PdfCleaner.AutoSweepCleanUp(pdf, strategy);
            pdf.Close();
            // compare
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_redactLipsumPatternStartsWithWhitespace_");
        }

        [NUnit.Framework.Test]
        [LogMessage(CleanUpLogMessageConstant.FAILED_TO_PROCESS_A_TRANSFORMATION_MATRIX, Count = 2)]
        public virtual void RedactPdfWithNoninvertibleMatrix() {
            String input = INPUT_PATH + "noninvertibleMatrix.pdf";
            String output = OUTPUT_PATH + "redactPdfWithNoninvertibleMatrix.pdf";
            String cmp = INPUT_PATH + "cmp_redactPdfWithNoninvertibleMatrix.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("Hello World!").SetRedactionColor(ColorConstants.GREEN));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            // sweep
            PdfCleaner.AutoSweepCleanUp(pdf, strategy);
            pdf.Close();
            // compare
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_redactPdfWithNoninvertibleMatrix_");
        }

        [NUnit.Framework.Test]
        [NUnit.Framework.Ignore("DEVSIX-4047")]
        public virtual void LineArtsDrawingOnCanvasTest() {
            String input = INPUT_PATH + "lineArtsDrawingOnCanvas.pdf";
            String output = OUTPUT_PATH + "lineArtsDrawingOnCanvas.pdf";
            String cmp = INPUT_PATH + "cmp_lineArtsDrawingOnCanvas.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(iphone)|(iPhone)"));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            PdfCleaner.AutoSweepCleanUp(pdf, strategy);
            pdf.Close();
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_lineArtsDrawingOnCanvasTest_");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpFullyFilteredImageTest() {
            String input = INPUT_PATH + "fullyFilteredImageDocument.pdf";
            String output = OUTPUT_PATH + "fullyFilteredImageDocument.pdf";
            String cmp = INPUT_PATH + "cmp_fullyFilteredImageDocument.pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output, new WriterProperties
                ()));
            iText.PdfCleanup.PdfCleanUpTool workingTool = new iText.PdfCleanup.PdfCleanUpTool(pdfDocument);
            int pageIndex = 1;
            Rectangle area = pdfDocument.GetPage(pageIndex).GetPageSize();
            workingTool.AddCleanupLocation(new iText.PdfCleanup.PdfCleanUpLocation(pageIndex, area));
            workingTool.CleanUp();
            pdfDocument.Close();
            CompareByContent(cmp, output, OUTPUT_PATH, "diff_fullyFilteredImageDocument_");
        }

        [NUnit.Framework.Test]
        public virtual void DirectPropertyObjectTest() {
            String input = INPUT_PATH + "DirectPropertyObject.pdf";
            String output = OUTPUT_PATH + "DirectPropertyObjectOutput.pdf";
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output, new WriterProperties
                ()));
            iText.PdfCleanup.PdfCleanUpTool workingTool = new iText.PdfCleanup.PdfCleanUpTool(pdfDocument);
            PageSize pgSize = pdfDocument.GetDefaultPageSize();
            Rectangle area = new Rectangle(0, 0, pgSize.GetWidth(), 50);
            workingTool.AddCleanupLocation(new iText.PdfCleanup.PdfCleanUpLocation(1, area));
            workingTool.CleanUp();
            pdfDocument.Close();
            PdfDocument resultDoc = new PdfDocument(new PdfReader(output));
            byte[] bytes = resultDoc.GetPage(1).GetFirstContentStream().GetBytes();
            String contentString = iText.Commons.Utils.JavaUtil.GetStringForBytes(bytes, System.Text.Encoding.UTF8);
            resultDoc.Close();
            //TODO DEVSIX-7387 change when bug is fixed
            NUnit.Framework.Assert.IsTrue(contentString.Contains("/PlacedPDF <</Metadata 14 0 R>> BDC"));
        }

        private void CleanUp(String input, String output, IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations
            ) {
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            if (cleanUpLocations == null) {
                PdfCleaner.CleanUpRedactAnnotations(pdfDocument);
            }
            else {
                PdfCleaner.CleanUp(pdfDocument, cleanUpLocations);
            }
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
