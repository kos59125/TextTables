TextTables
==========

This is a .NET Framework library providing classes handle plain-text tables like CSV.

Features
--------

* Easy to Use:

```csharp
using (var reader = new StreamReader(@"X:\Path\To\File.csv"))
using (var csv = new CsvReader(reader))
{
   while (csv.MoveNext())
   {
      var record = csv.Current;
      Console.WriteLine("First Column: {0}; Number of Fields: {1}", record[0], record.FieldCount);
   }
}
```

* Dynamic Mapping:

Suppose we have a space-separated table as the following:
```plain
Id     Name    Birthday
 1    Alice  2001-02-14
 2      Bob  1997-06-05   
 3  Charlie  1999-12-15
 4     Dave  2005-10-30
```

So we can define a class like this:
```csharp
public class Person
{
   [Column("Id")]
   public long ID { get; set; }

   [Column("Name")]
   public string Name { get; set; }

   [Column("Birthday")]
   public DateTime Birthday { get; set; }
}
```

Now, we can read the text file.
```csharp
using (var table = new SpaceSeparatedTableReader(reader))
{
   // Notify the reader that the first line is the header.
   table.HandleHeaderRow();

   // LINQ is available.
   var query = from person in table.ReadToEnd<Person>()
               where person.Birthday < new DateTime(2000, 01, 01)
               select person;
   foreach (var person in query)
   {
      Console.WriteLine(person.Name);
   }
}
```

