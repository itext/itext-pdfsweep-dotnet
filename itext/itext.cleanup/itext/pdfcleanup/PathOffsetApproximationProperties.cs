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
namespace iText.PdfCleanup {
    /// <summary>
    /// Contains properties for
    /// <see cref="iText.Kernel.Pdf.Canvas.Parser.ClipperLib.ClipperOffset"/>
    /// operations.
    /// </summary>
    public class PathOffsetApproximationProperties {
        private double arcTolerance = 0.0025;

        private bool calculateOffsetMultiplierDynamically = false;

        /// <summary>
        /// Creates new
        /// <see cref="PathOffsetApproximationProperties"/>
        /// instance.
        /// </summary>
        public PathOffsetApproximationProperties() {
        }

        // Empty constructor.
        /// <summary>Specifies if floatMultiplier should be calculated dynamically.</summary>
        /// <remarks>
        /// Specifies if floatMultiplier should be calculated dynamically. Default value is
        /// <see langword="false"/>.
        /// <para />
        /// When a document with line arts is being cleaned up, there are a lot of calculations with floating point numbers.
        /// All of them are translated into fixed point numbers by multiplying by this floatMultiplier coefficient.
        /// It is possible to dynamically adjust the preciseness of the calculations.
        /// </remarks>
        /// <param name="calculateDynamically">
        /// 
        /// <see langword="true"/>
        /// if floatMultiplier should be calculated dynamically,
        /// <see langword="false"/>
        /// for default value specified by
        /// <see cref="iText.Kernel.Pdf.Canvas.Parser.ClipperLib.ClipperBridge.ClipperBridge()"/>
        /// </param>
        /// <returns>
        /// this
        /// <see cref="PathOffsetApproximationProperties"/>
        /// instance
        /// </returns>
        public virtual iText.PdfCleanup.PathOffsetApproximationProperties CalculateOffsetMultiplierDynamically(bool
             calculateDynamically) {
            this.calculateOffsetMultiplierDynamically = calculateDynamically;
            return this;
        }

        /// <summary>Checks whether floatMultiplier should be calculated dynamically.</summary>
        /// <remarks>
        /// Checks whether floatMultiplier should be calculated dynamically.
        /// <para />
        /// When a document with line arts is being cleaned up, there are a lot of calculations with floating point numbers.
        /// All of them are translated into fixed point numbers by multiplying by this floatMultiplier coefficient.
        /// It is possible to dynamically adjust the preciseness of the calculations.
        /// </remarks>
        /// <returns>
        /// 
        /// <see langword="true"/>
        /// if floatMultiplier should be calculated dynamically,
        /// <see langword="false"/>
        /// for default value
        /// </returns>
        public virtual bool CalculateOffsetMultiplierDynamically() {
            return this.calculateOffsetMultiplierDynamically;
        }

        /// <summary>
        /// Gets arc tolerance which is the maximum difference between the true and the faceted representation of curves
        /// (arcs) in units.
        /// </summary>
        /// <remarks>
        /// Gets arc tolerance which is the maximum difference between the true and the faceted representation of curves
        /// (arcs) in units. Used as the criterion of a good approximation of rounded line joins and line caps.
        /// <para />
        /// Since flattened paths can never perfectly represent arcs, this field/property specifies a maximum acceptable
        /// imprecision (tolerance) when arcs are approximated in an offsetting operation. Smaller values will increase
        /// smoothness up to a point though at a cost of performance and in creating more vertices to construct the arc.
        /// </remarks>
        /// <returns>arc tolerance specifying maximum difference between the true and the faceted representation of arcs
        ///     </returns>
        public virtual double GetArcTolerance() {
            return arcTolerance;
        }

        /// <summary>
        /// Sets arc tolerance which is the maximum difference between the true and the faceted representation of curves
        /// (arcs) in units.
        /// </summary>
        /// <remarks>
        /// Sets arc tolerance which is the maximum difference between the true and the faceted representation of curves
        /// (arcs) in units. Used as the criterion of a good approximation of rounded line joins and line caps.
        /// <para />
        /// Since flattened paths can never perfectly represent arcs, this field/property specifies a maximum acceptable
        /// imprecision (tolerance) when arcs are approximated in an offsetting operation. Smaller values will increase
        /// smoothness up to a point though at a cost of performance and in creating more vertices to construct the arc.
        /// </remarks>
        /// <param name="arcTolerance">maximum difference between the true and the faceted representation of arcs</param>
        /// <returns>
        /// this
        /// <see cref="PathOffsetApproximationProperties"/>
        /// instance
        /// </returns>
        public virtual iText.PdfCleanup.PathOffsetApproximationProperties SetArcTolerance(double arcTolerance) {
            this.arcTolerance = arcTolerance;
            return this;
        }
    }
}
