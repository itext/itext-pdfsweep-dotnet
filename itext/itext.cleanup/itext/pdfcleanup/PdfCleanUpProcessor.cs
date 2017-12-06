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
address: sales@itextpdf.com
*/
using System;
using System.Collections.Generic;
using iText.IO.Image;
using iText.IO.Source;
using iText.IO.Util;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Colorspace;
using iText.Kernel.Pdf.Tagutils;
using iText.Kernel.Pdf.Xobject;

namespace iText.PdfCleanup {
    public class PdfCleanUpProcessor : PdfCanvasProcessor {
        private static readonly ICollection<String> textShowingOperators = new HashSet<String>(iText.IO.Util.JavaUtil.ArraysAsList
            ("TJ", "Tj", "'", "\""));

        private static readonly ICollection<String> pathConstructionOperators = new HashSet<String>(iText.IO.Util.JavaUtil.ArraysAsList
            ("m", "l", "c", "v", "y", "h", "re"));

        private static readonly ICollection<String> strokeOperators = new HashSet<String>(iText.IO.Util.JavaUtil.ArraysAsList
            ("S", "s", "B", "B*", "b", "b*"));

        private static readonly ICollection<String> nwFillOperators = new HashSet<String>(iText.IO.Util.JavaUtil.ArraysAsList
            ("f", "F", "B", "b"));

        private static readonly ICollection<String> eoFillOperators = new HashSet<String>(iText.IO.Util.JavaUtil.ArraysAsList
            ("f*", "B*", "b*"));

        private static readonly ICollection<String> pathPaintingOperators = new HashSet<String>();

        private static readonly ICollection<String> clippingPathOperators = new HashSet<String>(iText.IO.Util.JavaUtil.ArraysAsList
            ("W", "W*"));

        private static readonly ICollection<String> lineStyleOperators = new HashSet<String>(iText.IO.Util.JavaUtil.ArraysAsList
            ("w", "J", "j", "M", "d"));

        private static readonly ICollection<String> strokeColorOperators = new HashSet<String>(iText.IO.Util.JavaUtil.ArraysAsList
            ("CS", "SC", "SCN", "G", "RG", "K"));

        private static readonly ICollection<String> fillColorOperators = new HashSet<String>(iText.IO.Util.JavaUtil.ArraysAsList
            ("cs", "sc", "scn", "g", "rg", "k"));

        private static readonly ICollection<String> textPositioningOperators = new HashSet<String>(iText.IO.Util.JavaUtil.ArraysAsList
            ("Td", "TD", "Tm", "T*", "TL"));

        private static readonly ICollection<String> ignoredOperators = new HashSet<String>();

        static PdfCleanUpProcessor() {
            // TL actually is not a text positioning operator, but we need to process it with them
            // these operators are processed via PdfCanvasProcessor graphics state and event listener
            pathPaintingOperators.AddAll(strokeOperators);
            pathPaintingOperators.AddAll(nwFillOperators);
            pathPaintingOperators.AddAll(eoFillOperators);
            pathPaintingOperators.Add("n");
            ignoredOperators.AddAll(pathConstructionOperators);
            ignoredOperators.AddAll(clippingPathOperators);
            ignoredOperators.AddAll(lineStyleOperators);
            ignoredOperators.AddAll(iText.IO.Util.JavaUtil.ArraysAsList("Tc", "Tw", "Tz", "Tf", "Tr", "Ts"));
            ignoredOperators.AddAll(iText.IO.Util.JavaUtil.ArraysAsList("BMC", "BDC"));
        }

        private PdfDocument document;

        private PdfPage currentPage;

        private PdfCleanUpFilter filter;

        private Stack<PdfCanvas> canvasStack;

        private bool removeAnnotIfPartOverlap = true;

        /// <summary>
        /// In
        /// <c>notAppliedGsParams</c>
        /// field not written graphics state params are stored.
        /// Stack represents gs params on different levels of the q/Q nesting (see
        /// <see cref="NotAppliedGsParams"/>
        /// ).
        /// On "q" operator new
        /// <c>NotAppliedGsParams</c>
        /// is pushed to the stack and on "Q" it is popped.
        /// <p>
        /// When operators are applied, they are written from the outer to inner nesting level, separated by "q".
        /// After being written the stack is cleared.
        /// <p>
        /// Graphics state parameters are applied in two ways:
        /// <ul>
        /// <li>
        /// first - right before writing text content, text state in current gs is compare to the text state of the text
        /// render info gs and difference is applied to current gs;
        /// </li>
        /// <li>
        /// second - through list of the not applied gs params. Right before writing some content, this list is checked,
        /// and if something affecting content is stored in this list it will be applied.
        /// </li>
        /// </ul>
        /// </summary>
        private LinkedList<PdfCleanUpProcessor.NotAppliedGsParams> notAppliedGsParams;

        private LinkedList<CanvasTag> notWrittenTags;

        private int numOfOpenedTagsInsideText;

        private bool btEncountered;

        private bool isInText;

        private TextPositioning textPositioning;

        internal PdfCleanUpProcessor(IList<Rectangle> cleanUpRegions, PdfDocument document)
            : base(new PdfCleanUpEventListener()) {
            this.document = document;
            this.filter = new PdfCleanUpFilter(cleanUpRegions);
            this.canvasStack = new Stack<PdfCanvas>();
            this.notAppliedGsParams = new LinkedList<PdfCleanUpProcessor.NotAppliedGsParams>();
            this.notAppliedGsParams.AddFirst(new PdfCleanUpProcessor.NotAppliedGsParams());
            this.notWrittenTags = new LinkedList<CanvasTag>();
            this.numOfOpenedTagsInsideText = 0;
            this.btEncountered = false;
            this.isInText = false;
            this.textPositioning = new TextPositioning();
        }

        public override void ProcessPageContent(PdfPage page) {
            currentPage = page;
            base.ProcessPageContent(page);
        }

        /// <summary>Process the annotations of a page.</summary>
        /// <remarks>
        /// Process the annotations of a page.
        /// Default process behaviour is to remove the annotation if there is (partial) overlap with a redaction region
        /// </remarks>
        /// <param name="page">the page to process</param>
        /// <param name="regions">a list of redaction regions</param>
        public virtual void ProcessPageAnnotations(PdfPage page, IList<Rectangle> regions) {
            //Iterate over annotations
            foreach (PdfAnnotation annot in page.GetAnnotations()) {
                //Check against regions
                foreach (Rectangle region in regions) {
                    if (ProcessAnnotation(annot, region)) {
                        //Individual process methods will return true if the entire annotation needs to be removed
                        page.RemoveAnnotation(annot);
                    }
                }
            }
        }

        private bool ProcessAnnotation(PdfAnnotation annotation, Rectangle region) {
            //TODO(DEVSIX-1605,DEVSIX-1606,DEVSIX-1607,DEVSIX-1608,DEVSIX-1609)
            removeAnnotIfPartOverlap = true;
            //Check if the field is a terminal form-field, by checking if the FT entry exists
            if (annotation.GetPdfObject().Get(PdfName.FT) != null) {
                return ProcessAnnotationForm(annotation, region);
            }
            PdfName annotationType = annotation.GetPdfObject().GetAsName(PdfName.Subtype);
            PdfArray rectAsArray = annotation.GetRectangle();
            Rectangle rect = null;
            if (rectAsArray != null) {
                rect = rectAsArray.ToRectangle();
            }
            //For text and some link annotations, check passed region against rectangle entry
            if (PdfName.Link.Equals(annotationType)) {
                PdfArray quadPoints = ((PdfLinkAnnotation)annotation).GetQuadPoints();
                if (rect != null && UseRectangleForLinkAnnotation(rect, quadPoints)) {
                    return ProcessAnnotationRectangle(annotation, region, rect);
                }
                else {
                    return ProcessAnnotationQuadPoints(annotation, region, quadPoints);
                }
            }
            else {
                if (PdfName.Text.Equals(annotationType)) {
                    return rect != null && ProcessAnnotationRectangle(annotation, region, rect);
                }
                else {
                    if (PdfName.Line.Equals(annotationType)) {
                        //For line annotations, check against the /L array
                        return ProcessAnnotationLine(annotation, region);
                    }
                    else {
                        if (annotationType.Equals(PdfName.Highlight)) {
                            PdfArray quadPoints = ((PdfTextMarkupAnnotation)annotation).GetQuadPoints();
                            //For highlight annotations, check against the quadpoints array
                            return ProcessAnnotationQuadPoints(annotation, region, quadPoints);
                        }
                    }
                }
            }
            return false;
        }

        //Default to not remove
        private bool ProcessAnnotationRectangle(PdfAnnotation annotation, Rectangle region, Rectangle annotationRect
            ) {
            bool result = false;
            //Default to not remove
            //3 possible situations: full overlap, partial overlap, no overlap
            if (region.Overlaps(annotationRect)) {
                //full overlap
                if (region.Contains(annotationRect)) {
                    result = true;
                    return result;
                }
                //partial overlap
                Rectangle intersectionRect = region.GetIntersection(annotationRect);
                if (intersectionRect != null) {
                    if (removeAnnotIfPartOverlap) {
                        result = true;
                        return result;
                    }
                }
            }
            //TODO(DEVSIX-1605,DEVSIX-1606,DEVSIX-1609)
            //No overlap, do nothing
            return result;
        }

        private bool ProcessAnnotationLine(PdfAnnotation annotation, Rectangle region) {
            bool result = false;
            //Default to not remove;
            //TODO(DEVSIX-1607) Do we need to check Line entry on existence as we do with rect?
            Rectangle rect = new Rectangle(annotation.GetPdfObject().GetAsArray(PdfName.L).ToRectangle());
            if (region.Overlaps(rect)) {
                //Full overlap
                if (region.Contains(rect)) {
                    result = true;
                    return result;
                }
                //partial overlap
                Rectangle intersectionRect = region.GetIntersection(rect);
                if (intersectionRect != null) {
                    if (removeAnnotIfPartOverlap) {
                        result = true;
                        return result;
                    }
                }
            }
            //TODO(DEVSIX-1607)
            //No overlap, do nothing
            return result;
        }

        private bool ProcessAnnotationQuadPoints(PdfAnnotation annotation, Rectangle region, PdfArray quadPoints) {
            //Create rectangles from quadpoints array
            bool result = false;
            //Default to not remove;
            //TODO(DEVSIX-1605, DEVSIX-1608) consider possibility of missing quadPoints
            IList<Rectangle> boundingRectangles = Rectangle.CreateBoundingRectanglesFromQuadPoint(quadPoints);
            if (boundingRectangles != null) {
                foreach (Rectangle boundingRect in boundingRectangles) {
                    //3 possible situations: full overlap, partial overlap, no overlap
                    if (region.Overlaps(boundingRect)) {
                        //full overlap
                        if (region.Contains(boundingRect)) {
                            result = true;
                            return result;
                        }
                        //partial overlap
                        Rectangle intersectionRect = region.GetIntersection(boundingRect);
                        if (intersectionRect != null) {
                            if (removeAnnotIfPartOverlap) {
                                result = true;
                                return result;
                            }
                        }
                    }
                }
            }
            //TODO(DEVSIX-1605,DEVSIX-1608)
            //no overlap, do nothing
            return result;
        }

        private bool ProcessAnnotationForm(PdfAnnotation annotation, Rectangle region) {
            bool result = false;
            //Default to not remove;
            //Forms with no kids can be processed by their rectangle
            PdfArray rectAsArray = annotation.GetRectangle();
            if (rectAsArray != null) {
                result = ProcessAnnotationRectangle(annotation, region, rectAsArray.ToRectangle());
                return result;
            }
            //TODO{DEVSIX-1609) Partial redaction
            //Currently, a radiobutton field will get its kids processed (because they appear on a page), but will not be directly touched
            return result;
        }

        /// <summary>
        /// For a link annotation, a quadpoints array can be specified
        /// but it will be ignored in favour of the rectangle
        /// if one of the points is located outside the rectangle's boundaries
        /// </summary>
        /// <param name="rect">rectangle enry of the link annotation</param>
        /// <param name="quadPoints"/>
        /// <returns>True if the rectangle should be used, False if the quadpoint array should be used</returns>
        private bool UseRectangleForLinkAnnotation(Rectangle rect, PdfArray quadPoints) {
            if (quadPoints == null) {
                return true;
            }
            bool noPointsOutside = true;
            for (int i = 0; i < quadPoints.Size(); i += 8) {
                for (int j = 0; j < 8; j += 2) {
                    float x = quadPoints.GetAsNumber(i + j).FloatValue();
                    float y = quadPoints.GetAsNumber(i + j + 1).FloatValue();
                    noPointsOutside = noPointsOutside && rect.Contains(new Rectangle(x, y, 0, 0));
                }
            }
            return !noPointsOutside;
        }

        /// <param name="contentBytes">the bytes of a content stream</param>
        /// <param name="resources">the resources of the content stream. Must not be null.</param>
        public override void ProcessContent(byte[] contentBytes, PdfResources resources) {
            canvasStack.Push(new PdfCanvas(new PdfStream(), new PdfResources(), document));
            if (canvasStack.Count == 1) {
                // If it is the first canvas, we begin to wrap it with q
                GetCanvas().SaveState();
            }
            base.ProcessContent(contentBytes, resources);
        }

        // Here we don't pop() canvases by intent. It is the responsibility of the one who utilizes the canvas data
        public override IEventListener GetEventListener() {
            return (PdfCleanUpEventListener)eventListener;
        }

        internal virtual PdfCanvas PopCleanedCanvas() {
            // If it is the last canvas, we finish to wrap it with Q
            if (canvasStack.Count == 1) {
                GetCanvas().RestoreState();
            }
            return canvasStack.Pop();
        }

        protected override void InvokeOperator(PdfLiteral @operator, IList<PdfObject> operands) {
            String operatorString = @operator.ToString();
            WriteGsParamsIfFormXObject(operatorString, operands);
            base.InvokeOperator(@operator, operands);
            PopCanvasIfFormXObject(operatorString, operands);
            FilterContent(operatorString, operands);
        }

        protected override void BeginMarkedContent(PdfName tag, PdfDictionary dict) {
            base.BeginMarkedContent(tag, dict);
            notWrittenTags.AddFirst(new CanvasTag(tag).SetProperties(dict));
            if (btEncountered) {
                ++numOfOpenedTagsInsideText;
            }
        }

        internal static void WriteOperands(PdfCanvas canvas, IList<PdfObject> operands) {
            int index = 0;
            foreach (PdfObject obj in operands) {
                canvas.GetContentStream().GetOutputStream().Write(obj);
                if (operands.Count > ++index) {
                    canvas.GetContentStream().GetOutputStream().WriteSpace();
                }
                else {
                    canvas.GetContentStream().GetOutputStream().WriteNewLine();
                }
            }
        }

        internal static Matrix OperandsToMatrix(IList<PdfObject> operands) {
            float a = ((PdfNumber)operands[0]).FloatValue();
            float b = ((PdfNumber)operands[1]).FloatValue();
            float c = ((PdfNumber)operands[2]).FloatValue();
            float d = ((PdfNumber)operands[3]).FloatValue();
            float e = ((PdfNumber)operands[4]).FloatValue();
            float f = ((PdfNumber)operands[5]).FloatValue();
            return new Matrix(a, b, c, d, e, f);
        }

        protected override void EventOccurred(IEventData data, EventType type) {
            if (supportedEvents == null || supportedEvents.Contains(type)) {
                eventListener.EventOccurred(data, type);
            }
        }

        private void WriteGsParamsIfFormXObject(String @operator, IList<PdfObject> operands) {
            if ("Do".Equals(@operator)) {
                PdfStream formStream = GetXObjectStream((PdfName)operands[0]);
                if (PdfName.Form.Equals(formStream.GetAsName(PdfName.Subtype))) {
                    WriteNotAppliedGsParams(true, true);
                    OpenNotWrittenTags();
                }
            }
        }

        private void PopCanvasIfFormXObject(String @operator, IList<PdfObject> operands) {
            if ("Do".Equals(@operator)) {
                PdfStream formStream = GetXObjectStream((PdfName)operands[0]);
                if (PdfName.Form.Equals(formStream.GetAsName(PdfName.Subtype))) {
                    PdfCanvas cleanedCanvas = PopCleanedCanvas();
                    PdfFormXObject newFormXObject = new PdfFormXObject((Rectangle)null);
                    newFormXObject.GetPdfObject().PutAll(formStream);
                    if (formStream.ContainsKey(PdfName.Resources)) {
                        newFormXObject.Put(PdfName.Resources, cleanedCanvas.GetResources().GetPdfObject());
                    }
                    newFormXObject.GetPdfObject().SetData(cleanedCanvas.GetContentStream().GetBytes());
                    PdfName name = GetCanvas().GetResources().AddForm(newFormXObject);
                    GetCanvas().GetContentStream().GetOutputStream().Write(name).WriteSpace().WriteBytes(ByteUtils.GetIsoBytes
                        ("Do\n"));
                }
            }
        }

        private void FilterContent(String @operator, IList<PdfObject> operands) {
            if (textShowingOperators.Contains(@operator)) {
                CleanText(@operator, operands);
            }
            else {
                if ("Do".Equals(@operator)) {
                    CheckIfImageAndClean(operands);
                }
                else {
                    if ("EI".Equals(@operator)) {
                        CleanInlineImage();
                    }
                    else {
                        if (pathPaintingOperators.Contains(@operator)) {
                            WritePath();
                        }
                        else {
                            if ("q".Equals(@operator)) {
                                notAppliedGsParams.AddFirst(new PdfCleanUpProcessor.NotAppliedGsParams());
                            }
                            else {
                                if ("Q".Equals(@operator)) {
                                    notAppliedGsParams.JRemoveFirst();
                                    if (notAppliedGsParams.Count == 0) {
                                        GetCanvas().RestoreState();
                                        notAppliedGsParams.AddFirst(new PdfCleanUpProcessor.NotAppliedGsParams());
                                    }
                                }
                                else {
                                    if ("BT".Equals(@operator)) {
                                        btEncountered = true;
                                    }
                                    else {
                                        if ("ET".Equals(@operator)) {
                                            if (isInText) {
                                                WriteOperands(GetCanvas(), operands);
                                                isInText = false;
                                            }
                                            btEncountered = false;
                                            textPositioning.Clear();
                                        }
                                        else {
                                            if (textPositioningOperators.Contains(@operator)) {
                                                textPositioning.AppendPositioningOperator(@operator, operands);
                                            }
                                            else {
                                                if ("EMC".Equals(@operator)) {
                                                    // BMC and BDC are handled with BeginMarkedContent method
                                                    RemoveOrCloseTag();
                                                }
                                                else {
                                                    if (lineStyleOperators.Contains(@operator)) {
                                                        notAppliedGsParams.Peek().lineStyleOperators.Put(@operator, new List<PdfObject>(operands));
                                                    }
                                                    else {
                                                        if ("gs".Equals(@operator)) {
                                                            notAppliedGsParams.Peek().extGStates.Add(GetResources().GetResource(PdfName.ExtGState).GetAsDictionary((PdfName
                                                                )operands[0]));
                                                        }
                                                        else {
                                                            if ("cm".Equals(@operator)) {
                                                                notAppliedGsParams.Peek().ctms.Add(new List<PdfObject>(operands));
                                                            }
                                                            else {
                                                                if (strokeColorOperators.Contains(@operator)) {
                                                                    notAppliedGsParams.Peek().strokeColor = GetGraphicsState().GetStrokeColor();
                                                                }
                                                                else {
                                                                    if (fillColorOperators.Contains(@operator)) {
                                                                        notAppliedGsParams.Peek().fillColor = GetGraphicsState().GetFillColor();
                                                                    }
                                                                    else {
                                                                        if ("sh".Equals(@operator)) {
                                                                            PdfShading shading = PdfShading.MakeShading(GetResources().GetResource(PdfName.Shading).GetAsDictionary((PdfName
                                                                                )operands[0]));
                                                                            GetCanvas().PaintShading(shading);
                                                                        }
                                                                        else {
                                                                            if (!ignoredOperators.Contains(@operator)) {
                                                                                WriteOperands(GetCanvas(), operands);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CleanText(String @operator, IList<PdfObject> operands) {
            IList<TextRenderInfo> textChunks = null;
            PdfArray cleanedText = null;
            if ("TJ".Equals(@operator)) {
                PdfArray originalTJ = (PdfArray)operands[0];
                int i = 0;
                // text chunk index in original TJ
                PdfTextArray newTJ = new PdfTextArray();
                foreach (PdfObject e in originalTJ) {
                    if (e.IsString()) {
                        if (null == textChunks) {
                            textChunks = ((PdfCleanUpEventListener)((PdfCleanUpEventListener)GetEventListener())).GetEncounteredText();
                        }
                        PdfArray filteredText = filter.FilterText(textChunks[i++]).GetFilterResult();
                        newTJ.AddAll(filteredText);
                    }
                    else {
                        newTJ.Add(e);
                    }
                }
                cleanedText = newTJ;
            }
            else {
                // if operator is Tj or ' or "
                textChunks = ((PdfCleanUpEventListener)((PdfCleanUpEventListener)GetEventListener())).GetEncounteredText();
                PdfCleanUpFilter.FilterResult<PdfArray> filterResult = filter.FilterText(textChunks[0]);
                if (filterResult.IsModified()) {
                    cleanedText = filterResult.GetFilterResult();
                }
            }
            // if text wasn't modified cleanedText is null
            if (cleanedText == null || cleanedText.Size() != 1 || !cleanedText.Get(0).IsNumber()) {
                if (null == textChunks) {
                    textChunks = ((PdfCleanUpEventListener)((PdfCleanUpEventListener)GetEventListener())).GetEncounteredText();
                }
                TextRenderInfo text = textChunks[0];
                // all text chunks even in case of TJ have the same graphics state
                WriteNotAppliedGsParamsForText(text);
                BeginTextObjectAndOpenNotWrittenTags();
                WriteNotAppliedTextStateParams(text);
                textPositioning.WritePositionedText(@operator, operands, cleanedText, GetCanvas());
            }
            else {
                // cleaned text is tj array with single number - it means that the whole text chunk was removed
                CanvasGraphicsState gs = GetCanvas().GetGraphicsState();
                // process new lines if necessary
                if ("'".Equals(@operator) || "\"".Equals(@operator)) {
                    IList<PdfObject> newLineList = new List<PdfObject>();
                    newLineList.Add(new PdfLiteral("T*"));
                    textPositioning.AppendPositioningOperator("T*", newLineList);
                }
                textPositioning.AppendTjArrayWithSingleNumber(cleanedText, gs.GetFontSize(), gs.GetHorizontalScaling());
            }
        }

        private void BeginTextObjectAndOpenNotWrittenTags() {
            if (!isInText) {
                int numOfTagsBeforeBT = notWrittenTags.Count - numOfOpenedTagsInsideText;
                CanvasTag tag;
                for (int i = 0; i < numOfTagsBeforeBT; ++i) {
                    tag = notWrittenTags.PollLast();
                    GetCanvas().OpenTag(tag);
                }
                GetCanvas().BeginText();
                isInText = true;
                OpenNotWrittenTags();
            }
            else {
                OpenNotWrittenTags();
            }
        }

        private void WriteNotAppliedTextStateParams(TextRenderInfo text) {
            PdfCanvas canvas = GetCanvas();
            CanvasGraphicsState currGs = canvas.GetGraphicsState();
            if (currGs.GetCharSpacing() != text.GetCharSpacing()) {
                canvas.SetCharacterSpacing(text.GetCharSpacing());
            }
            if (currGs.GetWordSpacing() != text.GetWordSpacing()) {
                canvas.SetWordSpacing(text.GetWordSpacing());
            }
            if (currGs.GetHorizontalScaling() != text.GetHorizontalScaling()) {
                canvas.SetHorizontalScaling(text.GetHorizontalScaling());
            }
            // not writing leading here, it is processed along with positioning operators
            PdfFont currFont = currGs.GetFont();
            if (currFont == null || currFont.GetPdfObject() != text.GetFont().GetPdfObject() || currGs.GetFontSize() !=
                 text.GetFontSize()) {
                canvas.SetFontAndSize(text.GetFont(), text.GetFontSize());
            }
            if (currGs.GetTextRenderingMode() != text.GetTextRenderMode()) {
                canvas.SetTextRenderingMode(text.GetTextRenderMode());
            }
            if (currGs.GetTextRise() != text.GetRise()) {
                canvas.SetTextRise(text.GetRise());
            }
        }

        private void WriteNotAppliedGsParamsForText(TextRenderInfo textRenderInfo) {
            bool stroke = false;
            bool fill = false;
            switch (textRenderInfo.GetTextRenderMode()) {
                case PdfCanvasConstants.TextRenderingMode.STROKE:
                case PdfCanvasConstants.TextRenderingMode.STROKE_CLIP: {
                    stroke = true;
                    break;
                }

                case PdfCanvasConstants.TextRenderingMode.FILL:
                case PdfCanvasConstants.TextRenderingMode.FILL_CLIP: {
                    fill = true;
                    break;
                }

                case PdfCanvasConstants.TextRenderingMode.FILL_STROKE:
                case PdfCanvasConstants.TextRenderingMode.FILL_STROKE_CLIP: {
                    stroke = true;
                    fill = true;
                    break;
                }
            }
            WriteNotAppliedGsParams(fill, stroke);
        }

        private void CheckIfImageAndClean(IList<PdfObject> operands) {
            PdfStream imageStream = GetXObjectStream((PdfName)operands[0]);
            if (PdfName.Image.Equals(imageStream.GetAsName(PdfName.Subtype))) {
                ImageRenderInfo encounteredImage = ((PdfCleanUpEventListener)GetEventListener()).GetEncounteredImage();
                PdfCleanUpFilter.FilterResult<ImageData> imageFilterResult = filter.FilterImage(encounteredImage);
                PdfImageXObject imageToWrite = null;
                if (imageFilterResult.IsModified()) {
                    ImageData filteredImage = imageFilterResult.GetFilterResult();
                    if (filteredImage != null) {
                        imageToWrite = new PdfImageXObject(filteredImage);
                        CopySMaskData(encounteredImage.GetImage().GetPdfObject(), imageToWrite.GetPdfObject());
                    }
                }
                else {
                    imageToWrite = encounteredImage.GetImage();
                }
                if (imageToWrite != null) {
                    float[] ctm = PollNotAppliedCtm();
                    WriteNotAppliedGsParams(false, false);
                    OpenNotWrittenTags();
                    GetCanvas().AddXObject(imageToWrite, ctm[0], ctm[1], ctm[2], ctm[3], ctm[4], ctm[5]);
                }
            }
        }

        private void CleanInlineImage() {
            ImageRenderInfo encounteredImage = ((PdfCleanUpEventListener)GetEventListener()).GetEncounteredImage();
            PdfCleanUpFilter.FilterResult<ImageData> imageFilterResult = filter.FilterImage(encounteredImage);
            ImageData filteredImage = imageFilterResult.GetFilterResult();
            if (filteredImage != null) {
                bool? imageMaskFlag = encounteredImage.GetImage().GetPdfObject().GetAsBool(PdfName.ImageMask);
                if (imageMaskFlag != null && (bool)imageMaskFlag) {
                    filteredImage.MakeMask();
                }
                float[] ctm = PollNotAppliedCtm();
                WriteNotAppliedGsParams(false, false);
                OpenNotWrittenTags();
                GetCanvas().AddImage(filteredImage, ctm[0], ctm[1], ctm[2], ctm[3], ctm[4], ctm[5], true);
            }
        }

        // TODO
        // PdfCanvas doesn't have a method that writes inline image using pdf stream, and only have method which
        // accepts Image as parameter. That's why we can't write image just as it was in original file, we convert it to Image.
        // IMPORTANT: If writing of pdf stream of not changed inline image will be implemented, don't forget to ensure that
        // inline image color space is present in new resources if necessary.
        private void CopySMaskData(PdfStream imageStream, PdfStream filteredImageStream) {
            if (imageStream.ContainsKey(PdfName.SMask)) {
                filteredImageStream.Put(PdfName.SMask, imageStream.Get(PdfName.SMask));
            }
            if (imageStream.ContainsKey(PdfName.Mask)) {
                filteredImageStream.Put(PdfName.Mask, imageStream.Get(PdfName.Mask));
            }
            if (imageStream.ContainsKey(PdfName.SMaskInData)) {
                filteredImageStream.Put(PdfName.SMaskInData, imageStream.Get(PdfName.SMaskInData));
            }
        }

        private void WritePath() {
            PathRenderInfo path = ((PdfCleanUpEventListener)GetEventListener()).GetEncounteredPath();
            bool stroke = (path.GetOperation() & PathRenderInfo.STROKE) == PathRenderInfo.STROKE;
            bool fill = (path.GetOperation() & PathRenderInfo.FILL) == PathRenderInfo.FILL;
            bool clip = path.IsPathModifiesClippingPath();
            // Here we intentionally draw all three paths separately and not combining them in any way:
            // First of all, stroke converted to fill paths, therefore it could not be combined with fill (if it is
            // stroke-fill operation) or clip paths, and also it should be drawn after the fill, because in case it's
            // stroke-fill operation stroke should be "on top" of the filled area.
            // Secondly, current clipping path modifying happens AFTER the path painting. So if it is drawn separately, clip
            // path should be the last one.
            // So consider the situation when it is stroke-fill operation and also this path is marked as clip path.
            // And here we have it: fill path is the first, stroke path is the second and clip path is the last. And
            // stroke path could not be combined with neither fill nor clip paths.
            // Some improved logic could be applied to distinguish the cases when some paths actually could be drawn as one,
            // but this is the only generic solution.
            Path fillPath = null;
            PdfCanvas canvas = GetCanvas();
            if (fill) {
                fillPath = filter.FilterFillPath(path, path.GetRule());
                if (!fillPath.IsEmpty()) {
                    WriteNotAppliedGsParams(true, false);
                    OpenNotWrittenTags();
                    WritePath(fillPath);
                    if (path.GetRule() == PdfCanvasConstants.FillingRule.NONZERO_WINDING) {
                        canvas.Fill();
                    }
                    else {
                        // FillingRule.EVEN_ODD
                        canvas.EoFill();
                    }
                }
            }
            if (stroke) {
                Path strokePath = filter.FilterStrokePath(path);
                if (!strokePath.IsEmpty()) {
                    // we pass stroke here as false, because stroke is transformed into fill. we don't need to set stroke color
                    WriteNotAppliedGsParams(false, false);
                    OpenNotWrittenTags();
                    WriteStrokePath(strokePath, path.GetStrokeColor());
                }
            }
            if (clip) {
                Path clippingPath;
                if (fill && path.GetClippingRule() == path.GetRule()) {
                    clippingPath = fillPath;
                }
                else {
                    clippingPath = filter.FilterFillPath(path, path.GetClippingRule());
                }
                if (!clippingPath.IsEmpty()) {
                    WriteNotAppliedGsParams(false, false);
                    OpenNotWrittenTags();
                    WritePath(clippingPath);
                    if (path.GetClippingRule() == PdfCanvasConstants.FillingRule.NONZERO_WINDING) {
                        canvas.Clip();
                    }
                    else {
                        // FillingRule.EVEN_ODD
                        canvas.EoClip();
                    }
                }
                else {
                    // If the clipping path from the source document is cleaned (it happens when reduction
                    // area covers the path completely), then you should treat it as an empty set (no points
                    // are included in the path). Then the current clipping path (which is the intersection
                    // between previous clipping path and the new one) is also empty set, which means that
                    // there is no visible content at all. But at the same time as we removed the clipping
                    // path, the invisible content would become visible. So, to emulate the correct result,
                    // we would simply put a degenerate clipping path which consists of a single point at (0, 0).
                    WriteNotAppliedGsParams(false, false);
                    // we still need to open all q operators
                    canvas.MoveTo(0, 0).Clip();
                }
                canvas.NewPath();
            }
        }

        private void WritePath(Path path) {
            PdfCanvas canvas = GetCanvas();
            foreach (Subpath subpath in path.GetSubpaths()) {
                canvas.MoveTo((float)subpath.GetStartPoint().GetX(), (float)subpath.GetStartPoint().GetY());
                foreach (IShape segment in subpath.GetSegments()) {
                    if (segment is BezierCurve) {
                        IList<Point> basePoints = segment.GetBasePoints();
                        Point p2 = basePoints[1];
                        Point p3 = basePoints[2];
                        Point p4 = basePoints[3];
                        canvas.CurveTo((float)p2.GetX(), (float)p2.GetY(), (float)p3.GetX(), (float)p3.GetY(), (float)p4.GetX(), (
                            float)p4.GetY());
                    }
                    else {
                        // segment is Line
                        Point destination = segment.GetBasePoints()[1];
                        canvas.LineTo((float)destination.GetX(), (float)destination.GetY());
                    }
                }
                if (subpath.IsClosed()) {
                    canvas.ClosePath();
                }
            }
        }

        private void WriteStrokePath(Path strokePath, Color strokeColor) {
            PdfCanvas canvas = GetCanvas();
            // As we transformed stroke to fill, we set stroke color for filling here
            canvas.SaveState().SetFillColor(strokeColor);
            WritePath(strokePath);
            canvas.Fill().RestoreState();
        }

        private PdfCanvas GetCanvas() {
            return canvasStack.Peek();
        }

        // should be called before some content is drawn
        private void OpenNotWrittenTags() {
            CanvasTag tag = notWrittenTags.PollLast();
            while (tag != null) {
                GetCanvas().OpenTag(tag);
                tag = notWrittenTags.PollLast();
            }
        }

        private void RemoveOrCloseTag() {
            if (notWrittenTags.Count > 0) {
                CanvasTag tag = notWrittenTags.JRemoveFirst();
                if (tag.HasMcid() && document.IsTagged()) {
                    TagTreePointer pointer = document.GetTagStructureContext().RemoveContentItem(currentPage, tag.GetMcid());
                    if (pointer != null) {
                        while (pointer.GetKidsRoles().Count == 0) {
                            pointer.RemoveTag();
                        }
                    }
                }
            }
            else {
                GetCanvas().EndMarkedContent();
            }
            if (btEncountered) {
                --numOfOpenedTagsInsideText;
            }
        }

        /// <summary>To add images and formXObjects to canvas we pass ctm.</summary>
        /// <remarks>
        /// To add images and formXObjects to canvas we pass ctm. Here we try to find last not applied ctm in order to pass it to
        /// PdfCanvas method later. Returned ctm is written right before the image, that's why we care only for not applied ctms of
        /// the current (the "deepest") q/Q nesting level.
        /// If such ctm wasn't found identity ctm is returned.
        /// </remarks>
        private float[] PollNotAppliedCtm() {
            IList<IList<PdfObject>> ctms = notAppliedGsParams.Peek().ctms;
            if (ctms.Count == 0) {
                return new float[] { 1, 0, 0, 1, 0, 0 };
            }
            IList<PdfObject> lastCtm = ctms.JRemoveAt(ctms.Count - 1);
            float[] ctm = new float[6];
            ctm[0] = ((PdfNumber)lastCtm[0]).FloatValue();
            ctm[1] = ((PdfNumber)lastCtm[1]).FloatValue();
            ctm[2] = ((PdfNumber)lastCtm[2]).FloatValue();
            ctm[3] = ((PdfNumber)lastCtm[3]).FloatValue();
            ctm[4] = ((PdfNumber)lastCtm[4]).FloatValue();
            ctm[5] = ((PdfNumber)lastCtm[5]).FloatValue();
            return ctm;
        }

        private void WriteNotAppliedGsParams(bool fill, bool stroke) {
            if (notAppliedGsParams.Count > 0) {
                while (notAppliedGsParams.Count != 1) {
                    PdfCleanUpProcessor.NotAppliedGsParams gsParams = notAppliedGsParams.PollLast();
                    // We want to apply graphics state params of outer q/Q nesting level on it's level and not on the inner
                    // q/Q nesting level. Because of that we write all gs params for the outer q/Q, just in case it will be needed
                    // later (if we don't write it now, there will be no possibility to write it in the outer q/Q later).
                    ApplyGsParams(true, true, gsParams);
                    GetCanvas().SaveState();
                }
                ApplyGsParams(fill, stroke, notAppliedGsParams.Peek());
            }
        }

        private void ApplyGsParams(bool fill, bool stroke, PdfCleanUpProcessor.NotAppliedGsParams gsParams) {
            foreach (PdfDictionary extGState in gsParams.extGStates) {
                GetCanvas().SetExtGState(extGState);
            }
            gsParams.extGStates.Clear();
            if (gsParams.ctms.Count > 0) {
                Matrix m = new Matrix();
                foreach (IList<PdfObject> ctm in gsParams.ctms) {
                    m = OperandsToMatrix(ctm).Multiply(m);
                }
                GetCanvas().ConcatMatrix(m.Get(Matrix.I11), m.Get(Matrix.I12), m.Get(Matrix.I21), m.Get(Matrix.I22), m.Get
                    (Matrix.I31), m.Get(Matrix.I32));
                gsParams.ctms.Clear();
            }
            if (stroke) {
                foreach (IList<PdfObject> strokeState in gsParams.lineStyleOperators.Values) {
                    WriteOperands(GetCanvas(), strokeState);
                }
                gsParams.lineStyleOperators.Clear();
            }
            if (fill) {
                if (gsParams.fillColor != null) {
                    GetCanvas().SetFillColor(gsParams.fillColor);
                }
                gsParams.fillColor = null;
            }
            if (stroke) {
                if (gsParams.strokeColor != null) {
                    GetCanvas().SetStrokeColor(gsParams.strokeColor);
                }
                gsParams.strokeColor = null;
            }
        }

        /// <summary>Single instance of this class represents not applied graphics state params of the single q/Q nesting level.
        ///     </summary>
        /// <remarks>
        /// Single instance of this class represents not applied graphics state params of the single q/Q nesting level.
        /// For example:
        /// <p>
        /// 0 g
        /// 1 0 0 1 25 50 cm
        /// <p>
        /// q
        /// <p>
        /// 5 w
        /// /Gs1 gs
        /// 13 g
        /// <p>
        /// Q
        /// <p>
        /// 1 0 0 RG
        /// <p>
        /// Operators "0 g", "1 0 0 1 25 50 cm" and "1 0 0 RG" belong to the outer q/Q nesting level;
        /// Operators "5 w", "/Gs1 gs", "13 g" belong to the inner q/Q nesting level.
        /// Operators of every level of the q/Q nesting are stored in different instances of this class.
        /// </remarks>
        internal class NotAppliedGsParams {
            internal IList<PdfDictionary> extGStates = new List<PdfDictionary>();

            internal IList<IList<PdfObject>> ctms = new List<IList<PdfObject>>();

            internal Color fillColor;

            internal Color strokeColor;

            internal IDictionary<String, IList<PdfObject>> lineStyleOperators = new LinkedDictionary<String, IList<PdfObject
                >>();
            // list of operator statements
            // operator and it's operands
        }
    }
}
