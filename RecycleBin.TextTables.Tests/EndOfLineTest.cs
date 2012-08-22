using System;
using System.ComponentModel;
using NUnit.Framework;

namespace RecycleBin.TextTables
{
   [TestFixture]
   internal class EndOfLineTest
   {
      [Test]
      public void TestAsNewlineCRLF()
      {
         Assert.That(EndOfLine.CRLF.AsNewline(), Is.EqualTo("\r\n"));
      }

      [Test]
      public void TestAsNewlineLF()
      {
         Assert.That(EndOfLine.LF.AsNewline(), Is.EqualTo("\n"));
      }

      [Test]
      public void TestAsNewlineCR()
      {
         Assert.That(EndOfLine.CR.AsNewline(), Is.EqualTo("\r"));
      }

      [Test]
      [ExpectedException(typeof(InvalidEnumArgumentException))]
      public void TestAsNewlineOther()
      {
         Assert.That(((EndOfLine)Enum.ToObject(typeof(EndOfLine), -1)).AsNewline(), Throws.TypeOf<InvalidEnumArgumentException>());
      }
   }
}
