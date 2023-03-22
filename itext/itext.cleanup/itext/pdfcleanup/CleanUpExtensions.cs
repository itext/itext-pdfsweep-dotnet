/*
This file is part of the iText (R) project.
Copyright (c) 1998-2023 Apryse Group NV
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
