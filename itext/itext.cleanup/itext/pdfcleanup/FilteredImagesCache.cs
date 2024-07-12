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
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;

namespace iText.PdfCleanup {
//\cond DO_NOT_DOCUMENT
    internal class FilteredImagesCache {
        private IDictionary<PdfIndirectReference, IList<FilteredImagesCache.FilteredImageKey>> cache = new Dictionary
            <PdfIndirectReference, IList<FilteredImagesCache.FilteredImageKey>>();

//\cond DO_NOT_DOCUMENT
        internal static FilteredImagesCache.FilteredImageKey CreateFilteredImageKey(PdfImageXObject image, IList<Rectangle
            > areasToBeCleaned, PdfDocument document) {
            PdfStream imagePdfObject = image.GetPdfObject();
            if (imagePdfObject.GetIndirectReference() == null) {
                imagePdfObject.MakeIndirect(document);
            }
            return new FilteredImagesCache.FilteredImageKey(image, areasToBeCleaned);
        }
//\endcond

//\cond DO_NOT_DOCUMENT
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
//\endcond

//\cond DO_NOT_DOCUMENT
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
//\endcond

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

//\cond DO_NOT_DOCUMENT
        internal class FilteredImageKey {
            private PdfImageXObject image;

            private IList<Rectangle> cleanedAreas;

            private PdfImageXObject filteredImage;

//\cond DO_NOT_DOCUMENT
            internal FilteredImageKey(PdfImageXObject image, IList<Rectangle> cleanedAreas) {
                this.image = image;
                this.cleanedAreas = cleanedAreas;
            }
//\endcond

//\cond DO_NOT_DOCUMENT
            internal virtual IList<Rectangle> GetCleanedAreas() {
                return cleanedAreas;
            }
//\endcond

//\cond DO_NOT_DOCUMENT
            internal virtual PdfImageXObject GetImageXObject() {
                return image;
            }
//\endcond

//\cond DO_NOT_DOCUMENT
            internal virtual PdfIndirectReference GetImageIndRef() {
                return image.GetPdfObject().GetIndirectReference();
            }
//\endcond

//\cond DO_NOT_DOCUMENT
            internal virtual PdfImageXObject GetFilteredImage() {
                return filteredImage;
            }
//\endcond

//\cond DO_NOT_DOCUMENT
            internal virtual void SetFilteredImage(PdfImageXObject filteredImage) {
                this.filteredImage = filteredImage;
            }
//\endcond
        }
//\endcond
    }
//\endcond
}
