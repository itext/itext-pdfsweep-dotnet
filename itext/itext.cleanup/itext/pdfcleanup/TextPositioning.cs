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
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;

namespace iText.PdfCleanup {
    internal class TextPositioning {
        private String prevOperator;

        private float? removedTextShift;

        // shift in text space units, which is the result of the removed text
        /// <summary>
        /// Not null only when first pos operator encountered; when concatenation of operators is performed
        /// this field is cleaned and positioning info is stored in either tdShift or tmShift fields.
        /// </summary>
        private IList<PdfObject> firstPositioningOperands;

        private float[] tdShift;

        private Matrix tmShift;

        private float? currLeading;

        /// <summary>Get the current leading</summary>
        internal virtual float GetCurrLeading() {
            if (currLeading != null) {
                return (float)currLeading;
            }
            return 0f;
        }

        internal virtual void AppendPositioningOperator(String @operator, IList<PdfObject> operands) {
            if (firstPositioningOperands != null) {
                StorePositioningInfoInShiftFields();
            }
            if ("TD".Equals(@operator)) {
                currLeading = -((PdfNumber)operands[1]).FloatValue();
            }
            else {
                if ("TL".Equals(@operator)) {
                    currLeading = ((PdfNumber)operands[0]).FloatValue();
                    return;
                }
            }
            removedTextShift = null;
            if (prevOperator == null) {
                firstPositioningOperands = new List<PdfObject>(operands);
                prevOperator = @operator;
            }
            else {
                if ("Tm".Equals(@operator)) {
                    Clear();
                    firstPositioningOperands = new List<PdfObject>(operands);
                    prevOperator = @operator;
                }
                else {
                    float tx;
                    float ty;
                    if ("T*".Equals(@operator)) {
                        tx = 0;
                        ty = -GetCurrLeading();
                    }
                    else {
                        tx = ((PdfNumber)operands[0]).FloatValue();
                        ty = ((PdfNumber)operands[1]).FloatValue();
                    }
                    if ("Tm".Equals(prevOperator)) {
                        tmShift = new Matrix(tx, ty).Multiply(tmShift);
                    }
                    else {
                        // prevOperator is left as TM here
                        tdShift[0] += tx;
                        tdShift[1] += ty;
                        prevOperator = "Td";
                    }
                }
            }
        }

        // concatenation of two any TD, Td, T* result in Td
        private void StorePositioningInfoInShiftFields() {
            if ("Tm".Equals(prevOperator)) {
                tmShift = PdfCleanUpProcessor.OperandsToMatrix(firstPositioningOperands);
            }
            else {
                if ("T*".Equals(prevOperator)) {
                    tdShift = new float[] { 0, -GetCurrLeading() };
                }
                else {
                    tdShift = new float[2];
                    tdShift[0] = ((PdfNumber)firstPositioningOperands[0]).FloatValue();
                    tdShift[1] = ((PdfNumber)firstPositioningOperands[1]).FloatValue();
                }
            }
            firstPositioningOperands = null;
        }

        internal virtual void AppendTjArrayWithSingleNumber(PdfArray tjArray, float fontSize, float scaling) {
            if (removedTextShift == null) {
                removedTextShift = 0f;
            }
            float shift = tjArray.GetAsNumber(0).FloatValue();
            removedTextShift += shift * fontSize * (scaling / 100) / 1000;
        }

        /// <summary>is performed when text object is ended or text chunk is written</summary>
        internal virtual void Clear() {
            // leading is not removed, as it is preserved between different text objects
            firstPositioningOperands = null;
            prevOperator = null;
            removedTextShift = null;
            tdShift = null;
            tmShift = null;
        }

        internal virtual void WritePositionedText(String @operator, IList<PdfObject> operands, PdfArray cleanedText
            , PdfCanvas canvas) {
            WritePositioningOperator(canvas);
            WriteText(@operator, operands, cleanedText, canvas);
            Clear();
        }

        private void WritePositioningOperator(PdfCanvas canvas) {
            if (firstPositioningOperands != null) {
                if ("T*".Equals(prevOperator)) {
                    if (canvas.GetGraphicsState().GetLeading() != currLeading) {
                        canvas.SetLeading((float)currLeading);
                    }
                }
                PdfCleanUpProcessor.WriteOperands(canvas, firstPositioningOperands);
            }
            else {
                if (tdShift != null) {
                    canvas.MoveText(tdShift[0], tdShift[1]);
                }
                else {
                    if (tmShift != null) {
                        canvas.SetTextMatrix(tmShift.Get(Matrix.I11), tmShift.Get(Matrix.I12), tmShift.Get(Matrix.I21), tmShift.Get
                            (Matrix.I22), tmShift.Get(Matrix.I31), tmShift.Get(Matrix.I32));
                    }
                }
            }
        }

        private void WriteText(String @operator, IList<PdfObject> operands, PdfArray cleanedText, PdfCanvas canvas
            ) {
            CanvasGraphicsState canvasGs = canvas.GetGraphicsState();
            bool newLineShowText = "'".Equals(@operator) || "\"".Equals(@operator);
            if (newLineShowText) {
                if (canvasGs.GetLeading() != currLeading) {
                    canvas.SetLeading((float)currLeading);
                }
                // after new line operator, removed text shift doesn't matter
                removedTextShift = null;
            }
            PdfTextArray tjShiftArray = null;
            if (removedTextShift != null) {
                float tjShift = (float)removedTextShift * 1000 / (canvasGs.GetFontSize() * canvasGs.GetHorizontalScaling()
                     / 100);
                tjShiftArray = new PdfTextArray();
                tjShiftArray.Add(new PdfNumber(tjShift));
            }
            if (cleanedText != null) {
                if (newLineShowText) {
                    // char spacing and word spacing are set via writeNotAppliedTextStateParams() method
                    canvas.NewlineText();
                }
                if (removedTextShift != null) {
                    tjShiftArray.AddAll(cleanedText);
                    cleanedText = tjShiftArray;
                }
                canvas.ShowText(cleanedText);
            }
            else {
                if (removedTextShift != null) {
                    canvas.ShowText(tjShiftArray);
                }
                PdfCleanUpProcessor.WriteOperands(canvas, operands);
            }
        }
    }
}
