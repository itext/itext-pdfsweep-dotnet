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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using iText.IO.Source;
using iText.IO.Util;
using iText.Kernel;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout.Element;
using iText.Layout.Layout;
using iText.Layout.Properties;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Versions.Attributes;
using iText.Kernel;

namespace iText.PdfCleanup {
    /// <summary>Represents the main mechanism for cleaning a PDF document.</summary>
    public class PdfCleanUpTool {
        /// <summary>
        /// When a document with line arts is being cleaned up, there are lot of
        /// calculations with floating point numbers.
        /// </summary>
        /// <remarks>
        /// When a document with line arts is being cleaned up, there are lot of
        /// calculations with floating point numbers. All of them are translated
        /// into fixed point numbers by multiplying by this coefficient. Vary it
        /// to adjust the preciseness of the calculations.
        /// </remarks>
        public static double floatMultiplier = Math.Pow(10, 14);

        /// <summary>
        /// Used as the criterion of a good approximation of rounded line joins
        /// and line caps.
        /// </summary>
        public static double arcTolerance = 0.0025;

        private PdfDocument pdfDocument;

        /// <summary>Key - page number, value - list of locations related to the page.</summary>
        private IDictionary<int, IList<PdfCleanUpLocation>> pdfCleanUpLocations;

        /// <summary>
        /// Keys - redact annotations to be removed from the document after clean up,
        /// Values - list of regions defined by redact annotation
        /// </summary>
        private IDictionary<PdfRedactAnnotation, IList<Rectangle>> redactAnnotations;

        private const String PRODUCT_NAME = "pdfSweep";

        private const int PRODUCT_MAJOR = 1;

        private const int PRODUCT_MINOR = 0;

        /// <summary>
        /// Creates a
        /// <see cref="PdfCleanUpTool"/>
        /// object. No regions for erasing are specified.
        /// Use
        /// <see cref="AddCleanupLocation(PdfCleanUpLocation)"/>
        /// method
        /// to set regions to be erased from the document.
        /// </summary>
        /// <param name="pdfDocument">
        /// A
        /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
        /// object representing the document
        /// to which redaction applies.
        /// </param>
        public PdfCleanUpTool(PdfDocument pdfDocument)
            : this(pdfDocument, false) {
        }

        /// <summary>
        /// Creates a
        /// <see cref="PdfCleanUpTool"/>
        /// object. If
        /// 
        /// is true,
        /// regions to be erased are extracted from the redact annotations contained inside the given document.
        /// Those redact annotations will be removed from the resultant document. If
        /// 
        /// is false,
        /// then no regions for erasing are specified. In that case use
        /// <see cref="AddCleanupLocation(PdfCleanUpLocation)"/>
        /// method to set regions to be erased from the document.
        /// </summary>
        /// <param name="pdfDocument">
        /// A
        /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
        /// object representing the document
        /// to which redaction applies.
        /// </param>
        /// <param name="cleanRedactAnnotations">
        /// if true - regions to be erased are extracted from the redact annotations contained
        /// inside the given document.
        /// </param>
        public PdfCleanUpTool(PdfDocument pdfDocument, bool cleanRedactAnnotations) {

            try 
            {
                String licenseKeyClassName = "iText.License.LicenseKey, itext.licensekey";
                String licenseKeyProductClassName = "iText.License.LicenseKeyProduct, itext.licensekey";
                String licenseKeyFeatureClassName = "iText.License.LicenseKeyProductFeature, itext.licensekey";
                String checkLicenseKeyMethodName = "ScheduledCheck";
                Type licenseKeyClass = GetClass(licenseKeyClassName);
                if ( licenseKeyClass != null ) 
                {                
                    Type licenseKeyProductClass = GetClass(licenseKeyProductClassName);
                    Type licenseKeyProductFeatureClass = GetClass(licenseKeyFeatureClassName);
                    Array array = Array.CreateInstance(licenseKeyProductFeatureClass, 0);
                    object[] objects = new object[] { "pdfSweep", 1, 0, array };
                    Object productObject = System.Activator.CreateInstance(licenseKeyProductClass, objects);
                    MethodInfo m = licenseKeyClass.GetMethod(checkLicenseKeyMethodName);
                    m.Invoke(System.Activator.CreateInstance(licenseKeyClass), new object[] {productObject});
                }   
            } 
            catch ( Exception e ) 
            {
                if ( !Kernel.Version.IsAGPLVersion() )
                {
                    throw;
                }
            }
            if (pdfDocument.GetReader() == null || pdfDocument.GetWriter() == null) {
                throw new PdfException(PdfException.PdfDocumentMustBeOpenedInStampingMode);
            }
            this.pdfDocument = pdfDocument;
            this.pdfCleanUpLocations = new Dictionary<int, IList<PdfCleanUpLocation>>();
            if (cleanRedactAnnotations) {
                AddCleanUpLocationsBasedOnRedactAnnotations();
            }
        }

        private static Type GetClass(string className)
        {
            String licenseKeyClassFullName = null;
            Assembly assembly = typeof(PdfCleanUpTool).Assembly;
            Attribute keyVersionAttr = assembly.GetCustomAttribute(typeof(KeyVersionAttribute));
            if (keyVersionAttr is KeyVersionAttribute)
            {
                String keyVersion = ((KeyVersionAttribute)keyVersionAttr).KeyVersion;
                String format = "{0}, Version={1}, Culture=neutral, PublicKeyToken=8354ae6d2174ddca";
                licenseKeyClassFullName = String.Format(format, className, keyVersion);
            }
            Type type = null;
            if (licenseKeyClassFullName != null)
            {
                String fileLoadExceptionMessage = null;
                try
                {
                    type = System.Type.GetType(licenseKeyClassFullName);
                }
                catch (FileLoadException fileLoadException)
                {
                    fileLoadExceptionMessage = fileLoadException.Message;
                }
                if (fileLoadExceptionMessage != null)
                {
                    try
                    {
                        type = System.Type.GetType(className);
                    }
                    catch
                    {
                        // empty
                    }
                }
            }
            return type;
        }

        /// <summary>
        /// Creates a
        /// <see cref="PdfCleanUpTool"/>
        /// object based on the given
        /// <see cref="System.Collections.IList{E}"/>
        /// of
        /// <see cref="PdfCleanUpLocation"/>
        /// s representing regions to be erased from the document.
        /// </summary>
        /// <param name="cleanUpLocations">
        /// list of locations to be cleaned up
        /// <seealso>PdfCleanUpLocation</seealso>
        /// </param>
        /// <param name="pdfDocument">
        /// A
        /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
        /// object representing the document
        /// to which redaction applies.
        /// </param>
        public PdfCleanUpTool(PdfDocument pdfDocument, IList<PdfCleanUpLocation> cleanUpLocations)
            : this(pdfDocument) {
            foreach (PdfCleanUpLocation location in cleanUpLocations) {
                AddCleanupLocation(location);
            }
        }

        public virtual iText.PdfCleanup.PdfCleanUpTool AddCleanupLocation(PdfCleanUpLocation cleanUpLocation) {
            IList<PdfCleanUpLocation> pgLocations; 
            this.pdfCleanUpLocations.TryGetValue(cleanUpLocation.GetPage(), out pgLocations);
            if (pgLocations == null) {
                pgLocations = new List<PdfCleanUpLocation>();
                this.pdfCleanUpLocations[cleanUpLocation.GetPage()] = pgLocations;
            }
            pgLocations.Add(cleanUpLocation);
            return this;
        }

        /// <summary>
        /// Cleans the document by erasing all the areas which are either provided or
        /// extracted from redaction annotations.
        /// </summary>
        /// <exception cref="System.IO.IOException"/>
        public virtual void CleanUp() {
            foreach (KeyValuePair<int, IList<PdfCleanUpLocation>> entry in pdfCleanUpLocations) {
                CleanUpPage(entry.Key, entry.Value);
            }
            if (redactAnnotations != null) {
                // if it isn't null, then we are in "extract locations from redact annots" mode
                RemoveRedactAnnots();
            }
        }

        private void CleanUpPage(int pageNumber, IList<PdfCleanUpLocation> cleanUpLocations) {
            if (cleanUpLocations.Count == 0) {
                return;
            }
            IList<Rectangle> regions = new List<Rectangle>();
            foreach (PdfCleanUpLocation cleanUpLocation in cleanUpLocations) {
                regions.Add(cleanUpLocation.GetRegion());
            }
            PdfPage page = pdfDocument.GetPage(pageNumber);
            PdfCleanUpProcessor cleanUpProcessor = new PdfCleanUpProcessor(regions, pdfDocument);
            cleanUpProcessor.ProcessPageContent(page);
            PdfCanvas pageCleanedContents = cleanUpProcessor.PopCleanedCanvas();
            page.Put(PdfName.Contents, pageCleanedContents.GetContentStream());
            page.SetResources(pageCleanedContents.GetResources());
            ColorCleanedLocations(pageCleanedContents, cleanUpLocations);
        }

        private void ColorCleanedLocations(PdfCanvas canvas, IList<PdfCleanUpLocation> cleanUpLocations) {
            foreach (PdfCleanUpLocation location in cleanUpLocations) {
                if (location.GetCleanUpColor() != null) {
                    AddColoredRectangle(canvas, location);
                }
            }
        }

        private void AddColoredRectangle(PdfCanvas canvas, PdfCleanUpLocation location) {
            if (pdfDocument.IsTagged()) {
                canvas.OpenTag(new CanvasArtifact());
            }
            canvas.SaveState().SetFillColor(location.GetCleanUpColor()).Rectangle(location.GetRegion()).Fill().RestoreState
                ();
            if (pdfDocument.IsTagged()) {
                canvas.CloseTag();
            }
        }

        /// <summary>
        /// Adds clean up locations to be erased by extracting regions from the redact annotations
        /// contained inside the given document.
        /// </summary>
        /// <remarks>
        /// Adds clean up locations to be erased by extracting regions from the redact annotations
        /// contained inside the given document. Those redact annotations will be removed from the resultant document.
        /// </remarks>
        /// <returns>
        /// current
        /// <see cref="PdfCleanUpTool"/>
        /// instance.
        /// </returns>
        private void AddCleanUpLocationsBasedOnRedactAnnotations() {
            redactAnnotations = new LinkedDictionary<PdfRedactAnnotation, IList<Rectangle>>();
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); ++i) {
                ExtractLocationsFromRedactAnnotations(pdfDocument.GetPage(i));
            }
        }

        private void ExtractLocationsFromRedactAnnotations(PdfPage page) {
            IList<PdfAnnotation> annotations = page.GetAnnotations();
            foreach (PdfAnnotation annotation in annotations) {
                if (PdfName.Redact.Equals(annotation.GetSubtype())) {
                    ExtractLocationsFromSingleRedactAnnotation((PdfRedactAnnotation)annotation);
                }
            }
        }

        /// <summary>
        /// Note: annotation can consist not only of one area specified by the RECT entry, but also of multiple areas specified
        /// by the QuadPoints entry in the annotation dictionary.
        /// </summary>
        private void ExtractLocationsFromSingleRedactAnnotation(PdfRedactAnnotation redactAnnotation) {
            IList<Rectangle> regions;
            PdfArray quadPoints = redactAnnotation.GetQuadPoints();
            if (quadPoints != null && !quadPoints.IsEmpty()) {
                regions = TranslateQuadPointsToRectangles(quadPoints);
            }
            else {
                regions = new List<Rectangle>();
                regions.Add(redactAnnotation.GetRectangle().ToRectangle());
            }
            redactAnnotations[redactAnnotation] = regions;
            int page = pdfDocument.GetPageNumber(redactAnnotation.GetPage());
            Color cleanUpColor = redactAnnotation.GetInteriorColor();
            PdfDictionary ro = redactAnnotation.GetRedactRolloverAppearance();
            if (ro != null) {
                cleanUpColor = null;
            }
            foreach (Rectangle region in regions) {
                AddCleanupLocation(new PdfCleanUpLocation(page, region, cleanUpColor));
            }
        }

        private IList<Rectangle> TranslateQuadPointsToRectangles(PdfArray quadPoints) {
            IList<Rectangle> rectangles = new List<Rectangle>();
            for (int i = 0; i < quadPoints.Size(); i += 8) {
                float x = quadPoints.GetAsNumber(i + 4).FloatValue();
                float y = quadPoints.GetAsNumber(i + 5).FloatValue();
                float width = quadPoints.GetAsNumber(i + 2).FloatValue() - x;
                float height = quadPoints.GetAsNumber(i + 3).FloatValue() - y;
                rectangles.Add(new Rectangle(x, y, width, height));
            }
            // QuadPoints have "Z" order
            return rectangles;
        }

        /// <exception cref="System.IO.IOException"/>
        private void RemoveRedactAnnots() {
            foreach (PdfRedactAnnotation annotation in redactAnnotations.Keys) {
                PdfPage page = annotation.GetPage();
                page.RemoveAnnotation(annotation);
                PdfPopupAnnotation popup = annotation.GetPopup();
                if (popup != null) {
                    page.RemoveAnnotation(popup);
                }
                PdfCanvas canvas = new PdfCanvas(page);
                PdfStream redactRolloverAppearance = annotation.GetRedactRolloverAppearance();
                PdfString overlayText = annotation.GetOverlayText();
                Rectangle annotRect = annotation.GetRectangle().ToRectangle();
                if (redactRolloverAppearance != null) {
                    DrawRolloverAppearance(canvas, redactRolloverAppearance, annotRect, redactAnnotations[annotation]);
                }
                else {
                    if (overlayText != null && !String.IsNullOrEmpty(overlayText.ToUnicodeString())) {
                        DrawOverlayText(canvas, overlayText.ToUnicodeString(), annotRect, annotation.GetRepeat(), annotation.GetDefaultAppearance
                            (), annotation.GetJustification());
                    }
                }
            }
        }

        private void DrawRolloverAppearance(PdfCanvas canvas, PdfStream redactRolloverAppearance, Rectangle annotRect
            , IList<Rectangle> cleanedRegions) {
            if (pdfDocument.IsTagged()) {
                canvas.OpenTag(new CanvasArtifact());
            }
            canvas.SaveState();
            foreach (Rectangle rect in cleanedRegions) {
                canvas.Rectangle(rect.GetLeft(), rect.GetBottom(), rect.GetWidth(), rect.GetHeight());
            }
            canvas.Clip().NewPath();
            PdfFormXObject formXObject = new PdfFormXObject(redactRolloverAppearance);
            canvas.AddXObject(formXObject, 1, 0, 0, 1, annotRect.GetLeft(), annotRect.GetBottom());
            canvas.RestoreState();
            if (pdfDocument.IsTagged()) {
                canvas.CloseTag();
            }
        }

        /// <exception cref="System.IO.IOException"/>
        private void DrawOverlayText(PdfCanvas canvas, String overlayText, Rectangle annotRect, PdfBoolean repeat, 
            PdfString defaultAppearance, int justification) {
            IDictionary<String, IList> parsedDA = ParseDAParam(defaultAppearance);
            PdfFont font;
            float fontSize = 12;
            IList fontArgs;
            parsedDA.TryGetValue("Tf", out fontArgs);
            if (fontArgs != null) {
                font = GetFontFromAcroForm((PdfName)fontArgs[0]);
                fontSize = ((PdfNumber)fontArgs[1]).FloatValue();
            }
            else {
                font = PdfFontFactory.CreateFont();
            }
            if (pdfDocument.IsTagged()) {
                canvas.OpenTag(new CanvasArtifact());
            }
            iText.Layout.Canvas modelCanvas = new iText.Layout.Canvas(canvas, pdfDocument, annotRect, false);
            Paragraph p = new Paragraph(overlayText).SetFont(font).SetFontSize(fontSize).SetMargin(0);
            TextAlignment? textAlignment = TextAlignment.LEFT;
            switch (justification) {
                case 1: {
                    textAlignment = TextAlignment.CENTER;
                    break;
                }

                case 2: {
                    textAlignment = TextAlignment.RIGHT;
                    break;
                }

                default: {
                    break;
                }
            }
            p.SetTextAlignment(textAlignment);
            IList strokeColorArgs;
            parsedDA.TryGetValue("StrokeColor", out strokeColorArgs);
            if (strokeColorArgs != null) {
                p.SetStrokeColor(GetColor(strokeColorArgs));
            }
            IList fillColorArgs;
            parsedDA.TryGetValue("FillColor", out fillColorArgs);
            if (fillColorArgs != null) {
                p.SetFontColor(GetColor(fillColorArgs));
            }
            modelCanvas.Add(p);
            if (repeat != null && repeat.GetValue()) {
                bool? isFull = modelCanvas.GetRenderer().GetPropertyAsBoolean(Property.FULL);
                while (isFull == null || (bool)!isFull) {
                    p.Add(overlayText);
                    LayoutArea previousArea = modelCanvas.GetRenderer().GetCurrentArea().Clone();
                    modelCanvas.Relayout();
                    if (modelCanvas.GetRenderer().GetCurrentArea().Equals(previousArea)) {
                        // Avoid infinite loop. This might be caused by the fact that the font does not support the text we want to show
                        break;
                    }
                    isFull = modelCanvas.GetRenderer().GetPropertyAsBoolean(Property.FULL);
                }
            }
            modelCanvas.GetRenderer().Flush();
            if (pdfDocument.IsTagged()) {
                canvas.CloseTag();
            }
        }

        /// <exception cref="System.IO.IOException"/>
        private IDictionary<String, IList> ParseDAParam(PdfString DA) {
            IDictionary<String, IList> commandArguments = new Dictionary<String, IList>();
            PdfTokenizer tokeniser = new PdfTokenizer(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource
                (DA.ToUnicodeString().GetBytes())));
            IList currentArguments = new ArrayList();
            while (tokeniser.NextToken()) {
                if (tokeniser.GetTokenType() == PdfTokenizer.TokenType.Other) {
                    String key = tokeniser.GetStringValue();
                    if (key.Equals("RG") || key.Equals("G") || key.Equals("K")) {
                        key = "StrokeColor";
                    }
                    else {
                        if (key.Equals("rg") || key.Equals("g") || key.Equals("k")) {
                            key = "FillColor";
                        }
                    }
                    commandArguments[key] = currentArguments;
                    currentArguments = new ArrayList();
                }
                else {
                    switch (tokeniser.GetTokenType()) {
                        case PdfTokenizer.TokenType.Number: {
                            currentArguments.Add(new PdfNumber(Convert.ToSingle(tokeniser.GetStringValue(), CultureInfo.InvariantCulture)));
                            break;
                        }

                        case PdfTokenizer.TokenType.Name: {
                            currentArguments.Add(new PdfName(tokeniser.GetStringValue()));
                            break;
                        }

                        default: {
                            currentArguments.Add(tokeniser.GetStringValue());
                            break;
                        }
                    }
                }
            }
            return commandArguments;
        }

        private PdfFont GetFontFromAcroForm(PdfName fontName) {
            PdfDictionary formDictionary = pdfDocument.GetCatalog().GetPdfObject().GetAsDictionary(PdfName.AcroForm);
            PdfDictionary resources = formDictionary.GetAsDictionary(PdfName.DR);
            PdfDictionary fonts = resources.GetAsDictionary(PdfName.Font);
            return PdfFontFactory.CreateFont(fonts.GetAsDictionary(fontName));
        }

        private Color GetColor(IList colorArgs) {
            Color color = null;
            switch (colorArgs.Count) {
                case 1: {
                    color = new DeviceGray(((PdfNumber)colorArgs[0]).FloatValue());
                    break;
                }

                case 3: {
                    color = new DeviceRgb(((PdfNumber)colorArgs[0]).FloatValue(), ((PdfNumber)colorArgs[1]).FloatValue(), ((PdfNumber
                        )colorArgs[2]).FloatValue());
                    break;
                }

                case 4: {
                    color = new DeviceCmyk(((PdfNumber)colorArgs[0]).FloatValue(), ((PdfNumber)colorArgs[1]).FloatValue(), ((PdfNumber
                        )colorArgs[2]).FloatValue(), ((PdfNumber)colorArgs[3]).FloatValue());
                    break;
                }
            }
            return color;
        }
    }
}
