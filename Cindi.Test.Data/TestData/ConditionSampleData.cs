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
            Conditions = new Dictionary<string, Condition>()
                {
                    {"0", new AlwaysTrueCondition() } ,
                    {"1", new AlwaysTrueCondition() }
                }
        };

        public static ConditionGroup OneLayerORTrueConditionGroup = new ConditionGroup()
        {
            Operator = OperatorStatements.OR,
            Conditions = new Dictionary<string, Condition>()
                {
                    {"0", new AlwaysFalseCondition() },
                    {"1", new AlwaysTrueCondition() }
                }
        };

        public static ConditionGroup OneLayerFalseConditionGroup = new ConditionGroup()
        {
            Operator = OperatorStatements.AND,
            Conditions = new Dictionary<string, Condition>()
                {
                    {"0", new AlwaysTrueCondition() },
                    {"1", new AlwaysFalseCondition() }
                }
        };

        public static ConditionGroup TwoLayerTrueConditionGroup = new ConditionGroup()
        {
            Operator = OperatorStatements.AND,
            Conditions = new Dictionary<string, Condition>()
                {
                    {"0", new AlwaysTrueCondition() },
                    {"1", new AlwaysTrueCondition() }
                },
            ConditionGroups = new Dictionary<string, ConditionGroup>()
            {
                { "0", new ConditionGroup()
                {
                    Operator = OperatorStatements.AND,
                    Conditions = new Dictionary<string, Condition>()
                    {
                        {"0", new AlwaysTrueCondition() },
                        {"1", new AlwaysTrueCondition() }
                    }
                }
                },
                 { "1", new ConditionGroup()
                {
                    Operator = OperatorStatements.AND,
                    Conditions = new Dictionary<string, Condition>()
                    {
                        {"0", new AlwaysTrueCondition() },
                        {"1", new AlwaysTrueCondition() }
                    }
                }
            }
            }
        };

        public static ConditionGroup TwoLayerFalseConditionGroup = new ConditionGroup()
        {
            Operator = OperatorStatements.AND,
            Conditions = new Dictionary<string, Condition>()
                {
                    {"0", new AlwaysTrueCondition() },
                    {"1", new AlwaysTrueCondition() }
                },
            ConditionGroups = new Dictionary<string, ConditionGroup>()
            {
                 { "0", new ConditionGroup()
                {
                    Operator = OperatorStatements.AND,
                    Conditions = new Dictionary<string, Condition>()
                    {
                        {"0", new AlwaysTrueCondition() },
                        {"1", new AlwaysTrueCondition() }
                    }
                } },
                 { "1", new ConditionGroup()
                {
                    Operator = OperatorStatements.AND,
                    Conditions = new Dictionary<string, Condition>()
                    {
                        {"0", new AlwaysFalseCondition() },
                        {"1", new AlwaysTrueCondition() }
                    }
                }
            }
            }
        };

        public static ConditionGroup ThreeLayerTrueConditionGroup = new ConditionGroup()
        {
            Operator = OperatorStatements.AND,
            Conditions = new Dictionary<string, Condition>()
                {
                    {"0", new AlwaysTrueCondition() },
                    {"1", new AlwaysTrueCondition() }
                },
            ConditionGroups = new Dictionary<string, ConditionGroup>()
            {
                {"0", new ConditionGroup()
                {
                    //Should return true
                    Operator = OperatorStatements.AND,
                    Conditions = new Dictionary<string, Condition>()
                    {
                        {"0", new AlwaysTrueCondition() },
                        {"1", new AlwaysTrueCondition() }
                    },
                    ConditionGroups = new Dictionary<string, ConditionGroup>()
                    {
                        { "0", new ConditionGroup()
                        {
                            //Should return true
                            Operator = OperatorStatements.OR,
                            Conditions = new Dictionary<string, Condition>()
                            {
                                {"0", new AlwaysFalseCondition() },
                                {"1", new AlwaysTrueCondition() }
                            }
                        }
                    }
                }
                }
                },
                //Should return true
                 { "1", new ConditionGroup()
                {
                    Operator = OperatorStatements.AND,
                    Conditions = new Dictionary<string, Condition>()
                    {
                        {"0", new AlwaysTrueCondition() },
                        {"1", new AlwaysTrueCondition() }
                    }
                }
            }
            }
        };

        public static ConditionGroup ThreeLayerFalseConditionGroup = new ConditionGroup()
        {
            Operator = OperatorStatements.AND,
            Conditions = new Dictionary<string, Condition>()
                {
                    {"0", new AlwaysTrueCondition() },
                    {"1", new AlwaysTrueCondition() }
                },
            ConditionGroups = new Dictionary<string, ConditionGroup>()
            {
                {"0", new ConditionGroup()
                {
                    Operator = OperatorStatements.AND,
                    Conditions = new Dictionary<string, Condition>()
                    {
                        {"0", new AlwaysTrueCondition() },
                        {"1", new AlwaysTrueCondition() }
                    },
                    ConditionGroups = new Dictionary<string, ConditionGroup>()
                    {
                         { "0", new ConditionGroup()
                        {
                            Operator = OperatorStatements.AND,
                            Conditions = new Dictionary<string, Condition>()
                            {
                                {"0", new AlwaysFalseCondition() },
                                {"1", new AlwaysTrueCondition() }
                            }
                        }
                    }
                    }
                } },
                 { "1", new ConditionGroup()
                {
                    Operator = OperatorStatements.OR,
                    Conditions = new Dictionary<string, Condition>()
                    {
                        {"0", new AlwaysFalseCondition() },
                        {"1", new AlwaysTrueCondition() }
                    }
                }
                }
            }
        };
    }
}
