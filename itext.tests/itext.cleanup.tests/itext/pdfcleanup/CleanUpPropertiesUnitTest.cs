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
using iText.PdfCleanup.Exceptions;
using iText.Test;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("UnitTest")]
    public class CleanUpPropertiesUnitTest : ExtendedITextTest {
        [NUnit.Framework.Test]
        public virtual void NePropsAspectRatioReturnsNull() {
            CleanUpProperties properties = new CleanUpProperties();
            NUnit.Framework.Assert.IsNull(properties.GetOverlapRatio());
        }

        [NUnit.Framework.Test]
        public virtual void SetAspectRatioWithValue0IsOk() {
            CleanUpProperties properties = new CleanUpProperties();
            Exception e = NUnit.Framework.Assert.Catch(typeof(ArgumentException), () => properties.SetOverlapRatio(0d)
                );
            NUnit.Framework.Assert.AreEqual(CleanupExceptionMessageConstant.OVERLAP_RATIO_SHOULD_BE_IN_RANGE, e.Message
                );
        }

        [NUnit.Framework.Test]
        public virtual void SetAspectRatioWithValue1IsOk() {
            CleanUpProperties properties = new CleanUpProperties();
            properties.SetOverlapRatio(1.0);
            NUnit.Framework.Assert.AreEqual(1.0, properties.GetOverlapRatio());
        }

        [NUnit.Framework.Test]
        public virtual void SetAspectRatioWithValueGreaterThan1ThrowsException() {
            CleanUpProperties properties = new CleanUpProperties();
            Exception e = NUnit.Framework.Assert.Catch(typeof(ArgumentException), () => properties.SetOverlapRatio(1.1
                ));
            NUnit.Framework.Assert.AreEqual(CleanupExceptionMessageConstant.OVERLAP_RATIO_SHOULD_BE_IN_RANGE, e.Message
                );
        }

        [NUnit.Framework.Test]
        public virtual void SetAspectRatioWithValueLessThan0ThrowsException() {
            CleanUpProperties properties = new CleanUpProperties();
            Exception e = NUnit.Framework.Assert.Catch(typeof(ArgumentException), () => properties.SetOverlapRatio(-0.1
                ));
            NUnit.Framework.Assert.AreEqual(CleanupExceptionMessageConstant.OVERLAP_RATIO_SHOULD_BE_IN_RANGE, e.Message
                );
        }

        [NUnit.Framework.Test]
        public virtual void SetAspectRatioWithValue0_5IsOk() {
            CleanUpProperties properties = new CleanUpProperties();
            properties.SetOverlapRatio(0.5);
            NUnit.Framework.Assert.AreEqual(0.5, properties.GetOverlapRatio());
        }

        [NUnit.Framework.Test]
        public virtual void SettingAspectRatioToNullIsOk() {
            CleanUpProperties properties = new CleanUpProperties();
            properties.SetOverlapRatio(0.5);
            properties.SetOverlapRatio(null);
            NUnit.Framework.Assert.IsNull(properties.GetOverlapRatio());
        }

        [NUnit.Framework.Test]
        public virtual void SetGetPathOffsetApproximationPropertiesTest() {
            PathOffsetApproximationProperties pathOffsetApproximationProperties = new PathOffsetApproximationProperties
                ().CalculateOffsetMultiplierDynamically(true).SetArcTolerance(0.0015);
            CleanUpProperties properties = new CleanUpProperties().SetOffsetProperties(pathOffsetApproximationProperties
                );
            NUnit.Framework.Assert.IsTrue(properties.GetOffsetProperties().CalculateOffsetMultiplierDynamically());
            NUnit.Framework.Assert.AreEqual(0.0015, properties.GetOffsetProperties().GetArcTolerance());
        }
    }
}
