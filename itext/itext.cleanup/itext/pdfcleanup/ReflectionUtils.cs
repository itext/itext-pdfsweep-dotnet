/*
This file is part of the iText (R) project.
Copyright (c) 1998-2020 iText Group NV
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
using System.IO;
using System.Reflection;
using Common.Logging;
using Versions.Attributes;
using iText.PdfCleanup;

namespace iText.PdfCleanup
{

    /// <summary>Utility class for handling operation related to reflections.</summary>
    internal sealed class ReflectionUtils
    {
        private static readonly String LICENSEKEY = "iText.License.LicenseKey, itext.licensekey";
        private static readonly String LICENSEKEY_PRODUCT = "iText.License.LicenseKeyProduct, itext.licensekey";
        private static readonly String CHECK_LICENSEKEY_METHOD = "ScheduledCheck";

        private ReflectionUtils() {
        }

        /// <summary>Performs a scheduled license check.</summary>
        internal static void ScheduledLicenseCheck()
        {
            try
            {
                Type licenseKeyClass = GetClass(LICENSEKEY);
                if ( licenseKeyClass != null )
                {
                    Type licenseKeyProductClass = GetClass(LICENSEKEY_PRODUCT);
                    object[] objects = new object[]
                    {
                        PdfCleanupProductInfo.PRODUCT_NAME,
                        PdfCleanupProductInfo.MAJOR_VERSION.ToString(),
                        PdfCleanupProductInfo.MINOR_VERSION.ToString()
                    };
                    Object productObject = System.Activator.CreateInstance(licenseKeyProductClass, objects);
                    licenseKeyClass.GetMethod(CHECK_LICENSEKEY_METHOD)
                        .Invoke(System.Activator.CreateInstance(licenseKeyClass), new object[] { productObject });
                }
            }
            catch (Exception e)
            {
                if (!Kernel.Version.IsAGPLVersion())
                {
                    throw;
                }
            }
        }

        private static Type GetClass(string className)
        {
            String licenseKeyClassFullName = null;
            Assembly assembly = typeof(ReflectionUtils).GetAssembly();
            Attribute keyVersionAttr = assembly.GetCustomAttribute(typeof(KeyVersionAttribute));
            if (keyVersionAttr is KeyVersionAttribute)
            {
                String keyVersion = ((KeyVersionAttribute)keyVersionAttr).KeyVersion;
                String format = "{0}, Version={1}, Culture=neutral, PublicKeyToken=8354ae6d2174ddca";
                licenseKeyClassFullName = String.Format(format, className, keyVersion);
            }
            Type type = null;
            if (licenseKeyClassFullName != null)
            {
                String fileLoadExceptionMessage = null;
                try
                {
                    type = System.Type.GetType(licenseKeyClassFullName);
                }
                catch (FileLoadException fileLoadException)
                {
                    fileLoadExceptionMessage = fileLoadException.Message;
                }
                if (type == null)
                {
                    try
                    {
                        type = System.Type.GetType(className);
                    }
                    catch
                    {
                        // empty
                    }
                    if (type == null && fileLoadExceptionMessage != null)
                    {
                        LogManager.GetLogger(typeof(ReflectionUtils)).Error(fileLoadExceptionMessage);
                    }
                }
            }
            return type;
        }
    }
}
