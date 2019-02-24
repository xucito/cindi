using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.Steps;
using Cindi.Test.Global.TestData;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Test.Global.MockInterfaces
{
    public class MockStepsRepository
    {
        public static IStepsRepository GetMockRepository()
        {
            Mock<IStepsRepository> mock = new Mock<IStepsRepository>();
            mock.Setup(m => m.GetStepAsync(new Guid())).Returns(Task.FromResult(FibonacciSampleData.Step));
            mock.Setup(m => m.GetStepsAsync(0,1000)).Returns(Task.FromResult(new List<Step>(){
                FibonacciSampleData.Step
            }));
            return mock.Object;
        }
    }
}
