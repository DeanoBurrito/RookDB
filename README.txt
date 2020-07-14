RookDB is a simple to use JSON based database, based on CastleDB.
Most of the original design goals carry over, and the format is mostly compatable, however a few changes have been made to the spec.

The main differences are:
- Reference implementation in C#, rather then haxe/js.
- Sheets are now called tables (to be more in line with traditional database terminology).
- Columns cannot be marked as optional now. This flag is ignored in any cdb files, and will result in a default value being stored upon parsing instead.