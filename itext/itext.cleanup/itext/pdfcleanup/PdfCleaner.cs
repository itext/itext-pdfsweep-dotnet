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
using System.Collections.Generic;
using System.IO;
using iText.Commons.Actions.Contexts;
using iText.Commons.Utils;
using iText.Kernel.Pdf;
using iText.PdfCleanup.Autosweep;

namespace iText.PdfCleanup {
    /// <summary>Main entry point for cleaning a PDF document.</summary>
    /// <remarks>
    /// Main entry point for cleaning a PDF document.
    /// This class contains a series of static methods that accept PDF file as a
    /// <see cref="System.IO.Stream"/>
    /// or already opened
    /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
    /// and performs erasing of data in regions specified by input
    /// arguments. The output can either be preserved in passed
    /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
    /// with possibility to
    /// post-process the document, or in an
    /// <see cref="System.IO.Stream"/>
    /// in a form of a complete PDF file.
    /// <para />
    /// The important difference between overloads with InputStream/OutputStream parameters and
    /// <see cref="iText.Kernel.Pdf.PdfDocument"/>
    /// parameter is in the consumption of product license limits.
    /// </remarks>
    public sealed class PdfCleaner {
        private PdfCleaner() {
        }

        // this class is designed to be used with static methods only
        /// <summary>Cleans the document by erasing all the areas which are provided.</summary>
        /// <remarks>
        /// Cleans the document by erasing all the areas which are provided.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="inputPdf">the pdf document InputStream to which cleaned up applies</param>
        /// <param name="outputPdf">the cleaned up pdf document OutputStream</param>
        /// <param name="cleanUpLocations">list of locations to be cleaned up</param>
        public static void CleanUp(Stream inputPdf, Stream outputPdf, IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations
            ) {
            CleanUp(inputPdf, outputPdf, cleanUpLocations, new CleanUpProperties());
        }

        /// <summary>Cleans the document by erasing all the areas which are provided.</summary>
        /// <remarks>
        /// Cleans the document by erasing all the areas which are provided.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="inputPdf">the pdf document InputStream to which cleaned up applies</param>
        /// <param name="outputPdf">the cleaned up pdf document OutputStream</param>
        /// <param name="cleanUpLocations">list of locations to be cleaned up</param>
        /// <param name="properties">additional properties for cleanUp</param>
        public static void CleanUp(Stream inputPdf, Stream outputPdf, IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations
            , CleanUpProperties properties) {
            StampingProperties stampingProperties = new StampingProperties();
            IMetaInfo propertiesMetaInfo = properties.GetMetaInfo();
            stampingProperties.SetEventCountingMetaInfo(propertiesMetaInfo == null ? new PdfCleaner.CleanUpToolMetaInfo
                () : propertiesMetaInfo);
            using (PdfReader reader = new PdfReader(inputPdf)) {
                using (PdfWriter writer = new PdfWriter(outputPdf)) {
                    using (PdfDocument pdfDocument = new PdfDocument(reader, writer, stampingProperties)) {
                        CleanUp(pdfDocument, cleanUpLocations, properties);
                    }
                }
            }
        }

        /// <summary>Cleans the document by erasing all the areas which are provided.</summary>
        /// <remarks>
        /// Cleans the document by erasing all the areas which are provided.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="pdfDocument">a document to which cleaned up applies</param>
        /// <param name="cleanUpLocations">list of locations to be cleaned up</param>
        public static void CleanUp(PdfDocument pdfDocument, IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations
            ) {
            CleanUp(pdfDocument, cleanUpLocations, new CleanUpProperties());
        }

        /// <summary>Cleans the document by erasing all the areas which are provided.</summary>
        /// <remarks>
        /// Cleans the document by erasing all the areas which are provided.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="pdfDocument">a document to which cleaned up applies</param>
        /// <param name="cleanUpLocations">list of locations to be cleaned up</param>
        /// <param name="properties">additional properties for cleanUp</param>
        public static void CleanUp(PdfDocument pdfDocument, IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations
            , CleanUpProperties properties) {
            iText.PdfCleanup.PdfCleanUpTool cleanUpTool = new iText.PdfCleanup.PdfCleanUpTool(pdfDocument, cleanUpLocations
                , properties);
            cleanUpTool.CleanUp();
        }

        /// <summary>Perform cleanup of areas of interest based on a given cleanup strategy.</summary>
        /// <remarks>
        /// Perform cleanup of areas of interest based on a given cleanup strategy.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="inputPdf">the pdf document InputStream to which cleaned up applies</param>
        /// <param name="outputPdf">the cleaned up pdf document OutputStream</param>
        /// <param name="strategy">cleanup strategy to be used</param>
        public static void AutoSweepCleanUp(Stream inputPdf, Stream outputPdf, ICleanupStrategy strategy) {
            AutoSweepCleanUp(inputPdf, outputPdf, strategy, new CleanUpProperties());
        }

        /// <summary>Perform cleanup of areas of interest based on a given cleanup strategy.</summary>
        /// <remarks>
        /// Perform cleanup of areas of interest based on a given cleanup strategy.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="inputPdf">the pdf document InputStream to which cleaned up applies</param>
        /// <param name="outputPdf">the cleaned up pdf document OutputStream</param>
        /// <param name="strategy">cleanup strategy to be used</param>
        /// <param name="additionalCleanUpLocations">list of additional locations to be cleaned up</param>
        public static void AutoSweepCleanUp(Stream inputPdf, Stream outputPdf, ICleanupStrategy strategy, IList<iText.PdfCleanup.PdfCleanUpLocation
            > additionalCleanUpLocations) {
            AutoSweepCleanUp(inputPdf, outputPdf, strategy, additionalCleanUpLocations, new CleanUpProperties());
        }

        /// <summary>Perform cleanup of areas of interest based on a given cleanup strategy.</summary>
        /// <remarks>
        /// Perform cleanup of areas of interest based on a given cleanup strategy.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="inputPdf">the pdf document InputStream to which cleaned up applies</param>
        /// <param name="outputPdf">the cleaned up pdf document OutputStream</param>
        /// <param name="strategy">cleanup strategy to be used</param>
        /// <param name="properties">additional properties for cleanUp</param>
        public static void AutoSweepCleanUp(Stream inputPdf, Stream outputPdf, ICleanupStrategy strategy, CleanUpProperties
             properties) {
            AutoSweepCleanUp(inputPdf, outputPdf, strategy, JavaCollectionsUtil.EmptyList<iText.PdfCleanup.PdfCleanUpLocation
                >(), properties);
        }

        /// <summary>Perform cleanup of areas of interest based on a given cleanup strategy.</summary>
        /// <remarks>
        /// Perform cleanup of areas of interest based on a given cleanup strategy.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="inputPdf">the pdf document InputStream to which cleaned up applies</param>
        /// <param name="outputPdf">the cleaned up pdf document OutputStream</param>
        /// <param name="strategy">cleanup strategy to be used</param>
        /// <param name="additionalCleanUpLocations">list of additional locations to be cleaned up</param>
        /// <param name="properties">additional properties for cleanUp</param>
        public static void AutoSweepCleanUp(Stream inputPdf, Stream outputPdf, ICleanupStrategy strategy, IList<iText.PdfCleanup.PdfCleanUpLocation
            > additionalCleanUpLocations, CleanUpProperties properties) {
            StampingProperties stampingProperties = new StampingProperties();
            IMetaInfo propertiesMetaInfo = properties.GetMetaInfo();
            stampingProperties.SetEventCountingMetaInfo(propertiesMetaInfo == null ? new PdfCleaner.CleanUpToolMetaInfo
                () : propertiesMetaInfo);
            using (PdfReader reader = new PdfReader(inputPdf)) {
                using (PdfWriter writer = new PdfWriter(outputPdf)) {
                    using (PdfDocument pdfDocument = new PdfDocument(reader, writer, stampingProperties)) {
                        AutoSweepCleanUp(pdfDocument, strategy, additionalCleanUpLocations, properties);
                    }
                }
            }
        }

        /// <summary>Perform cleanup of areas of interest based on a given cleanup strategy.</summary>
        /// <remarks>
        /// Perform cleanup of areas of interest based on a given cleanup strategy.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="pdfDocument">a document to which cleaned up applies</param>
        /// <param name="strategy">cleanup strategy to be used</param>
        public static void AutoSweepCleanUp(PdfDocument pdfDocument, ICleanupStrategy strategy) {
            AutoSweepCleanUp(pdfDocument, strategy, new CleanUpProperties());
        }

        /// <summary>Perform cleanup of areas of interest based on a given cleanup strategy.</summary>
        /// <remarks>
        /// Perform cleanup of areas of interest based on a given cleanup strategy.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="pdfDocument">a document to which cleaned up applies</param>
        /// <param name="strategy">cleanup strategy to be used</param>
        /// <param name="properties">additional properties for cleanUp</param>
        public static void AutoSweepCleanUp(PdfDocument pdfDocument, ICleanupStrategy strategy, CleanUpProperties 
            properties) {
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new PdfAutoSweepTools(strategy).GetPdfCleanUpLocations
                (pdfDocument);
            CleanUp(pdfDocument, cleanUpLocations, properties);
        }

        /// <summary>Perform cleanup of areas of interest based on a given cleanup strategy.</summary>
        /// <remarks>
        /// Perform cleanup of areas of interest based on a given cleanup strategy.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="pdfDocument">a document to which cleaned up applies</param>
        /// <param name="strategy">cleanup strategy to be used</param>
        /// <param name="additionalCleanUpLocations">list of additional locations to be cleaned up</param>
        public static void AutoSweepCleanUp(PdfDocument pdfDocument, ICleanupStrategy strategy, IList<iText.PdfCleanup.PdfCleanUpLocation
            > additionalCleanUpLocations) {
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new PdfAutoSweepTools(strategy).GetPdfCleanUpLocations
                (pdfDocument);
            cleanUpLocations.AddAll(additionalCleanUpLocations);
            CleanUp(pdfDocument, cleanUpLocations, new CleanUpProperties());
        }

        /// <summary>Perform cleanup of areas of interest based on a given cleanup strategy.</summary>
        /// <remarks>
        /// Perform cleanup of areas of interest based on a given cleanup strategy.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="pdfDocument">a document to which cleaned up applies</param>
        /// <param name="strategy">cleanup strategy to be used</param>
        /// <param name="additionalCleanUpLocations">list of additional locations to be cleaned up</param>
        /// <param name="properties">additional properties for cleanUp</param>
        public static void AutoSweepCleanUp(PdfDocument pdfDocument, ICleanupStrategy strategy, IList<iText.PdfCleanup.PdfCleanUpLocation
            > additionalCleanUpLocations, CleanUpProperties properties) {
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new PdfAutoSweepTools(strategy).GetPdfCleanUpLocations
                (pdfDocument);
            cleanUpLocations.AddAll(additionalCleanUpLocations);
            CleanUp(pdfDocument, cleanUpLocations, properties);
        }

        /// <summary>Perform cleanup of areas of interest based on a given cleanup strategy.</summary>
        /// <remarks>
        /// Perform cleanup of areas of interest based on a given cleanup strategy.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="pdfPage">
        /// the
        /// <see cref="iText.Kernel.Pdf.PdfPage"/>
        /// to which cleaned up applies
        /// </param>
        /// <param name="strategy">cleanup strategy to be used</param>
        public static void AutoSweepCleanUp(PdfPage pdfPage, ICleanupStrategy strategy) {
            AutoSweepCleanUp(pdfPage, strategy, new CleanUpProperties());
        }

        /// <summary>Perform cleanup of areas of interest based on a given cleanup strategy.</summary>
        /// <remarks>
        /// Perform cleanup of areas of interest based on a given cleanup strategy.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="pdfPage">
        /// the
        /// <see cref="iText.Kernel.Pdf.PdfPage"/>
        /// to which cleaned up applies
        /// </param>
        /// <param name="strategy">cleanup strategy to be used</param>
        /// <param name="additionalCleanUpLocations">list of additional locations to be cleaned up</param>
        public static void AutoSweepCleanUp(PdfPage pdfPage, ICleanupStrategy strategy, IList<iText.PdfCleanup.PdfCleanUpLocation
            > additionalCleanUpLocations) {
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new PdfAutoSweepTools(strategy).GetPdfCleanUpLocations
                (pdfPage);
            cleanUpLocations.AddAll(additionalCleanUpLocations);
            CleanUp(pdfPage.GetDocument(), cleanUpLocations, new CleanUpProperties());
        }

        /// <summary>Perform cleanup of areas of interest based on a given cleanup strategy.</summary>
        /// <remarks>
        /// Perform cleanup of areas of interest based on a given cleanup strategy.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="pdfPage">
        /// the
        /// <see cref="iText.Kernel.Pdf.PdfPage"/>
        /// to which cleaned up applies
        /// </param>
        /// <param name="strategy">cleanup strategy to be used</param>
        /// <param name="properties">additional properties for cleanUp</param>
        public static void AutoSweepCleanUp(PdfPage pdfPage, ICleanupStrategy strategy, CleanUpProperties properties
            ) {
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new PdfAutoSweepTools(strategy).GetPdfCleanUpLocations
                (pdfPage);
            CleanUp(pdfPage.GetDocument(), cleanUpLocations, properties);
        }

        /// <summary>Perform cleanup of areas of interest based on a given cleanup strategy.</summary>
        /// <remarks>
        /// Perform cleanup of areas of interest based on a given cleanup strategy.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="pdfPage">
        /// the
        /// <see cref="iText.Kernel.Pdf.PdfPage"/>
        /// to which cleaned up applies
        /// </param>
        /// <param name="strategy">cleanup strategy to be used</param>
        /// <param name="additionalCleanUpLocations">list of additional locations to be cleaned up</param>
        /// <param name="properties">additional properties for cleanUp</param>
        public static void AutoSweepCleanUp(PdfPage pdfPage, ICleanupStrategy strategy, IList<iText.PdfCleanup.PdfCleanUpLocation
            > additionalCleanUpLocations, CleanUpProperties properties) {
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new PdfAutoSweepTools(strategy).GetPdfCleanUpLocations
                (pdfPage);
            cleanUpLocations.AddAll(additionalCleanUpLocations);
            CleanUp(pdfPage.GetDocument(), cleanUpLocations, properties);
        }

        /// <summary>Cleans the document by erasing regions defined by redact annotations inside the document.</summary>
        /// <remarks>
        /// Cleans the document by erasing regions defined by redact annotations inside the document.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="inputPdf">the pdf document InputStream to which cleaned up applies</param>
        /// <param name="outputPdf">the cleaned up pdf document OutputStream</param>
        public static void CleanUpRedactAnnotations(Stream inputPdf, Stream outputPdf) {
            CleanUpRedactAnnotations(inputPdf, outputPdf, new CleanUpProperties());
        }

        /// <summary>Cleans the document by erasing regions defined by redact annotations inside the document.</summary>
        /// <remarks>
        /// Cleans the document by erasing regions defined by redact annotations inside the document.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="inputPdf">the pdf document InputStream to which cleaned up applies</param>
        /// <param name="outputPdf">the cleaned up pdf document OutputStream</param>
        /// <param name="properties">additional properties for cleanUp</param>
        public static void CleanUpRedactAnnotations(Stream inputPdf, Stream outputPdf, CleanUpProperties properties
            ) {
            CleanUpRedactAnnotations(inputPdf, outputPdf, null, properties);
        }

        /// <summary>Cleans the document by erasing regions defined by redact annotations inside the document.</summary>
        /// <remarks>
        /// Cleans the document by erasing regions defined by redact annotations inside the document.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="pdfDocument">a document to which cleaned up applies</param>
        public static void CleanUpRedactAnnotations(PdfDocument pdfDocument) {
            CleanUpRedactAnnotations(pdfDocument, null, new CleanUpProperties());
        }

        /// <summary>Cleans the document by erasing regions defined by redact annotations inside the document.</summary>
        /// <remarks>
        /// Cleans the document by erasing regions defined by redact annotations inside the document.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="pdfDocument">a document to which cleaned up applies</param>
        /// <param name="properties">additional properties for cleanUp</param>
        public static void CleanUpRedactAnnotations(PdfDocument pdfDocument, CleanUpProperties properties) {
            CleanUpRedactAnnotations(pdfDocument, null, properties);
        }

        /// <summary>
        /// Cleans the document by erasing regions defined by redact annotations and additional cleanup locations inside the
        /// document.
        /// </summary>
        /// <remarks>
        /// Cleans the document by erasing regions defined by redact annotations and additional cleanup locations inside the
        /// document.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="inputPdf">the pdf document InputStream to which cleaned up applies</param>
        /// <param name="outputPdf">the cleaned up pdf document OutputStream</param>
        /// <param name="additionalCleanUpLocations">list of locations to be cleaned up</param>
        public static void CleanUpRedactAnnotations(Stream inputPdf, Stream outputPdf, IList<iText.PdfCleanup.PdfCleanUpLocation
            > additionalCleanUpLocations) {
            CleanUpRedactAnnotations(inputPdf, outputPdf, additionalCleanUpLocations, new CleanUpProperties());
        }

        /// <summary>
        /// Cleans the document by erasing regions defined by redact annotations and additional cleanup locations inside the
        /// document.
        /// </summary>
        /// <remarks>
        /// Cleans the document by erasing regions defined by redact annotations and additional cleanup locations inside the
        /// document.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="pdfDocument">a document to which cleaned up applies</param>
        /// <param name="additionalCleanUpLocations">list of locations to be cleaned up</param>
        public static void CleanUpRedactAnnotations(PdfDocument pdfDocument, IList<iText.PdfCleanup.PdfCleanUpLocation
            > additionalCleanUpLocations) {
            CleanUpRedactAnnotations(pdfDocument, additionalCleanUpLocations, new CleanUpProperties());
        }

        /// <summary>Cleans the document by erasing regions defined by redact annotations inside the document.</summary>
        /// <remarks>
        /// Cleans the document by erasing regions defined by redact annotations inside the document.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="inputPdf">the pdf document InputStream to which cleaned up applies</param>
        /// <param name="outputPdf">the cleaned up pdf document OutputStream</param>
        /// <param name="additionalCleanUpLocations">list of locations to be cleaned up</param>
        /// <param name="properties">additional properties for cleanUp</param>
        public static void CleanUpRedactAnnotations(Stream inputPdf, Stream outputPdf, IList<iText.PdfCleanup.PdfCleanUpLocation
            > additionalCleanUpLocations, CleanUpProperties properties) {
            StampingProperties stampingProperties = new StampingProperties();
            IMetaInfo propertiesMetaInfo = properties.GetMetaInfo();
            stampingProperties.SetEventCountingMetaInfo(propertiesMetaInfo == null ? new PdfCleaner.CleanUpToolMetaInfo
                () : propertiesMetaInfo);
            using (PdfReader reader = new PdfReader(inputPdf)) {
                using (PdfWriter writer = new PdfWriter(outputPdf)) {
                    using (PdfDocument pdfDocument = new PdfDocument(reader, writer, stampingProperties)) {
                        CleanUpRedactAnnotations(pdfDocument, additionalCleanUpLocations, properties);
                    }
                }
            }
        }

        /// <summary>Cleans the document by erasing regions defined by redact annotations inside the document.</summary>
        /// <remarks>
        /// Cleans the document by erasing regions defined by redact annotations inside the document.
        /// Note, use methods with InputStream/OutputStream params if you don't want to consume itext-core product license
        /// limits.
        /// </remarks>
        /// <param name="pdfDocument">a document to which cleaned up applies</param>
        /// <param name="additionalCleanUpLocations">list of locations to be cleaned up</param>
        /// <param name="properties">additional properties for cleanUp</param>
        public static void CleanUpRedactAnnotations(PdfDocument pdfDocument, IList<iText.PdfCleanup.PdfCleanUpLocation
            > additionalCleanUpLocations, CleanUpProperties properties) {
            iText.PdfCleanup.PdfCleanUpTool cleanUpTool = new iText.PdfCleanup.PdfCleanUpTool(pdfDocument, true, properties
                );
            if (additionalCleanUpLocations != null) {
                foreach (iText.PdfCleanup.PdfCleanUpLocation cleanUpLocation in additionalCleanUpLocations) {
                    cleanUpTool.AddCleanupLocation(cleanUpLocation);
                }
            }
            cleanUpTool.CleanUp();
        }

        internal class CleanUpToolMetaInfo : IMetaInfo {
        }
    }
}
