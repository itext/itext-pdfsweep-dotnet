using System;

namespace iText.PdfCleanup.Exceptions {
    /// <summary>Class that bundles all the error message templates as constants.</summary>
    public sealed class CleanupExceptionMessageConstant {
        public const String DEFAULT_APPEARANCE_NOT_FOUND = "DefaultAppearance is required but not found";

        public const String NONINVERTIBLE_MATRIX_CANNOT_BE_PROCESSED = "A noninvertible matrix has been parsed. The behaviour is unpredictable.";

        public const String PDF_DOCUMENT_MUST_BE_OPENED_IN_STAMPING_MODE = "PdfDocument must be opened in stamping mode.";
    }
}
