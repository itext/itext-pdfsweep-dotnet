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
using iText.IO.Util;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;

namespace iText.PdfCleanup {
    /// <summary>Represents the line dash pattern.</summary>
    /// <remarks>
    /// Represents the line dash pattern. The line dash pattern shall control the pattern
    /// of dashes and gaps used to stroke paths. It shall be specified by a dash array and
    /// a dash phase.
    /// </remarks>
    public class LineDashPattern {
        private PdfArray dashArray;

        private float dashPhase;

        private int currentIndex;

        private int elemOrdinalNumber = 1;

        private LineDashPattern.DashArrayElem currentElem;

        /// <summary>
        /// Creates new
        /// <see cref="LineDashPattern"/>
        /// object.
        /// </summary>
        /// <param name="dashArray">
        /// The dash array. See
        /// <see cref="GetDashArray()"/>
        /// </param>
        /// <param name="dashPhase">
        /// The dash phase. See
        /// <see cref="GetDashPhase()"/>
        /// </param>
        public LineDashPattern(PdfArray dashArray, float dashPhase) {
            this.dashArray = new PdfArray(dashArray);
            this.dashPhase = dashPhase;
            InitFirst(dashPhase);
        }

        /// <summary>Getter for the dash array.</summary>
        /// <remarks>
        /// Getter for the dash array.
        /// <para />
        /// The dash array’s elements is number that specify the lengths of
        /// alternating dashes and gaps; the numbers are nonnegative. The
        /// elements are expressed in user space units.
        /// </remarks>
        /// <returns>The dash array.</returns>
        public virtual PdfArray GetDashArray() {
            return dashArray;
        }

        /// <summary>Setter for the dash array.</summary>
        /// <remarks>
        /// Setter for the dash array. See
        /// <see cref="GetDashArray()"/>
        /// </remarks>
        /// <param name="dashArray">New dash array.</param>
        public virtual void SetDashArray(PdfArray dashArray) {
            this.dashArray = dashArray;
        }

        /// <summary>Getter for the dash phase.</summary>
        /// <remarks>
        /// Getter for the dash phase.
        /// <para />
        /// The dash phase shall specify the distance into the dash pattern at which
        /// to start the dash. The elements are expressed in user space units.
        /// </remarks>
        /// <returns>The dash phase.</returns>
        public virtual float GetDashPhase() {
            return dashPhase;
        }

        /// <summary>Setter for the dash phase.</summary>
        /// <remarks>
        /// Setter for the dash phase. See
        /// <see cref="GetDashArray()"/>
        /// </remarks>
        /// <param name="dashPhase">New dash phase.</param>
        public virtual void SetDashPhase(float dashPhase) {
            this.dashPhase = dashPhase;
        }

        /// <summary>Calculates and returns the next element which is either gap or dash.</summary>
        /// <returns>The next dash array's element.</returns>
        private LineDashPattern.DashArrayElem Next() {
            LineDashPattern.DashArrayElem ret = currentElem;
            if (dashArray.Size() > 0) {
                currentIndex = (currentIndex + 1) % dashArray.Size();
                currentElem = new LineDashPattern.DashArrayElem(this, dashArray.GetAsNumber(currentIndex).FloatValue(), IsEven
                    (++elemOrdinalNumber));
            }
            return ret;
        }

        /// <summary>
        /// Resets the dash array so that the
        /// <see cref="Next()"/>
        /// method will start
        /// from the beginning of the dash array.
        /// </summary>
        private void Reset() {
            currentIndex = 0;
            elemOrdinalNumber = 1;
            InitFirst(dashPhase);
        }

        /// <summary>Checks whether the dashed pattern is solid or not.</summary>
        /// <remarks>
        /// Checks whether the dashed pattern is solid or not. It's solid when the
        /// size of a dash array is even and sum of all the units off in the array
        /// is 0.
        /// For example: [3 0 4 0 5 0 6 0] (sum is 0), [3 0 4 0 5 1] (sum is 1).
        /// </remarks>
        /// <returns>is the dashed pattern solid or not</returns>
        public virtual bool IsSolid() {
            if (dashArray.Size() % 2 != 0) {
                return false;
            }
            float unitsOffSum = 0;
            for (int i = 1; i < dashArray.Size(); i += 2) {
                unitsOffSum += dashArray.GetAsNumber(i).FloatValue();
            }
            return JavaUtil.FloatCompare(unitsOffSum, 0) == 0;
        }

        private void InitFirst(float phase) {
            if (dashArray.Size() > 0) {
                while (phase > 0) {
                    phase -= dashArray.GetAsNumber(currentIndex).FloatValue();
                    currentIndex = (currentIndex + 1) % dashArray.Size();
                    elemOrdinalNumber++;
                }
                if (phase < 0) {
                    --elemOrdinalNumber;
                    --currentIndex;
                    currentElem = new LineDashPattern.DashArrayElem(this, -phase, IsEven(elemOrdinalNumber));
                }
                else {
                    currentElem = new LineDashPattern.DashArrayElem(this, dashArray.GetAsNumber(currentIndex).FloatValue(), IsEven
                        (elemOrdinalNumber));
                }
            }
        }

        /// <summary>Return whether or not a given number is even</summary>
        /// <param name="num">input number</param>
        /// <returns>true if the input number is even, false otherwise</returns>
        private bool IsEven(int num) {
            return (num % 2) == 0;
        }

        /// <summary>Class representing a single element of a dash array</summary>
        public class DashArrayElem {
            private float val;

            private bool isGap;

            /// <summary>Construct a new DashArrayElem object</summary>
            /// <param name="val">the length of the dash array element</param>
            /// <param name="isGap">whether this element indicates a gap, or a stroke</param>
            internal DashArrayElem(LineDashPattern _enclosing, float val, bool isGap) {
                this._enclosing = _enclosing;
                this.val = val;
                this.isGap = isGap;
            }

            internal virtual float GetVal() {
                return this.val;
            }

            internal virtual void SetVal(float val) {
                this.val = val;
            }

            internal virtual bool IsGap() {
                return this.isGap;
            }

            internal virtual void SetGap(bool isGap) {
                this.isGap = isGap;
            }

            private readonly LineDashPattern _enclosing;
        }

        /// <summary>Apply a LineDashPattern along a Path</summary>
        /// <param name="path">input path</param>
        /// <param name="lineDashPattern">input LineDashPattern</param>
        /// <returns>a dashed Path</returns>
        public static Path ApplyDashPattern(Path path, LineDashPattern lineDashPattern) {
            ICollection<int> modifiedSubpaths = new HashSet<int>(path.ReplaceCloseWithLine());
            Path dashedPath = new Path();
            int currentSubpath = 0;
            foreach (Subpath subpath in path.GetSubpaths()) {
                IList<Point> subpathApprox = subpath.GetPiecewiseLinearApproximation();
                if (subpathApprox.Count > 1) {
                    dashedPath.MoveTo((float)subpathApprox[0].GetX(), (float)subpathApprox[0].GetY());
                    float remainingDist = 0;
                    bool remainingIsGap = false;
                    for (int i = 1; i < subpathApprox.Count; ++i) {
                        Point nextPoint = null;
                        if (remainingDist != 0) {
                            nextPoint = GetNextPoint(subpathApprox[i - 1], subpathApprox[i], remainingDist);
                            remainingDist = ApplyDash(dashedPath, subpathApprox[i - 1], subpathApprox[i], nextPoint, remainingIsGap);
                        }
                        while (JavaUtil.FloatCompare(remainingDist, 0) == 0 && !dashedPath.GetCurrentPoint().Equals(subpathApprox[
                            i])) {
                            LineDashPattern.DashArrayElem currentElem = lineDashPattern.Next();
                            nextPoint = GetNextPoint(nextPoint != null ? nextPoint : subpathApprox[i - 1], subpathApprox[i], currentElem
                                .GetVal());
                            remainingDist = ApplyDash(dashedPath, subpathApprox[i - 1], subpathApprox[i], nextPoint, currentElem.IsGap
                                ());
                            remainingIsGap = currentElem.IsGap();
                        }
                    }
                    // If true, then the line closing the subpath was explicitly added (see Path.ReplaceCloseWithLine).
                    // This causes a loss of a visual effect of line join style parameter, so in this clause
                    // we simply add overlapping dash (or gap, no matter), which continues the last dash and equals to
                    // the first dash (or gap) of the path.
                    if (modifiedSubpaths.Contains(currentSubpath)) {
                        lineDashPattern.Reset();
                        LineDashPattern.DashArrayElem currentElem = lineDashPattern.Next();
                        Point nextPoint = GetNextPoint(subpathApprox[0], subpathApprox[1], currentElem.GetVal());
                        ApplyDash(dashedPath, subpathApprox[0], subpathApprox[1], nextPoint, currentElem.IsGap());
                    }
                }
                // According to PDF spec. line dash pattern should be restarted for each new subpath.
                lineDashPattern.Reset();
                ++currentSubpath;
            }
            return dashedPath;
        }

        private static Point GetNextPoint(Point segStart, Point segEnd, float dist) {
            Point vector = ComponentwiseDiff(segEnd, segStart);
            Point unitVector = GetUnitVector(vector);
            return new Point(segStart.GetX() + dist * unitVector.GetX(), segStart.GetY() + dist * unitVector.GetY());
        }

        /// <summary>Returns the componentwise difference between two vectors</summary>
        /// <param name="minuend">first vector</param>
        /// <param name="subtrahend">second vector</param>
        /// <returns>first vector .- second vector</returns>
        private static Point ComponentwiseDiff(Point minuend, Point subtrahend) {
            return new Point(minuend.GetX() - subtrahend.GetX(), minuend.GetY() - subtrahend.GetY());
        }

        /// <summary>Construct a unit vector from a given vector</summary>
        /// <param name="vector">input vector</param>
        /// <returns>a vector of length 1, with the same orientation as the original vector</returns>
        private static Point GetUnitVector(Point vector) {
            double vectorLength = GetVectorEuclideanNorm(vector);
            return new Point(vector.GetX() / vectorLength, vector.GetY() / vectorLength);
        }

        /// <summary>Returns the Euclidean vector norm.</summary>
        /// <remarks>
        /// Returns the Euclidean vector norm.
        /// This is the Euclidean distance between the tip of the vector and the origin.
        /// </remarks>
        /// <param name="vector">input vector</param>
        private static double GetVectorEuclideanNorm(Point vector) {
            return vector.Distance(0, 0);
        }

        private static float ApplyDash(Path dashedPath, Point segStart, Point segEnd, Point dashTo, bool isGap) {
            float remainingDist = 0;
            if (!LiesOnSegment(segStart, segEnd, dashTo)) {
                remainingDist = (float)dashTo.Distance(segEnd);
                dashTo = segEnd;
            }
            if (isGap) {
                dashedPath.MoveTo((float)dashTo.GetX(), (float)dashTo.GetY());
            }
            else {
                dashedPath.LineTo((float)dashTo.GetX(), (float)dashTo.GetY());
            }
            return remainingDist;
        }

        /// <summary>Returns whether a given point lies on a line-segment specified by start and end point</summary>
        /// <param name="segStart">start of the line segment</param>
        /// <param name="segEnd">end of the line segment</param>
        /// <param name="point">query point</param>
        private static bool LiesOnSegment(Point segStart, Point segEnd, Point point) {
            return point.GetX() >= Math.Min(segStart.GetX(), segEnd.GetX()) && point.GetX() <= Math.Max(segStart.GetX(
                ), segEnd.GetX()) && point.GetY() >= Math.Min(segStart.GetY(), segEnd.GetY()) && point.GetY() <= Math.
                Max(segStart.GetY(), segEnd.GetY());
        }
    }
}
