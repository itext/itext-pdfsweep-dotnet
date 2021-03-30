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
        public static Regex MODERN_ROMAN_NUMERALS_STRICT = iText.IO.Util.StringUtil.RegexCompile("(?=[MDCLXVI])M*(C[MD]|D?C{0,3})(X[CL]|L?X{0,3})(I[XV]|V?I{0,3})"
            );

        public static Regex MODERN_ROMAN_NUMERALS_FLEXIBLE = iText.IO.Util.StringUtil.RegexCompile("(?=[MDCLXVI])M*(C[MD]|D?C*)(X[CL]|L?X*)(I[XV]|V?I*)"
            );

        /*
        * MISC.
        */
        public static Regex US_SOCIAL_SECURITY_NUMBER = iText.IO.Util.StringUtil.RegexCompile("\\d{3}-\\d{2}-\\d{4}"
            );

        public static Regex US_ZIP_CODE = iText.IO.Util.StringUtil.RegexCompile("\\d{5}(-\\d{4})?");

        public static Regex US_CURRENCY = iText.IO.Util.StringUtil.RegexCompile("\\$(\\d{1,3}(\\,\\d{3})*|(\\d+))(\\.\\d{2})?"
            );

        public static Regex CANADA_SOCIAL_SECURITY_NUMBER = iText.IO.Util.StringUtil.RegexCompile("\\d{3}-\\d{3}-\\d{3}"
            );

        public static Regex CANADA_ZIP_CODE = iText.IO.Util.StringUtil.RegexCompile("(?!.*[DFIOQU])[A-VXY][0-9][A-Z] ?[0-9][A-Z][0-9]"
            );

        public static Regex UK_SOCIAL_SECURITY_NUMBER = iText.IO.Util.StringUtil.RegexCompile("[A-Z]{2}\\d{6}[A-Z]]"
            );

        public static Regex UK_ZIP_CODE = iText.IO.Util.StringUtil.RegexCompile("[A-Z]{1,2}[0-9R][0-9A-Z]? [0-9][ABD-HJLNP-UW-Z]{2}"
            );

        public static Regex UK_CURRENCY = iText.IO.Util.StringUtil.RegexCompile("£(\\d{1,3}(\\,\\d{3})*|(\\d+))(\\.\\d{2})?"
            );

        public static Regex EU_CURRENCY = iText.IO.Util.StringUtil.RegexCompile("€(\\d{1,3}(\\,\\d{3})*|(\\d+))(\\.\\d{2})?"
            );

        /*
        * DATE AND TIME
        */
        public static Regex DATE_MM_DD_YYYY = iText.IO.Util.StringUtil.RegexCompile("\\d{1,2}[ \\/-]\\d{1,2}[ \\/-]\\d{4}"
            );

        public static Regex DATE_MM_DD_YYYY_HH_MM_SS = iText.IO.Util.StringUtil.RegexCompile("\\d{1,2}[ \\/-]\\d{1,2}[ \\/-]\\d{4} \\d{1,2}:\\d{1,2}:\\d{1,2}"
            );

        public static Regex DATE_DD_MM_YYYY = iText.IO.Util.StringUtil.RegexCompile("\\d{1,2}[ \\/-]\\d{1,2}[ \\/-]\\d{4}"
            );

        public static Regex DATE_DD_MM_YYYY_HH_MM_SS = iText.IO.Util.StringUtil.RegexCompile("\\d{1,2}[ \\/-]\\d{1,2}[ \\/-]\\d{4} \\d{1,2}:\\d{1,2}:\\d{1,2}"
            );

        /*
        * ICT
        */
        public static Regex IPV4_ADDRESS = iText.IO.Util.StringUtil.RegexCompile("(?:[0-9]{1,3}\\.){3}[0-9]{1,3}");

        public static Regex IPV6_ADDRESS = iText.IO.Util.StringUtil.RegexCompile("(?:[a-fA-F0-9]{1,4}:){7}[a-fA-F0-9]{1,4}"
            );

        public static Regex MAC_ADDRESS = iText.IO.Util.StringUtil.RegexCompile("([0-9a-fA-F][0-9a-fA-F]:){5}([0-9a-fA-F][0-9a-fA-F])"
            );

        public static Regex EMAIL_ADDRESS = iText.IO.Util.StringUtil.RegexCompile("[0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9}"
            );

        public static Regex HTTP_URL = iText.IO.Util.StringUtil.RegexCompile("(https?|ftp)://[a-z0-9-]+(\\.[a-z0-9-]+)+([/?].+)?"
            );
    }
}
