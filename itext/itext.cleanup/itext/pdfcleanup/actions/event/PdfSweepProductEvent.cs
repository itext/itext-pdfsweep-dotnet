/*
This file is part of the iText (R) project.
Copyright (c) 1998-2023 Apryse Group NV
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
