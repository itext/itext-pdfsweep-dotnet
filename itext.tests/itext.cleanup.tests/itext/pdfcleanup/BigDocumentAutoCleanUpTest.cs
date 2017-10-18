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
/*
* To change this license header, choose License Headers in Project Properties.
* To change this template file, choose Tools | Templates
* and open the template in the editor.
*/
using System;
using System.Collections.Generic;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Utils;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Versions.Attributes;
using iText.Kernel;
using iText.PdfCleanup.Autosweep;
using iText.Test;

namespace iText.PdfCleanup {
    /// <author>Joris Schellekens</author>
    public class BigDocumentAutoCleanUpTest {

        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/BigDocumentAutoCleanUpTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/BigDocumentAutoCleanUpTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            ITextTest.CreateOrClearDestinationFolder(outputPath);
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void RedactLipsum() {
            String input = inputPath + "Lipsum.pdf";
            String output = outputPath + "redactLipsum.pdf";
            String cmp = inputPath + "cmp_redactLipsum.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(D|d)olor").SetRedactionColor(ColorConstants.GREEN));
            PdfWriter writer = new PdfWriter(output);
            writer.SetCompressionLevel(0);
            PdfDocument pdf = new PdfDocument(new PdfReader(input), writer);
            // sweep
            PdfAutoSweep autoSweep = new PdfAutoSweep(strategy);
            autoSweep.CleanUp(pdf);
            pdf.Close();
            // compare
            CompareByContent(cmp, output, outputPath, "diff_redactLipsum_");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
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
            PdfAutoSweep autoSweep = new PdfAutoSweep(strategy);
            autoSweep.CleanUp(pdf);
            pdf.Close();
            // compare
            CompareByContent(cmp, output, outputPath, "diff_redactTonySoprano_");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void RedactIPhoneUserManualMatchColor() {
            String input = inputPath + "iphone_user_guide_untagged.pdf";
            String output = outputPath + "redactIPhoneUserManualMatchColor.pdf";
            String cmp = inputPath + "cmp_redactIPhoneUserManualMatchColor.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new CustomLocationExtractionStrategy("(iphone)|(iPhone)"));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            // sweep
            PdfAutoSweep autoSweep = new PdfAutoSweep(strategy);
            autoSweep.CleanUp(pdf);
            pdf.Close();
            // compare
            CompareByContent(cmp, output, outputPath, "diff_redactIPhoneUserManualMatchColor_");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void RedactIPhoneUserManual() {
            String input = inputPath + "iphone_user_guide_untagged.pdf";
            String output = outputPath + "redactIPhoneUserManual.pdf";
            String cmp = inputPath + "cmp_redactIPhoneUserManual.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("(iphone)|(iPhone)"));
            PdfDocument pdf = new PdfDocument(new PdfReader(input), new PdfWriter(output));
            // sweep
            PdfAutoSweep autoSweep = new PdfAutoSweep(strategy);
            autoSweep.CleanUp(pdf);
            pdf.Close();
            // compare
            CompareByContent(cmp, output, outputPath, "diff_redactIPhoneUserManual_");
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        private void CompareByContent(String cmp, String output, String targetDir, String diffPrefix) {
            CompareTool cmpTool = new CompareTool();
            String errorMessage = cmpTool.CompareVisually(output, cmp, targetDir, diffPrefix + "_");
            if (errorMessage != null) {
                NUnit.Framework.Assert.Fail(errorMessage);
            }
        }
    }

    internal class CCharacterRenderInfo : CharacterRenderInfo {
        private Color strokeColor;

        private Color fillColor;

        public CCharacterRenderInfo(TextRenderInfo tri)
            : base(tri) {
            /*
            * color matching text redaction
            */
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
}
