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
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace iText.PdfCleanup.Autosweep {
    /// <summary>
    /// This class is a composite pattern for
    /// <see cref="ICleanupStrategy"/>.
    /// </summary>
    /// <remarks>
    /// This class is a composite pattern for
    /// <see cref="ICleanupStrategy"/>.
    /// It allows users to have multiple ICleanupStrategy implementations and bundle them as one.
    /// </remarks>
    public class CompositeCleanupStrategy : ICleanupStrategy {
        private IDictionary<int, ICollection<IPdfTextLocation>> locations = new Dictionary<int, ICollection<IPdfTextLocation
            >>();

        private IList<ICleanupStrategy> strategies = new List<ICleanupStrategy>();

        /// <summary>
        /// Creates a
        /// <see cref="CompositeCleanupStrategy">composite pattern</see>
        /// for
        /// <see cref="ICleanupStrategy">cleanup strategies</see>.
        /// </summary>
        public CompositeCleanupStrategy() {
        }

        /// <summary>
        /// Adds a
        /// <see cref="ICleanupStrategy">cleanup strategy</see>
        /// to this
        /// <see cref="CompositeCleanupStrategy">composite pattern</see>.
        /// </summary>
        /// <param name="strategy">
        /// a
        /// <see cref="ICleanupStrategy">cleanup strategy</see>
        /// to be added to this
        /// <see cref="CompositeCleanupStrategy">composite pattern</see>.
        /// </param>
        public virtual void Add(ICleanupStrategy strategy) {
            strategies.Add(strategy);
        }

        /// <summary><inheritDoc/></summary>
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
            JavaCollectionsUtil.Sort(rectangles, new _IComparer_86());
            return rectangles;
        }

        private sealed class _IComparer_86 : IComparer<IPdfTextLocation> {
            public _IComparer_86() {
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

        /// <summary><inheritDoc/></summary>
        public virtual Color GetRedactionColor(IPdfTextLocation location) {
            for (int i = 0; i < strategies.Count; i++) {
                if (locations.Get(i).Contains(location)) {
                    return strategies[i].GetRedactionColor(location);
                }
            }
            return ColorConstants.BLACK;
        }

        /// <summary><inheritDoc/></summary>
        public virtual void EventOccurred(IEventData data, EventType type) {
            foreach (ILocationExtractionStrategy s in strategies) {
                s.EventOccurred(data, type);
            }
        }

        /// <summary><inheritDoc/></summary>
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

        /// <summary>
        /// Returns a
        /// <see cref="ICleanupStrategy">cleanup strategy</see>
        /// which represents
        /// a reset
        /// <see cref="CompositeCleanupStrategy">composite cleanup strategy</see>.
        /// </summary>
        /// <remarks>
        /// Returns a
        /// <see cref="ICleanupStrategy">cleanup strategy</see>
        /// which represents
        /// a reset
        /// <see cref="CompositeCleanupStrategy">composite cleanup strategy</see>.
        /// <para />
        /// Note that all the inner
        /// <see cref="ICleanupStrategy">strategies</see>
        /// will be reset as well.
        /// </remarks>
        /// <returns>
        /// a reset
        /// <see cref="CompositeCleanupStrategy">composite strategy</see>
        /// </returns>
        public virtual ICleanupStrategy Reset() {
            iText.PdfCleanup.Autosweep.CompositeCleanupStrategy resetCompositeStrategy = new iText.PdfCleanup.Autosweep.CompositeCleanupStrategy
                ();
            foreach (ICleanupStrategy s in strategies) {
                resetCompositeStrategy.Add(s.Reset());
            }
            return resetCompositeStrategy;
        }
    }
}
