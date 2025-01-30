/*
This file is part of the iText (R) project.
Copyright (c) 1998-2025 Apryse Group NV
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
using Microsoft.Extensions.Logging;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<iText.Kernel.Pdf.Canvas.Parser.ClipperLib.IntPoint>>;
using iText.Commons;
using iText.Commons.Datastructures;
using iText.Commons.Utils;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Exceptions;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser.ClipperLib;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Xobject;
using iText.PdfCleanup.Exceptions;
using iText.PdfCleanup.Logs;
using iText.PdfCleanup.Util;

namespace iText.PdfCleanup {
//\cond DO_NOT_DOCUMENT
    internal class PdfCleanUpFilter {
        private static readonly ILogger logger = ITextLogManager.GetLogger(typeof(iText.PdfCleanup.PdfCleanUpFilter
            ));

        /* There is no exact representation of the circle using Bézier curves.
        * But, for a Bézier curve with n segments the optimal distance to the control points,
        * in the sense that the middle of the curve lies on the circle itself, is (4/3) * tan(pi / (2*n))
        * So for 4 points it is (4/3) * tan(pi/8) = 4 * (sqrt(2)-1)/3 = 0.5522847498
        * In this approximation, the Bézier curve always falls outside the circle,
        * except momentarily when it dips in to touch the circle at the midpoint and endpoints.
        * However, a better approximation is possible using 0.55191502449
        */
        private const double CIRCLE_APPROXIMATION_CONST = 0.55191502449;

        private const float EPS = 1e-4f;

        private static readonly ICollection<PdfName> NOT_SUPPORTED_FILTERS_FOR_DIRECT_CLEANUP = JavaCollectionsUtil
            .UnmodifiableSet(new LinkedHashSet<PdfName>(JavaUtil.ArraysAsList(PdfName.JBIG2Decode, PdfName.DCTDecode
            , PdfName.JPXDecode)));

        private readonly IList<Rectangle> regions;

        private readonly CleanUpProperties properties;

        public PdfCleanUpFilter(IList<Rectangle> regions, CleanUpProperties properties) {
            this.regions = regions;
            this.properties = properties;
        }

//\cond DO_NOT_DOCUMENT
        internal static bool ImageSupportsDirectCleanup(PdfImageXObject image) {
            PdfObject filter = image.GetPdfObject().Get(PdfName.Filter);
            bool supportedFilterForDirectCleanup = IsSupportedFilterForDirectImageCleanup(filter);
            bool deviceGrayOrNoCS = PdfName.DeviceGray.Equals(image.GetPdfObject().GetAsName(PdfName.ColorSpace)) || !
                image.GetPdfObject().ContainsKey(PdfName.ColorSpace);
            return deviceGrayOrNoCS && supportedFilterForDirectCleanup;
        }
//\endcond

//\cond DO_NOT_DOCUMENT
        /// <summary>Return true if two given rectangles (specified by an array of points) intersect.</summary>
        /// <param name="rect1">
        /// the first rectangle, considered as a subject of intersection. Even if it's width is zero,
        /// it still can be intersected by second rectangle.
        /// </param>
        /// <param name="rect2">
        /// the second rectangle, considered as intersecting rectangle. If it has zero width rectangles
        /// are never considered as intersecting.
        /// </param>
        /// <returns>true if the rectangles intersect, false otherwise</returns>
        internal virtual bool CheckIfRectanglesIntersect(Point[] rect1, Point[] rect2) {
            Clipper clipper = new Clipper();
            ClipperBridge clipperBridge = properties.GetOffsetProperties().CalculateOffsetMultiplierDynamically() ? new 
                ClipperBridge(rect1, rect2) : new ClipperBridge();
            // If the redaction area is degenerate, the result will be false
            if (!clipperBridge.AddPolygonToClipper(clipper, rect2, PolyType.CLIP)) {
                // If the content area is not degenerate (and the redaction area is), let's return false:
                // even if they overlaps somehow, we do not consider it as an intersection.
                // If the content area is degenerate, let's process this case specifically
                if (!clipperBridge.AddPolygonToClipper(clipper, rect1, PolyType.SUBJECT)) {
                    // Clipper fails to process degenerate redaction areas. However, that's vital for pdfAutoSweep,
                    // because in some cases (for example, noninvertible cm) the text's area might be degenerate,
                    // but we still need to sweep the content.
                    // The idea is as follows:
                    // a) if the degenerate redaction area represents a point, there is no intersection
                    // b) if the degenerate redaction area represents a line, let's check that there the redaction line
                    // equals to one of the edges of the content's area. That is implemented in respect to area generation,
                    // because the redaction line corresponds to the descent line of the content.
                    if (!clipperBridge.AddPolylineSubjectToClipper(clipper, rect2)) {
                        return false;
                    }
                    if (rect1.Length != rect2.Length) {
                        return false;
                    }
                    Point startPoint = rect2[0];
                    Point endPoint = rect2[0];
                    for (int i = 1; i < rect2.Length; i++) {
                        if (rect2[i].Distance(startPoint) > EPS) {
                            endPoint = rect2[i];
                            break;
                        }
                    }
                    foreach (Point point in rect1) {
                        if (IsPointOnALineSegment(point, startPoint, endPoint, true)) {
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
            bool intersectionSubjectAdded = clipperBridge.AddPolygonToClipper(clipper, rect1, PolyType.SUBJECT);
            if (intersectionSubjectAdded) {
                // working with paths is considered to be a bit faster in terms of performance.
                Paths paths = new Paths();
                clipper.Execute(ClipType.INTERSECTION, paths, PolyFillType.NON_ZERO, PolyFillType.NON_ZERO);
                return CheckIfIntersectionOccurs(paths, rect1, false, clipperBridge);
            }
            intersectionSubjectAdded = clipperBridge.AddPolylineSubjectToClipper(clipper, rect1);
            if (!intersectionSubjectAdded) {
                // According to the comment above,
                // this could have happened only if all four passed points are actually the same point.
                // Adding here a point really close to the original point, to make sure it's not covered by the
                // intersecting rectangle.
                double SMALL_DIFF = 0.01;
                Point[] expandedRect1 = new Point[rect1.Length + 1];
                Array.Copy(rect1, 0, expandedRect1, 0, rect1.Length);
                expandedRect1[rect1.Length] = new Point(rect1[0].GetX() + SMALL_DIFF, rect1[0].GetY());
                rect1 = expandedRect1;
                intersectionSubjectAdded = clipperBridge.AddPolylineSubjectToClipper(clipper, rect1);
                System.Diagnostics.Debug.Assert(intersectionSubjectAdded);
            }
            PolyTree polyTree = new PolyTree();
            clipper.Execute(ClipType.INTERSECTION, polyTree, PolyFillType.NON_ZERO, PolyFillType.NON_ZERO);
            return CheckIfIntersectionOccurs(iText.Kernel.Pdf.Canvas.Parser.ClipperLib.Clipper.PolyTreeToPaths(polyTree
                ), rect1, true, clipperBridge);
        }
//\endcond

        private bool CheckIfIntersectionOccurs(Paths paths, Point[] rect1, bool isDegenerate, ClipperBridge clipperBridge
            ) {
            if (paths.IsEmpty()) {
                return false;
            }
            IntRect intersectionRectangle = iText.Kernel.Pdf.Canvas.Parser.ClipperLib.Clipper.GetBounds(paths);
            // If the user defines a overlappingRatio we use this to calculate whether it intersects enough
            // To pass as an intersection
            if (properties.GetOverlapRatio() == null) {
                return !CheckIfIntersectionRectangleDegenerate(intersectionRectangle, isDegenerate, clipperBridge);
            }
            double overlappedArea = CleanUpHelperUtil.CalculatePolygonArea(rect1);
            double intersectionArea = clipperBridge.LongRectCalculateHeight(intersectionRectangle) * clipperBridge.LongRectCalculateWidth
                (intersectionRectangle);
            double percentageOfOverlapping = intersectionArea / overlappedArea;
            float SMALL_VALUE_FOR_ROUNDING_ERRORS = 1e-5f;
            return percentageOfOverlapping + SMALL_VALUE_FOR_ROUNDING_ERRORS > properties.GetOverlapRatio();
        }

//\cond DO_NOT_DOCUMENT
        /// <summary>Filter a TextRenderInfo object.</summary>
        /// <param name="text">the TextRenderInfo to be filtered</param>
        /// <returns>
        /// a
        /// <see cref="FilterResult{T}"/>
        /// object with filtered text.
        /// </returns>
        internal virtual PdfCleanUpFilter.FilterResult<PdfArray> FilterText(TextRenderInfo text) {
            PdfTextArray textArray = new PdfTextArray();
            // Overlap ratio should not be taken into account when we check the whole text not to be cleaned up
            if (properties.GetOverlapRatio() == null && IsTextNotToBeCleaned(text)) {
                return new PdfCleanUpFilter.FilterResult<PdfArray>(false, new PdfArray(text.GetPdfString()));
            }
            bool isModified = false;
            foreach (TextRenderInfo ri in text.GetCharacterRenderInfos()) {
                if (IsTextNotToBeCleaned(ri)) {
                    textArray.Add(ri.GetPdfString());
                }
                else {
                    isModified = true;
                    textArray.Add(new PdfNumber(FontProgram.ConvertGlyphSpaceToTextSpace(-ri.GetUnscaledWidth()) / (text.GetFontSize
                        () * text.GetHorizontalScaling() / FontProgram.HORIZONTAL_SCALING_FACTOR)));
                }
            }
            return new PdfCleanUpFilter.FilterResult<PdfArray>(isModified, textArray);
        }
//\endcond

//\cond DO_NOT_DOCUMENT
        /// <summary>Filter an ImageRenderInfo object.</summary>
        /// <param name="image">the ImageRenderInfo object to be filtered</param>
        /// <returns>
        /// an
        /// <see cref="FilterResult{T}"/>
        /// object with filtered image data.filterStrokePath
        /// </returns>
        internal virtual PdfCleanUpFilter.FilterResult<ImageData> FilterImage(ImageRenderInfo image) {
            return FilterImage(image.GetImage(), GetImageAreasToBeCleaned(image.GetImageCtm()));
        }
//\endcond

//\cond DO_NOT_DOCUMENT
        internal virtual PdfCleanUpFilter.FilterResult<ImageData> FilterImage(FilteredImagesCache.FilteredImageKey
             imageKey) {
            return FilterImage(imageKey.GetImageXObject(), imageKey.GetCleanedAreas());
        }
//\endcond

//\cond DO_NOT_DOCUMENT
        /// <summary>Filter a PathRenderInfo object.</summary>
        /// <param name="path">the PathRenderInfo object to be filtered</param>
        /// <returns>
        /// a filtered
        /// <see cref="iText.Kernel.Geom.Path"/>
        /// object.
        /// </returns>
        internal virtual Tuple2<Path, bool> FilterStrokePath(PathRenderInfo path) {
            PdfArray dashPattern = path.GetLineDashPattern();
            LineDashPattern lineDashPattern = new LineDashPattern(dashPattern.GetAsArray(0), dashPattern.GetAsNumber(1
                ).FloatValue());
            return FilterStrokePath(path.GetPath(), path.GetCtm(), path.GetLineWidth(), path.GetLineCapStyle(), path.GetLineJoinStyle
                (), path.GetMiterLimit(), lineDashPattern);
        }
//\endcond

//\cond DO_NOT_DOCUMENT
        /// <summary>Filter a PathRenderInfo object.</summary>
        /// <param name="path">the PathRenderInfo object to be filtered</param>
        /// <param name="fillingRule">
        /// an integer parameter, specifying whether the subpath is contour.
        /// If the subpath is contour, pass any value.
        /// </param>
        /// <returns>
        /// a filtered
        /// <see cref="iText.Kernel.Geom.Path"/>
        /// object.
        /// </returns>
        internal virtual Path FilterFillPath(PathRenderInfo path, int fillingRule) {
            return FilterFillPath(path.GetPath(), path.GetCtm(), fillingRule, false);
        }
//\endcond

//\cond DO_NOT_DOCUMENT
        internal virtual FilteredImagesCache.FilteredImageKey CreateFilteredImageKey(PdfImageXObject image, Matrix
             imageCtm, PdfDocument document) {
            return FilteredImagesCache.CreateFilteredImageKey(image, GetImageAreasToBeCleaned(imageCtm), document);
        }
//\endcond

        /// <summary>Note: this method will close all unclosed subpaths of the passed path.</summary>
        /// <param name="path">the PathRenderInfo object to be filtered.</param>
        /// <param name="ctm">
        /// a
        /// <see cref="iText.Kernel.Geom.Path"/>
        /// transformation matrix.
        /// </param>
        /// <param name="fillingRule">If the subpath is contour, pass any value.</param>
        /// <param name="checkForIntersection">
        /// if true, the intersection check of path and regions will be performed, and if
        /// there is no intersection, original path from parameters will be returned.
        /// We pass true when we filter stroke path (stroke converted to fill)
        /// not to put fill path into the output if it's not intersected with cleanup area.
        /// We pass false when we filter fill and clip paths (there we don't convert stroke to
        /// fill) and thus happy with the result from ClipperBridge DIFFERENCES.
        /// </param>
        /// <returns>
        /// a filtered
        /// <see cref="iText.Kernel.Geom.Path"/>
        /// object.
        /// </returns>
        private Path FilterFillPath(Path path, Matrix ctm, int fillingRule, bool checkForIntersection) {
            path.CloseAllSubpaths();
            IList<Point[]> transfRectVerticesList = new List<Point[]>();
            foreach (Rectangle rectangle in regions) {
                try {
                    transfRectVerticesList.Add(TransformPoints(ctm, true, GetRectangleVertices(rectangle)));
                }
                catch (PdfException e) {
                    if (!(e.InnerException is NoninvertibleTransformException)) {
                        throw;
                    }
                    else {
                        logger.LogError(MessageFormatUtil.Format(CleanUpLogMessageConstant.FAILED_TO_PROCESS_A_TRANSFORMATION_MATRIX
                            ));
                    }
                }
            }
            Clipper clipper = new Clipper();
            ClipperBridge clipperBridge = properties.GetOffsetProperties().CalculateOffsetMultiplierDynamically() ? GetClipperBridge
                (path, transfRectVerticesList) : new ClipperBridge();
            clipperBridge.AddPath(clipper, path, PolyType.SUBJECT);
            foreach (Point[] transfRectVertices in transfRectVerticesList) {
                clipperBridge.AddPolygonToClipper(clipper, transfRectVertices, PolyType.CLIP);
            }
            PolyFillType fillType = PolyFillType.NON_ZERO;
            if (fillingRule == PdfCanvasConstants.FillingRule.EVEN_ODD) {
                fillType = PolyFillType.EVEN_ODD;
            }
            if (checkForIntersection) {
                //Find intersection with cleanup areas
                PolyTree cleanupAreaIntersection = new PolyTree();
                clipper.Execute(ClipType.INTERSECTION, cleanupAreaIntersection, fillType, PolyFillType.NON_ZERO);
                if (iText.Kernel.Pdf.Canvas.Parser.ClipperLib.Clipper.PolyTreeToPaths(cleanupAreaIntersection).IsEmpty()) {
                    //if there are no intersections, return original path as mark that no need to filter anything
                    return path;
                }
            }
            PolyTree resultTree = new PolyTree();
            clipper.Execute(ClipType.DIFFERENCE, resultTree, fillType, PolyFillType.NON_ZERO);
            return clipperBridge.ConvertToPath(resultTree);
        }

        /// <summary>Calculates intersection of the image and the render filter region in the coordinate system relative to the image.
        ///     </summary>
        /// <returns>
        /// 
        /// <see langword="null"/>
        /// if the image is fully covered and therefore is completely cleaned,
        /// <see cref="System.Collections.IList{E}"/>
        /// of
        /// <see cref="iText.Kernel.Geom.Rectangle"/>
        /// objects otherwise.
        /// </returns>
        private IList<Rectangle> GetImageAreasToBeCleaned(Matrix imageCtm) {
            Rectangle imageRect = CalcImageRect(imageCtm);
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
                    areasToBeCleaned.Add(TransformRectIntoImageCoordinates(intersectionRect, imageCtm));
                }
            }
            return areasToBeCleaned;
        }

        private Tuple2<Path, bool> FilterStrokePath(Path sourcePath, Matrix ctm, float lineWidth, int lineCapStyle
            , int lineJoinStyle, float miterLimit, LineDashPattern lineDashPattern) {
            Path path = sourcePath;
            JoinType joinType = ClipperBridge.GetJoinType(lineJoinStyle);
            EndType endType = ClipperBridge.GetEndType(lineCapStyle);
            if (lineDashPattern != null && !lineDashPattern.IsSolid()) {
                path = LineDashPattern.ApplyDashPattern(path, lineDashPattern);
            }
            ClipperBridge clipperBridge = properties.GetOffsetProperties().CalculateOffsetMultiplierDynamically() ? new 
                ClipperBridge(path) : new ClipperBridge();
            ClipperOffset offset = new ClipperOffset(miterLimit, properties.GetOffsetProperties().GetArcTolerance() * 
                clipperBridge.GetFloatMultiplier());
            IList<Subpath> degenerateSubpaths = clipperBridge.AddPath(offset, path, joinType, endType);
            PolyTree resultTree = new PolyTree();
            offset.Execute(resultTree, lineWidth * clipperBridge.GetFloatMultiplier() / 2);
            Path offsetedPath = clipperBridge.ConvertToPath(resultTree);
            if (!degenerateSubpaths.IsEmpty()) {
                if (endType == EndType.OPEN_ROUND) {
                    IList<Subpath> circles = ConvertToCircles(degenerateSubpaths, lineWidth / 2);
                    offsetedPath.AddSubpaths(circles);
                }
                else {
                    if (endType == EndType.OPEN_SQUARE && lineDashPattern != null) {
                        IList<Subpath> squares = ConvertToSquares(degenerateSubpaths, lineWidth, sourcePath);
                        offsetedPath.AddSubpaths(squares);
                    }
                }
            }
            Path resultPath = FilterFillPath(offsetedPath, ctm, PdfCanvasConstants.FillingRule.NONZERO_WINDING, true);
            //if path was not filtered, return original path
            if (resultPath == offsetedPath) {
                return new Tuple2<Path, bool>(sourcePath, false);
            }
            return new Tuple2<Path, bool>(resultPath, true);
        }

        /// <summary>Returns whether the given TextRenderInfo object needs to be cleaned up.</summary>
        /// <param name="renderInfo">the input TextRenderInfo object</param>
        private bool IsTextNotToBeCleaned(TextRenderInfo renderInfo) {
            Point[] textRect = GetTextRectangle(renderInfo);
            foreach (Rectangle region in regions) {
                Point[] redactRect = GetRectangleVertices(region);
                // Text rectangle might be rotated, hence we are using precise polygon intersection checker and not
                // just intersecting two rectangles that are parallel to the x and y coordinate vectors
                if (CheckIfRectanglesIntersect(textRect, redactRect)) {
                    return false;
                }
            }
            return true;
        }

        private static PdfCleanUpFilter.FilterResult<ImageData> FilterImage(PdfImageXObject image, IList<Rectangle
            > imageAreasToBeCleaned) {
            if (imageAreasToBeCleaned == null) {
                return new PdfCleanUpFilter.FilterResult<ImageData>(true, null);
            }
            else {
                if (imageAreasToBeCleaned.IsEmpty()) {
                    return new PdfCleanUpFilter.FilterResult<ImageData>(false, null);
                }
            }
            byte[] filteredImageBytes;
            if (ImageSupportsDirectCleanup(image)) {
                byte[] imageStreamBytes = ProcessImageDirectly(image, imageAreasToBeCleaned);
                // Creating imageXObject clone in order to avoid modification of the original XObject in the document.
                // We require to set filtered image bytes to the image XObject only for the sake of simplifying code:
                // in this method we return ImageData, so in order to convert PDF image to the common image format we
                // reuse PdfImageXObject#getImageBytes method.
                // I think this is acceptable here, because monochrome and grayscale images are not very common,
                // so the overhead would be not that big. But anyway, this should be refactored in future if this
                // direct image bytes cleaning approach would be found useful and will be preserved in future.
                PdfImageXObject tempImageClone = new PdfImageXObject((PdfStream)image.GetPdfObject().Clone());
                tempImageClone.GetPdfObject().SetData(imageStreamBytes);
                filteredImageBytes = tempImageClone.GetImageBytes();
            }
            else {
                byte[] originalImageBytes = image.GetImageBytes();
                filteredImageBytes = CleanUpImageUtil.CleanUpImage(originalImageBytes, imageAreasToBeCleaned);
            }
            return new PdfCleanUpFilter.FilterResult<ImageData>(true, ImageDataFactory.Create(filteredImageBytes));
        }

        /// <summary>Checks if the input intersection rectangle is degenerate.</summary>
        /// <remarks>
        /// Checks if the input intersection rectangle is degenerate.
        /// In case of intersection subject is degenerate (isIntersectSubjectDegenerate
        /// is true) and it is included into intersecting rectangle, this method returns false,
        /// despite the intersection rectangle is degenerate.
        /// </remarks>
        /// <param name="rect">intersection rectangle</param>
        /// <param name="isIntersectSubjectDegenerate">
        /// value, specifying if the intersection subject
        /// is degenerate.
        /// </param>
        /// <returns>true - if the intersection rectangle is degenerate.</returns>
        private static bool CheckIfIntersectionRectangleDegenerate(IntRect rect, bool isIntersectSubjectDegenerate
            , ClipperBridge clipperBridge) {
            float width = clipperBridge.LongRectCalculateWidth(rect);
            float height = clipperBridge.LongRectCalculateHeight(rect);
            return isIntersectSubjectDegenerate ? (width < EPS && height < EPS) : (width < EPS || height < EPS);
        }

        private static bool IsPointOnALineSegment(Point currPoint, Point linePoint1, Point linePoint2, bool isBetweenLinePoints
            ) {
            double dxc = currPoint.GetX() - linePoint1.GetX();
            double dyc = currPoint.GetY() - linePoint1.GetY();
            double dxl = linePoint2.GetX() - linePoint1.GetX();
            double dyl = linePoint2.GetY() - linePoint1.GetY();
            double cross = dxc * dyl - dyc * dxl;
            // if point is on a line, let's check whether it's between provided line points
            if (Math.Abs(cross) <= EPS) {
                if (isBetweenLinePoints) {
                    if (Math.Abs(dxl) >= Math.Abs(dyl)) {
                        return dxl > 0 ? linePoint1.GetX() - EPS <= currPoint.GetX() && currPoint.GetX() <= linePoint2.GetX() + EPS
                             : linePoint2.GetX() - EPS <= currPoint.GetX() && currPoint.GetX() <= linePoint1.GetX() + EPS;
                    }
                    else {
                        return dyl > 0 ? linePoint1.GetY() - EPS <= currPoint.GetY() && currPoint.GetY() <= linePoint2.GetY() + EPS
                             : linePoint2.GetY() - EPS <= currPoint.GetY() && currPoint.GetY() <= linePoint1.GetY() + EPS;
                    }
                }
                else {
                    return true;
                }
            }
            return false;
        }

        private static bool IsSupportedFilterForDirectImageCleanup(PdfObject filter) {
            if (filter == null) {
                return true;
            }
            if (filter.IsName()) {
                return !NOT_SUPPORTED_FILTERS_FOR_DIRECT_CLEANUP.Contains((PdfName)filter);
            }
            else {
                if (filter.IsArray()) {
                    PdfArray filterArray = (PdfArray)filter;
                    for (int i = 0; i < filterArray.Size(); ++i) {
                        if (NOT_SUPPORTED_FILTERS_FOR_DIRECT_CLEANUP.Contains(filterArray.GetAsName(i))) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <returns>Image boundary rectangle in device space.</returns>
        private static Rectangle CalcImageRect(Matrix imageCtm) {
            if (imageCtm == null) {
                return null;
            }
            Point[] points = TransformPoints(imageCtm, false, new Point(0d, 0d), new Point(0d, 1d), new Point(1d, 0d), 
                new Point(1d, 1d));
            return Rectangle.CalculateBBox(JavaUtil.ArraysAsList(points));
        }

        /// <summary>Transforms the given Rectangle into the image coordinate system which is [0,1]x[0,1] by default.</summary>
        private static Rectangle TransformRectIntoImageCoordinates(Rectangle rect, Matrix imageCtm) {
            Point[] points = TransformPoints(imageCtm, true, new Point(rect.GetLeft(), rect.GetBottom()), new Point(rect
                .GetLeft(), rect.GetTop()), new Point(rect.GetRight(), rect.GetBottom()), new Point(rect.GetRight(), rect
                .GetTop()));
            return Rectangle.CalculateBBox(JavaUtil.ArraysAsList(points));
        }

        /// <summary>Filters image content using direct manipulation over PDF image samples stream.</summary>
        /// <remarks>
        /// Filters image content using direct manipulation over PDF image samples stream. Implemented according to ISO 32000-2,
        /// "8.9.3 Sample representation".
        /// </remarks>
        /// <param name="image">image XObject which will be filtered</param>
        /// <param name="imageAreasToBeCleaned">list of rectangle areas for clean up with coordinates in (0,1)x(0,1) space
        ///     </param>
        /// <returns>raw bytes of the PDF image samples stream which is already cleaned.</returns>
        private static byte[] ProcessImageDirectly(PdfImageXObject image, IList<Rectangle> imageAreasToBeCleaned) {
            int X = 0;
            int Y = 1;
            int W = 2;
            int H = 3;
            byte[] originalImageBytes = image.GetPdfObject().GetBytes();
            PdfNumber bpcVal = image.GetPdfObject().GetAsNumber(PdfName.BitsPerComponent);
            if (bpcVal == null) {
                throw new ArgumentException("/BitsPerComponent entry is required for image dictionaries.");
            }
            int bpc = bpcVal.IntValue();
            if (bpc != 1 && bpc != 2 && bpc != 4 && bpc != 8 && bpc != 16) {
                throw new ArgumentException("/BitsPerComponent only allowed values are: 1, 2, 4, 8 and 16.");
            }
            double bytesInComponent = (double)bpc / 8;
            int firstComponentInByte = 0;
            if (bpc < 16) {
                for (int i = 0; i < bpc; ++i) {
                    firstComponentInByte += (int)Math.Pow(2, 7 - i);
                }
            }
            double width = image.GetWidth();
            double height = image.GetHeight();
            int rowPadding = 0;
            if ((width * bpc) % 8 > 0) {
                rowPadding = (int)(8 - (width * bpc) % 8);
            }
            foreach (Rectangle rect in imageAreasToBeCleaned) {
                int[] cleanImgRect = CleanUpHelperUtil.GetImageRectToClean(rect, (int)width, (int)height);
                for (int j = cleanImgRect[Y]; j < cleanImgRect[Y] + cleanImgRect[H]; ++j) {
                    for (int i = cleanImgRect[X]; i < cleanImgRect[X] + cleanImgRect[W]; ++i) {
                        // based on assumption that numOfComponents always equals 1, because this method is only for monochrome and grayscale images
                        double pixelPos = j * ((width * bpc + rowPadding) / 8) + i * bytesInComponent;
                        int pixelByteInd = (int)pixelPos;
                        byte byteWithSample = originalImageBytes[pixelByteInd];
                        if (bpc == 16) {
                            originalImageBytes[pixelByteInd] = 0;
                            originalImageBytes[pixelByteInd + 1] = 0;
                        }
                        else {
                            int reset = ~(firstComponentInByte >> (int)((pixelPos - pixelByteInd) * 8)) & 0xFF;
                            originalImageBytes[pixelByteInd] = (byte)(byteWithSample & reset);
                        }
                    }
                }
            }
            return originalImageBytes;
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
            Point approxPt1 = approxIter.Next();
            Point approxPt2 = approxIter.Next();
            PdfCleanUpFilter.StandardLine line = new PdfCleanUpFilter.StandardLine(approxPt1, approxPt2);
            IList<Subpath> squares = new List<Subpath>(degenerateSubpaths.Count);
            float widthHalf = (float)squareWidth / 2;
            foreach (Subpath subpath in degenerateSubpaths) {
                Point point = subpath.GetStartPoint();
                while (!line.Contains(point)) {
                    approxPt1 = approxPt2;
                    approxPt2 = approxIter.Next();
                    line = new PdfCleanUpFilter.StandardLine(approxPt1, approxPt2);
                }
                double slope = line.GetSlope();
                double angle;
                if (slope != float.PositiveInfinity) {
                    angle = Math.Atan(slope);
                }
                else {
                    angle = Math.PI / 2;
                }
                squares.Add(ConstructSquare(point, widthHalf, angle));
            }
            return squares;
        }

        /// <summary>Approximates a given Path with a List of Point objects.</summary>
        /// <param name="path">input path</param>
        private static IList<Point> GetPathApproximation(Path path) {
            PdfCleanUpFilter.ApproxPointList<Point> approx = new PdfCleanUpFilter.ApproxPointList<Point>();
            foreach (Subpath subpath in path.GetSubpaths()) {
                approx.AddAllPoints(subpath.GetPiecewiseLinearApproximation());
            }
            return approx;
        }

        private static Subpath ConstructSquare(Point squareCenter, double widthHalf, double rotationAngle) {
            // Orthogonal square is the square with sides parallel to one of the axes.
            Point[] ortogonalSquareVertices = new Point[] { new Point(-widthHalf, -widthHalf), new Point(-widthHalf, widthHalf
                ), new Point(widthHalf, widthHalf), new Point(widthHalf, -widthHalf) };
            Point[] rotatedSquareVertices = GetRotatedSquareVertices(ortogonalSquareVertices, rotationAngle, squareCenter
                );
            Subpath square = new Subpath();
            square.AddSegment(new Line(rotatedSquareVertices[0], rotatedSquareVertices[1]));
            square.AddSegment(new Line(rotatedSquareVertices[1], rotatedSquareVertices[2]));
            square.AddSegment(new Line(rotatedSquareVertices[2], rotatedSquareVertices[3]));
            square.AddSegment(new Line(rotatedSquareVertices[3], rotatedSquareVertices[0]));
            return square;
        }

        private static Point[] GetRotatedSquareVertices(Point[] orthogonalSquareVertices, double angle, Point squareCenter
            ) {
            Point[] rotatedSquareVertices = new Point[orthogonalSquareVertices.Length];
            AffineTransform.GetRotateInstance((float)angle).Transform(orthogonalSquareVertices, 0, rotatedSquareVertices
                , 0, rotatedSquareVertices.Length);
            AffineTransform.GetTranslateInstance((float)squareCenter.GetX(), (float)squareCenter.GetY()).Transform(rotatedSquareVertices
                , 0, rotatedSquareVertices, 0, orthogonalSquareVertices.Length);
            return rotatedSquareVertices;
        }

        /// <summary>Approximate a circle with 4 Bezier curves (one for each 90 degrees sector).</summary>
        /// <param name="center">center of the circle</param>
        /// <param name="radius">radius of the circle</param>
        private static BezierCurve[] ApproximateCircle(Point center, double radius) {
            // The circle is split into 4 sectors. Arc of each sector
            // is approximated  with bezier curve separately.
            BezierCurve[] approximation = new BezierCurve[4];
            double x = center.GetX();
            double y = center.GetY();
            approximation[0] = new BezierCurve(JavaUtil.ArraysAsList(new Point(x, y + radius), new Point(x + radius * 
                CIRCLE_APPROXIMATION_CONST, y + radius), new Point(x + radius, y + radius * CIRCLE_APPROXIMATION_CONST
                ), new Point(x + radius, y)));
            approximation[1] = new BezierCurve(JavaUtil.ArraysAsList(new Point(x + radius, y), new Point(x + radius, y
                 - radius * CIRCLE_APPROXIMATION_CONST), new Point(x + radius * CIRCLE_APPROXIMATION_CONST, y - radius
                ), new Point(x, y - radius)));
            approximation[2] = new BezierCurve(JavaUtil.ArraysAsList(new Point(x, y - radius), new Point(x - radius * 
                CIRCLE_APPROXIMATION_CONST, y - radius), new Point(x - radius, y - radius * CIRCLE_APPROXIMATION_CONST
                ), new Point(x - radius, y)));
            approximation[3] = new BezierCurve(JavaUtil.ArraysAsList(new Point(x - radius, y), new Point(x - radius, y
                 + radius * CIRCLE_APPROXIMATION_CONST), new Point(x - radius * CIRCLE_APPROXIMATION_CONST, y + radius
                ), new Point(x, y + radius)));
            return approximation;
        }

        private static Point[] TransformPoints(Matrix transformationMatrix, bool inverse, params Point[] points) {
            AffineTransform t = new AffineTransform(transformationMatrix.Get(Matrix.I11), transformationMatrix.Get(Matrix
                .I12), transformationMatrix.Get(Matrix.I21), transformationMatrix.Get(Matrix.I22), transformationMatrix
                .Get(Matrix.I31), transformationMatrix.Get(Matrix.I32));
            Point[] transformed = new Point[points.Length];
            if (inverse) {
                try {
                    t = t.CreateInverse();
                }
                catch (NoninvertibleTransformException e) {
                    throw new PdfException(CleanupExceptionMessageConstant.NONINVERTIBLE_MATRIX_CANNOT_BE_PROCESSED, e);
                }
            }
            t.Transform(points, 0, transformed, 0, points.Length);
            return transformed;
        }

        /// <summary>Get the bounding box of a TextRenderInfo object.</summary>
        /// <param name="renderInfo">input TextRenderInfo object</param>
        private static Point[] GetTextRectangle(TextRenderInfo renderInfo) {
            LineSegment ascent = renderInfo.GetAscentLine();
            LineSegment descent = renderInfo.GetDescentLine();
            return new Point[] { new Point(ascent.GetStartPoint().Get(0), ascent.GetStartPoint().Get(1)), new Point(ascent
                .GetEndPoint().Get(0), ascent.GetEndPoint().Get(1)), new Point(descent.GetEndPoint().Get(0), descent.GetEndPoint
                ().Get(1)), new Point(descent.GetStartPoint().Get(0), descent.GetStartPoint().Get(1)) };
        }

        /// <summary>Convert a Rectangle object into 4 Points</summary>
        /// <param name="rect">input Rectangle</param>
        private static Point[] GetRectangleVertices(Rectangle rect) {
            return new Point[] { new Point(rect.GetLeft(), rect.GetBottom()), new Point(rect.GetRight(), rect.GetBottom
                ()), new Point(rect.GetRight(), rect.GetTop()), new Point(rect.GetLeft(), rect.GetTop()) };
        }

        /// <summary>Calculate the intersection of 2 Rectangles.</summary>
        /// <param name="rect1">first Rectangle</param>
        /// <param name="rect2">second Rectangle</param>
        private static Rectangle GetRectanglesIntersection(Rectangle rect1, Rectangle rect2) {
            float x1 = Math.Max(rect1.GetLeft(), rect2.GetLeft());
            float y1 = Math.Max(rect1.GetBottom(), rect2.GetBottom());
            float x2 = Math.Min(rect1.GetRight(), rect2.GetRight());
            float y2 = Math.Min(rect1.GetTop(), rect2.GetTop());
            return (x2 - x1 > 0 && y2 - y1 > 0) ? new Rectangle(x1, y1, x2 - x1, y2 - y1) : null;
        }

        private static ClipperBridge GetClipperBridge(Path path, IList<Point[]> transfRectVerticesList) {
            IList<Point> pointsList = new List<Point>();
            foreach (Subpath subpath in path.GetSubpaths()) {
                if (!subpath.IsSinglePointClosed() && !subpath.IsSinglePointOpen()) {
                    pointsList.AddAll(subpath.GetPiecewiseLinearApproximation());
                }
            }
            foreach (Point[] transfRectVertices in transfRectVerticesList) {
                pointsList.AddAll(JavaUtil.ArraysAsList(transfRectVertices));
            }
            return new ClipperBridge(pointsList.ToArray(new Point[0]));
        }

//\cond DO_NOT_DOCUMENT
        /// <summary>Generic class representing the result of filtering an object of type T.</summary>
        internal class FilterResult<T> {
            private readonly bool isModified;

            private readonly T filterResult;

            public FilterResult(bool isModified, T filterResult) {
                this.isModified = isModified;
                this.filterResult = filterResult;
            }

//\cond DO_NOT_DOCUMENT
            /// <summary>Get whether the object was modified or not.</summary>
            /// <returns>true if the object was modified, false otherwise</returns>
            internal virtual bool IsModified() {
                return isModified;
            }
//\endcond

//\cond DO_NOT_DOCUMENT
            /// <summary>Get the result after filtering</summary>
            /// <returns>the result of filtering an object of type T.</returns>
            internal virtual T GetFilterResult() {
                return filterResult;
            }
//\endcond
        }
//\endcond

        private class ApproxPointList<T> : List<Point> {
            public ApproxPointList()
                : base() {
            }

            public virtual bool AddAllPoints(ICollection<Point> c) {
                Point prevPoint = (Count - 1 < 0 ? null : this[Count - 1]);
                foreach (Point pt in c) {
                    if (!pt.Equals(prevPoint)) {
                        Add(pt);
                        prevPoint = pt;
                    }
                }
                return true;
            }
        }

        // Constants from the standard line representation: Ax+By+C
        private class StandardLine {
//\cond DO_NOT_DOCUMENT
            internal float A;
//\endcond

//\cond DO_NOT_DOCUMENT
            internal float B;
//\endcond

//\cond DO_NOT_DOCUMENT
            internal float C;
//\endcond

//\cond DO_NOT_DOCUMENT
            internal StandardLine(Point p1, Point p2) {
                A = (float)(p2.GetY() - p1.GetY());
                B = (float)(p1.GetX() - p2.GetX());
                C = (float)(p1.GetY() * (-B) - p1.GetX() * A);
            }
//\endcond

//\cond DO_NOT_DOCUMENT
            internal virtual float GetSlope() {
                if (B == 0) {
                    return float.PositiveInfinity;
                }
                return -A / B;
            }
//\endcond

//\cond DO_NOT_DOCUMENT
            internal virtual bool Contains(Point point) {
                return JavaUtil.FloatCompare(Math.Abs(A * (float)point.GetX() + B * (float)point.GetY() + C), 0.1f) < 0;
            }
//\endcond
        }
    }
//\endcond
}
