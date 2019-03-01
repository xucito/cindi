# Namespace

https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces

# ID Generation

Cindi is designed to be document orientated for atomic operations. 

ID's for Steps, Step Templates, Sequences and SequenceTemplates are composed of a Name and Version which create a unique composite key that cannot change.

# Enums

Enums are useful to symbolic represent values however do not lend themselves to readability. Cindi objects are designed to be highly readable therefore string enums are used.


# DateTime Fields

DateTimeOffset is always used to ensure absolute representation of dates and times.

# Persistence

https://stackoverflow.com/questions/47324412/mongodb-connection-thread-safety

MongoDb is created as a singleton as it is threadsafe.

All Repositories will have two constructors, one with string intializers and one with inherited client.

All persistence related information (Including data annotation for BSON (MongoDB serialization information)) should be isolated in the Persistenence layer.

#Exceptions

Exceptions are made up of 7 parts
Parents:
* A00 - Global
* A01 - Steps
* A02 - StepTemplates
* A03 - StepTestTemplates
* A04 - Sequences
* A05 - SequenceTemplates'

Specific Exception

* 0000 - Specific error code


# URL Conventions

https://restfulapi.net/resource-naming/

URLs use Snake Casing i.e. api/step-templates


Reference: https://google.github.io/styleguide/jsoncstyleguide.xml

The Body of requests does uses snake-casing for field names:
```
{
	"name":"",
	"Version":"",
	"allowDynamicInputs":"",
	"inputDefinitions":"",
	"outputDefinitions": ""
}
```

www://api/step-templates?query-word=testWord&