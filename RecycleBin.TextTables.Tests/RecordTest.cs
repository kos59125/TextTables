using System.IO;
using System.Linq;
using NUnit.Framework;

namespace RecycleBin.TextTables
{
   [TestFixture]
   internal class RecordTest
   {
      [Test]
      public void TestNamedIndexer()
      {
         var csv = string.Format("1,2,3");
         using (var stringReader = new StringReader(csv))
         using (var reader = new CsvReader(stringReader))
         {
            reader.SetHeader(new[] { "First", "Second", "Third" });
            Assert.That(reader.MoveNext(), Is.True);

            var record = reader.Current;
            Assert.That(record, Is.Not.Null);
            Assert.That(record["First"], Is.EqualTo("1"));
            Assert.That(record["Second"], Is.EqualTo("2"));
            Assert.That(record["Third"], Is.EqualTo("3"));
         }
      }

      [Test]
      public void TestMultipleColumn([Values(EndOfLine.CRLF, EndOfLine.LF, EndOfLine.CR)] EndOfLine eol)
      {
         var csv = string.Format("1,二{0}", eol.AsNewline());
         var settings = new CsvReaderSettings()
         {
            RecordDelimiter = eol,
         };
         using (var stringReader = new StringReader(csv))
         using (var reader = new CsvReader(stringReader, settings))
         {
            Assert.That(reader.MoveNext(), Is.True);
            var record = reader.Current.Convert<MultipleColumnRecord>();
            Assert.That(record, Is.Not.Null);
            Assert.That(record.Value, Is.EqualTo(1).Or.EqualTo(2));
            Assert.That(reader.MoveNext(), Is.False);
         }
      }

      [Test]
      public void TestReadArrayRecord([Values(EndOfLine.CRLF, EndOfLine.LF, EndOfLine.CR)] EndOfLine eol)
      {
         var csv = string.Format("1,2,3,4,5,6,一,二{0}", eol.AsNewline());
         var settings = new CsvReaderSettings()
         {
            RecordDelimiter = eol,
         };
         using (var stringReader = new StringReader(csv))
         using (var reader = new CsvReader(stringReader, settings))
         {
            Assert.That(reader.MoveNext(), Is.True);
            var record = reader.Current.Convert<ArrayRecord>();
            Assert.That(record, Is.Not.Null);
            var property = record.Property;
            var field = record.Field;
            Assert.That(property, Is.Not.Null);
            Assert.That(field, Is.Not.Null);
            Assert.That(property[0, 0], Is.EqualTo(1));
            Assert.That(property[0, 1], Is.EqualTo(2));
            Assert.That(property[1, 0], Is.EqualTo(3));
            Assert.That(property[1, 1], Is.EqualTo(4));
            Assert.That(property[2, 0], Is.EqualTo(5));
            Assert.That(property[2, 1], Is.EqualTo(6));
            Assert.That(field[0], Is.EqualTo(1));
            Assert.That(field[1], Is.EqualTo(2));
            Assert.That(reader.MoveNext(), Is.False);
         }
      }

      [Test]
      public void TestReadIndexerRecord([Values(EndOfLine.CRLF, EndOfLine.LF, EndOfLine.CR)] EndOfLine eol)
      {
         var csv = string.Format("1,二,3{0}", eol.AsNewline());
         var settings = new CsvReaderSettings()
         {
            RecordDelimiter = eol,
         };
         using (var stringReader = new StringReader(csv))
         using (var reader = new CsvReader(stringReader, settings))
         {
            Assert.That(reader.MoveNext(), Is.True);
            var record = reader.Current.Convert<IndexerRecord>();
            Assert.That(record, Is.Not.Null);
            Assert.That(record["A"], Is.EqualTo(1));
            Assert.That(record["B"], Is.EqualTo(2));
            Assert.That(record["C"], Is.EqualTo(3));
            Assert.That(reader.MoveNext(), Is.False);
         }
      }

      [Test]
      public void TestReadOmittableRecord([Values(EndOfLine.CRLF, EndOfLine.LF, EndOfLine.CR)] EndOfLine eol)
      {
         var csv = string.Format("1,2,3{0}4,5{0}6{0}", eol.AsNewline());
         var settings = new CsvReaderSettings()
         {
            RecordDelimiter = eol,
         };
         using (var stringReader = new StringReader(csv))
         using (var reader = new CsvReader(stringReader, settings))
         {
            Assert.That(reader.MoveNext(), Is.True);
            var record = reader.Current.Convert<OmittableRecord>();
            Assert.That(record, Is.Not.Null);
            Assert.That(record.Column1, Is.EqualTo(1));
            Assert.That(record.Column2, Is.EqualTo(2));
            Assert.That(record.Column3, Is.EqualTo(3));
            Assert.That(reader.MoveNext(), Is.True);
            record = reader.Current.Convert<OmittableRecord>();
            Assert.That(record, Is.Not.Null);
            Assert.That(record.Column1, Is.EqualTo(4));
            Assert.That(record.Column2, Is.EqualTo(5));
            Assert.That(record.Column3, Is.EqualTo(default(int)));
            Assert.That(reader.MoveNext(), Is.True);
            record = reader.Current.Convert<OmittableRecord>();
            Assert.That(record, Is.Not.Null);
            Assert.That(record.Column1, Is.EqualTo(6));
            Assert.That(record.Column2, Is.EqualTo(default(int)));
            Assert.That(record.Column3, Is.EqualTo(default(int)));
            Assert.That(reader.MoveNext(), Is.False);
         }
      }

      [Test]
      public void TestReadRecordGeneric([Values(EndOfLine.CRLF, EndOfLine.LF, EndOfLine.CR)] EndOfLine eol)
      {
         var headerLine = "string1,int1,double1,parsed1,ignored1,string2,int2,double2,parsed2,ignored2";
         var csv = string.Format(
            "{0}{1}" +
               "hello,1,-4.0,一,_,world,-1,3.0,二,_{1}" +
                  "good,2,NA,NA,_,bye,0,NA,NA,_{1}",
            headerLine, eol.AsNewline()
            );
         var settings = new CsvReaderSettings()
         {
            RecordDelimiter = eol,
         };
         using (var stringReader = new StringReader(csv))
         using (var reader = new CsvReader(stringReader, settings))
         {
            Assert.That(reader.MoveNext(), Is.True);
            var header = reader.Current.ToArray();
            reader.SetHeader(header);
            Assert.That(header, Is.Not.Null);
            Assert.That(header, Is.EquivalentTo(headerLine.Split(',')));
            Assert.That(reader.MoveNext(), Is.True);
            var record = reader.Current.Convert<PartialRecord>();
            Assert.That(record, Is.Not.Null);
            Assert.That(record.StringField, Is.EqualTo("hello"));
            Assert.That(record.IntField, Is.EqualTo(1));
            Assert.That(record.NullableDoubleField, Is.EqualTo(-4.0));
            Assert.That(record.ParsedField, Is.EqualTo(1));
            Assert.That(record.IgnoredField, Is.EqualTo(default(object)));
            Assert.That(record.StringProperty, Is.EqualTo("world"));
            Assert.That(record.IntProperty, Is.EqualTo(-1));
            Assert.That(record.NullableDoubleProperty, Is.EqualTo(3.0));
            Assert.That(record.ParsedProperty, Is.EqualTo(2));
            Assert.That(record.IgnoredProperty, Is.EqualTo(default(int)));
            Assert.That(reader.MoveNext(), Is.True);
            record = reader.Current.Convert<PartialRecord>();
            Assert.That(record, Is.Not.Null);
            Assert.That(record.StringField, Is.EqualTo("good"));
            Assert.That(record.IntField, Is.EqualTo(2));
            Assert.That(record.NullableDoubleField, Is.Null);
            Assert.That(record.ParsedField, Is.Null);
            Assert.That(record.IgnoredField, Is.EqualTo(default(object)));
            Assert.That(record.StringProperty, Is.EqualTo("bye"));
            Assert.That(record.IntProperty, Is.EqualTo(0));
            Assert.That(record.NullableDoubleProperty, Is.Null);
            Assert.That(record.ParsedProperty, Is.Null);
            Assert.That(record.IgnoredProperty, Is.EqualTo(default(int)));
            Assert.That(reader.MoveNext(), Is.False);
         }
      }

      [Test]
      public void TestReadToEndGeneric([Values(EndOfLine.CRLF, EndOfLine.LF, EndOfLine.CR)] EndOfLine eol)
      {
         var headerLine = "string1,int1,double1,parsed1,ignored1,string2,int2,double2,parsed2,ignored2";
         var csv = string.Format(
            "{0}{1}" +
               "hello,1,-4.0,一,_,world,-1,3.0,二,_{1}" +
                  "good,2,NA,NA,_,bye,0,NA,NA,_{1}",
            headerLine, eol.AsNewline()
            );
         var settings = new CsvReaderSettings()
         {
            RecordDelimiter = eol,
         };
         using (var stringReader = new StringReader(csv))
         using (var reader = new CsvReader(stringReader, settings))
         {
            Assert.That(reader.MoveNext(), Is.True);
            var header = reader.Current.ToArray();
            reader.SetHeader(header);
            Assert.That(header, Is.Not.Null);
            Assert.That(header, Is.EquivalentTo(headerLine.Split(',')));
            var records = reader.ReadToEnd<PartialRecord>().ToList();
            Assert.That(records.Count, Is.EqualTo(2));
            Assert.That(reader.MoveNext(), Is.False);
            var record = records[0];
            Assert.That(record, Is.Not.Null);
            Assert.That(record.StringField, Is.EqualTo("hello"));
            Assert.That(record.IntField, Is.EqualTo(1));
            Assert.That(record.NullableDoubleField, Is.EqualTo(-4.0));
            Assert.That(record.ParsedField, Is.EqualTo(1));
            Assert.That(record.IgnoredField, Is.EqualTo(default(object)));
            Assert.That(record.StringProperty, Is.EqualTo("world"));
            Assert.That(record.IntProperty, Is.EqualTo(-1));
            Assert.That(record.NullableDoubleProperty, Is.EqualTo(3.0));
            Assert.That(record.ParsedProperty, Is.EqualTo(2));
            Assert.That(record.IgnoredProperty, Is.EqualTo(default(int)));
            record = records[1];
            Assert.That(record, Is.Not.Null);
            Assert.That(record.StringField, Is.EqualTo("good"));
            Assert.That(record.IntField, Is.EqualTo(2));
            Assert.That(record.NullableDoubleField, Is.Null);
            Assert.That(record.ParsedField, Is.Null);
            Assert.That(record.IgnoredField, Is.EqualTo(default(object)));
            Assert.That(record.StringProperty, Is.EqualTo("bye"));
            Assert.That(record.IntProperty, Is.EqualTo(0));
            Assert.That(record.NullableDoubleProperty, Is.Null);
            Assert.That(record.ParsedProperty, Is.Null);
            Assert.That(record.IgnoredProperty, Is.EqualTo(default(int)));
         }
      }
   }
}
