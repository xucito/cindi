# Software Design

[/Diagrams/Software_Architecture.jpg]


1. Domain
	* Cross Application Concerns
	* Contains Enums, Enums, Exceptions, Types, Types

2. Application
	* Application Logic
	* Use of outside layers is defined via interfaces to force clean design
3. Persistent Layer
	* Persistent storage of data logic 
	* Repositories, DB configurations, Migations
4. Infrastructure Layer
	* Environment/external specific logic
	* Services, DTOs
5. Presentation Layer
	* Web API
	* Client Application




## Recognitions

Inspired by https://www.youtube.com/watch?v=_lwCVE_XgqI&t=0s&list=WL&index=75