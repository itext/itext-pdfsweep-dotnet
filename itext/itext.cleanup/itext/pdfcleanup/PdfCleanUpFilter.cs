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
using Common.Logging;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<iText.Kernel.Pdf.Canvas.Parser.ClipperLib.IntPoint>>;
using iText.IO.Image;
using iText.IO.Util;
using iText.Kernel;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser.ClipperLib;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Xobject;
using iText.PdfCleanup.Util;

namespace iText.PdfCleanup {
    [System.ObsoleteAttribute(@"This class will be changed to package-private in 7.2.")]
    public class PdfCleanUpFilter {
        private static readonly ILog logger = LogManager.GetLogger(typeof(iText.PdfCleanup.PdfCleanUpFilter));

        /* There is no exact representation of the circle using Bezier curves.
        * But, for a Bezier curve with n segments the optimal distance to the control points,
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

        private IList<Rectangle> regions;

        public PdfCleanUpFilter(IList<Rectangle> regions) {
            this.regions = regions;
        }

        /// <summary>Filter a TextRenderInfo object.</summary>
        /// <param name="text">the TextRenderInfo to be filtered</param>
        /// <returns>
        /// a
        /// <see cref="FilterResult{T}"/>
        /// object with filtered text.
        /// </returns>
        [System.ObsoleteAttribute(@"This method will be changed to package-private in 7.2.")]
        public virtual PdfCleanUpFilter.FilterResult<PdfArray> FilterText(TextRenderInfo text) {
            PdfTextArray textArray = new PdfTextArray();
            if (IsTextNotToBeCleaned(text)) {
                return new PdfCleanUpFilter.FilterResult<PdfArray>(false, new PdfArray(text.GetPdfString()));
            }
            foreach (TextRenderInfo ri in text.GetCharacterRenderInfos()) {
                if (IsTextNotToBeCleaned(ri)) {
                    textArray.Add(ri.GetPdfString());
                }
                else {
                    textArray.Add(new PdfNumber(-ri.GetUnscaledWidth() * 1000f / (text.GetFontSize() * text.GetHorizontalScaling
                        () / 100)));
                }
            }
            return new PdfCleanUpFilter.FilterResult<PdfArray>(true, textArray);
        }

        internal virtual FilteredImagesCache.FilteredImageKey CreateFilteredImageKey(PdfImageXObject image, Matrix
             imageCtm, PdfDocument document) {
            return FilteredImagesCache.CreateFilteredImageKey(image, GetImageAreasToBeCleaned(imageCtm), document);
        }

        /// <summary>Filter an ImageRenderInfo object.</summary>
        /// <param name="image">the ImageRenderInfo object to be filtered</param>
        /// <returns>
        /// an
        /// <see cref="FilterResult{T}"/>
        /// object with filtered image data.filterStrokePath
        /// </returns>
        [System.ObsoleteAttribute(@"This method will be changed to package-private in 7.2.")]
        public virtual PdfCleanUpFilter.FilterResult<ImageData> FilterImage(ImageRenderInfo image) {
            return FilterImage(image.GetImage(), GetImageAreasToBeCleaned(image.GetImageCtm()));
        }

        internal virtual PdfCleanUpFilter.FilterResult<ImageData> FilterImage(FilteredImagesCache.FilteredImageKey
             imageKey) {
            return FilterImage(imageKey.GetImageXObject(), imageKey.GetCleanedAreas());
        }

        private PdfCleanUpFilter.FilterResult<ImageData> FilterImage(PdfImageXObject image, IList<Rectangle> imageAreasToBeCleaned
            ) {
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

        /// <summary>Filter a PathRenderInfo object</summary>
        /// <param name="path">the PathRenderInfo object to be filtered</param>
        /// <returns>
        /// a filtered
        /// <see cref="iText.Kernel.Geom.Path"/>
        /// object.
        /// </returns>
        [System.ObsoleteAttribute(@"This method will be changed to package-private in 7.2.")]
        public virtual Path FilterStrokePath(PathRenderInfo path) {
            PdfArray dashPattern = path.GetLineDashPattern();
            LineDashPattern lineDashPattern = new LineDashPattern(dashPattern.GetAsArray(0), dashPattern.GetAsNumber(1
                ).FloatValue());
            return FilterStrokePath(path.GetPath(), path.GetCtm(), path.GetLineWidth(), path.GetLineCapStyle(), path.GetLineJoinStyle
                (), path.GetMiterLimit(), lineDashPattern);
        }

        /// <summary>Filter a PathRenderInfo object</summary>
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
        [System.ObsoleteAttribute(@"This method will be changed to package-private in 7.2.")]
        public virtual Path FilterFillPath(PathRenderInfo path, int fillingRule) {
            return FilterFillPath(path.GetPath(), path.GetCtm(), fillingRule);
        }

        /// <summary>Note: this method will close all unclosed subpaths of the passed path.</summary>
        /// <param name="path">the PathRenderInfo object to be filtered.</param>
        /// <param name="ctm">
        /// a
        /// <see cref="iText.Kernel.Geom.Path"/>
        /// transformation matrix.
        /// </param>
        /// <param name="fillingRule">If the subpath is contour, pass any value.</param>
        /// <returns>
        /// a filtered
        /// <see cref="iText.Kernel.Geom.Path"/>
        /// object.
        /// </returns>
        [System.ObsoleteAttribute(@"This method will be changed to private in 7.2")]
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
                    if (!(e.InnerException is NoninvertibleTransformException)) {
                        throw;
                    }
                    else {
                        logger.Error(MessageFormatUtil.Format(CleanUpLogMessageConstant.FAILED_TO_PROCESS_A_TRANSFORMATION_MATRIX)
                            );
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

        /// <summary>Returns whether the given TextRenderInfo object needs to be cleaned up</summary>
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
            ClipperOffset offset = new ClipperOffset(miterLimit, iText.PdfCleanup.PdfCleanUpTool.arcTolerance * iText.PdfCleanup.PdfCleanUpTool
                .floatMultiplier);
            IList<Subpath> degenerateSubpaths = ClipperBridge.AddPath(offset, path, joinType, endType);
            PolyTree resultTree = new PolyTree();
            offset.Execute(resultTree, lineWidth * iText.PdfCleanup.PdfCleanUpTool.floatMultiplier / 2);
            Path offsetedPath = ClipperBridge.ConvertToPath(resultTree);
            if (degenerateSubpaths.Count > 0) {
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
            return FilterFillPath(offsetedPath, ctm, PdfCanvasConstants.FillingRule.NONZERO_WINDING);
        }

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
        internal static bool CheckIfRectanglesIntersect(Point[] rect1, Point[] rect2) {
            Clipper clipper = new Clipper();
            // If the redaction area is degenerate, the result will be false
            if (!ClipperBridge.AddPolygonToClipper(clipper, rect2, PolyType.CLIP)) {
                // If the content area is not degenerate (and the redaction area is), let's return false:
                // even if they overlaps somehow, we do not consider it as an intersection.
                // If the content area is degenerate, let's process this case specifically
                if (!ClipperBridge.AddPolygonToClipper(clipper, rect1, PolyType.SUBJECT)) {
                    // Clipper fails to process degenerate redaction areas. However that's vital for pdfAutoSweep,
                    // because in some cases (for example, noninvertible cm) the text's area might be degenerate,
                    // but we still need to sweep the content.
                    // The idea is as follows:
                    // a) if the degenerate redaction area represents a point, there is no intersection
                    // b) if the degenerate redaction area represents a line, let's check that there the redaction line
                    // equals to one of the edges of the content's area. That is implemented in respect to area generation,
                    // because the redaction line corresponds to the descent line of the content.
                    if (!ClipperBridge.AddPolylineSubjectToClipper(clipper, rect2)) {
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
                    for (int i = 0; i < rect1.Length; i++) {
                        if (IsPointOnALineSegment(rect1[i], startPoint, endPoint, true)) {
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
                return !CheckIfIntersectionRectangleDegenerate(iText.Kernel.Pdf.Canvas.Parser.ClipperLib.Clipper.GetBounds
                    (paths), false) && !paths.IsEmpty();
            }
            else {
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
                Paths paths = iText.Kernel.Pdf.Canvas.Parser.ClipperLib.Clipper.PolyTreeToPaths(polyTree);
                return !CheckIfIntersectionRectangleDegenerate(iText.Kernel.Pdf.Canvas.Parser.ClipperLib.Clipper.GetBounds
                    (paths), true) && !paths.IsEmpty();
            }
        }

        /// <summary>Checks if the input intersection rectangle is degenerate.</summary>
        /// <remarks>
        /// Checks if the input intersection rectangle is degenerate.
        /// In case of intersection subject is degenerate (isIntersectSubjectDegenerate
        /// is true) and it is included into intersecting rectangle, this method returns false,
        /// despite of the intersection rectangle is degenerate.
        /// </remarks>
        /// <param name="rect">intersection rectangle</param>
        /// <param name="isIntersectSubjectDegenerate">
        /// value, specifying if the intersection subject
        /// is degenerate.
        /// </param>
        /// <returns>true - if the intersection rectangle is degenerate.</returns>
        private static bool CheckIfIntersectionRectangleDegenerate(IntRect rect, bool isIntersectSubjectDegenerate
            ) {
            float width = (float)(Math.Abs(rect.left - rect.right) / ClipperBridge.floatMultiplier);
            float height = (float)(Math.Abs(rect.top - rect.bottom) / ClipperBridge.floatMultiplier);
            return isIntersectSubjectDegenerate ? (width < EPS && height < EPS) : (width < EPS || height < EPS);
        }

        private static bool IsPointOnALineSegment(Point currPoint, Point linePoint1, Point linePoint2, bool isBetweenLinePoints
            ) {
            double dxc = currPoint.x - linePoint1.x;
            double dyc = currPoint.y - linePoint1.y;
            double dxl = linePoint2.x - linePoint1.x;
            double dyl = linePoint2.y - linePoint1.y;
            double cross = dxc * dyl - dyc * dxl;
            // if point is on a line, let's check whether it's between provided line points
            if (Math.Abs(cross) <= EPS) {
                if (isBetweenLinePoints) {
                    if (Math.Abs(dxl) >= Math.Abs(dyl)) {
                        return dxl > 0 ? linePoint1.x - EPS <= currPoint.x && currPoint.x <= linePoint2.x + EPS : linePoint2.x - EPS
                             <= currPoint.x && currPoint.x <= linePoint1.x + EPS;
                    }
                    else {
                        return dyl > 0 ? linePoint1.y - EPS <= currPoint.y && currPoint.y <= linePoint2.y + EPS : linePoint2.y - EPS
                             <= currPoint.y && currPoint.y <= linePoint1.y + EPS;
                    }
                }
                else {
                    return true;
                }
            }
            return false;
        }

        internal static bool ImageSupportsDirectCleanup(PdfImageXObject image) {
            PdfObject filter = image.GetPdfObject().Get(PdfName.Filter);
            bool supportedFilterForDirectCleanup = IsSupportedFilterForDirectImageCleanup(filter);
            bool deviceGrayOrNoCS = PdfName.DeviceGray.Equals(image.GetPdfObject().GetAsName(PdfName.ColorSpace)) || !
                image.GetPdfObject().ContainsKey(PdfName.ColorSpace);
            return deviceGrayOrNoCS && supportedFilterForDirectCleanup;
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
            Point[] points = TransformPoints(imageCtm, false, new Point(0, 0), new Point(0, 1), new Point(1, 0), new Point
                (1, 1));
            return Rectangle.CalculateBBox(JavaUtil.ArraysAsList(points));
        }

        /// <summary>Transforms the given Rectangle into the image coordinate system which is [0,1]x[0,1] by default</summary>
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
        private byte[] ProcessImageDirectly(PdfImageXObject image, IList<Rectangle> imageAreasToBeCleaned) {
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

        /// <summary>Approximates a given Path with a List of Point objects</summary>
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
            return new Point[] { new Point(ascent.GetStartPoint().Get(0), ascent.GetStartPoint().Get(1)), new Point(ascent
                .GetEndPoint().Get(0), ascent.GetEndPoint().Get(1)), new Point(descent.GetEndPoint().Get(0), descent.GetEndPoint
                ().Get(1)), new Point(descent.GetStartPoint().Get(0), descent.GetStartPoint().Get(1)) };
        }

        /// <summary>Convert a Rectangle object into 4 Points</summary>
        /// <param name="rect">input Rectangle</param>
        private static Point[] GetRectangleVertices(Rectangle rect) {
            Point[] points = new Point[] { new Point(rect.GetLeft(), rect.GetBottom()), new Point(rect.GetRight(), rect
                .GetBottom()), new Point(rect.GetRight(), rect.GetTop()), new Point(rect.GetLeft(), rect.GetTop()) };
            return points;
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

        /// <summary>Generic class representing the result of filtering an object of type T</summary>
        [System.ObsoleteAttribute(@"this class will be changed to package-private in 7.2.")]
        public class FilterResult<T> {
            private bool isModified;

            private T filterResult;

            public FilterResult(bool isModified, T filterResult) {
                this.isModified = isModified;
                this.filterResult = filterResult;
            }

            /// <summary>Get whether the object was modified or not</summary>
            /// <returns>true if the object was modified, false otherwise</returns>
            [System.ObsoleteAttribute(@"this method will be changed to package-private in 7.2.")]
            public virtual bool IsModified() {
                return isModified;
            }

            /// <summary>Get the result after filtering</summary>
            /// <returns>the result of filtering an object of type T.</returns>
            [System.ObsoleteAttribute(@"this method will be changed to package-private in 7.2.")]
            public virtual T GetFilterResult() {
                return filterResult;
            }
        }

        // Constants from the standard line representation: Ax+By+C
        private class StandardLine {
            internal float A;

            internal float B;

            internal float C;

            internal StandardLine(Point p1, Point p2) {
                A = (float)(p2.GetY() - p1.GetY());
                B = (float)(p1.GetX() - p2.GetX());
                C = (float)(p1.GetY() * (-B) - p1.GetX() * A);
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
