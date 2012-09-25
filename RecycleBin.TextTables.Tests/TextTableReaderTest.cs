using System.IO;
using NUnit.Framework;

namespace RecycleBin.TextTables
{
   [TestFixture]
   internal class TextTableReaderTest
   {
      [Test]
      public void TestHandleHeaderRow()
      {
         const string csv = "X,Y\r\n1,2";
         using (var stringReader = new StringReader(csv))
         using (var csvReader = new CsvReader(stringReader))
         {
            csvReader.HandleHeaderRow();
            var record = csvReader.Current;
            //Assert.That(record, Is.EquivalentTo(new[] { "X", "Y" }));
            Assert.That(csvReader.MoveNext(), Is.True);
            record = csvReader.Current;
            Assert.That(record["X"], Is.EqualTo("1"));
            Assert.That(record["Y"], Is.EqualTo("2"));
         }
      }
   }
}
