/*
This file is part of the iText (R) project.
Copyright (c) 1998-2025 Apryse Group NV
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
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Utils;
using iText.PdfCleanup.Autosweep;
using iText.PdfCleanup.Util;
using iText.Test;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("IntegrationTest")]
    public class BigDocumentAutoCleanUpTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/BigDocumentAutoCleanUpTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/BigDocumentAutoCleanUpTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        [NUnit.Framework.Test]
        public virtual void RedactTonySoprano() {
            String input = inputPath + "TheSopranos.pdf";
            String output = outputPath + "redactTonySoprano.pdf";
            String cmp = inputPath + "cmp_redactTonySoprano.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("Tony( |_)Soprano"));
            strategy.Add(new RegexBasedCleanupStrategy("Soprano"));
            strategy.Add(new RegexBasedCleanupStrategy("Sopranos"));
            PdfWriter writer = new PdfWriter(output);
            writer.SetCompressionLevel(0);
            PdfDocument pdf = new PdfDocument(new PdfReader(input), writer);
            // sweep
            PdfCleaner.AutoSweepCleanUp(pdf, strategy);
            pdf.Close();
            // compare
            CompareResults(cmp, output, outputPath, "4");
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpAreaCalculationPrecisionTest() {
            String input = inputPath + "cleanUpAreaCalculationPrecision.pdf";
            String output = outputPath + "cleanUpAreaCalculationPrecision.pdf";
            String cmp = inputPath + "cmp_cleanUpAreaCalculationPrecision.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new CustomLocationExtractionStrategy("(iphone)|(iPhone)"));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            // sweep
            PdfCleaner.AutoSweepCleanUp(pdf, strategy);
            pdf.Close();
            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(output, cmp, outputPath);
            NUnit.Framework.Assert.IsNull(errorMessage);
        }

        [NUnit.Framework.Test]
        public virtual void RedactIPhoneUserManualMatchColor() {
            String input = inputPath + "iphone_user_guide_untagged.pdf";
            String output = outputPath + "redactIPhoneUserManualMatchColor.pdf";
            String cmp = inputPath + "cmp_redactIPhoneUserManualMatchColor.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new CustomLocationExtractionStrategy("(iphone)|(iPhone)"));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            // sweep
            PdfCleaner.AutoSweepCleanUp(pdf, strategy);
            pdf.Close();
            // compare
            CleanUpImagesCompareTool cmpTool = new CleanUpImagesCompareTool();
            String errorMessage = cmpTool.ExtractAndCompareImages(output, cmp, outputPath, "4");
            // TODO DEVSIX-4047 Switch to compareByContent() when the ticket will be resolved
            String compareByContentResult = cmpTool.CompareVisually(output, cmp, outputPath, "diff_redactIPhoneUserManualMatchColor_"
                , cmpTool.GetIgnoredImagesAreas());
            if (compareByContentResult != null) {
                errorMessage += compareByContentResult;
            }
            if (!errorMessage.Equals("")) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }

        [NUnit.Framework.Test]
        public virtual void RedactIPhoneUserManual() {
            String input = inputPath + "iphone_user_guide_untagged.pdf";
            String output = outputPath + "redactIPhoneUserManual.pdf";
            String cmp = inputPath + "cmp_redactIPhoneUserManual.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(iphone)|(iPhone)"));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            // sweep
            PdfCleaner.AutoSweepCleanUp(pdf, strategy);
            pdf.Close();
            // compare
            CleanUpImagesCompareTool cmpTool = new CleanUpImagesCompareTool();
            String errorMessage = cmpTool.ExtractAndCompareImages(output, cmp, outputPath, "4");
            // TODO DEVSIX-4047 Switch to compareByContent() when the ticket will be resolved
            String compareByContentResult = cmpTool.CompareVisually(output, cmp, outputPath, "diff_redactIPhoneUserManual_"
                , cmpTool.GetIgnoredImagesAreas());
            if (compareByContentResult != null) {
                errorMessage += compareByContentResult;
            }
            if (!errorMessage.Equals("")) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }

        [NUnit.Framework.Test]
        public virtual void RedactIPhoneUserManualColored() {
            String input = inputPath + "iphone_user_guide_untagged_small.pdf";
            String output = outputPath + "redactIPhoneUserManualColored.pdf";
            String cmp = inputPath + "cmp_redactIPhoneUserManualColored.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(iphone)|(iPhone)").SetRedactionColor(ColorConstants.GREEN));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            // sweep
            PdfCleaner.AutoSweepCleanUp(pdf, strategy);
            pdf.Close();
            CompareResults(cmp, output, outputPath, "4");
        }

        private void CompareResults(String cmp, String output, String targetDir, String fuzzValue) {
            CleanUpImagesCompareTool cmpTool = new CleanUpImagesCompareTool();
            String errorMessage = cmpTool.ExtractAndCompareImages(output, cmp, targetDir, fuzzValue);
            String compareByContentResult = cmpTool.CompareByContent(output, cmp, targetDir);
            if (compareByContentResult != null) {
                errorMessage += compareByContentResult;
            }
            if (!errorMessage.Equals("")) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }
    }

//\cond DO_NOT_DOCUMENT
    /*
    * color matching text redaction
    */
    internal class CCharacterRenderInfo : CharacterRenderInfo {
        private Color strokeColor;

        private Color fillColor;

        public CCharacterRenderInfo(TextRenderInfo tri)
            : base(tri) {
            this.strokeColor = tri.GetStrokeColor();
            this.fillColor = tri.GetFillColor();
        }

        public virtual Color GetStrokeColor() {
            return strokeColor;
        }

        public virtual Color GetFillColor() {
            return fillColor;
        }
    }
//\endcond

//\cond DO_NOT_DOCUMENT
    internal class CustomLocationExtractionStrategy : RegexBasedLocationExtractionStrategy, ICleanupStrategy {
        private String regex;

        private IDictionary<Rectangle, Color> colorByRectangle = new Dictionary<Rectangle, Color>();

        public CustomLocationExtractionStrategy(String regex)
            : base(regex) {
            this.regex = regex;
        }

        protected override IList<CharacterRenderInfo> ToCRI(TextRenderInfo tri) {
            IList<CharacterRenderInfo> cris = new List<CharacterRenderInfo>();
            foreach (TextRenderInfo subTri in tri.GetCharacterRenderInfos()) {
                cris.Add(new CCharacterRenderInfo(subTri));
            }
            return cris;
        }

        protected override IList<Rectangle> ToRectangles(IList<CharacterRenderInfo> cris) {
            Color col = ((CCharacterRenderInfo)cris[0]).GetFillColor();
            IList<Rectangle> rects = new List<Rectangle>(base.ToRectangles(cris));
            foreach (Rectangle rect in rects) {
                colorByRectangle.Put(rect, col);
            }
            return rects;
        }

        public virtual Color GetRedactionColor(IPdfTextLocation rect) {
            return colorByRectangle.ContainsKey(rect.GetRectangle()) ? colorByRectangle.Get(rect.GetRectangle()) : ColorConstants
                .BLACK;
        }

        public virtual ICleanupStrategy Reset() {
            return new iText.PdfCleanup.CustomLocationExtractionStrategy(regex);
        }
    }
//\endcond
}
