/*
This file is part of the iText (R) project.
Copyright (c) 1998-2019 iText Group NV
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Common.Logging;
using iText.IO.Image;
using iText.IO.Util;
using iText.Kernel;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser.ClipperLib;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using Path = iText.Kernel.Geom.Path;
using Point = iText.Kernel.Geom.Point;
using Rectangle = iText.Kernel.Geom.Rectangle;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<iText.Kernel.Pdf.Canvas.Parser.ClipperLib.IntPoint>>;

namespace iText.PdfCleanup {
    public class PdfCleanUpFilter {
        private static readonly Color? CLEANED_AREA_FILL_COLOR = Color.White;

        private static float EPS = 1e-4f;

        /* There is no exact representation of the circle using Bezier curves.
         * But, for a Bezier curve with n segments the optimal distance to the control points,
         * in the sense that the middle of the curve lies on the circle itself, is (4/3) * tan(pi / (2*n))
         * So for 4 points it is (4/3) * tan(pi/8) = 4 * (sqrt(2)-1)/3 = 0.5522847498
         * In this approximation, the B�zier curve always falls outside the circle,
         * except momentarily when it dips in to touch the circle at the midpoint and endpoints.
         * However, a better approximation is possible using 0.55191502449
         */
        private const double CIRCLE_APPROXIMATION_CONST = 0.55191502449;

        private IList<Rectangle> regions;

        public PdfCleanUpFilter(IList<Rectangle> regions) {
            this.regions = regions;
        }

        /// <summary>Filter a TextRenderInfo object</summary>
        /// <param name="text">the TextRenderInfo to be filtered</param>
        public virtual PdfCleanUpFilter.FilterResult<PdfArray> FilterText(TextRenderInfo text) {
            PdfTextArray textArray = new PdfTextArray();
            if (IsTextNotToBeCleaned(text)) {
                return new PdfCleanUpFilter.FilterResult<PdfArray>(false, new PdfArray(text.GetPdfString()));
            }

            foreach (TextRenderInfo ri in text.GetCharacterRenderInfos()) {
                if (IsTextNotToBeCleaned(ri)) {
                    textArray.Add(ri.GetPdfString());
                } else {
                    textArray.Add(new PdfNumber(-ri.GetUnscaledWidth() * 1000f /
                                                (text.GetFontSize() * text.GetHorizontalScaling
                                                     () / 100)));
                }
            }

            return new PdfCleanUpFilter.FilterResult<PdfArray>(true, textArray);
        }

        internal virtual FilteredImagesCache.FilteredImageKey CreateFilteredImageKey(ImageRenderInfo image, PdfDocument
            document) {
            return FilteredImagesCache.CreateFilteredImageKey(image, GetImageAreasToBeCleaned(image), document);
        }

        /// <summary>Filter an ImageRenderInfo object</summary>
        /// <param name="image">the ImageRenderInfo object to be filtered</param>
        public virtual PdfCleanUpFilter.FilterResult<ImageData> FilterImage(ImageRenderInfo image) {
            return FilterImage(image, GetImageAreasToBeCleaned(image));
        }

        internal virtual PdfCleanUpFilter.FilterResult<ImageData> FilterImage(FilteredImagesCache.FilteredImageKey
            imageKey) {
            return FilterImage(imageKey.GetImageRenderInfo(), imageKey.GetCleanedAreas());
        }

        private PdfCleanUpFilter.FilterResult<ImageData> FilterImage(ImageRenderInfo image, IList<Rectangle
        > imageAreasToBeCleaned) {
            if (imageAreasToBeCleaned == null) {
                return new PdfCleanUpFilter.FilterResult<ImageData>(true, null);
            } else {
                if (imageAreasToBeCleaned.IsEmpty()) {
                    return new PdfCleanUpFilter.FilterResult<ImageData>(false, null);
                }
            }

            byte[] filteredImageBytes;
            try {
                byte[] originalImageBytes = image.GetImage().GetImageBytes();
                filteredImageBytes = ProcessImage(originalImageBytes, imageAreasToBeCleaned);
            } catch (Exception e) {
                throw new Exception(e.Message);
            }

            return new PdfCleanUpFilter.FilterResult<ImageData>(true, ImageDataFactory.Create(filteredImageBytes));
        }

        /// <summary>Filter a PathRenderInfo object</summary>
        /// <param name="path">the PathRenderInfo object to be filtered</param>
        public virtual Path FilterStrokePath(PathRenderInfo path) {
            PdfArray dashPattern = path.GetLineDashPattern();
            LineDashPattern lineDashPattern = new LineDashPattern(dashPattern.GetAsArray(0), dashPattern.GetAsNumber(1
            ).FloatValue());
            return FilterStrokePath(path.GetPath(), path.GetCtm(), path.GetLineWidth(), path.GetLineCapStyle(),
                path.GetLineJoinStyle
                    (), path.GetMiterLimit(), lineDashPattern);
        }

        /// <summary>Filter a PathRenderInfo object</summary>
        /// <param name="path">the PathRenderInfo object to be filtered</param>
        public virtual Path FilterFillPath(PathRenderInfo path, int fillingRule) {
            return FilterFillPath(path.GetPath(), path.GetCtm(), fillingRule);
        }

        /// <summary>Returns whether the given TextRenderInfo object needs to be cleaned up</summary>
        /// <param name="renderInfo">the input TextRenderInfo object</param>
        private bool IsTextNotToBeCleaned(TextRenderInfo renderInfo) {
            Point[] textRect = GetTextRectangle(renderInfo);
            foreach (Rectangle region in regions) {
                Point[] redactRect = GetRectangleVertices(region);
                if (CheckIfRectanglesIntersect(textRect, redactRect)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Calculates intersection of the image and the render filter region in the coordinate system relative to the image.
        ///     </summary>
        /// <returns>
        /// <code>null</code> if the image is fully covered and therefore is completely cleaned,
        /// <see cref="System.Collections.IList{E}"/>
        /// of
        /// <see cref="iText.Kernel.Geom.Rectangle"/>
        /// objects otherwise.
        /// </returns>
        private IList<Rectangle> GetImageAreasToBeCleaned(ImageRenderInfo image) {
            Rectangle imageRect = CalcImageRect(image);
            if (imageRect == null) {
                return null;
            }

            IList<Rectangle> areasToBeCleaned = new List<Rectangle>();
            foreach (Rectangle region in regions) {
                Rectangle intersectionRect = GetRectanglesIntersection(imageRect, region);
                if (intersectionRect != null) {
                    if (imageRect.EqualsWithEpsilon(intersectionRect)) {
                        // true if the image is completely covered
                        return null;
                    }

                    areasToBeCleaned.Add(TransformRectIntoImageCoordinates(intersectionRect, image.GetImageCtm()));
                }
            }

            return areasToBeCleaned;
        }

        private Path FilterStrokePath(Path sourcePath, Matrix ctm, float lineWidth, int lineCapStyle, int lineJoinStyle
            , float miterLimit, LineDashPattern lineDashPattern) {
            Path path = sourcePath;
            JoinType joinType = ClipperBridge.GetJoinType(lineJoinStyle);
            EndType endType = ClipperBridge.GetEndType(lineCapStyle);
            if (lineDashPattern != null) {
                if (!lineDashPattern.IsSolid()) {
                    path = LineDashPattern.ApplyDashPattern(path, lineDashPattern);
                }
            }

            ClipperOffset offset = new ClipperOffset(miterLimit,
                PdfCleanUpTool.arcTolerance * PdfCleanUpTool.floatMultiplier
            );
            IList<Subpath> degenerateSubpaths = ClipperBridge.AddPath(offset, path, joinType, endType);
            PolyTree resultTree = new PolyTree();
            offset.Execute(ref resultTree, lineWidth * PdfCleanUpTool.floatMultiplier / 2);
            Path offsetedPath = ClipperBridge.ConvertToPath(resultTree);
            if (degenerateSubpaths.Count > 0) {
                if (endType == EndType.OPEN_ROUND) {
                    IList<Subpath> circles = ConvertToCircles(degenerateSubpaths, lineWidth / 2);
                    offsetedPath.AddSubpaths(circles);
                } else {
                    if (endType == EndType.OPEN_SQUARE && lineDashPattern != null) {
                        IList<Subpath> squares = ConvertToSquares(degenerateSubpaths, lineWidth, sourcePath);
                        offsetedPath.AddSubpaths(squares);
                    }
                }
            }

            return FilterFillPath(offsetedPath, ctm, PdfCanvasConstants.FillingRule.NONZERO_WINDING);
        }

        /// <summary>Note: this method will close all unclosed subpaths of the passed path.</summary>
        /// <param name="fillingRule">If the subpath is contour, pass any value.</param>
        protected internal virtual Path FilterFillPath(Path path, Matrix ctm, int fillingRule) {
            path.CloseAllSubpaths();
            Clipper clipper = new Clipper();
            ClipperBridge.AddPath(clipper, path, PolyType.SUBJECT);
            foreach (Rectangle rectangle in regions) {
                try {
                    Point[] transfRectVertices = TransformPoints(ctm, true, GetRectangleVertices(rectangle));
                    ClipperBridge.AddRectToClipper(clipper, transfRectVertices, PolyType.CLIP);
                }
                catch (PdfException e) {
                    if (!(e.GetBaseException() is NoninvertibleTransformException)) {
                        throw e;
                    } else {
                        ILog logger = LogManager.GetLogger(typeof(PdfCleanUpFilter));
                        logger.Error(MessageFormatUtil.Format(iText.IO.LogMessageConstant.FAILED_TO_PROCESS_A_TRANSFORMATION_MATRIX));
                    }
                }
            }

            PolyFillType fillType = PolyFillType.NON_ZERO;
            if (fillingRule == PdfCanvasConstants.FillingRule.EVEN_ODD) {
                fillType = PolyFillType.EVEN_ODD;
            }

            PolyTree resultTree = new PolyTree();
            clipper.Execute(ClipType.DIFFERENCE, resultTree, fillType, PolyFillType.NON_ZERO);
            return ClipperBridge.ConvertToPath(resultTree);
        }

        /// <summary>Return true if two given rectangles (specified by an array of points) intersect.</summary>
        /// <param name="rect1">the first rectanglee, considered as a subject of intersection. Even if it's width is zero,
        /// it still can be intersected by second rectangle.</param>
        /// <param name="rect2">the second rectangle, considered as intersecting rectangle. If it has zero width rectangles
        /// are never considered as intersecting.</param>
        /// <returns>true if the rectangles intersect, false otherwise</returns>
        internal static bool CheckIfRectanglesIntersect(Point[] rect1, Point[] rect2) {
            Clipper clipper = new Clipper();
            // If the redaction area is degenerate, the result will be false
            if (!ClipperBridge.AddPolygonToClipper(clipper, rect2, PolyType.CLIP))
            {
                // If the content area is not degenerate (and the redaction area is), let's return false:
                // even if they overlaps somehow, we do not consider it as an intersection.
                // If the content area is degenerate, let's process this case specifically
                if (!ClipperBridge.AddPolygonToClipper(clipper, rect1, PolyType.SUBJECT))
                {
                    // Clipper fails to process degenerate redaction areas. However that's vital for pdfAutoSweep,
                    // because in some cases (for example, noninvertible cm) the text's area might be degenerate,
                    // but we still need to sweep the content.
                    // The idea is as follows:
                    // a) if the degenerate redaction area represents a point, there is no intersection
                    // b) if the degenerate redaction area represents a line, let's check that there the redaction line
                    // equals to one of the edges of the content's area. That is implemented in respect to area generation,
                    // because the redaction line corresponds to the descent line of the content.
                    if (!ClipperBridge.AddPolylineSubjectToClipper(clipper, rect2))
                    {
                        return false;
                    }
                    if (rect1.Length != rect2.Length)
                    {
                        return false;
                    }
                    Point startPoint = rect2[0];
                    Point endPoint = rect2[0];
                    for (int i = 1; i < rect2.Length; i++)
                    {
                        if (rect2[i].Distance(startPoint) > EPS)
                        {
                            endPoint = rect2[i];
                            break;
                        }
                    }
                    for (int i = 0; i < rect1.Length; i++)
                    {
                        if (IsPointOnALineSegment(rect1[i], startPoint, endPoint, true))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            // According to clipper documentation:
            // The function will return false if the path is invalid for clipping. A path is invalid for clipping when:
            // - it has less than 2 vertices;
            // - it has 2 vertices but is not an open path;
            // - the vertices are all co-linear and it is not an open path.
            // Reference: http://www.angusj.com/delphi/clipper/documentation/Docs/Units/ClipperLib/Classes/ClipperBase/Methods/AddPath.htm
            // If addition returns false, this means that there are less than 3 distinct points, because of rectangle zero width.
            // Let's in this case specify the path as polyline, because we still want to know if redaction area
            // intersects even with zero-width rectangles.
            bool intersectionSubjectAdded = ClipperBridge.AddPolygonToClipper(clipper, rect1, PolyType.SUBJECT);
            if (intersectionSubjectAdded) {
                // working with paths is considered to be a bit faster in terms of performance.
                Paths paths = new Paths();
                clipper.Execute(ClipType.INTERSECTION, paths, PolyFillType.NON_ZERO, PolyFillType.NON_ZERO);
                return !paths.IsEmpty();
            } else {
                int rect1Size = rect1.Length;
                intersectionSubjectAdded = ClipperBridge.AddPolylineSubjectToClipper(clipper, rect1);
                if (!intersectionSubjectAdded) {
                    // According to the comment above,
                    // this could have happened only if all four passed points are actually the same point.
                    // Adding here a point really close to the original point, to make sure it's not covered by the
                    // intersecting rectangle.
                    double smallDiff = 0.01;
                    IList<Point> rect1List = new List<Point>(JavaUtil.ArraysAsList(rect1));
                    rect1List.Add(new Point(rect1[0].GetX() + smallDiff, rect1[0].GetY()));
                    rect1 = rect1List.ToArray(new Point[rect1Size]);
                    intersectionSubjectAdded = ClipperBridge.AddPolylineSubjectToClipper(clipper, rect1);
                    System.Diagnostics.Debug.Assert(intersectionSubjectAdded);
                }
                PolyTree polyTree = new PolyTree();
                clipper.Execute(ClipType.INTERSECTION, polyTree, PolyFillType.NON_ZERO, PolyFillType.NON_ZERO);
                return !Clipper.PolyTreeToPaths(polyTree).IsEmpty();
            }
        }

        private static bool IsPointOnALineSegment(Point currPoint, Point linePoint1, Point linePoint2, bool isBetweenLinePoints)
        {
            double dxc = currPoint.x - linePoint1.x;
            double dyc = currPoint.y - linePoint1.y;

            double dxl = linePoint2.x - linePoint1.x;
            double dyl = linePoint2.y - linePoint1.y;

            double cross = dxc * dyl - dyc * dxl;

            // if point is on a line, let's check whether it's between provided line points
            if (Math.Abs(cross) <= EPS)
            {
                if (isBetweenLinePoints)
                {
                    if (Math.Abs(dxl) >= Math.Abs(dyl))
                    {
                        return dxl > 0 ?
                            linePoint1.x - EPS <= currPoint.x && currPoint.x <= linePoint2.x + EPS :
                            linePoint2.x - EPS <= currPoint.x && currPoint.x <= linePoint1.x + EPS;
                    }
                    else
                    {
                        return dyl > 0 ?
                            linePoint1.y - EPS <= currPoint.y && currPoint.y <= linePoint2.y + EPS :
                            linePoint2.y - EPS <= currPoint.y && currPoint.y <= linePoint1.y + EPS;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }



        /// <returns>Image boundary rectangle in device space.</returns>
        private static Rectangle CalcImageRect(ImageRenderInfo renderInfo) {
            Matrix ctm = renderInfo.GetImageCtm();
            if (ctm == null) {
                return null;
            }

            Point[] points = TransformPoints(ctm, false, new Point(0, 0), new Point(0, 1), new Point(1, 0), new Point(
                1, 1));
            return GetAsRectangle(points[0], points[1], points[2], points[3]);
        }

        /// <summary>Transforms the given Rectangle into the image coordinate system which is [0,1]x[0,1] by default</summary>
        private static Rectangle TransformRectIntoImageCoordinates(Rectangle rect, Matrix imageCtm) {
            Point[] points = TransformPoints(imageCtm, true, new Point(rect.GetLeft(), rect.GetBottom()), new Point(rect
                .GetLeft(), rect.GetTop()), new Point(rect.GetRight(), rect.GetBottom()), new Point(rect.GetRight(),
                rect
                    .GetTop()));
            return GetAsRectangle(points[0], points[1], points[2], points[3]);
        }

        /// <summary>Clean up an image using a List of Rectangles that need to be redacted</summary>
        /// <param name="imageBytes">the image to be cleaned up</param>
        /// <param name="areasToBeCleaned">the List of Rectangles that need to be redacted out of the image</param>
        private static byte[] ProcessImage(byte[] imageBytes, IList<Rectangle> areasToBeCleaned) {
            if (areasToBeCleaned.IsEmpty()) {
                return imageBytes;
            }

            using (Stream imageStream = new MemoryStream(imageBytes)) {
                Image image = Image.FromStream(imageStream);
                CleanImage(image, areasToBeCleaned);

                using (MemoryStream outStream = new MemoryStream()) {
                    if (Equals(image.RawFormat, ImageFormat.Tiff)) {
                        EncoderParameters encParams = new EncoderParameters(1);
                        encParams.Param[0] =
                            new EncoderParameter(Encoder.Compression, (long) EncoderValue.CompressionLZW);
                        image.Save(outStream, GetEncoderInfo(image.RawFormat), encParams);
                    } else if (Equals(image.RawFormat, ImageFormat.Jpeg)) {
                        EncoderParameters encParams = new EncoderParameters(1);
                        encParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                        image.Save(outStream, GetEncoderInfo(image.RawFormat), encParams);
                    } else {
                        image.Save(outStream, image.RawFormat);
                    }

                    return outStream.ToArray();
                }
            }
        }

        /// <summary>Clean up a BufferedImage using a List of Rectangles that need to be redacted</summary>
        /// <param name="image">the image to be cleaned up</param>
        /// <param name="areasToBeCleaned">the List of Rectangles that need to be redacted out of the image</param>
        private static void CleanImage(Image image, IList<Rectangle> areasToBeCleaned) {
            Graphics g = Graphics.FromImage(image);
            // A rectangle in the areasToBeCleaned list is treated to be in standard [0,1]x[0,1] image space
            // (y varies from bottom to top and x from left to right), so we should scale the rectangle and also
            // invert and shear the y axe.
            foreach (Rectangle rect in areasToBeCleaned) {
                int scaledBottomY = (int) System.Math.Ceiling(rect.GetBottom() * image.Height);
                int scaledTopY = (int) Math.Floor(rect.GetTop() * image.Height);
                int x = (int) System.Math.Ceiling(rect.GetLeft() * image.Width);
                int y = scaledTopY * -1 + image.Height;
                int width = (int) Math.Floor(rect.GetRight() * image.Width) - x;
                int height = scaledTopY - scaledBottomY;
                g.FillRectangle(new SolidBrush(CLEANED_AREA_FILL_COLOR.Value), x, y, width, height);
            }

            g.Dispose();
        }

        /// <summary>Converts specified degenerate subpaths to circles.</summary>
        /// <remarks>
        /// Converts specified degenerate subpaths to circles.
        /// Note: actually the resultant subpaths are not real circles but approximated.
        /// </remarks>
        /// <param name="radius">Radius of each constructed circle.</param>
        /// <returns>
        /// 
        /// <see cref="System.Collections.IList{E}"/>
        /// consisting of circles constructed on given degenerated subpaths.
        /// </returns>
        private static IList<Subpath> ConvertToCircles(IList<Subpath> degenerateSubpaths, double radius) {
            IList<Subpath> circles = new List<Subpath>(degenerateSubpaths.Count);
            foreach (Subpath subpath in degenerateSubpaths) {
                BezierCurve[] circleSectors = ApproximateCircle(subpath.GetStartPoint(), radius);
                Subpath circle = new Subpath();
                circle.AddSegment(circleSectors[0]);
                circle.AddSegment(circleSectors[1]);
                circle.AddSegment(circleSectors[2]);
                circle.AddSegment(circleSectors[3]);
                circles.Add(circle);
            }

            return circles;
        }

        /// <summary>Converts specified degenerate subpaths to squares.</summary>
        /// <remarks>
        /// Converts specified degenerate subpaths to squares.
        /// Note: the list of degenerate subpaths should contain at least 2 elements. Otherwise
        /// we can't determine the direction which the rotation of each square depends on.
        /// </remarks>
        /// <param name="squareWidth">Width of each constructed square.</param>
        /// <param name="sourcePath">The path which dash pattern applied to. Needed to calc rotation angle of each square.
        ///     </param>
        /// <returns>
        /// 
        /// <see cref="System.Collections.IList{E}"/>
        /// consisting of squares constructed on given degenerated subpaths.
        /// </returns>
        private static IList<Subpath> ConvertToSquares(IList<Subpath> degenerateSubpaths, double squareWidth, Path
            sourcePath) {
            IList<Point> pathApprox = GetPathApproximation(sourcePath);
            if (pathApprox.Count < 2) {
                return JavaCollectionsUtil.EmptyList<Subpath>();
            }

            IEnumerator<Point> approxIter = pathApprox.GetEnumerator();
            approxIter.MoveNext();
            Point approxPt1 = approxIter.Current;
            approxIter.MoveNext();
            Point approxPt2 = approxIter.Current;
            PdfCleanUpFilter.StandardLine line = new PdfCleanUpFilter.StandardLine(approxPt1, approxPt2);
            IList<Subpath> squares = new List<Subpath>(degenerateSubpaths.Count);
            float widthHalf = (float) squareWidth / 2;
            foreach (Subpath subpath in degenerateSubpaths) {
                Point point = subpath.GetStartPoint();
                while (!line.Contains(point)) {
                    approxPt1 = approxPt2;
                    approxIter.MoveNext();
                    approxPt2 = approxIter.Current;
                    line = new PdfCleanUpFilter.StandardLine(approxPt1, approxPt2);
                }

                double slope = line.GetSlope();
                double angle;
                if (!double.IsPositiveInfinity(slope)) {
                    angle = Math.Atan(slope);
                } else {
                    angle = Math.PI / 2;
                }

                squares.Add(ConstructSquare(point, widthHalf, angle));
            }

            return squares;
        }

        /// <summary>Approximates a given Path with a List of Point objects</summary>
        /// <param name="path">input path</param>
        private static IList<Point> GetPathApproximation(Path path) {
            IList<Point> approx = new ApproxPoints();
            foreach (Subpath subpath in path.GetSubpaths()) {
                approx.AddAll(subpath.GetPiecewiseLinearApproximation());
            }

            return approx;
        }

        private sealed class ApproxPoints : List<Point> {
            public bool AddAll(ICollection<Point> c) {
                Point prevPoint = (this.Count - 1 < 0 ? null : this[this.Count - 1]);
                bool ret = false;
                foreach (Point pt in c) {
                    if (!pt.Equals(prevPoint)) {
                        this.Add(pt);
                        prevPoint = pt;
                        ret = true;
                    }
                }

                return true;
            }
        }

        private static Subpath ConstructSquare(Point squareCenter, double widthHalf, double rotationAngle) {
            // Orthogonal square is the square with sides parallel to one of the axes.
            Point[] ortogonalSquareVertices = new Point[] {
                new Point(-widthHalf, -widthHalf), new Point(-widthHalf, widthHalf
                ),
                new Point(widthHalf, widthHalf), new Point(widthHalf, -widthHalf)
            };
            Point[] rotatedSquareVertices = GetRotatedSquareVertices(ortogonalSquareVertices, rotationAngle,
                squareCenter
            );
            Subpath square = new Subpath();
            square.AddSegment(new Line(rotatedSquareVertices[0], rotatedSquareVertices[1]));
            square.AddSegment(new Line(rotatedSquareVertices[1], rotatedSquareVertices[2]));
            square.AddSegment(new Line(rotatedSquareVertices[2], rotatedSquareVertices[3]));
            square.AddSegment(new Line(rotatedSquareVertices[3], rotatedSquareVertices[0]));
            return square;
        }

        private static Point[] GetRotatedSquareVertices(Point[] orthogonalSquareVertices, double angle,
            Point squareCenter
        ) {
            Point[] rotatedSquareVertices = new Point[orthogonalSquareVertices.Length];
            AffineTransform.GetRotateInstance((float) angle).Transform(orthogonalSquareVertices, 0,
                rotatedSquareVertices
                , 0, rotatedSquareVertices.Length);
            AffineTransform.GetTranslateInstance((float) squareCenter.GetX(), (float) squareCenter.GetY()).Transform(
                rotatedSquareVertices
                , 0, rotatedSquareVertices, 0, orthogonalSquareVertices.Length);
            return rotatedSquareVertices;
        }

        /// <summary>Approximate a circle with 4 Bezier curves (one for each 90 degrees sector)</summary>
        /// <param name="center">center of the circle</param>
        /// <param name="radius">radius of the circle</param>
        private static BezierCurve[] ApproximateCircle(Point center, double radius) {
            // The circle is split into 4 sectors. Arc of each sector
            // is approximated  with bezier curve separately.
            BezierCurve[] approximation = new BezierCurve[4];
            double x = center.GetX();
            double y = center.GetY();
            approximation[0] = new BezierCurve(JavaUtil.ArraysAsList(new Point(x, y + radius), new Point(x + radius *
                                                                                                         CIRCLE_APPROXIMATION_CONST,
                y + radius), new Point(x + radius, y + radius * CIRCLE_APPROXIMATION_CONST
            ), new Point(x + radius, y)));
            approximation[1] = new BezierCurve(JavaUtil.ArraysAsList(new Point(x + radius, y), new Point(x + radius, y
                                                                                                                     - radius *
                                                                                                                     CIRCLE_APPROXIMATION_CONST),
                new Point(x + radius * CIRCLE_APPROXIMATION_CONST, y - radius
                ), new Point(x, y - radius)));
            approximation[2] = new BezierCurve(JavaUtil.ArraysAsList(new Point(x, y - radius), new Point(x - radius *
                                                                                                         CIRCLE_APPROXIMATION_CONST,
                y - radius), new Point(x - radius, y - radius * CIRCLE_APPROXIMATION_CONST
            ), new Point(x - radius, y)));
            approximation[3] = new BezierCurve(JavaUtil.ArraysAsList(new Point(x - radius, y), new Point(x - radius, y
                                                                                                                     + radius *
                                                                                                                     CIRCLE_APPROXIMATION_CONST),
                new Point(x - radius * CIRCLE_APPROXIMATION_CONST, y + radius
                ), new Point(x, y + radius)));
            return approximation;
        }

        private static Point[] TransformPoints(Matrix transformationMatrix, bool inverse, params Point[] points) {
            AffineTransform t = new AffineTransform(transformationMatrix.Get(Matrix.I11), transformationMatrix.Get(
                    Matrix
                        .I12), transformationMatrix.Get(Matrix.I21), transformationMatrix.Get(Matrix.I22),
                transformationMatrix
                    .Get(Matrix.I31), transformationMatrix.Get(Matrix.I32));
            Point[] transformed = new Point[points.Length];
            if (inverse) {
                try {
                    t = t.CreateInverse();
                } catch (NoninvertibleTransformException e) {
                    throw new PdfException(PdfException.NoninvertibleMatrixCannotBeProcessed, e);
                }
            }

            t.Transform(points, 0, transformed, 0, points.Length);
            return transformed;
        }

        /// <summary>Get the bounding box of a TextRenderInfo object</summary>
        /// <param name="renderInfo">input TextRenderInfo object</param>
        private static Point[] GetTextRectangle(TextRenderInfo renderInfo) {
            LineSegment ascent = renderInfo.GetAscentLine();
            LineSegment descent = renderInfo.GetDescentLine();
            return new Point[] {
                new Point(ascent.GetStartPoint().Get(0), ascent.GetStartPoint().Get(1)), new Point(ascent
                    .GetEndPoint().Get(0), ascent.GetEndPoint().Get(1)),
                new Point(descent.GetEndPoint().Get(0), descent.GetEndPoint
                    ().Get(1)),
                new Point(descent.GetStartPoint().Get(0), descent.GetStartPoint().Get(1))
            };
        }

        /// <summary>Convert a Rectangle object into 4 Points</summary>
        /// <param name="rect">input Rectangle</param>
        private static Point[] GetRectangleVertices(Rectangle rect) {
            Point[] points = new Point[] {
                new Point(rect.GetLeft(), rect.GetBottom()), new Point(rect.GetRight(), rect
                    .GetBottom()),
                new Point(rect.GetRight(), rect.GetTop()), new Point(rect.GetLeft(), rect.GetTop())
            };
            return points;
        }

        /// <summary>Convert 4 Point objects into a Rectangle</summary>
        /// <param name="p1">first Point</param>
        /// <param name="p2">second Point</param>
        /// <param name="p3">third Point</param>
        /// <param name="p4">fourth Point</param>
        private static Rectangle GetAsRectangle(Point p1, Point p2, Point p3, Point p4) {
            IList<double> xs = JavaUtil.ArraysAsList(p1.GetX(), p2.GetX(), p3.GetX(), p4.GetX());
            IList<double> ys = JavaUtil.ArraysAsList(p1.GetY(), p2.GetY(), p3.GetY(), p4.GetY());
            double left = Enumerable.Min(xs);
            double bottom = Enumerable.Min(ys);
            double right = Enumerable.Max(xs);
            double top = Enumerable.Max(ys);
            return new Rectangle((float) left, (float) bottom, (float) (right - left), (float) (top - bottom));
        }

        /// <summary>Calculate the intersection of 2 Rectangles</summary>
        /// <param name="rect1">first Rectangle</param>
        /// <param name="rect2">second Rectangle</param>
        private static Rectangle GetRectanglesIntersection(Rectangle rect1, Rectangle rect2) {
            float x1 = Math.Max(rect1.GetLeft(), rect2.GetLeft());
            float y1 = Math.Max(rect1.GetBottom(), rect2.GetBottom());
            float x2 = Math.Min(rect1.GetRight(), rect2.GetRight());
            float y2 = Math.Min(rect1.GetTop(), rect2.GetTop());
            return (x2 - x1 > 0 && y2 - y1 > 0) ? new Rectangle(x1, y1, x2 - x1, y2 - y1) : null;
        }

        private static void CloseOutputStream(System.IO.Stream os) {
            if (os != null) {
                try {
                    os.Close();
                } catch (System.IO.IOException e) {
                    throw new Exception(e.Message);
                }
            }
        }

        private static ImageCodecInfo GetEncoderInfo(ImageFormat format) {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

            for (int j = 0; j < encoders.Length; ++j) {
                if (encoders[j].FormatID == format.Guid)
                    return encoders[j];
            }

            return null;
        }

        /// <summary>Generic class representing the result of filtering an object of type T</summary>
        public class FilterResult<T> {
            private bool isModified;

            private T filterResult;

            public FilterResult(bool isModified, T filterResult) {
                this.isModified = isModified;
                this.filterResult = filterResult;
            }

            /// <summary>Get whether the object was modified or not</summary>
            /// <returns>true if the object was modified, false otherwise</returns>
            public virtual bool IsModified() {
                return isModified;
            }

            /// <summary>Get the result after filtering</summary>
            public virtual T GetFilterResult() {
                return filterResult;
            }
        }

        private class StandardLine {
            internal float A;

            internal float B;

            internal float C;

            internal StandardLine(Point p1, Point p2) {
                // Constants from the standard line representation: Ax+By+C
                A = (float) (p2.GetY() - p1.GetY());
                B = (float) (p1.GetX() - p2.GetX());
                C = (float) (p1.GetY() * (-B) - p1.GetX() * A);
            }

            internal virtual float GetSlope() {
                if (B == 0) {
                    return float.PositiveInfinity;
                }

                return -A / B;
            }

            internal virtual bool Contains(Point point) {
                return JavaUtil.FloatCompare(Math.Abs(A * (float)point.GetX() + B * (float)point.GetY() + C), 0.1f) < 0;
            }
        }
    }
}
