﻿using System;
using System.IO;
using NUnit.Framework;

namespace RecycleBin.TextTables
{
   [TestFixture]
   internal class RecordConstructorTest
   {
      [Test]
      public void TestRecordWithParameteredConstructor([Values(EndOfLine.CRLF, EndOfLine.LF, EndOfLine.CR)] EndOfLine eol)
      {
         var csv = string.Format("1,2,3{0}4,5,6{0}", eol.AsNewline());
         var settings = new CsvReaderSettings()
         {
            RecordDelimiter = eol,
         };
         using (var stringReader = new StringReader(csv))
         using (var reader = new CsvReader(stringReader, settings))
         {
            Assert.That(reader.MoveNext(), Is.True);
            var record = reader.Current.Convert<RecordWithParameteredConstructor>();
            Assert.That(record, Is.Not.Null);
            Assert.That(record.Column1, Is.EqualTo(1));
            Assert.That(record.Column2, Is.EqualTo(2));
            Assert.That(record.Column3, Is.EqualTo(3));
            Assert.That(reader.MoveNext(), Is.True);
            record = reader.Current.Convert<RecordWithParameteredConstructor>();
            Assert.That(record, Is.Not.Null);
            Assert.That(record.Column1, Is.EqualTo(4));
            Assert.That(record.Column2, Is.EqualTo(5));
            Assert.That(record.Column3, Is.EqualTo(6));
            Assert.That(reader.MoveNext(), Is.False);
         }
      }

      [Test]
      public void TestRecordWithDefaultConstructor([Values(EndOfLine.CRLF, EndOfLine.LF, EndOfLine.CR)] EndOfLine eol)
      {
         var csv = string.Format("1,2,3{0}4,5,6{0}", eol.AsNewline());
         var settings = new CsvReaderSettings()
         {
            RecordDelimiter = eol,
         };
         using (var stringReader = new StringReader(csv))
         using (var reader = new CsvReader(stringReader, settings))
         {
            Assert.That(reader.MoveNext(), Is.True);
            var record = reader.Current.Convert<RecordWithDefaultConstructor>();
            Assert.That(record, Is.Not.Null);
            Assert.That(record.Column1, Is.EqualTo(1));
            Assert.That(record.Column2, Is.EqualTo(2));
            Assert.That(record.Column3, Is.EqualTo(3));
            Assert.That(reader.MoveNext(), Is.True);
            record = reader.Current.Convert<RecordWithDefaultConstructor>();
            Assert.That(record, Is.Not.Null);
            Assert.That(record.Column1, Is.EqualTo(4));
            Assert.That(record.Column2, Is.EqualTo(5));
            Assert.That(record.Column3, Is.EqualTo(6));
            Assert.That(reader.MoveNext(), Is.False);
         }
      }

      [Test]
      [ExpectedException(typeof(InvalidOperationException))]  // Single() throws InvalidOperationException with multiple elements.
      public void TestRecordWithMultipleRecordConstructor([Values(EndOfLine.CRLF, EndOfLine.LF, EndOfLine.CR)] EndOfLine eol)
      {
         var csv = string.Format("1,2,3{0}4,5,6{0}", eol.AsNewline());
         var settings = new CsvReaderSettings()
         {
            RecordDelimiter = eol,
         };
         using (var stringReader = new StringReader(csv))
         using (var reader = new CsvReader(stringReader, settings))
         {
            Assert.That(reader.MoveNext(), Is.True);
            Assert.That(reader.Current.Convert<RecordWithMultipleRecordConstructor>(), Throws.Exception);
         }
      }
   }

   public class RecordWithParameteredConstructor
   {
      private readonly int c1;
      private readonly int c2;

      public int Column1 { get { return this.c1; } }
      public int Column2 { get { return this.c2; } }
      [Column(2)]
      public int Column3 { get; set; }

      [RecordConstructor]
      public RecordWithParameteredConstructor([Column(0)] int c1, [Column(1)] int c2)
      {
         this.c1 = c1;
         this.c2 = c2;
      }
   }

   public class RecordWithDefaultConstructor
   {
      [Column(0)]
      public int Column1 { get; set; }
      [Column(1)]
      public int Column2 { get; set; }
      [Column(2)]
      public int Column3 { get; set; }

      [RecordConstructor]
      public RecordWithDefaultConstructor()
      {
      }
   }

   public class RecordWithMultipleRecordConstructor
   {
      [Column(0)]
      public int Column1 { get; set; }
      [Column(1)]
      public int Column2 { get; set; }
      [Column(2)]
      public int Column3 { get; set; }

      [RecordConstructor]
      public RecordWithMultipleRecordConstructor()
      {
      }

      [RecordConstructor]
      public RecordWithMultipleRecordConstructor([Column(0)] int c1, [Column(1)] int c2, [Column(2)] int c3)
      {
         Column1 = c1;
         Column2 = c2;
         Column3 = c3;
      }
   }
}
