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
using iText.Commons.Actions.Contexts;

namespace iText.PdfCleanup {
    /// <summary>
    /// Contains properties for
    /// <see cref="PdfCleanUpTool"/>
    /// operations.
    /// </summary>
    public class CleanUpProperties {
        private IMetaInfo metaInfo;

        private bool processAnnotations;

        /// <summary>Creates default CleanUpProperties instance.</summary>
        public CleanUpProperties() {
            processAnnotations = true;
        }

        /// <summary>Returns metaInfo property.</summary>
        /// <returns>metaInfo property</returns>
        internal virtual IMetaInfo GetMetaInfo() {
            return metaInfo;
        }

        /// <summary>Sets additional meta info.</summary>
        /// <param name="metaInfo">the meta info to set</param>
        public virtual void SetMetaInfo(IMetaInfo metaInfo) {
            this.metaInfo = metaInfo;
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
        public virtual void SetProcessAnnotations(bool processAnnotations) {
            this.processAnnotations = processAnnotations;
        }
    }
}
