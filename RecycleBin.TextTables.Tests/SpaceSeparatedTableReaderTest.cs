using System.IO;
using NUnit.Framework;

namespace RecycleBin.TextTables
{
   [TestFixture]
   internal class SpaceSeparatedTableReaderTest
   {
      [Test]
      public void TestReadRecordRaw([Values(EndOfLine.CRLF, EndOfLine.LF, EndOfLine.CR)] EndOfLine eol)
      {
         const string ascii = "ASCII";
         const string nonAscii = "日本語";
         const string whitespace = "  ";
         var withNewline = string.Format("hello{0}world", eol.AsNewline());

         var csv = string.Format("    {0}  {1}     \"{2}\"  \"{3}\" {0}{4}", ascii, nonAscii, whitespace, withNewline, eol.AsNewline());
         var settings = new SpaceSeparatedTableReaderSettings()
         {
            RecordDelimiter = eol,
         };
         using (var stringReader = new StringReader(csv))
         using (var reader = new SpaceSeparatedTableReader(stringReader, settings))
         {
            Assert.That(reader.MoveNext(), Is.True);
            var record = reader.Current;
            Assert.That(record, Is.Not.Null);
            Assert.That(record.FieldCount, Is.EqualTo(5));
            Assert.That(record[0], Is.EqualTo(ascii));
            Assert.That(record[1], Is.EqualTo(nonAscii));
            Assert.That(record[2], Is.EqualTo(whitespace));
            Assert.That(record[3], Is.EqualTo(withNewline));
            Assert.That(record[0], Is.EqualTo(ascii));
            Assert.That(reader.MoveNext(), Is.False);
         }
      }
   }
}
