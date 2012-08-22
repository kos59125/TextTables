using System.IO;
using NUnit.Framework;

namespace RecycleBin.TextTables
{
   [TestFixture]
   internal class StackBufferedTextReaderTest
   {
      [Test]
      public void TestRead1()
      {
         const string s = "hello";
         using (var reader = new StackBufferedTextReader(new StringReader(s)))
         {
            Assert.That(reader.Read(), Is.EqualTo((int)'h'));
            Assert.That(reader.Read(), Is.EqualTo((int)'e'));
            Assert.That(reader.Read(), Is.EqualTo((int)'l'));
            Assert.That(reader.Read(), Is.EqualTo((int)'l'));
            Assert.That(reader.Read(), Is.EqualTo((int)'o'));
            Assert.That(reader.Read(), Is.EqualTo(-1));
         }
      }

      [Test]
      public void TestRead2()
      {
         const string s = "hello";
         using (var reader = new StackBufferedTextReader(new StringReader(s)))
         {
            var buffer = new char[4];
            Assert.That(reader.Read(buffer, 0, buffer.Length), Is.EqualTo(buffer.Length));
            Assert.That(new string(buffer), Is.EqualTo("hell"));
            Assert.That(reader.Read(buffer, 2, 2), Is.EqualTo(1));
            Assert.That(new string(buffer)[2], Is.EqualTo('o'));
         }
      }

      [Test]
      public void TestReadLine([Values(EndOfLine.CR, EndOfLine.CRLF, EndOfLine.LF)] EndOfLine eol)
      {
         var s = string.Format("hello{0}{0}world{0}", eol.AsNewline());
         using (var reader = new StackBufferedTextReader(new StringReader(s)))
         {
            Assert.That(reader.ReadLine(), Is.EqualTo("hello"));
            Assert.That(reader.ReadLine(), Is.EqualTo(""));
            reader.Push("good-bye ");
            Assert.That(reader.ReadLine(), Is.EqualTo("good-bye world"));
            Assert.That(reader.ReadLine(), Is.Null);
         }
      }

      [Test]
      public void TestReadToEnd([Values(EndOfLine.CR, EndOfLine.CRLF, EndOfLine.LF)] EndOfLine eol)
      {
         var s = string.Format("hello{0}world{0}", eol.AsNewline());
         using (var reader = new StackBufferedTextReader(new StringReader(s)))
         {
            Assert.That(reader.ReadToEnd(), Is.EqualTo(s));
            Assert.That(reader.ReadToEnd(), Is.EqualTo(""));
         }
      }

      [Test]
      public void TestRevert()
      {
         const string s = "world";
         using (var reader = new StackBufferedTextReader(new StringReader(s)))
         {
            reader.Push(' ');
            reader.Push("hello");
            Assert.That(reader.ReadToEnd(), Is.EqualTo("hello world"));
         }
      }
   }
}
