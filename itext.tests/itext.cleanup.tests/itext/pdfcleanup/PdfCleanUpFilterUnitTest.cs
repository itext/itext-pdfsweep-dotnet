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
using iText.Kernel.Geom;
using iText.Test;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("UnitTest")]
    public class PdfCleanUpFilterUnitTest : ExtendedITextTest {
        [NUnit.Framework.Test]
        public virtual void PointIntersectLineCaseTest1() {
            Point[] intersectSubject = new Point[] { new Point(50, 60), new Point(70, 60), new Point(50, 60) };
            Point[] intersecting = new Point[] { new Point(50, 50), new Point(50, 70), new Point(50, 50) };
            PdfCleanUpFilter filter = new PdfCleanUpFilter(new List<Rectangle>(), new CleanUpProperties());
            NUnit.Framework.Assert.IsTrue(filter.CheckIfRectanglesIntersect(intersectSubject, intersecting));
        }

        [NUnit.Framework.Test]
        public virtual void PointIntersectLineCaseTest2() {
            Point[] intersectSubject = new Point[] { new Point(50, 60), new Point(70, 60), new Point(50, 60) };
            Point[] intersecting = new Point[] { new Point(50, 50), new Point(50, 30), new Point(50, 50) };
            PdfCleanUpFilter filter = new PdfCleanUpFilter(new List<Rectangle>(), new CleanUpProperties());
            NUnit.Framework.Assert.IsFalse(filter.CheckIfRectanglesIntersect(intersectSubject, intersecting));
        }

        [NUnit.Framework.Test]
        public virtual void PointIntersectLineCaseTest3() {
            Point[] intersectSubject = new Point[] { new Point(50, 65), new Point(70, 65), new Point(50, 65) };
            Point[] intersecting = new Point[] { new Point(40, 50), new Point(60, 70), new Point(40, 50) };
            PdfCleanUpFilter filter = new PdfCleanUpFilter(new List<Rectangle>(), new CleanUpProperties());
            NUnit.Framework.Assert.IsFalse(filter.CheckIfRectanglesIntersect(intersectSubject, intersecting));
        }

        [NUnit.Framework.Test]
        public virtual void PointIntersectLineCaseTest4() {
            Point[] intersectSubject = new Point[] { new Point(50, 60), new Point(70, 60), new Point(50, 60) };
            Point[] intersecting = new Point[] { new Point(30, 50), new Point(70, 70), new Point(30, 50) };
            PdfCleanUpFilter filter = new PdfCleanUpFilter(new List<Rectangle>(), new CleanUpProperties());
            NUnit.Framework.Assert.IsTrue(filter.CheckIfRectanglesIntersect(intersectSubject, intersecting));
        }

        [NUnit.Framework.Test]
        public virtual void PointIntersectLineCaseTest5() {
            Point[] intersectSubject = new Point[] { new Point(50, 60), new Point(70, 60), new Point(50, 60) };
            Point[] intersecting = new Point[] { new Point(70, 50), new Point(30, 70), new Point(70, 50) };
            PdfCleanUpFilter filter = new PdfCleanUpFilter(new List<Rectangle>(), new CleanUpProperties());
            NUnit.Framework.Assert.IsTrue(filter.CheckIfRectanglesIntersect(intersectSubject, intersecting));
        }

        [NUnit.Framework.Test]
        public virtual void CheckIfRectanglesIntersect_completelyCoveredBasic() {
            Point[] intersectSubject = new Point[] { new Point(70, 70), new Point(80, 70), new Point(80, 80), new Point
                (70, 80) };
            Point[] intersecting = new Point[] { new Point(50, 50), new Point(100, 50), new Point(100, 100), new Point
                (50, 100) };
            PdfCleanUpFilter filter = new PdfCleanUpFilter(new List<Rectangle>(), new CleanUpProperties());
            NUnit.Framework.Assert.IsTrue(filter.CheckIfRectanglesIntersect(intersectSubject, intersecting));
        }

        [NUnit.Framework.Test]
        public virtual void CheckIfRectanglesIntersect_completelyCoveredDegenerateWidth() {
            Point[] intersectSubject = new Point[] { new Point(70, 70), new Point(70, 70), new Point(70, 80), new Point
                (70, 80) };
            Point[] intersecting = new Point[] { new Point(50, 50), new Point(100, 50), new Point(100, 100), new Point
                (50, 100) };
            NUnit.Framework.Assert.IsTrue(new PdfCleanUpFilter(new List<Rectangle>(), new CleanUpProperties()).CheckIfRectanglesIntersect
                (intersectSubject, intersecting));
        }

        [NUnit.Framework.Test]
        public virtual void CheckIfRectanglesIntersect_completelyCoveredDegenerateHeight() {
            Point[] intersectSubject = new Point[] { new Point(70, 70), new Point(80, 70), new Point(80, 70), new Point
                (70, 70) };
            Point[] intersecting = new Point[] { new Point(50, 50), new Point(100, 50), new Point(100, 100), new Point
                (50, 100) };
            NUnit.Framework.Assert.IsTrue(new PdfCleanUpFilter(new List<Rectangle>(), new CleanUpProperties()).CheckIfRectanglesIntersect
                (intersectSubject, intersecting));
        }

        [NUnit.Framework.Test]
        public virtual void CheckIfRectanglesIntersect_completelyCoveredDegeneratePoint() {
            Point[] intersectSubject = new Point[] { new Point(70, 70), new Point(70, 70), new Point(70, 70), new Point
                (70, 70) };
            Point[] intersecting = new Point[] { new Point(50, 50), new Point(100, 50), new Point(100, 100), new Point
                (50, 100) };
            NUnit.Framework.Assert.IsTrue(new PdfCleanUpFilter(new List<Rectangle>(), new CleanUpProperties()).CheckIfRectanglesIntersect
                (intersectSubject, intersecting));
        }
    }
}
