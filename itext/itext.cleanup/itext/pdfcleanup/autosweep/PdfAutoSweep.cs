/*
This file is part of the iText (R) project.
Copyright (c) 1998-2020 iText Group NV
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
using System.Collections.Generic;
using iText.IO.Util;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace iText.PdfCleanup.Autosweep {
    /// <summary>Class that automatically extracts all regions of interest from a given PdfDocument and redacts them.
    ///     </summary>
    public class PdfAutoSweep {
        private ICleanupStrategy strategy;

        private int annotationNumber = 1;

        /// <summary>Construct a new instance of PdfAutoSweep with a given ICleanupStrategy</summary>
        /// <param name="strategy">the redaction strategy to be used</param>
        public PdfAutoSweep(ICleanupStrategy strategy) {
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
        /// Perform cleanup of areas of interest on a given
        /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
        /// </summary>
        /// <param name="pdfDocument">
        /// the
        /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
        /// to be redacted
        /// </param>
        public virtual void CleanUp(PdfDocument pdfDocument) {
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = GetPdfCleanUpLocations(pdfDocument);
            iText.PdfCleanup.PdfCleanUpTool cleaner = (cleanUpLocations == null) ? new iText.PdfCleanup.PdfCleanUpTool
                (pdfDocument, true) : new iText.PdfCleanup.PdfCleanUpTool(pdfDocument, cleanUpLocations);
            cleaner.CleanUp();
        }

        /// <summary>
        /// Perform cleanup of areas of interest on a given
        /// <see cref="iText.Kernel.Pdf.PdfPage"/>
        /// </summary>
        /// <param name="pdfPage">
        /// the
        /// <see cref="iText.Kernel.Pdf.PdfPage"/>
        /// to be redacted
        /// </param>
        public virtual void CleanUp(PdfPage pdfPage) {
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = GetPdfCleanUpLocations(pdfPage);
            iText.PdfCleanup.PdfCleanUpTool cleaner = (cleanUpLocations == null) ? new iText.PdfCleanup.PdfCleanUpTool
                (pdfPage.GetDocument(), true) : new iText.PdfCleanup.PdfCleanUpTool(pdfPage.GetDocument(), cleanUpLocations
                );
            cleaner.CleanUp();
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
        /// <see cref="iText.Kernel.Pdf.PdfPage"/>
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
        /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
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
            JavaCollectionsUtil.Sort(toClean, new _IComparer_222());
            return toClean;
        }

        private sealed class _IComparer_222 : IComparer<iText.PdfCleanup.PdfCleanUpLocation> {
            public _IComparer_222() {
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
