using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace RecycleBin.TextTables
{
   [TestFixture]
   internal class CsvWriterTest
   {
      [Test]
      public void TestSetHeader1()
      {
         using (var stream = new MemoryStream())
         using (var streamWriter = new StreamWriter(stream))
         using (var writer = new CsvWriter(streamWriter))
         {
            writer.SetHeader(typeof(FullRecord));
         }
      }

      [Test]
      [ExpectedException(typeof(InvalidOperationException))]
      public void TestSetHeader2()
      {
         using (var stream = new MemoryStream())
         using (var streamWriter = new StreamWriter(stream))
         using (var writer = new CsvWriter(streamWriter))
         {
            writer.SetHeader(typeof(PartialRecord));
         }
      }

      [Test]
      public void TestWriteArrayRecord([Values(EndOfLine.CRLF, EndOfLine.LF, EndOfLine.CR)] EndOfLine eol)
      {
         var record = new ArrayRecord
         {
            Property = new[,] { { 1, 2 }, { 3, 4 }, { 5, 6 } },
            Field = new[] { 1, 2 },
         };

         var settings = new CsvWriterSettings()
         {
            RecordDelimiter = eol,
         };

         using (var stream = new MemoryStream())
         using (var streamWriter = new StreamWriter(stream))
         using (var writer = new CsvWriter(streamWriter, settings) { AutoFlush = true })
         {
            writer.WriteRecord(record);
            stream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(stream))
            {
               var line = reader.ReadLine();
               Assert.That(line, Is.Not.Null);
               var actual = line.Split(',');
               Assert.That(actual.Length, Is.EqualTo(8));
               Assert.That(actual[0], Is.EqualTo("\"1\""));
               Assert.That(actual[1], Is.EqualTo("\"2\""));
               Assert.That(actual[2], Is.EqualTo("\"3\""));
               Assert.That(actual[3], Is.EqualTo("\"4\""));
               Assert.That(actual[4], Is.EqualTo("\"5\""));
               Assert.That(actual[5], Is.EqualTo("\"6\""));
               Assert.That(actual[6], Is.EqualTo("\"一\""));
               Assert.That(actual[7], Is.EqualTo("\"二\""));
            }
         }
      }

      [Test]
      [Sequential]
      public void TestWriteHeader1()
      {
         const string ascii = "ASCII";
         const string nonAscii = "日本語";
         const string empty = "";
         const string withNewline = "hello\r\nworld";
         var header = new[] { ascii, nonAscii, empty, withNewline };

         using (var stream = new MemoryStream())
         using (var streamWriter = new StreamWriter(stream))
         using (var writer = new CsvWriter(streamWriter) { AutoFlush = true })
         {
            writer.SetHeader(header);
            writer.WriteHeader();
            stream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(stream))
            {
               var expected = string.Join(",", header.Select(value => "\"" + value + "\"")) + "\r\n";
               var actual = reader.ReadToEnd();
               Assert.That(actual, Is.EqualTo(expected));
            }
         }
      }

      [Test]
      [ExpectedException(typeof(InvalidOperationException))]
      public void TestWriteHeader2()
      {
         using (var stream = new MemoryStream())
         using (var streamWriter = new StreamWriter(stream))
         using (var writer = new CsvWriter(streamWriter))
         {
            writer.WriteHeader();
         }
      }

      [Test]
      public void TestWriteIndexerRecord([Values(EndOfLine.CRLF, EndOfLine.LF, EndOfLine.CR)] EndOfLine eol)
      {
         var record = new IndexerRecord();
         record["A"] = 1;
         record["B"] = 2;
         record["C"] = 3;

         var settings = new CsvWriterSettings()
         {
            RecordDelimiter = eol,
         };

         using (var stream = new MemoryStream())
         using (var streamWriter = new StreamWriter(stream))
         using (var writer = new CsvWriter(streamWriter, settings) { AutoFlush = true })
         {
            writer.WriteRecord(record);
            stream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(stream))
            {
               var line = reader.ReadLine();
               Assert.That(line, Is.Not.Null);
               var actual = line.Split(',');
               Assert.That(actual.Length, Is.EqualTo(3));
               Assert.That(actual[0], Is.EqualTo("\"1\""));
               Assert.That(actual[1], Is.EqualTo("\"二\""));
               Assert.That(actual[2], Is.EqualTo("\"3\""));
            }
         }
      }

      [Test]
      public void TestWriteMultipleRecord([Values(EndOfLine.CRLF, EndOfLine.LF, EndOfLine.CR)] EndOfLine eol)
      {
         var record = new MultipleColumnRecord
         {
            Value = 1
         };

         var settings = new CsvWriterSettings()
         {
            RecordDelimiter = eol,
         };

         using (var stream = new MemoryStream())
         using (var streamWriter = new StreamWriter(stream))
         using (var writer = new CsvWriter(streamWriter, settings) { AutoFlush = true })
         {
            writer.WriteRecord(record);
            stream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(stream))
            {
               var line = reader.ReadLine();
               Assert.That(line, Is.Not.Null);
               var actual = line.Split(',');
               Assert.That(actual.Length, Is.EqualTo(2));
               Assert.That(actual[0], Is.EqualTo("\"1\""));
               Assert.That(actual[1], Is.EqualTo("\"一\""));
            }
         }
      }

      [Test]
      public void TestWriteRecord1([Values(EndOfLine.CRLF, EndOfLine.LF, EndOfLine.CR)] EndOfLine eol)
      {
         const string ascii = "ASCII";
         const string nonAscii = "日本語";
         const string empty = "";
         const string withNewline = "hello\r\nworld";
         var record = new[] { ascii, nonAscii, empty, withNewline };

         var settings = new CsvWriterSettings()
         {
            RecordDelimiter = eol,
         };

         using (var stream = new MemoryStream())
         using (var streamWriter = new StreamWriter(stream))
         using (var writer = new CsvWriter(streamWriter, settings) { AutoFlush = true })
         {
            writer.WriteRecordRaw(record);
            stream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(stream))
            {
               var expected = string.Join(",", record.Select(value => "\"" + value + "\"")) + eol.AsNewline();
               var actual = reader.ReadToEnd();
               Assert.That(actual, Is.EqualTo(expected));
            }
         }
      }

      [Test]
      public void TestWriteRecord2([Values(EndOfLine.CRLF, EndOfLine.LF, EndOfLine.CR)] EndOfLine eol)
      {
         var record = new FullRecord
         {
            IntField = 3,
            IntProperty = -1,
            NullableDoubleField = null,
            NullableDoubleProperty = 1.5,
            ParsedField = 1,
            ParsedProperty = null,
            StringField = "hello",
            StringProperty = null,
         };

         var settings = new CsvWriterSettings
         {
            RecordDelimiter = eol,
         };

         using (var stream = new MemoryStream())
         using (var streamWriter = new StreamWriter(stream))
         using (var writer = new CsvWriter(streamWriter, settings) { AutoFlush = true })
         {
            writer.WriteRecord(record);
            stream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(stream))
            {
               var line = reader.ReadLine();
               Assert.That(line, Is.Not.Null);
               var actual = line.Split(',');
               Assert.That(actual.Length, Is.EqualTo(8));
               Assert.That(actual[0], Is.EqualTo("\"hello\""));
               Assert.That(actual[1], Is.EqualTo("\"3\""));
               Assert.That(actual[6], Is.EqualTo("\"1.5\""));
               Assert.That(actual[7], Is.EqualTo("\"NA(Int32)\""));
               var restActual = new HashSet<string> { actual[2], actual[3], actual[4], actual[5] };
               var restExpected = new HashSet<string> { "\"NA\"", "\"一\"", "\"\"", "\"-1\"" };
               Assert.That(restActual.SetEquals(restExpected), Is.True);
            }
         }
      }

      [Test]
      public void CanCloseAfterInnerReaderClosed()
      {
         using (var stream = new MemoryStream())
         using (var streamWriter = new StreamWriter(stream))
         using (var writer = new CsvWriter(streamWriter))
         {
            streamWriter.Close();
            writer.Close();
         }
      }
   }
}
