/*
This file is part of the iText (R) project.
Copyright (c) 1998-2023 iText Group NV
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
using iText.Commons.Actions;
using iText.Commons.Actions.Confirmations;
using iText.Commons.Actions.Contexts;
using iText.Commons.Actions.Sequence;
using iText.PdfCleanup.Actions.Data;

namespace iText.PdfCleanup.Actions.Event {
    /// <summary>Class represents events registered in iText cleanup module.</summary>
    public class PdfSweepProductEvent : AbstractProductProcessITextEvent {
        /// <summary>Cleanup event type description.</summary>
        public const String CLEANUP_PDF = "cleanup-pdf";

        private readonly String eventType;

        /// <summary>Creates an event associated with a general identifier and additional meta data.</summary>
        /// <param name="sequenceId">is an identifier associated with the event</param>
        /// <param name="metaInfo">is an additional meta info</param>
        /// <param name="eventType">is a string description of the event</param>
        private PdfSweepProductEvent(SequenceId sequenceId, IMetaInfo metaInfo, String eventType)
            : base(sequenceId, PdfSweepProductData.GetInstance(), metaInfo, EventConfirmationType.ON_CLOSE) {
            this.eventType = eventType;
        }

        /// <summary>Creates a cleanup-pdf event which associated with a general identifier and additional meta data.</summary>
        /// <param name="sequenceId">is an identifier associated with the event</param>
        /// <param name="metaInfo">is an additional meta info</param>
        /// <returns>the cleanup-pdf event</returns>
        public static iText.PdfCleanup.Actions.Event.PdfSweepProductEvent CreateCleanupPdfEvent(SequenceId sequenceId
            , IMetaInfo metaInfo) {
            return new iText.PdfCleanup.Actions.Event.PdfSweepProductEvent(sequenceId, metaInfo, CLEANUP_PDF);
        }

        public override String GetEventType() {
            return eventType;
        }
    }
}
