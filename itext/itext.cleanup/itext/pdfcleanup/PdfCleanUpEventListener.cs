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
using System.Collections.Generic;
using iText.Kernel.Exceptions;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace iText.PdfCleanup {
    /// <summary>An event listener which handles cleanup related events.</summary>
    public class PdfCleanUpEventListener : IEventListener {
        private const String textDataExpected = "Text data expected.";

        private const String imageDataExpected = "Image data expected.";

        private const String pathDataExpected = "Path data expected.";

        private IList<IEventData> content = new List<IEventData>();

        /// <summary><inheritDoc/></summary>
        public virtual void EventOccurred(IEventData data, EventType type) {
            switch (type) {
                case EventType.RENDER_TEXT:
                case EventType.RENDER_IMAGE:
                case EventType.RENDER_PATH: {
                    content.Add(data);
                    break;
                }

                default: {
                    break;
                }
            }
        }

        /// <summary>Get the last encountered TextRenderInfo objects, then clears the internal buffer</summary>
        /// <returns>the TextRenderInfo objects that were encountered when processing the last text rendering operation
        ///     </returns>
        internal virtual IList<TextRenderInfo> GetEncounteredText() {
            if (content.Count == 0) {
                throw new PdfException(textDataExpected);
            }
            List<TextRenderInfo> text = new List<TextRenderInfo>(content.Count);
            foreach (IEventData data in content) {
                if (data is TextRenderInfo) {
                    text.Add((TextRenderInfo)data);
                }
                else {
                    throw new PdfException(textDataExpected);
                }
            }
            content.Clear();
            return text;
        }

        /// <summary>Get the last encountered ImageRenderInfo, then clears the internal buffer</summary>
        /// <returns>the ImageRenderInfo object that was encountered when processing the last image rendering operation
        ///     </returns>
        internal virtual ImageRenderInfo GetEncounteredImage() {
            if (content.Count == 0) {
                throw new PdfException(imageDataExpected);
            }
            IEventData eventData = content[0];
            if (!(eventData is ImageRenderInfo)) {
                throw new PdfException(imageDataExpected);
            }
            content.Clear();
            return (ImageRenderInfo)eventData;
        }

        /// <summary>Get the last encountered PathRenderInfo, then clears the internal buffer</summary>
        /// <returns>the PathRenderInfo object that was encountered when processing the last path rendering operation</returns>
        internal virtual PathRenderInfo GetEncounteredPath() {
            if (content.Count == 0) {
                throw new PdfException(pathDataExpected);
            }
            IEventData eventData = content[0];
            if (!(eventData is PathRenderInfo)) {
                throw new PdfException(pathDataExpected);
            }
            content.Clear();
            return (PathRenderInfo)eventData;
        }

        /// <summary><inheritDoc/></summary>
        public virtual ICollection<EventType> GetSupportedEvents() {
            return null;
        }
    }
}
