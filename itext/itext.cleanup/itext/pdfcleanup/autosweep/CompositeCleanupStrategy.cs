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
using iText.IO.Util;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace iText.PdfCleanup.Autosweep {
    /// <summary>
    /// This class is a composite pattern for
    /// <c>ICleanupStrategy</c>
    /// It allows users to have multiple ICleanupStrategy implementations and bundle them as one.
    /// </summary>
    public class CompositeCleanupStrategy : ICleanupStrategy {
        private IDictionary<int, ICollection<IPdfTextLocation>> locations = new Dictionary<int, ICollection<IPdfTextLocation
            >>();

        private IList<ICleanupStrategy> strategies = new List<ICleanupStrategy>();

        public CompositeCleanupStrategy() {
        }

        public virtual void Add(ICleanupStrategy ies) {
            strategies.Add(ies);
        }

        public virtual ICollection<IPdfTextLocation> GetResultantLocations() {
            locations.Clear();
            // build return value
            ICollection<IPdfTextLocation> retval = new LinkedHashSet<IPdfTextLocation>();
            for (int i = 0; i < strategies.Count; i++) {
                ILocationExtractionStrategy s = strategies[i];
                ICollection<IPdfTextLocation> rects = s.GetResultantLocations();
                retval.AddAll(rects);
                locations.Put(i, new HashSet<IPdfTextLocation>(rects));
            }
            IList<IPdfTextLocation> rectangles = new List<IPdfTextLocation>(retval);
            JavaCollectionsUtil.Sort(rectangles, new _IComparer_94());
            // return
            return rectangles;
        }

        private sealed class _IComparer_94 : IComparer<IPdfTextLocation> {
            public _IComparer_94() {
            }

            public int Compare(IPdfTextLocation l1, IPdfTextLocation l2) {
                Rectangle r1 = l1.GetRectangle();
                Rectangle r2 = l2.GetRectangle();
                if (r1.GetY() == r2.GetY()) {
                    return r1.GetX() == r2.GetX() ? 0 : (r1.GetX() < r2.GetX() ? -1 : 1);
                }
                else {
                    return r1.GetY() < r2.GetY() ? -1 : 1;
                }
            }
        }

        public virtual Color GetRedactionColor(IPdfTextLocation location) {
            for (int i = 0; i < strategies.Count; i++) {
                if (locations.Get(i).Contains(location)) {
                    return strategies[i].GetRedactionColor(location);
                }
            }
            return ColorConstants.BLACK;
        }

        public virtual void EventOccurred(IEventData data, EventType type) {
            foreach (ILocationExtractionStrategy s in strategies) {
                s.EventOccurred(data, type);
            }
        }

        public virtual ICollection<EventType> GetSupportedEvents() {
            ICollection<EventType> evts = new HashSet<EventType>();
            foreach (ILocationExtractionStrategy s in strategies) {
                ICollection<EventType> se = s.GetSupportedEvents();
                if (se != null) {
                    evts.AddAll(se);
                }
            }
            return evts.IsEmpty() ? null : evts;
        }

        public virtual ICleanupStrategy Reset() {
            iText.PdfCleanup.Autosweep.CompositeCleanupStrategy retval = new iText.PdfCleanup.Autosweep.CompositeCleanupStrategy
                ();
            foreach (ICleanupStrategy s in strategies) {
                retval.Add(s.Reset());
            }
            return retval;
        }
    }
}
