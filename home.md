# Cindi

Queue Based Automation Platform

## What is Cindi

Cindi is a lightweight queue based automation system that considers 4 main principles.

* Micro-serviced designed: Micro-services should share the same orchestration, logging and error management platform to reduce management overhead
* Generic Automation: All steps \(often referred to as tasks in other systems\) and processes are simply managed as a immutable document \(JSON notated\) between services
* Scale-out: High-availability and reliability is core to maintaining large scale automation
* Common Language principle: All communication is done via human read-able and a common language \(Web API Based\). 

## Why use Cindi?

Although built on primarily .NET Core, Cindi is language agnostic, highly integratable and built for manageability.

For organizations with different development teams and a diverse range of integration, the automation of processes that are multi-system becomes complex to not only build but maintain and debug.

In reality all steps can be broken down into 2 main considerations

* INPUT
* OUTPUT

With this understanding, if the INPUT and OUTPUT is validated and passed from step to step using a common platform there is a greater decoupling of the different step chains \(Referred to as Sequences in Cindi\).

Through the use of queues and a in-memory DB \(Redis\) the orchestration of different steps and the actual execution of the steps can be truly seperated, the advantages of which include but are not limited to:

* Efficiency: Scaling up, down or out of resources dedicated to the execution of a specific or group of steps by step types
* Fault-tolerance: where-in a failed step will not cause issues to the overall automation but still being centrally tracked and managed
* Traceable Parallelism: Tasks are run within their own owner however by scaling out owners, tasks can be run in parallel even within the same sequence for automation while still being completed isolated and traceable.

## Credited Projects

Material 2- [https://github.com/angular/material2](https://github.com/angular/material2)

.NET Core -[https://github.com/dotnet/core](https://github.com/dotnet/core)

