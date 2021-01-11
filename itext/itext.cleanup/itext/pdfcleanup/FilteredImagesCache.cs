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
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;

namespace iText.PdfCleanup {
    internal class FilteredImagesCache {
        private IDictionary<PdfIndirectReference, IList<FilteredImagesCache.FilteredImageKey>> cache = new Dictionary
            <PdfIndirectReference, IList<FilteredImagesCache.FilteredImageKey>>();

        internal static FilteredImagesCache.FilteredImageKey CreateFilteredImageKey(PdfImageXObject image, IList<Rectangle
            > areasToBeCleaned, PdfDocument document) {
            PdfStream imagePdfObject = image.GetPdfObject();
            if (imagePdfObject.GetIndirectReference() == null) {
                imagePdfObject.MakeIndirect(document);
            }
            return new FilteredImagesCache.FilteredImageKey(image, areasToBeCleaned);
        }

        /// <summary>Retrieves saved result of image filtering based on given set of cleaning areas.</summary>
        /// <remarks>
        /// Retrieves saved result of image filtering based on given set of cleaning areas.
        /// This won't handle the case when same filtering result is produced by different sets of areas,
        /// e.g. if one set is { (0, 0, 50, 100), (50, 0, 50, 100)} and another one is {(0, 0, 100, 100)},
        /// even though filtering results are essentially the same, current
        /// <see cref="FilteredImagesCache"/>
        /// will treat this two cases as different filtering results.
        /// </remarks>
        /// <param name="imageKey">the defining filtering case</param>
        /// <returns>
        /// result of image filtering based on given set of cleaning areas if such was already processed and saved,
        /// null otherwise.
        /// </returns>
        internal virtual PdfImageXObject Get(FilteredImagesCache.FilteredImageKey imageKey) {
            IList<FilteredImagesCache.FilteredImageKey> cachedFilteredImageKeys = cache.Get(imageKey.GetImageIndRef());
            if (cachedFilteredImageKeys != null) {
                foreach (FilteredImagesCache.FilteredImageKey cacheKey in cachedFilteredImageKeys) {
                    if (RectanglesEqualWithEps(cacheKey.GetCleanedAreas(), imageKey.GetCleanedAreas())) {
                        return cacheKey.GetFilteredImage();
                    }
                }
            }
            return null;
        }

        internal virtual void Put(FilteredImagesCache.FilteredImageKey imageKey, PdfImageXObject filteredImage) {
            if (imageKey.GetCleanedAreas() == null || imageKey.GetCleanedAreas().IsEmpty()) {
                return;
            }
            IList<FilteredImagesCache.FilteredImageKey> filteredImageKeys = cache.Get(imageKey.GetImageIndRef());
            if (filteredImageKeys == null) {
                cache.Put(imageKey.GetImageIndRef(), filteredImageKeys = new List<FilteredImagesCache.FilteredImageKey>());
            }
            filteredImageKeys.Add(imageKey);
            imageKey.SetFilteredImage(filteredImage);
        }

        private bool RectanglesEqualWithEps(IList<Rectangle> cacheRects, IList<Rectangle> keyRects) {
            if (keyRects == null || cacheRects.Count != keyRects.Count) {
                return false;
            }
            ICollection<Rectangle> cacheRectsSet = new LinkedHashSet<Rectangle>(cacheRects);
            foreach (Rectangle keyArea in keyRects) {
                bool found = false;
                foreach (Rectangle cacheArea in cacheRectsSet) {
                    if (keyArea.EqualsWithEpsilon(cacheArea)) {
                        found = true;
                        cacheRectsSet.Remove(cacheArea);
                        break;
                    }
                }
                if (!found) {
                    break;
                }
            }
            return cacheRectsSet.IsEmpty();
        }

        internal class FilteredImageKey {
            private PdfImageXObject image;

            private IList<Rectangle> cleanedAreas;

            private PdfImageXObject filteredImage;

            internal FilteredImageKey(PdfImageXObject image, IList<Rectangle> cleanedAreas) {
                this.image = image;
                this.cleanedAreas = cleanedAreas;
            }

            internal virtual IList<Rectangle> GetCleanedAreas() {
                return cleanedAreas;
            }

            internal virtual PdfImageXObject GetImageXObject() {
                return image;
            }

            internal virtual PdfIndirectReference GetImageIndRef() {
                return image.GetPdfObject().GetIndirectReference();
            }

            internal virtual PdfImageXObject GetFilteredImage() {
                return filteredImage;
            }

            internal virtual void SetFilteredImage(PdfImageXObject filteredImage) {
                this.filteredImage = filteredImage;
            }
        }
    }
}
