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
    }
}
