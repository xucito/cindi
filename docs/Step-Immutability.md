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