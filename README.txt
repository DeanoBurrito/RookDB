--- Description ---
RookDB is a simple to use JSON based database, based on CastleDB.
Most of the original design goals carry over, and the format is mostly compatable, however a few changes have been made to the spec.

The main differences are:
- Reference implementation in C#, rather then haxe/js.
- Sheets are now called tables (to be more in line with traditional database terminology).
- Columns cannot be marked as optional now. This flag is ignored in any cdb files, and will result in a default value being stored upon parsing instead.


--- Implementation Notes ---
For the most part, I've tried to store exact values in memory (the JSON is more ref oriented).
Enum and flag values store the exact text of the enum value, where as the JSON stores the index.
Reference values store the reference as "TABLE_ID/RECORD_ID", rather than having to fully compute the path each time.

Whilst RookDB dosnt support optional values, to keep compatability with CastleDB (which does), the parsing section
contains default values for all fields in case their are optional and left empty. With the exception of UniqueIdentifiers,
since these have to be unique, a default value cannot be used and will throw an error if no value is found.