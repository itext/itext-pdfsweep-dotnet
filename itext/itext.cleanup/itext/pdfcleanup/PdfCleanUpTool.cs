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
using System.Collections;
using System.Collections.Generic;
using iText.Commons.Actions;
using iText.Commons.Utils;
using iText.IO.Source;
using iText.Kernel.Colors;
using iText.Kernel.Exceptions;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout.Element;
using iText.Layout.Layout;
using iText.Layout.Properties;
using iText.PdfCleanup.Actions.Event;
using iText.PdfCleanup.Exceptions;

namespace iText.PdfCleanup {
    /// <summary>Represents the main mechanism for cleaning a PDF document.</summary>
    public class PdfCleanUpTool {
        /// <summary>
        /// When a document with line arts is being cleaned up, there are a lot of
        /// calculations with floating point numbers.
        /// </summary>
        /// <remarks>
        /// When a document with line arts is being cleaned up, there are a lot of
        /// calculations with floating point numbers. All of them are translated
        /// into fixed point numbers by multiplying by this coefficient. Vary it
        /// to adjust the preciseness of the calculations.
        /// </remarks>
        [System.ObsoleteAttribute]
        public static double floatMultiplier = Math
                //TODO DEVSIX-5770 make this constant a single non-static configuration
                .Pow(10, 14);

        /// <summary>
        /// Used as the criterion of a good approximation of rounded line joins
        /// and line caps.
        /// </summary>
        [System.ObsoleteAttribute]
        public static double arcTolerance = 
                //TODO DEVSIX-5770 make this constant a single non-static configuration
                0.0025;

        private PdfDocument pdfDocument;

        private CleanUpProperties properties;

        /// <summary>Key - page number, value - list of locations related to the page.</summary>
        private IDictionary<int, IList<iText.PdfCleanup.PdfCleanUpLocation>> pdfCleanUpLocations;

        /// <summary>
        /// Keys - redact annotations to be removed from the document after clean up,
        /// values - list of regions defined by redact annotation.
        /// </summary>
        private IDictionary<PdfRedactAnnotation, IList<Rectangle>> redactAnnotations;

        private FilteredImagesCache filteredImagesCache;

        /// <summary>
        /// Creates a
        /// <see cref="PdfCleanUpTool"/>
        /// object.
        /// </summary>
        /// <remarks>
        /// Creates a
        /// <see cref="PdfCleanUpTool"/>
        /// object. No regions for erasing are specified.
        /// Use
        /// <see cref="AddCleanupLocation(PdfCleanUpLocation)"/>
        /// method
        /// to set regions to be erased from the document.
        /// </remarks>
        /// <param name="pdfDocument">
        /// A
        /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
        /// object representing the document to which redaction applies.
        /// </param>
        public PdfCleanUpTool(PdfDocument pdfDocument)
            : this(pdfDocument, false, new CleanUpProperties()) {
        }

        /// <summary>
        /// Creates a
        /// <see cref="PdfCleanUpTool"/>
        /// object.
        /// </summary>
        /// <remarks>
        /// Creates a
        /// <see cref="PdfCleanUpTool"/>
        /// object. If
        /// <paramref name="cleanRedactAnnotations"/>
        /// is true,
        /// regions to be erased are extracted from the redact annotations contained inside the given document.
        /// Those redact annotations will be removed from the resultant document. If
        /// <paramref name="cleanRedactAnnotations"/>
        /// is false,
        /// then no regions for erasing are specified. In that case use
        /// <see cref="AddCleanupLocation(PdfCleanUpLocation)"/>
        /// method to set regions to be erased from the document.
        /// </remarks>
        /// <param name="pdfDocument">
        /// A
        /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
        /// object representing the document to which redaction applies.
        /// </param>
        /// <param name="cleanRedactAnnotations">if true - regions to be erased are extracted from the redact annotations contained
        ///     </param>
        /// <param name="properties">
        /// additional properties for clean-up process
        /// inside the given document.
        /// </param>
        public PdfCleanUpTool(PdfDocument pdfDocument, bool cleanRedactAnnotations, CleanUpProperties properties) {
            EventManager.GetInstance().OnEvent(PdfSweepProductEvent.CreateCleanupPdfEvent(pdfDocument.GetDocumentIdWrapper
                (), properties.GetMetaInfo()));
            if (pdfDocument.GetReader() == null || pdfDocument.GetWriter() == null) {
                throw new PdfException(CleanupExceptionMessageConstant.PDF_DOCUMENT_MUST_BE_OPENED_IN_STAMPING_MODE);
            }
            this.properties = properties;
            this.pdfDocument = pdfDocument;
            this.pdfCleanUpLocations = new Dictionary<int, IList<iText.PdfCleanup.PdfCleanUpLocation>>();
            this.filteredImagesCache = new FilteredImagesCache();
            if (cleanRedactAnnotations) {
                AddCleanUpLocationsBasedOnRedactAnnotations();
            }
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
        /// <see cref="PdfCleanUpLocation"/>
        /// </param>
        /// <param name="pdfDocument">
        /// a
        /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
        /// object representing the document to which redaction applies.
        /// </param>
        /// <param name="properties">additional properties for clean-up process</param>
        public PdfCleanUpTool(PdfDocument pdfDocument, IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations
            , CleanUpProperties properties)
            : this(pdfDocument, false, properties) {
            foreach (iText.PdfCleanup.PdfCleanUpLocation location in cleanUpLocations) {
                AddCleanupLocation(location);
            }
        }

        /// <summary>
        /// Adds a
        /// <see cref="PdfCleanUpLocation"/>
        /// to be cleaned up.
        /// </summary>
        /// <param name="cleanUpLocation">
        /// a
        /// <see cref="PdfCleanUpLocation"/>
        /// to be cleaned up
        /// </param>
        /// <returns>
        /// this
        /// <see cref="PdfCleanUpTool"/>
        /// </returns>
        public virtual iText.PdfCleanup.PdfCleanUpTool AddCleanupLocation(iText.PdfCleanup.PdfCleanUpLocation cleanUpLocation
            ) {
            IList<iText.PdfCleanup.PdfCleanUpLocation> pgLocations = this.pdfCleanUpLocations.Get(cleanUpLocation.GetPage
                ());
            if (pgLocations == null) {
                pgLocations = new List<iText.PdfCleanup.PdfCleanUpLocation>();
                this.pdfCleanUpLocations.Put(cleanUpLocation.GetPage(), pgLocations);
            }
            pgLocations.Add(cleanUpLocation);
            return this;
        }

        /// <summary>Cleans the document by erasing all the areas which are provided or extracted from redaction annotations.
        ///     </summary>
        public virtual void CleanUp() {
            foreach (KeyValuePair<int, IList<iText.PdfCleanup.PdfCleanUpLocation>> entry in pdfCleanUpLocations) {
                CleanUpPage(entry.Key, entry.Value);
            }
            if (redactAnnotations != null) {
                // if it isn't null, then we are in "extract locations from redact annots" mode
                RemoveRedactAnnots();
            }
            pdfCleanUpLocations.Clear();
        }

        /// <summary>
        /// Cleans a page from the document by erasing all the areas which
        /// are provided or extracted from redaction annotations.
        /// </summary>
        /// <param name="pageNumber">the page to be cleaned up</param>
        /// <param name="cleanUpLocations">the locations to be cleaned up</param>
        private void CleanUpPage(int pageNumber, IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations) {
            if (cleanUpLocations.Count == 0) {
                return;
            }
            IList<Rectangle> regions = new List<Rectangle>();
            foreach (iText.PdfCleanup.PdfCleanUpLocation cleanUpLocation in cleanUpLocations) {
                regions.Add(cleanUpLocation.GetRegion());
            }
            PdfPage page = pdfDocument.GetPage(pageNumber);
            PdfCleanUpProcessor cleanUpProcessor = new PdfCleanUpProcessor(regions, pdfDocument, this.properties);
            cleanUpProcessor.SetFilteredImagesCache(filteredImagesCache);
            cleanUpProcessor.ProcessPageContent(page);
            if (properties.IsProcessAnnotations()) {
                cleanUpProcessor.ProcessPageAnnotations(page, regions, redactAnnotations != null);
            }
            PdfCanvas pageCleanedContents = cleanUpProcessor.PopCleanedCanvas();
            page.Put(PdfName.Contents, pageCleanedContents.GetContentStream());
            page.SetResources(pageCleanedContents.GetResources());
            ColorCleanedLocations(pageCleanedContents, cleanUpLocations);
        }

        /// <summary>Draws colored rectangles on the PdfCanvas corresponding to the PdfCleanUpLocation objects.</summary>
        /// <param name="canvas">the PdfCanvas on which to draw</param>
        /// <param name="cleanUpLocations">the PdfCleanUpLocations</param>
        private void ColorCleanedLocations(PdfCanvas canvas, IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations
            ) {
            foreach (iText.PdfCleanup.PdfCleanUpLocation location in cleanUpLocations) {
                if (location.GetCleanUpColor() != null) {
                    AddColoredRectangle(canvas, location);
                }
            }
        }

        /// <summary>Draws a colored rectangle on the PdfCanvas correponding to a PdfCleanUpLocation.</summary>
        /// <param name="canvas">the PdfCanvas on which to draw</param>
        /// <param name="location">the PdfCleanUpLocation</param>
        private void AddColoredRectangle(PdfCanvas canvas, iText.PdfCleanup.PdfCleanUpLocation location) {
            if (pdfDocument.IsTagged()) {
                canvas.OpenTag(new CanvasArtifact());
            }
            // To avoid the float calculation precision differences in Java and .Net,
            // the values of rectangles to be drawn are rounded
            float x = (float)(Math.Floor(location.GetRegion().GetX() * 2.0) / 2.0);
            float y = (float)(Math.Floor(location.GetRegion().GetY() * 2.0) / 2.0);
            float width = (float)(Math.Floor(location.GetRegion().GetWidth() * 2.0) / 2.0);
            float height = (float)(Math.Floor(location.GetRegion().GetHeight() * 2.0) / 2.0);
            Rectangle rect = new Rectangle(x, y, width, height);
            canvas.SaveState().SetFillColor(location.GetCleanUpColor()).Rectangle(rect).Fill().RestoreState();
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
            redactAnnotations.Put(redactAnnotation, regions);
            int page = pdfDocument.GetPageNumber(redactAnnotation.GetPage());
            Color cleanUpColor = redactAnnotation.GetInteriorColor();
            PdfDictionary ro = redactAnnotation.GetRedactRolloverAppearance();
            if (ro != null) {
                cleanUpColor = null;
            }
            foreach (Rectangle region in regions) {
                AddCleanupLocation(new iText.PdfCleanup.PdfCleanUpLocation(page, region, cleanUpColor));
            }
        }

        /// <summary>Convert a PdfArray of floats into a List of Rectangle objects.</summary>
        /// <param name="quadPoints">input PdfArray</param>
        private IList<Rectangle> TranslateQuadPointsToRectangles(PdfArray quadPoints) {
            IList<Rectangle> rectangles = new List<Rectangle>();
            for (int i = 0; i < quadPoints.Size(); i += 8) {
                float x = quadPoints.GetAsNumber(i + 4).FloatValue();
                float y = quadPoints.GetAsNumber(i + 5).FloatValue();
                float width = quadPoints.GetAsNumber(i + 2).FloatValue() - x;
                float height = quadPoints.GetAsNumber(i + 3).FloatValue() - y;
                rectangles.Add(new Rectangle(x, 
                                // QuadPoints in redact annotations have "Z" order
                                y, width, height));
            }
            return rectangles;
        }

        /// <summary>Remove the redaction annotations.</summary>
        /// <remarks>
        /// Remove the redaction annotations.
        /// This method is called after the annotations are processed.
        /// </remarks>
        private void RemoveRedactAnnots() {
            foreach (PdfRedactAnnotation annotation in redactAnnotations.Keys) {
                PdfPage page = annotation.GetPage();
                if (page != null) {
                    page.RemoveAnnotation(annotation);
                    PdfPopupAnnotation popup = annotation.GetPopup();
                    if (popup != null) {
                        page.RemoveAnnotation(popup);
                    }
                }
                PdfCanvas canvas = new PdfCanvas(page);
                PdfStream redactRolloverAppearance = annotation.GetRedactRolloverAppearance();
                PdfString overlayText = annotation.GetOverlayText();
                Rectangle annotRect = annotation.GetRectangle().ToRectangle();
                if (redactRolloverAppearance != null) {
                    DrawRolloverAppearance(canvas, redactRolloverAppearance, annotRect, redactAnnotations.Get(annotation));
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
            canvas.Clip().EndPath();
            PdfFormXObject formXObject = new PdfFormXObject(redactRolloverAppearance);
            canvas.AddXObjectWithTransformationMatrix(formXObject, 1, 0, 0, 1, annotRect.GetLeft(), annotRect.GetBottom
                ());
            canvas.RestoreState();
            if (pdfDocument.IsTagged()) {
                canvas.CloseTag();
            }
        }

        private void DrawOverlayText(PdfCanvas canvas, String overlayText, Rectangle annotRect, PdfBoolean repeat, 
            PdfString defaultAppearance, int justification) {
            IDictionary<String, IList> parsedDA;
            try {
                parsedDA = ParseDAParam(defaultAppearance);
            }
            catch (NullReferenceException) {
                throw new PdfException(CleanupExceptionMessageConstant.DEFAULT_APPEARANCE_NOT_FOUND);
            }
            PdfFont font;
            float fontSize = 12;
            IList fontArgs = parsedDA.Get("Tf");
            PdfDictionary formDictionary = pdfDocument.GetCatalog().GetPdfObject().GetAsDictionary(PdfName.AcroForm);
            if (fontArgs != null && formDictionary != null) {
                font = GetFontFromAcroForm((PdfName)fontArgs[0]);
                fontSize = ((PdfNumber)fontArgs[1]).FloatValue();
            }
            else {
                font = PdfFontFactory.CreateFont();
            }
            if (pdfDocument.IsTagged()) {
                canvas.OpenTag(new CanvasArtifact());
            }
            iText.Layout.Canvas modelCanvas = new iText.Layout.Canvas(canvas, annotRect, false);
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
            IList strokeColorArgs = parsedDA.Get("StrokeColor");
            if (strokeColorArgs != null) {
                p.SetStrokeColor(GetColor(strokeColorArgs));
            }
            IList fillColorArgs = parsedDA.Get("FillColor");
            if (fillColorArgs != null) {
                p.SetFontColor(GetColor(fillColorArgs));
            }
            modelCanvas.Add(p);
            if (repeat != null && repeat.GetValue()) {
                bool hasFull = modelCanvas.GetRenderer().HasProperty(Property.FULL);
                bool isFull = hasFull ? (bool)modelCanvas.GetRenderer().GetPropertyAsBoolean(Property.FULL) : false;
                while (!isFull) {
                    p.Add(overlayText);
                    LayoutArea previousArea = modelCanvas.GetRenderer().GetCurrentArea().Clone();
                    modelCanvas.Relayout();
                    if (modelCanvas.GetRenderer().GetCurrentArea().Equals(previousArea)) {
                        // Avoid infinite loop. This might be caused by the fact that the font does not support the text we want to show
                        break;
                    }
                    hasFull = modelCanvas.GetRenderer().HasProperty(Property.FULL);
                    isFull = hasFull ? (bool)modelCanvas.GetRenderer().GetPropertyAsBoolean(Property.FULL) : false;
                }
            }
            modelCanvas.GetRenderer().Flush();
            if (pdfDocument.IsTagged()) {
                canvas.CloseTag();
            }
        }

        private IDictionary<String, IList> ParseDAParam(PdfString DA) {
            IDictionary<String, IList> commandArguments = new Dictionary<String, IList>();
            PdfTokenizer tokeniser = new PdfTokenizer(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource
                (DA.ToUnicodeString().GetBytes(System.Text.Encoding.UTF8))));
            IList currentArguments = new ArrayList();
            while (tokeniser.NextToken()) {
                if (tokeniser.GetTokenType() == PdfTokenizer.TokenType.Other) {
                    String key = tokeniser.GetStringValue();
                    if ("RG".Equals(key) || "G".Equals(key) || "K".Equals(key)) {
                        key = "StrokeColor";
                    }
                    else {
                        if ("rg".Equals(key) || "g".Equals(key) || "k".Equals(key)) {
                            key = "FillColor";
                        }
                    }
                    commandArguments.Put(key, currentArguments);
                    currentArguments = new ArrayList();
                }
                else {
                    switch (tokeniser.GetTokenType()) {
                        case PdfTokenizer.TokenType.Number: {
                            currentArguments.Add(new PdfNumber(System.Convert.ToSingle(tokeniser.GetStringValue(), System.Globalization.CultureInfo.InvariantCulture
                                )));
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
