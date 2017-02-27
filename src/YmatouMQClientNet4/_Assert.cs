using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace YmatouMessageBusClientNet4
{
    class _Assert
    {
        public static void AssertArgumentIsTrue(Func<bool> condition, string message)
        {
            //if (!ycfg.EnableAssert) return;
            if (!condition())
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertArgumentIsFalse(Func<bool> condition, string message)
        {
            //if (!ycfg.EnableAssert) return;
            if (!condition())
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertArgumentEquals(object object1, object object2, string message)
        {
            //if (!ycfg.EnableAssert) return;
            //Debug.Assert(object1.Equals(object2), message);
            if (!object1.Equals(object2))
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertArgumentFalse(bool boolValue, string message)
        {
            //if (!ycfg.EnableAssert) return;
            if (boolValue)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertArgumentLength(string stringValue, int maximum, string message)
        {
            //if (!ycfg.EnableAssert) return;
            int length = stringValue.Trim().Length;
            if (length > maximum)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertArgumentLength(string stringValue, int minimum, int maximum, string message)
        {
            //if (!ycfg.EnableAssert) return;
            int length = stringValue.Trim().Length;
            if (length < minimum || length > maximum)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertArgumentMatches(string pattern, string stringValue, string message)
        {
            //if (!ycfg.EnableAssert) return;
            Regex regex = new Regex(pattern);

            if (!regex.IsMatch(stringValue))
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertArgumentNotEmpty(string stringValue, string message)
        {
            //if (!ycfg.EnableAssert) return;
            if (stringValue == null || stringValue.Trim().Length == 0)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertArgumentNotEquals(object object1, object object2, string message)
        {
            //if (!ycfg.EnableAssert) return;
            if (object1.Equals(object2))
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertArgumentNotNull(object object1, string message)
        {
            //if (!ycfg.EnableAssert) return;
            if (object1 == null)
            {
                //YmatouLoggingService.Error("{0} {1}", object1, message);
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertArgumentRange(double value, double minimum, double maximum, string message)
        {
            //if (!ycfg.EnableAssert) return;
            if (value < minimum || value > maximum)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertArgumentRange(float value, float minimum, float maximum, string message)
        {
            //if (!ycfg.EnableAssert) return;
            if (value < minimum || value > maximum)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertArgumentRange(int value, int minimum, int maximum, string message)
        {
            //if (!ycfg.EnableAssert) return;
            if (value < minimum || value > maximum)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertArgumentRange(long value, long minimum, long maximum, string message)
        {
            //if (!ycfg.EnableAssert) return;
            if (value < minimum || value > maximum)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertArgumentTrue(bool boolValue, string message)
        {
            //if (!ycfg.EnableAssert) return;
            if (!boolValue)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertStateFalse(bool boolValue, string message)
        {
            //if (!ycfg.EnableAssert) return;
            if (boolValue)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertStateTrue(bool boolValue, string message)
        {
            //if (!ycfg.EnableAssert) return;
            if (!boolValue)
            {
                throw new InvalidOperationException(message);
            }
        }
        //private static readonly YmatouConfig ycfg = CommonConfiguration.GetConfig();
        protected _Assert()
        {
            // ycfg = CommonConfiguration.GetConfig();
        }

        protected void SelfAssertArgumentEquals(object object1, object object2, string message)
        {
            //if (!ycfg.EnableAssert) return;
            _Assert.AssertArgumentEquals(object1, object2, message);
        }

        protected void SelfAssertArgumentFalse(bool boolValue, string message)
        {
            //if (!ycfg.EnableAssert) return;
            _Assert.AssertArgumentFalse(boolValue, message);
        }

        protected void SelfAssertArgumentLength(string stringValue, int maximum, string message)
        {
            //if (!ycfg.EnableAssert) return;
            _Assert.AssertArgumentLength(stringValue, maximum, message);
        }

        protected void SelfAssertArgumentLength(string stringValue, int minimum, int maximum, string message)
        {
            //if (!ycfg.EnableAssert) return;
            _Assert.AssertArgumentLength(stringValue, minimum, maximum, message);
        }

        protected void SelfAssertArgumentMatches(string pattern, string stringValue, string message)
        {
            //if (!ycfg.EnableAssert) return;
            _Assert.AssertArgumentMatches(pattern, stringValue, message);
        }

        protected void SelfAssertArgumentNotEmpty(string stringValue, string message)
        {
            //if (!ycfg.EnableAssert) return;
            _Assert.AssertArgumentNotEmpty(stringValue, message);
        }

        protected void SelfAssertArgumentNotEquals(object object1, object object2, string message)
        {
            //if (!ycfg.EnableAssert) return;
            _Assert.AssertArgumentNotEquals(object1, object2, message);
        }

        protected void SelfAssertArgumentNotNull(object object1, string message)
        {
            //if (!ycfg.EnableAssert) return;
            _Assert.AssertArgumentNotNull(object1, message);
        }

        protected void SelfAssertArgumentRange(double value, double minimum, double maximum, string message)
        {
            //if (!ycfg.EnableAssert) return;
            _Assert.AssertArgumentRange(value, minimum, maximum, message);
        }

        protected void SelfAssertArgumentRange(float value, float minimum, float maximum, string message)
        {
            //if (!ycfg.EnableAssert) return;
            _Assert.AssertArgumentRange(value, minimum, maximum, message);
        }

        protected void SelfAssertArgumentRange(int value, int minimum, int maximum, string message)
        {
            //if (!ycfg.EnableAssert) return;
            _Assert.AssertArgumentRange(value, minimum, maximum, message);
        }

        protected void SelfAssertArgumentRange(long value, long minimum, long maximum, string message)
        {
            //if (!ycfg.EnableAssert) return;
            _Assert.AssertArgumentRange(value, minimum, maximum, message);
        }

        protected void SelfAssertArgumentTrue(bool boolValue, string message)
        {
            //if (!ycfg.EnableAssert) return;
            _Assert.AssertArgumentTrue(boolValue, message);
        }

        protected void SelfAssertStateFalse(bool boolValue, string message)
        {
            //if (!ycfg.EnableAssert) return;
            _Assert.AssertStateFalse(boolValue, message);
        }

        protected void SelfAssertStateTrue(bool boolValue, string message)
        {
            //if (!ycfg.EnableAssert) return;
            _Assert.AssertStateTrue(boolValue, message);
        }
    }
}
