# Namespace

https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces

# ID Generation

Cindi is designed to be document orientated for atomic operations. 

ID's for Steps, Step Templates, Sequences and SequenceTemplates are composed of a Name and Version which create a unique composite key that cannot change.

# Enums

Enums are useful to symbolic represent values however do not lend themselves to readability. Cindi objects are designed to be highly readable therefore string enums are used.