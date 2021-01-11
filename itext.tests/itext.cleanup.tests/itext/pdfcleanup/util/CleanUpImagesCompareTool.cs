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
using System.IO;
using System.Text;
using iText.IO.Util;
using iText.Kernel;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Utils;
using iText.Test;

namespace iText.PdfCleanup.Util {
    public class CleanUpImagesCompareTool : CompareTool {
        private ICollection<CompareTool.ObjectPath> ignoredObjectPaths = new LinkedHashSet<CompareTool.ObjectPath>
            ();

        private IDictionary<int, IList<Rectangle>> ignoredImagesAreas = new Dictionary<int, IList<Rectangle>>();

        private String defaultFuzzValue = "0";

        private bool useGs = false;

        private readonly ImageMagickHelper imageMagickHelper = new ImageMagickHelper();

        private readonly GhostscriptHelper ghostscriptHelper = new GhostscriptHelper();

        public CleanUpImagesCompareTool() {
        }

        /// <summary>
        /// Specifies a flag, indicating if the required images to compare should be extracted
        /// as pdf page images using GhostScript.
        /// </summary>
        /// <remarks>
        /// Specifies a flag, indicating if the required images to compare should be extracted
        /// as pdf page images using GhostScript. This is an alternative to the default approach
        /// of extracting image XObject data using iText functionality.
        /// It might be required if iText fails to extract the image.
        /// </remarks>
        /// 
        public virtual void UseGsImageExtracting(bool flag) {
            this.useGs = flag;
        }

        public virtual IDictionary<int, IList<Rectangle>> GetIgnoredImagesAreas() {
            return ignoredImagesAreas;
        }

        public virtual String ExtractAndCompareImages(String outputPdf, String cmpPdf, String outputPath) {
            return ExtractAndCompareImages(cmpPdf, outputPdf, outputPath, defaultFuzzValue);
        }

        public override String CompareByContent(String outPdf, String cmpPdf, String outPath, String differenceImagePrefix
            , IDictionary<int, IList<Rectangle>> ignoredAreas, byte[] outPass, byte[] cmpPass) {
            if (ignoredImagesAreas.Count != 0) {
                if (ignoredAreas != null) {
                    foreach (int pageNumber in ignoredAreas.Keys) {
                        if (ignoredImagesAreas.ContainsKey(pageNumber)) {
                            ignoredImagesAreas.Get(pageNumber).AddAll(ignoredAreas.Get(pageNumber));
                        }
                        else {
                            ignoredImagesAreas.Put(pageNumber, ignoredAreas.Get(pageNumber));
                        }
                    }
                }
                return base.CompareByContent(outPdf, cmpPdf, outPath, differenceImagePrefix, ignoredImagesAreas, outPass, 
                    cmpPass);
            }
            else {
                return base.CompareByContent(outPdf, cmpPdf, outPath, differenceImagePrefix, ignoredAreas, outPass, cmpPass
                    );
            }
        }

        public virtual String ExtractAndCompareImages(String outputPdf, String cmpPdf, String outputPath, String fuzzValue
            ) {
            String outImgPath = outputPath + "out/";
            String cmpImgPath = outputPath + "cmp/";
            ITextTest.CreateOrClearDestinationFolder(outImgPath);
            ITextTest.CreateOrClearDestinationFolder(cmpImgPath);
            IDictionary<int, CleanUpImagesCompareTool.PageImageObjectsPaths> cmpObjectDatas = ExtractImagesFromPdf(cmpPdf
                , cmpImgPath);
            IDictionary<int, CleanUpImagesCompareTool.PageImageObjectsPaths> outObjectDatas = ExtractImagesFromPdf(outputPdf
                , outImgPath);
            if (cmpObjectDatas.Count != outObjectDatas.Count) {
                return "Number of pages differs in out and cmp pdf documents:\nout = " + outObjectDatas.Count + "\ncmp = "
                     + cmpObjectDatas.Count;
            }
            try {
                foreach (int page in cmpObjectDatas.Keys) {
                    InitializeIgnoredObjectPath(cmpObjectDatas.Get(page), outObjectDatas.Get(page));
                }
            }
            catch (Exception e) {
                return e.Message;
            }
            String[] cmpImages = FileUtil.ListFilesInDirectory(cmpImgPath, true);
            String[] outImages = FileUtil.ListFilesInDirectory(outImgPath, true);
            JavaUtil.Sort(cmpImages);
            JavaUtil.Sort(outImages);
            if (cmpImages.Length != outImages.Length) {
                return "Number of images should be the same!";
            }
            StringBuilder resultErrorMessage = new StringBuilder();
            try {
                for (int i = 0; i < cmpImages.Length; i++) {
                    String diffImage = outputPath + "diff_" + new FileInfo(cmpImages[i]).Name;
                    String errorText = CompareImages(outImages[i], cmpImages[i], diffImage, fuzzValue);
                    if (errorText != null) {
                        resultErrorMessage.Append(errorText);
                    }
                }
            }
            catch (Exception e) {
                return e.Message;
            }
            return resultErrorMessage.ToString();
        }

        protected override bool CompareObjects(PdfObject outObj, PdfObject cmpObj, CompareTool.ObjectPath currentPath
            , CompareTool.CompareResult compareResult) {
            if (ignoredObjectPaths.Contains(currentPath)) {
                // Current objects should not be compared, if its ObjectPath is contained in ignored list
                return true;
            }
            return base.CompareObjects(outObj, cmpObj, currentPath, compareResult);
        }

        private String CompareImages(String outImage, String cmpImage, String differenceImage, String fuzzValue) {
            System.Console.Out.Write("Number of different pixels = ");
            if (!imageMagickHelper.RunImageMagickImageCompare(outImage, cmpImage, differenceImage, fuzzValue)) {
                return "Images are visually different! Inspect " + differenceImage + " to see the differences.\n";
            }
            return null;
        }

        private IDictionary<int, CleanUpImagesCompareTool.PageImageObjectsPaths> ExtractImagesFromPdf(String pdf, 
            String outputPath) {
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(pdf));
            IDictionary<int, CleanUpImagesCompareTool.PageImageObjectsPaths> imageObjectDatas = new Dictionary<int, CleanUpImagesCompareTool.PageImageObjectsPaths
                >();
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++) {
                PdfPage page = pdfDoc.GetPage(i);
                CleanUpImagesCompareTool.PageImageObjectsPaths imageObjectData = new CleanUpImagesCompareTool.PageImageObjectsPaths
                    (page.GetPdfObject().GetIndirectReference());
                Stack<CompareTool.ObjectPath.LocalPathItem> baseLocalPath = new Stack<CompareTool.ObjectPath.LocalPathItem
                    >();
                PdfResources pdfResources = page.GetResources();
                if (pdfResources.GetPdfObject().IsIndirect()) {
                    imageObjectData.AddIndirectReference(pdfResources.GetPdfObject().GetIndirectReference());
                }
                else {
                    baseLocalPath.Push(new CompareTool.ObjectPath.DictPathItem(PdfName.Resources));
                }
                PdfDictionary xObjects = pdfResources.GetResource(PdfName.XObject);
                if (xObjects == null) {
                    continue;
                }
                if (xObjects.IsIndirect()) {
                    imageObjectData.AddIndirectReference(xObjects.GetIndirectReference());
                    baseLocalPath.Clear();
                }
                else {
                    baseLocalPath.Push(new CompareTool.ObjectPath.DictPathItem(PdfName.XObject));
                }
                bool isPageToGsExtract = false;
                foreach (PdfName objectName in xObjects.KeySet()) {
                    if (!xObjects.Get(objectName).IsStream() || !PdfName.Image.Equals(xObjects.GetAsStream(objectName).GetAsName
                        (PdfName.Subtype))) {
                        continue;
                    }
                    PdfImageXObject pdfObject = new PdfImageXObject(xObjects.GetAsStream(objectName));
                    baseLocalPath.Push(new CompareTool.ObjectPath.DictPathItem(objectName));
                    if (!useGs) {
                        String extension = pdfObject.IdentifyImageFileExtension();
                        String fileName = outputPath + objectName + "_" + i + "." + extension;
                        CreateImageFromPdfXObject(fileName, pdfObject);
                    }
                    else {
                        isPageToGsExtract = true;
                    }
                    Stack<CompareTool.ObjectPath.LocalPathItem> reversedStack = new Stack<CompareTool.ObjectPath.LocalPathItem
                        >();
                    reversedStack.AddAll(baseLocalPath);
                    Stack<CompareTool.ObjectPath.LocalPathItem> resultStack = new Stack<CompareTool.ObjectPath.LocalPathItem>(
                        );
                    resultStack.AddAll(reversedStack);
                    imageObjectData.AddLocalPath(resultStack);
                    baseLocalPath.Pop();
                }
                if (useGs && isPageToGsExtract) {
                    String fileName = "Page_" + i + "-%03d.png";
                    ghostscriptHelper.RunGhostScriptImageGeneration(pdf, outputPath, fileName, i.ToString());
                }
                CleanUpImagesCompareTool.ImageRenderListener listener = new CleanUpImagesCompareTool.ImageRenderListener();
                PdfCanvasProcessor parser = new PdfCanvasProcessor(listener);
                parser.ProcessPageContent(page);
                ignoredImagesAreas.Put(i, listener.GetImageRectangles());
                imageObjectDatas.Put(i, imageObjectData);
            }
            pdfDoc.Close();
            return imageObjectDatas;
        }

        private void CreateImageFromPdfXObject(String imageFileName, PdfImageXObject imageObject) {
            using (FileStream stream = new FileStream(imageFileName, FileMode.Create)) {
                byte[] image = imageObject.GetImageBytes();
                stream.Write(image, 0, image.Length);
            }
        }

        private void InitializeIgnoredObjectPath(CleanUpImagesCompareTool.PageImageObjectsPaths cmpPageObjects, CleanUpImagesCompareTool.PageImageObjectsPaths
             outPageObjects) {
            try {
                IList<PdfIndirectReference> cmpIndirects = cmpPageObjects.GetIndirectReferences();
                IList<PdfIndirectReference> outIndirects = outPageObjects.GetIndirectReferences();
                PdfIndirectReference baseCmpIndirect = cmpIndirects[0];
                PdfIndirectReference baseOutIndirect = outIndirects[0];
                CompareTool.ObjectPath baseObjectPath = new CompareTool.ObjectPath(baseCmpIndirect, baseCmpIndirect);
                for (int i = 1; i < cmpIndirects.Count; i++) {
                    baseObjectPath.ResetDirectPath(cmpIndirects[i], outIndirects[i]);
                    baseCmpIndirect = cmpIndirects[i];
                    baseOutIndirect = outIndirects[i];
                }
                foreach (Stack<CompareTool.ObjectPath.LocalPathItem> path in cmpPageObjects.GetDirectPaths()) {
                    ignoredObjectPaths.Add(new CompareTool.ObjectPath(baseCmpIndirect, baseOutIndirect, path, baseObjectPath.GetIndirectPath
                        ()));
                }
            }
            catch (Exception) {
                throw new ArgumentException("Out and cmp pdf documents have different object structure");
            }
        }

        private class ImageRenderListener : IEventListener {
            private IList<Rectangle> imageRectangles = new List<Rectangle>();

            public virtual void EventOccurred(IEventData data, EventType type) {
                switch (type) {
                    case EventType.RENDER_IMAGE: {
                        ImageRenderInfo renderInfo = (ImageRenderInfo)data;
                        if (!renderInfo.IsInline()) {
                            Rectangle boundingRect = GetImageBoundingBox(renderInfo.GetImageCtm());
                            imageRectangles.Add(boundingRect);
                        }
                        break;
                    }

                    default: {
                        break;
                    }
                }
            }

            public virtual IList<Rectangle> GetImageRectangles() {
                return imageRectangles;
            }

            public virtual ICollection<EventType> GetSupportedEvents() {
                return null;
            }

            private Rectangle GetImageBoundingBox(Matrix imageConcatMatrix) {
                Point[] points = TransformPoints(imageConcatMatrix, false, new Point(0, 0), new Point(0, 1), new Point(1, 
                    0), new Point(1, 1));
                return Rectangle.CalculateBBox(JavaUtil.ArraysAsList(points));
            }

            private Point[] TransformPoints(Matrix transformationMatrix, bool inverse, params Point[] points) {
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
        }

        private class PageImageObjectsPaths {
            private IList<PdfIndirectReference> pageIndirectReferencesPathToImageResources = new List<PdfIndirectReference
                >();

            private IList<Stack<CompareTool.ObjectPath.LocalPathItem>> directPaths = new List<Stack<CompareTool.ObjectPath.LocalPathItem
                >>();

            public PageImageObjectsPaths(PdfIndirectReference baseIndirectReference) {
                this.pageIndirectReferencesPathToImageResources.Add(baseIndirectReference);
            }

            public virtual void AddLocalPath(Stack<CompareTool.ObjectPath.LocalPathItem> path) {
                this.directPaths.Add(path);
            }

            public virtual void AddIndirectReference(PdfIndirectReference reference) {
                this.pageIndirectReferencesPathToImageResources.Add(reference);
            }

            public virtual IList<Stack<CompareTool.ObjectPath.LocalPathItem>> GetDirectPaths() {
                return this.directPaths;
            }

            public virtual IList<PdfIndirectReference> GetIndirectReferences() {
                return this.pageIndirectReferencesPathToImageResources;
            }
        }
    }
}
