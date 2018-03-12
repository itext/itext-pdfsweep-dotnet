using System.Collections.Generic;
using iText.IO.Util;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Xobject;

namespace iText.PdfCleanup {
    internal class FilteredImagesCache {
        private IDictionary<PdfIndirectReference, IList<FilteredImagesCache.FilteredImageKey>> cache = new Dictionary
            <PdfIndirectReference, IList<FilteredImagesCache.FilteredImageKey>>();

        internal static FilteredImagesCache.FilteredImageKey CreateFilteredImageKey(ImageRenderInfo image, IList<Rectangle
            > areasToBeCleaned, PdfDocument document) {
            PdfStream imagePdfObject = image.GetImage().GetPdfObject();
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
            private ImageRenderInfo image;

            private IList<Rectangle> cleanedAreas;

            private PdfImageXObject filteredImage;

            internal FilteredImageKey(ImageRenderInfo imageInfo, IList<Rectangle> cleanedAreas) {
                this.image = imageInfo;
                this.cleanedAreas = cleanedAreas;
            }

            internal virtual IList<Rectangle> GetCleanedAreas() {
                return cleanedAreas;
            }

            internal virtual ImageRenderInfo GetImageRenderInfo() {
                return image;
            }

            internal virtual PdfIndirectReference GetImageIndRef() {
                return image.GetImage().GetPdfObject().GetIndirectReference();
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
