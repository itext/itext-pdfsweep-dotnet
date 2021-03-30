/*
This file is part of the iText (R) project.
Copyright (c) 1998-2021 iText Group NV
Authors: iText Software.

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

namespace iText.PdfCleanup {
    public class CleanUpLogMessageConstant {
        /// <summary>The Constant CANNOT_OBTAIN_IMAGE_INFO_AFTER_FILTERING.</summary>
        public const String CANNOT_OBTAIN_IMAGE_INFO_AFTER_FILTERING = "Cannot obtain image info after filtering.";

        /// <summary>The Constant FAILED_TO_PROCESS_A_TRANSFORMATION_MATRIX.</summary>
        public const String FAILED_TO_PROCESS_A_TRANSFORMATION_MATRIX = "Failed to process a transformation matrix which is noninvertible. Some content may be placed not as expected.";

        /// <summary>The Constant IMAGE_MASK_CLEAN_UP_NOT_SUPPORTED.</summary>
        public const String IMAGE_MASK_CLEAN_UP_NOT_SUPPORTED = "Partial clean up of transparent images with mask encoded with one of the following filters is not supported: JBIG2Decode, DCTDecode, JPXDecode. Image will become non-transparent.";

        /// <summary>The Constant REDACTION_OF_ANNOTATION_TYPE_WATERMARK_IS_NOT_SUPPORTED.</summary>
        public const String REDACTION_OF_ANNOTATION_TYPE_WATERMARK_IS_NOT_SUPPORTED = "Redaction of annotation subtype /Watermark is not supported";

        private CleanUpLogMessageConstant() {
        }
    }
}
