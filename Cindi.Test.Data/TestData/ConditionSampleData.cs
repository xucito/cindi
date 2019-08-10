using Cindi.Domain.Entities.WorkflowTemplates.Conditions;
using Cindi.Domain.Entities.WorkflowTemplates.ValueObjects;
using Cindi.Test.Global.TestClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Test.Global.TestData
{
    public static class ConditionSampleData
    {
       public static ConditionGroup OneLayerTrueConditionGroup = new ConditionGroup()
        {
            Operator = OperatorStatements.AND,
            Conditions = new List<Condition>()
                {
                    new AlwaysTrueCondition(),
                    new AlwaysTrueCondition()
                }
        };

        public static ConditionGroup OneLayerORTrueConditionGroup = new ConditionGroup()
        {
            Operator = OperatorStatements.OR,
            Conditions = new List<Condition>()
                {
                    new AlwaysFalseCondition(),
                    new AlwaysTrueCondition()
                }
        };

        public static ConditionGroup OneLayerFalseConditionGroup = new ConditionGroup()
        {
            Operator = OperatorStatements.AND,
            Conditions = new List<Condition>()
                {
                    new AlwaysTrueCondition(),
                    new AlwaysFalseCondition()
                }
        };

       public static ConditionGroup TwoLayerTrueConditionGroup = new ConditionGroup()
        {
            Operator = OperatorStatements.AND,
            Conditions = new List<Condition>()
                {
                    new AlwaysTrueCondition(),
                    new AlwaysTrueCondition()
                },
            ConditionGroups = new List<ConditionGroup>()
            {
                new ConditionGroup()
                {
                    Operator = OperatorStatements.AND,
                    Conditions = new List<Condition>()
                    {
                        new AlwaysTrueCondition(),
                        new AlwaysTrueCondition()
                    }
                },
                new ConditionGroup()
                {
                    Operator = OperatorStatements.AND,
                    Conditions = new List<Condition>()
                    {
                        new AlwaysTrueCondition(),
                        new AlwaysTrueCondition()
                    }
                }
            }
        };

       public static ConditionGroup TwoLayerFalseConditionGroup = new ConditionGroup()
        {
            Operator = OperatorStatements.AND,
            Conditions = new List<Condition>()
                {
                    new AlwaysTrueCondition(),
                    new AlwaysTrueCondition()
                },
            ConditionGroups = new List<ConditionGroup>()
            {
                new ConditionGroup()
                {
                    Operator = OperatorStatements.AND,
                    Conditions = new List<Condition>()
                    {
                        new AlwaysTrueCondition(),
                        new AlwaysTrueCondition()
                    }
                },
                new ConditionGroup()
                {
                    Operator = OperatorStatements.AND,
                    Conditions = new List<Condition>()
                    {
                        new AlwaysFalseCondition(),
                        new AlwaysTrueCondition()
                    }
                }
            }
        };

        public static ConditionGroup ThreeLayerTrueConditionGroup = new ConditionGroup()
        {
            Operator = OperatorStatements.AND,
            Conditions = new List<Condition>()
                {
                    new AlwaysTrueCondition(),
                    new AlwaysTrueCondition()
                },
            ConditionGroups = new List<ConditionGroup>()
            {
                new ConditionGroup()
                {
                    //Should return true
                    Operator = OperatorStatements.AND,
                    Conditions = new List<Condition>()
                    {
                        new AlwaysTrueCondition(),
                        new AlwaysTrueCondition()
                    },
                    ConditionGroups = new List<ConditionGroup>()
                    {
                        new ConditionGroup()
                        {
                            //Should return true
                            Operator = OperatorStatements.OR,
                            Conditions = new List<Condition>()
                            {
                                new AlwaysFalseCondition(),
                                new AlwaysTrueCondition()
                            }
                        }
                    }
                },
                //Should return true
                new ConditionGroup()
                {
                    Operator = OperatorStatements.AND,
                    Conditions = new List<Condition>()
                    {
                        new AlwaysTrueCondition(),
                        new AlwaysTrueCondition()
                    }
                }
            }
        };

        public static ConditionGroup ThreeLayerFalseConditionGroup = new ConditionGroup()
        {
            Operator = OperatorStatements.AND,
            Conditions = new List<Condition>()
                {
                    new AlwaysTrueCondition(),
                    new AlwaysTrueCondition()
                },
            ConditionGroups = new List<ConditionGroup>()
            {
                new ConditionGroup()
                {
                    Operator = OperatorStatements.AND,
                    Conditions = new List<Condition>()
                    {
                        new AlwaysTrueCondition(),
                        new AlwaysTrueCondition()
                    },
                    ConditionGroups = new List<ConditionGroup>()
                    {
                        new ConditionGroup()
                        {
                            Operator = OperatorStatements.AND,
                            Conditions = new List<Condition>()
                            {
                                new AlwaysFalseCondition(),
                                new AlwaysTrueCondition()
                            }
                        }
                    }
                },
                new ConditionGroup()
                {
                    Operator = OperatorStatements.OR,
                    Conditions = new List<Condition>()
                    {
                        new AlwaysFalseCondition(),
                        new AlwaysTrueCondition()
                    }
                }
            }
        };
    }
}
