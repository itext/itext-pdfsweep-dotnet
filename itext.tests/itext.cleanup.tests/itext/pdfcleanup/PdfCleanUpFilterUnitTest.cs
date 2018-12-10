using iText.Kernel.Geom;

namespace iText.PdfCleanup {
    public class PdfCleanUpFilterUnitTest {
        [NUnit.Framework.Test]
        public virtual void CheckIfRectanglesIntersect_completelyCoveredBasic() {
            Point[] intersectSubject = new Point[] { new Point(70, 70), new Point(80, 70), new Point(80, 80), new Point
                (70, 80) };
            Point[] intersecting = new Point[] { new Point(50, 50), new Point(100, 50), new Point(100, 100), new Point
                (50, 100) };
            NUnit.Framework.Assert.IsTrue(PdfCleanUpFilter.CheckIfRectanglesIntersect(intersectSubject, intersecting));
        }

        [NUnit.Framework.Test]
        public virtual void CheckIfRectanglesIntersect_completelyCoveredDegenerateWidth() {
            Point[] intersectSubject = new Point[] { new Point(70, 70), new Point(70, 70), new Point(70, 80), new Point
                (70, 80) };
            Point[] intersecting = new Point[] { new Point(50, 50), new Point(100, 50), new Point(100, 100), new Point
                (50, 100) };
            NUnit.Framework.Assert.IsTrue(PdfCleanUpFilter.CheckIfRectanglesIntersect(intersectSubject, intersecting));
        }

        [NUnit.Framework.Test]
        public virtual void CheckIfRectanglesIntersect_completelyCoveredDegenerateHeight() {
            Point[] intersectSubject = new Point[] { new Point(70, 70), new Point(80, 70), new Point(80, 70), new Point
                (70, 70) };
            Point[] intersecting = new Point[] { new Point(50, 50), new Point(100, 50), new Point(100, 100), new Point
                (50, 100) };
            NUnit.Framework.Assert.IsTrue(PdfCleanUpFilter.CheckIfRectanglesIntersect(intersectSubject, intersecting));
        }

        [NUnit.Framework.Test]
        public virtual void CheckIfRectanglesIntersect_completelyCoveredDegeneratePoint() {
            Point[] intersectSubject = new Point[] { new Point(70, 70), new Point(70, 70), new Point(70, 70), new Point
                (70, 70) };
            Point[] intersecting = new Point[] { new Point(50, 50), new Point(100, 50), new Point(100, 100), new Point
                (50, 100) };
            NUnit.Framework.Assert.IsTrue(PdfCleanUpFilter.CheckIfRectanglesIntersect(intersectSubject, intersecting));
        }
    }
}
