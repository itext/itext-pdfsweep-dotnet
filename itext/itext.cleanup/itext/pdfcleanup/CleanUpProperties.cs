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
using System;
using iText.Commons.Actions.Contexts;
using iText.PdfCleanup.Exceptions;

namespace iText.PdfCleanup {
    /// <summary>
    /// Contains properties for
    /// <see cref="PdfCleanUpTool"/>
    /// operations.
    /// </summary>
    public class CleanUpProperties {
        private IMetaInfo metaInfo;

        private bool processAnnotations;

        private double? overlapRatio;

        private PathOffsetApproximationProperties offsetProperties = new PathOffsetApproximationProperties();

        /// <summary>Creates default CleanUpProperties instance.</summary>
        public CleanUpProperties() {
            processAnnotations = true;
        }

//\cond DO_NOT_DOCUMENT
        /// <summary>Returns metaInfo property.</summary>
        /// <returns>metaInfo property</returns>
        internal virtual IMetaInfo GetMetaInfo() {
            return metaInfo;
        }
//\endcond

        /// <summary>Sets additional meta info.</summary>
        /// <param name="metaInfo">the meta info to set</param>
        /// <returns>
        /// this
        /// <see cref="CleanUpProperties"/>
        /// instance
        /// </returns>
        public virtual iText.PdfCleanup.CleanUpProperties SetMetaInfo(IMetaInfo metaInfo) {
            this.metaInfo = metaInfo;
            return this;
        }

        /// <summary>Check if page annotations will be processed.</summary>
        /// <remarks>
        /// Check if page annotations will be processed.
        /// Default:
        /// <see langword="true"/>.
        /// </remarks>
        /// <returns>
        /// 
        /// <see langword="true"/>
        /// if annotations will be processed by the
        /// <see cref="PdfCleanUpTool"/>
        /// </returns>
        public virtual bool IsProcessAnnotations() {
            return processAnnotations;
        }

        /// <summary>Set if page annotations will be processed.</summary>
        /// <remarks>
        /// Set if page annotations will be processed.
        /// Default processing behaviour: remove annotation if there is overlap with a redaction region.
        /// </remarks>
        /// <param name="processAnnotations">is page annotations will be processed</param>
        /// <returns>
        /// this
        /// <see cref="CleanUpProperties"/>
        /// instance
        /// </returns>
        public virtual iText.PdfCleanup.CleanUpProperties SetProcessAnnotations(bool processAnnotations) {
            this.processAnnotations = processAnnotations;
            return this;
        }

        /// <summary>Gets the overlap ratio.</summary>
        /// <remarks>
        /// Gets the overlap ratio.
        /// This is a value between 0 and 1 that indicates how much the content region should overlap with the redaction
        /// area to be removed.
        /// </remarks>
        /// <returns>
        /// the overlap ratio or
        /// <see langword="null"/>
        /// if it has not been set.
        /// </returns>
        public virtual double? GetOverlapRatio() {
            return overlapRatio;
        }

        /// <summary>Sets the overlap ratio.</summary>
        /// <remarks>
        /// Sets the overlap ratio.
        /// This is a value between 0 and 1 that indicates how much the content region should overlap with the
        /// redaction area to be removed.
        /// <para />
        /// Example: if the overlap ratio is set to 0.3, the content region will be removed if it overlaps with
        /// the redaction area by at least 30%.
        /// </remarks>
        /// <param name="overlapRatio">the overlap ratio to set</param>
        /// <returns>
        /// this
        /// <see cref="CleanUpProperties"/>
        /// instance
        /// </returns>
        public virtual iText.PdfCleanup.CleanUpProperties SetOverlapRatio(double? overlapRatio) {
            if (overlapRatio == null) {
                this.overlapRatio = null;
                return this;
            }
            if (overlapRatio <= 0 || overlapRatio > 1) {
                throw new ArgumentException(CleanupExceptionMessageConstant.OVERLAP_RATIO_SHOULD_BE_IN_RANGE);
            }
            this.overlapRatio = overlapRatio;
            return this;
        }

        /// <summary>
        /// Get
        /// <see cref="PathOffsetApproximationProperties"/>
        /// specifying approximation parameters for
        /// <see cref="iText.Kernel.Pdf.Canvas.Parser.ClipperLib.ClipperOffset"/>
        /// operations.
        /// </summary>
        /// <returns>
        /// 
        /// <see cref="PathOffsetApproximationProperties"/>
        /// parameters
        /// </returns>
        public virtual PathOffsetApproximationProperties GetOffsetProperties() {
            return offsetProperties;
        }

        /// <summary>
        /// Set
        /// <see cref="PathOffsetApproximationProperties"/>
        /// specifying approximation parameters for
        /// <see cref="iText.Kernel.Pdf.Canvas.Parser.ClipperLib.ClipperOffset"/>
        /// operations.
        /// </summary>
        /// <param name="offsetProperties">
        /// 
        /// <see cref="PathOffsetApproximationProperties"/>
        /// to set
        /// </param>
        /// <returns>
        /// this
        /// <see cref="CleanUpProperties"/>
        /// instance
        /// </returns>
        public virtual iText.PdfCleanup.CleanUpProperties SetOffsetProperties(PathOffsetApproximationProperties offsetProperties
            ) {
            this.offsetProperties = offsetProperties;
            return this;
        }
    }
}
