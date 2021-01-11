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
using iText.Kernel;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace iText.PdfCleanup {
    public class PdfCleanUpEventListener : IEventListener {
        private const String textDataExpected = "Text data expected.";

        private const String imageDataExpected = "Image data expected.";

        private const String pathDataExpected = "Path data expected.";

        private IList<IEventData> content = new List<IEventData>();

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

        public virtual ICollection<EventType> GetSupportedEvents() {
            return null;
        }
    }
}
