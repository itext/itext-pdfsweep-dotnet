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
using System.Collections.Generic;
using iText.Commons.Utils;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace iText.PdfCleanup.Autosweep {
    /// <summary>Class that automatically extracts all regions of interest from a given PdfDocument and redacts them.
    ///     </summary>
    public class PdfAutoSweepTools {
        private ICleanupStrategy strategy;

        private int annotationNumber = 1;

        /// <summary>Construct a new instance of PdfAutoSweepTools with a given ICleanupStrategy</summary>
        /// <param name="strategy">the redaction strategy to be used</param>
        public PdfAutoSweepTools(ICleanupStrategy strategy) {
            this.strategy = strategy;
        }

        /// <summary>
        /// Highlight areas of interest in a given
        /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
        /// </summary>
        /// <param name="pdfDocument">
        /// the
        /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
        /// to be highlighted
        /// </param>
        public virtual void Highlight(PdfDocument pdfDocument) {
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++) {
                Highlight(pdfDocument.GetPage(i));
            }
        }

        /// <summary>
        /// Highlight areas of interest in a given
        /// <see cref="iText.Kernel.Pdf.PdfPage"/>
        /// </summary>
        /// <param name="pdfPage">
        /// the
        /// <see cref="iText.Kernel.Pdf.PdfPage"/>
        /// to be highlighted
        /// </param>
        public virtual void Highlight(PdfPage pdfPage) {
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = GetPdfCleanUpLocations(pdfPage);
            foreach (iText.PdfCleanup.PdfCleanUpLocation loc in cleanUpLocations) {
                PdfCanvas canvas = new PdfCanvas(pdfPage);
                canvas.SetColor(loc.GetCleanUpColor(), true);
                canvas.Rectangle(loc.GetRegion());
                canvas.Fill();
            }
        }

        /// <summary>
        /// Perform tentative cleanup of areas of interest on a given
        /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
        /// This method will add all redaction annotations to the given document, allowing
        /// the end-user to choose which redactions to keep or delete.
        /// </summary>
        /// <param name="pdfDocument">the document to clean up</param>
        public virtual void TentativeCleanUp(PdfDocument pdfDocument) {
            annotationNumber = 1;
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++) {
                TentativeCleanUp(pdfDocument.GetPage(i));
            }
        }

        /// <summary>
        /// Perform tentative cleanup of areas of interest on a given
        /// <see cref="iText.Kernel.Pdf.PdfPage"/>
        /// This method will add all redaction annotations to the given page, allowing
        /// the end-user to choose which redactions to keep or delete.
        /// </summary>
        /// <param name="pdfPage">the page to clean up</param>
        public virtual void TentativeCleanUp(PdfPage pdfPage) {
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = GetPdfCleanUpLocations(pdfPage);
            foreach (iText.PdfCleanup.PdfCleanUpLocation loc in cleanUpLocations) {
                PdfString title = new PdfString("Annotation:" + annotationNumber);
                annotationNumber++;
                float[] color = loc.GetCleanUpColor().GetColorValue();
                // convert to annotation
                PdfAnnotation redact = new PdfRedactAnnotation(loc.GetRegion()).SetDefaultAppearance(new PdfString("Helvetica 12 Tf 0 g"
                    )).SetTitle(title).Put(PdfName.Subj, PdfName.Redact).Put(PdfName.IC, new PdfArray(new float[] { 0f, 0f
                    , 0f })).Put(PdfName.OC, new PdfArray(color));
                pdfPage.AddAnnotation(redact);
            }
        }

        /// <summary>
        /// Get all
        /// <see cref="iText.PdfCleanup.PdfCleanUpLocation"/>
        /// objects from a given
        /// <see cref="iText.Kernel.Pdf.PdfPage"/>.
        /// </summary>
        /// <param name="page">
        /// the
        /// <see cref="iText.Kernel.Pdf.PdfPage"/>
        /// to be processed
        /// </param>
        /// <returns>
        /// a List of
        /// <see cref="iText.PdfCleanup.PdfCleanUpLocation"/>
        /// objects
        /// </returns>
        public virtual IList<iText.PdfCleanup.PdfCleanUpLocation> GetPdfCleanUpLocations(PdfPage page) {
            // get document
            PdfDocument doc = page.GetDocument();
            // create parser
            PdfDocumentContentParser parser = new PdfDocumentContentParser(doc);
            // get page number
            int pageNr = doc.GetPageNumber(page);
            // process document
            IList<iText.PdfCleanup.PdfCleanUpLocation> toClean = new List<iText.PdfCleanup.PdfCleanUpLocation>();
            parser.ProcessContent(pageNr, strategy);
            foreach (IPdfTextLocation rect in strategy.GetResultantLocations()) {
                if (rect != null) {
                    toClean.Add(new iText.PdfCleanup.PdfCleanUpLocation(pageNr, rect.GetRectangle(), strategy.GetRedactionColor
                        (rect)));
                }
            }
            // reset strategy for next iteration
            ResetStrategy();
            // return
            return toClean;
        }

        /// <summary>
        /// Get all
        /// <see cref="iText.PdfCleanup.PdfCleanUpLocation"/>
        /// objects from a given
        /// <see cref="iText.Kernel.Pdf.PdfDocument"/>.
        /// </summary>
        /// <param name="doc">
        /// the
        /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
        /// to be processed
        /// </param>
        /// <returns>
        /// a List of
        /// <see cref="iText.PdfCleanup.PdfCleanUpLocation"/>
        /// objects
        /// </returns>
        public virtual IList<iText.PdfCleanup.PdfCleanUpLocation> GetPdfCleanUpLocations(PdfDocument doc) {
            PdfDocumentContentParser parser = new PdfDocumentContentParser(doc);
            IList<iText.PdfCleanup.PdfCleanUpLocation> toClean = new List<iText.PdfCleanup.PdfCleanUpLocation>();
            for (int pageNr = 1; pageNr <= doc.GetNumberOfPages(); pageNr++) {
                parser.ProcessContent(pageNr, strategy);
                foreach (IPdfTextLocation rect in strategy.GetResultantLocations()) {
                    if (rect != null) {
                        toClean.Add(new iText.PdfCleanup.PdfCleanUpLocation(pageNr, rect.GetRectangle(), strategy.GetRedactionColor
                            (rect)));
                    }
                }
                ResetStrategy();
            }
            JavaCollectionsUtil.Sort(toClean, new _IComparer_177());
            return toClean;
        }

        private sealed class _IComparer_177 : IComparer<iText.PdfCleanup.PdfCleanUpLocation> {
            public _IComparer_177() {
            }

            public int Compare(iText.PdfCleanup.PdfCleanUpLocation o1, iText.PdfCleanup.PdfCleanUpLocation o2) {
                if (o1.GetPage() != o2.GetPage()) {
                    return o1.GetPage() < o2.GetPage() ? -1 : 1;
                }
                Rectangle r1 = o1.GetRegion();
                Rectangle r2 = o2.GetRegion();
                if (r1.GetY() == r2.GetY()) {
                    return r1.GetX() == r2.GetX() ? 0 : (r1.GetX() < r2.GetX() ? -1 : 1);
                }
                else {
                    return r1.GetY() < r2.GetY() ? -1 : 1;
                }
            }
        }

        private void ResetStrategy() {
            strategy = strategy.Reset();
        }
    }
}
