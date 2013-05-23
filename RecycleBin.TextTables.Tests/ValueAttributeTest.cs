using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
      public void TestParseWithParser()
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
      [ExpectedException(typeof(FormatException))]
      public void TestParseWithNumberStyle()
      {
         var attribute = new ValueAttribute()
         {
            NumberStyle = NumberStyles.Integer
         };
         var expectedType = typeof(Double);
         Assert.That(attribute.Parse("1", expectedType), Is.EqualTo(Double.Parse("1")));
         Assert.That(attribute.Parse("1.0", expectedType), Throws.TypeOf(typeof(FormatException)));
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
