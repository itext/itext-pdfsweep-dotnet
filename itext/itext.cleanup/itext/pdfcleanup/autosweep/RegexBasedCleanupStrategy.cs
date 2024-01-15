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
using System.Text.RegularExpressions;
using iText.Kernel.Colors;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace iText.PdfCleanup.Autosweep {
    /// <summary>This class represents a regular expression based cleanup strategy</summary>
    public class RegexBasedCleanupStrategy : RegexBasedLocationExtractionStrategy, ICleanupStrategy {
        private Regex pattern;

        private Color redactionColor = ColorConstants.BLACK;

        /// <summary>Creates an object of regular expression based cleanup strategy.</summary>
        /// <param name="regex">regular expression on which cleanup strategy will be based</param>
        public RegexBasedCleanupStrategy(String regex)
            : base(regex) {
            this.pattern = iText.Commons.Utils.StringUtil.RegexCompile(regex);
        }

        /// <summary>Creates an object of regular expression based cleanup strategy.</summary>
        /// <param name="pattern">
        /// 
        /// <see cref="System.Text.RegularExpressions.Regex"/>
        /// pattern on which cleanup strategy will be based
        /// </param>
        public RegexBasedCleanupStrategy(Regex pattern)
            : base(pattern) {
            this.pattern = pattern;
        }

        /// <summary><inheritDoc/></summary>
        public virtual Color GetRedactionColor(IPdfTextLocation location) {
            return redactionColor;
        }

        /// <summary>Sets the color in which redaction is to take place.</summary>
        /// <param name="color">the color in which redaction is to take place</param>
        /// <returns>
        /// this
        /// <see cref="RegexBasedCleanupStrategy">strategy</see>
        /// </returns>
        public virtual iText.PdfCleanup.Autosweep.RegexBasedCleanupStrategy SetRedactionColor(Color color) {
            this.redactionColor = color;
            return this;
        }

        /// <summary>
        /// Returns an
        /// <see cref="ICleanupStrategy"/>
        /// object which is set to this regular pattern and redaction color.
        /// </summary>
        /// <returns>
        /// a reset
        /// <see cref="ICleanupStrategy">cleanup strategy</see>
        /// </returns>
        public virtual ICleanupStrategy Reset() {
            return new iText.PdfCleanup.Autosweep.RegexBasedCleanupStrategy(pattern).SetRedactionColor(redactionColor);
        }
    }
}
