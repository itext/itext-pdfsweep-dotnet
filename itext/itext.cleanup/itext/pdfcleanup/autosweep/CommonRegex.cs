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
using System.Text.RegularExpressions;

namespace iText.PdfCleanup.Autosweep {
    /// <summary>This class contains some of the more common regular expressions to be used for redaction.</summary>
    /// <remarks>
    /// This class contains some of the more common regular expressions to be used for redaction.
    /// Most of these are copied verbatim from the "regular expression cookbook 2nd edition".
    /// </remarks>
    public class CommonRegex {
        /*
        * NUMBERS
        */
        public static Regex MODERN_ROMAN_NUMERALS_STRICT = iText.Commons.Utils.StringUtil.RegexCompile("(?=[MDCLXVI])M*(C[MD]|D?C{0,3})(X[CL]|L?X{0,3})(I[XV]|V?I{0,3})"
            );

        public static Regex MODERN_ROMAN_NUMERALS_FLEXIBLE = iText.Commons.Utils.StringUtil.RegexCompile("(?=[MDCLXVI])M*(C[MD]|D?C*)(X[CL]|L?X*)(I[XV]|V?I*)"
            );

        /*
        * MISC.
        */
        public static Regex US_SOCIAL_SECURITY_NUMBER = iText.Commons.Utils.StringUtil.RegexCompile("\\d{3}-\\d{2}-\\d{4}"
            );

        public static Regex US_ZIP_CODE = iText.Commons.Utils.StringUtil.RegexCompile("\\d{5}(-\\d{4})?");

        public static Regex US_CURRENCY = iText.Commons.Utils.StringUtil.RegexCompile("\\$(\\d{1,3}(\\,\\d{3})*|(\\d+))(\\.\\d{2})?"
            );

        public static Regex CANADA_SOCIAL_SECURITY_NUMBER = iText.Commons.Utils.StringUtil.RegexCompile("\\d{3}-\\d{3}-\\d{3}"
            );

        public static Regex CANADA_ZIP_CODE = iText.Commons.Utils.StringUtil.RegexCompile("(?!.*[DFIOQU])[A-VXY][0-9][A-Z] ?[0-9][A-Z][0-9]"
            );

        public static Regex UK_SOCIAL_SECURITY_NUMBER = iText.Commons.Utils.StringUtil.RegexCompile("[A-Z]{2}\\d{6}[A-Z]]"
            );

        public static Regex UK_ZIP_CODE = iText.Commons.Utils.StringUtil.RegexCompile("[A-Z]{1,2}[0-9R][0-9A-Z]? [0-9][ABD-HJLNP-UW-Z]{2}"
            );

        public static Regex UK_CURRENCY = iText.Commons.Utils.StringUtil.RegexCompile("£(\\d{1,3}(\\,\\d{3})*|(\\d+))(\\.\\d{2})?"
            );

        public static Regex EU_CURRENCY = iText.Commons.Utils.StringUtil.RegexCompile("€(\\d{1,3}(\\,\\d{3})*|(\\d+))(\\.\\d{2})?"
            );

        /*
        * DATE AND TIME
        */
        public static Regex DATE_MM_DD_YYYY = iText.Commons.Utils.StringUtil.RegexCompile("\\d{1,2}[ \\/-]\\d{1,2}[ \\/-]\\d{4}"
            );

        public static Regex DATE_MM_DD_YYYY_HH_MM_SS = iText.Commons.Utils.StringUtil.RegexCompile("\\d{1,2}[ \\/-]\\d{1,2}[ \\/-]\\d{4} \\d{1,2}:\\d{1,2}:\\d{1,2}"
            );

        public static Regex DATE_DD_MM_YYYY = iText.Commons.Utils.StringUtil.RegexCompile("\\d{1,2}[ \\/-]\\d{1,2}[ \\/-]\\d{4}"
            );

        public static Regex DATE_DD_MM_YYYY_HH_MM_SS = iText.Commons.Utils.StringUtil.RegexCompile("\\d{1,2}[ \\/-]\\d{1,2}[ \\/-]\\d{4} \\d{1,2}:\\d{1,2}:\\d{1,2}"
            );

        /*
        * ICT
        */
        public static Regex IPV4_ADDRESS = iText.Commons.Utils.StringUtil.RegexCompile("(?:[0-9]{1,3}\\.){3}[0-9]{1,3}"
            );

        public static Regex IPV6_ADDRESS = iText.Commons.Utils.StringUtil.RegexCompile("(?:[a-fA-F0-9]{1,4}:){7}[a-fA-F0-9]{1,4}"
            );

        public static Regex MAC_ADDRESS = iText.Commons.Utils.StringUtil.RegexCompile("([0-9a-fA-F][0-9a-fA-F]:){5}([0-9a-fA-F][0-9a-fA-F])"
            );

        public static Regex EMAIL_ADDRESS = iText.Commons.Utils.StringUtil.RegexCompile("[0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9}"
            );

        public static Regex HTTP_URL = iText.Commons.Utils.StringUtil.RegexCompile("(https?|ftp)://[a-z0-9-]+(\\.[a-z0-9-]+)+([/?].+)?"
            );
    }
}
