using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace RecycleBin.TextTables
{
   [TestFixture]
   internal class ValueAttributeTest
   {
      [Test]
      public void TestParseSByte()
      {
         sbyte expected = 1;
         var expectedType = typeof(SByte);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("1", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [Test]
      public void TestParseInt16()
      {
         short expected = 1;
         var expectedType = typeof(Int16);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("1", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [Test]
      public void TestParseInt32()
      {
         int expected = 1;
         var expectedType = typeof(Int32);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("1", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [Test]
      public void TestParseInt64()
      {
         long expected = 1;
         var expectedType = typeof(Int64);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("1", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [Test]
      public void TestParseByte()
      {
         byte expected = 1;
         var expectedType = typeof(Byte);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("1", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [Test]
      public void TestParseUInt16()
      {
         ushort expected = 1;
         var expectedType = typeof(UInt16);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("1", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [Test]
      public void TestParseUInt32()
      {
         uint expected = 1;
         var expectedType = typeof(UInt32);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("1", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [Test]
      public void TestParseUInt64()
      {
         ulong expected = 1;
         var expectedType = typeof(UInt64);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("1", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [Test]
      public void TestParseBoolean()
      {
         bool expected = true;
         var expectedType = typeof(Boolean);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("True", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [Test]
      public void TestParseChar()
      {
         char expected = 'K';
         var expectedType = typeof(Char);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("K", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [Test]
      public void TestParseString()
      {
         var expected = "Foo";
         var expectedType = typeof(String);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("Foo", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [Test]
      public void TestParseSingle()
      {
         float expected = 1.5f;
         var expectedType = typeof(Single);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("1.5", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [Test]
      public void TestParseDouble()
      {
         double expected = 1.5;
         var expectedType = typeof(Double);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("1.5", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [TestCase(null)]
      [TestCase("yyyy-MM-dd HH:mm:ss.fff")]
      [TestCase("yyyy-MM-dd HH:mm:ss", ExpectedException = typeof(FormatException))]
      public void TestParseExactDateTimeSuccess(string format)
      {
         var expected = new DateTime(2000, 1, 23, 4, 56, 7, 890);
         var expectedType = typeof(DateTime);
         var attribute = new ValueAttribute() { FormatString = format };
         var actual = attribute.Parse("2000-01-23 04:56:07.890", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [TestCase("yyyy-MM-dd HH:mm:ss", ExpectedException = typeof(FormatException))]
      [TestCase("yyyy-MM-dd HH:mm:ss.ffff", ExpectedException = typeof(FormatException))]
      public void TestParseExactDateTimeFailure(string format)
      {
         var expected = new DateTime(2000, 1, 23, 4, 56, 7, 890);
         var expectedType = typeof(DateTime);
         var attribute = new ValueAttribute() { FormatString = format };
         Assert.That(attribute.Parse("2000-01-23 04:56:07.890", expectedType), Throws.TypeOf(typeof(FormatException)));
      }

      [TestCase(null)]
      [TestCase("D")]
      public void TestParseExactGuidSuccess(string format)
      {
         var expected = Guid.Empty;
         var expectedType = typeof(Guid);
         var attribute = new ValueAttribute() { FormatString = format };
         var actual = attribute.Parse("00000000-0000-0000-0000-000000000000", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [TestCase("B")]
      [TestCase("N")]
      public void TestParseExactGuidFailure(string format)
      {
         var attribute = new ValueAttribute() { FormatString = format };
         try
         {
            // Calls ParseExact method via reflection.
            // The error is wraped by TargetInvocationException
            attribute.Parse(Guid.Empty.ToString("D"), typeof(Guid));
         }
         catch (TargetInvocationException ex)
         {
            Assert.That(ex.InnerException.GetType (), Is.EqualTo(typeof(FormatException)));
         }
      }

      [Test]
      public void TestParseEnum()
      {
         var expected = TestEnum.TestValue;
         var expectedType = typeof(TestEnum);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("testvalue", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [Test]
      public void TestParseLazy()
      {
         var expected = 1;
         var expectedType = typeof(Lazy<Int32>);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("1", expectedType);
         Assert.That(actual.GetType(), Is.EqualTo(expectedType));
         var lazy = (Lazy<Int32>)actual;
         Assert.That(lazy.IsValueCreated, Is.False);
         Assert.That(lazy.Value, Is.EqualTo(expected));
         Assert.That(lazy.IsValueCreated, Is.True);
      }

      [Test]
      public void TestParseWithExplicitParser()
      {
         var expected = new TestClass(1);
         var attribute = new ValueAttribute()
         {
            ParserType = typeof(TestClassParserFormatter)
         };
         var actual = attribute.Parse("1", typeof(TestClass));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [Test]
      public void TestParseWithImplicitParser()
      {
         var expected = new TestClass(1);
         var attribute = new ValueAttribute();
         var actual = attribute.Parse("1", typeof(TestClass));
         Assert.That(actual, Is.EqualTo(expected));
      }

      [Test]
      [ExpectedException(typeof(FormatException))]
      public void TestParseWithNumberStyle()
      {
         var attribute = new ValueAttribute()
         {
            NumberStyle = NumberStyles.Integer
         };
         var type = typeof(Double);
         Assert.That(attribute.Parse("1", type), Is.EqualTo(Double.Parse("1")));
         Assert.That(attribute.Parse("1.0", type), Throws.TypeOf(typeof(FormatException)));
      }

      [Test]
      public void TestParseWithDateTimeStyle()
      {
         var expected = DateTime.Parse("2000-01-01 00:00");
         var attribute = new ValueAttribute()
         {
            CultureName = "ja-JP",  // JST is 9 hours ahead of UTC
            DateTimeStyle = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal
         };
         var actual = attribute.Parse("2000-01-01 09:00", typeof(DateTime));
         Assert.That(actual, Is.EqualTo(expected));
      }

      // See http://msdn.microsoft.com/en-us/library/dwhawy9k.aspx
      [TestCase(1, null, null, Result = "1")]
      [TestCase(1, "###", null, Result = "1")]
      [TestCase(1, "000", null, Result = "001")]
      [TestCase(1.2, "#.###", null, Result = "1.2")]
      [TestCase(1.2, "#.000", null, Result = "1.200")]
      [TestCase(12345.6789, "C", "en-US", Result = "$12,345.68")]
      [TestCase(0x12AB, "X8", null, Result = "000012AB")]
      public string TestFormatNumberWithFormatString(object value, string format, string cultureName)
      {
         var attribute = new ValueAttribute()
         {
            CultureName = cultureName,
            FormatString = format
         };
         return attribute.Format(value);
      }

      // See http://msdn.microsoft.com/en-us/library/az4se3k1.aspx
      [TestCase("u", null, Result = "2000-01-23 04:56:07Z")]
      [TestCase("o", null, Result = "2000-01-23T04:56:07.8900000")]
      [TestCase("d", "en-US", Result = "1/23/2000")]
      [TestCase("d", "en-NZ", Result = "23/01/2000")]
      [TestCase("d", "de-DE", Result = "23.01.2000")]
      [TestCase("yyyy", null, Result = "2000")]
      public string TestFormatDateTimeWithFormatString(string format, string cultureName)
      {
         var datetime = new DateTime(2000, 1, 23, 4, 56, 7, 890);
         var attribute = new ValueAttribute()
         {
            CultureName = cultureName,
            FormatString = format
         };
         return attribute.Format(datetime);
      }
   }

   enum TestEnum
   {
      TestValue = 4
   }

   class TestClass
   {
      public int Value { get; private set; }
      public TestClass(int value)
      {
         Value = value;
      }
      public override int GetHashCode()
      {
         return Value;
      }
      public override bool Equals(object obj)
      {
         var other = obj as TestClass;
         return other != null && this.Value == other.Value;
      }
      public static TestClass Parse(string value, IFormatProvider provider)
      {
         return new TestClass(Int32.Parse(value, provider));
      }
   }

   public class TestClassParserFormatter
   {
      public object Parse(string value, IFormatProvider provider)
      {
         return new TestClass(Int32.Parse(value, provider));
      }
      public string Format(object value)
      {
         return ((TestClass)value).Value.ToString();
      }
   }
}
