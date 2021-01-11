/*

This file is part of the iText (R) project.
Copyright (c) 1998-2021 iText Group NV
Authors: Bruno Lowagie, Paulo Soares, et al.

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser.ClipperLib;

namespace iText.PdfCleanup {
    internal static class CleanUpExtensions {

        public static bool AddAll(this IList<Point> list, ICollection<Point> c) {
            Point prevPoint = list.Count - 1 < 0 ? null : list[list.Count - 1];
            bool ret = false;

            foreach (Point pt in c) {
                if (!pt.Equals(prevPoint)) {
                    list.Add(pt);
                    prevPoint = pt;
                    ret = true;
                }
            }

            return true;
        }

        public static void AddAll<T>(this Stack<T> c, Stack<T> collectionToAdd) {
            foreach (T o in collectionToAdd) {
                c.Push(o);
            }
        }
        
        public static void AddAll<T>(this ICollection<T> c, IEnumerable<T> collectionToAdd) {
            foreach (T o in collectionToAdd) {
                c.Add(o);
            }
        }
        
        public static String JSubstring(this String str, int beginIndex, int endIndex) {
            return str.Substring(beginIndex, endIndex - beginIndex);
        }

        public static T JRemoveAt<T>(this IList<T> list, int index) {
            T value = list[index];
            list.RemoveAt(index);

            return value;
        }

        public static T PollLast<T>(this LinkedList<T> list) {
            T item = default(T);
            if (list.Count > 0) {
                item = list.Last();
                list.Remove(item);
            }

            return item;
        }

        public static T JRemoveFirst<T>(this LinkedList<T> list) {
            T value = list.First.Value;
            list.RemoveFirst();

            return value;
        }
        
        public static T Next<T>(this IEnumerator<T> enumerator) {
            enumerator.MoveNext();
            return enumerator.Current;
        }
        
        public static void Execute(this ClipperOffset clipperOffset, PolyTree solution, double delta) {
            clipperOffset.Execute(ref solution, delta);
        }

        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> col, TKey key) {
            TValue value = default(TValue);
            if (key != null) {
                col.TryGetValue(key, out value);
            }
            return value;
        }

        public static TValue Put<TKey, TValue>(this IDictionary<TKey, TValue> col, TKey key, TValue value) {
            TValue oldVal = col.Get(key);
            col[key] = value;
            return oldVal;
        }

        public static byte[] GetBytes(this String str) {
            return Encoding.UTF8.GetBytes(str);
        }

        public static byte[] GetBytes(this String str, Encoding encoding) {
            return encoding.GetBytes(str);
        }

        public static T Peek<T>(this LinkedList<T> list) {
            T value = default(T);
            if (list.Count > 0) {
                value = list.First.Value;
            }
            return value;
        }
        
        public static bool IsEmpty<T>(this ICollection<T> collection) {
            return 0 == collection.Count;
        }

        public static bool IsEmptys(this ICollection collection) {
            return 0 == collection.Count;
        }

        public static T[] ToArray<T>(this ICollection<T> col, T[] toArray) {
            T[] r;
            int colSize = col.Count;
            if (colSize <= toArray.Length) {
                col.CopyTo(toArray, 0);
                if (colSize != toArray.Length) {
                    toArray[colSize] = default(T);
                }
                r = toArray;
            } else {
                r = new T[colSize];
                col.CopyTo(r, 0);
            }

            return r;
        }
        
        public static Attribute GetCustomAttribute(this Assembly assembly, Type attributeType) {
            object[] customAttributes = assembly.GetCustomAttributes(attributeType, false);
            if (customAttributes.Length > 0 && customAttributes[0] is Attribute) {
                return customAttributes[0] as Attribute;
            } else {
                return null;
            }
        }

        public static Assembly GetAssembly(this Type type) {
            return type.Assembly;
        }
    }
}
