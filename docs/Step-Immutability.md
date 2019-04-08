# Step Immutability

Steps are referencially immutable.

Data cannot be changed however new data can be appended based on the life-cycle of a step.

The current state of the step can be derived by looking at its history.

## Journal

A Step's Journal is used to keep record of any updates to the step across the platform, this includes status updates.

The Journal contains many journal entry, each journal represents a collection of actions taken against the original step record.

## Journal Entry

The Id of a Journal entry is sequential and unique, the greater the number the later the transaction.

Actions:

* Change: Change a existing field
* Append: Append data to the record

## Immutability

As step updates are not stored as a update to the original record but as a new write with a chainId (unique identification of the layer of the update the record should apply to), steps are truly immutable as concurrently processed step updates will fail on one side as one of the inserted updates will fail due to key inconsistency.

This process can be represented as follows (Example assignment process):

|Stage| Lastest Chain Id|Process State (Thread 1)| Process State (Thread 2) |
|-|-|-|-|
|Pull record | 0 | Ok | Ok|
|Update Record | 0 | Update record in memory (Update 1) | Update record in memory (Update 1) |
|Write Data to DB | 1 | Successful write to DB | Failed Write (Conflicting key on insert)
