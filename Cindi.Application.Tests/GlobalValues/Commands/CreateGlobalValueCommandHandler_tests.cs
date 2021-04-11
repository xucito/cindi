using Cindi.Application.GlobalValues.Commands.CreateGlobalValue;
using Cindi.Application.Interfaces;

using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.GlobalValues;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace Cindi.Application.Tests.GlobalValues.Commands
{
    public class CreateGlobalValueCommandHandler_tests
    {
        Mock<IEntitiesRepository> _entitiesRepositoryMock = new Mock<IEntitiesRepository>();
        Mock<IStateMachine> _stateMachineMock = new Mock<IStateMachine>();


        public CreateGlobalValueCommandHandler_tests()
        {
        }

        [Fact]
        public async void DetectDuplicateGlobalValueBasedOnName()
        {
            _entitiesRepositoryMock.Setup(cs => cs.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<GlobalValue, bool>>>())).Returns(Task.FromResult(new GlobalValue()
            {

            }));

            var handler = new CreateGlobalValueCommandHandler(_entitiesRepositoryMock.Object, _stateMachineMock.Object);
            await Assert.ThrowsAsync<DuplicateGlobalValueException>(async () =>
            {
                await handler.Handle(new CreateGlobalValueCommand()
                {
                    Name = "TestGV",
                    Type = InputDataTypes.Int,
                    Value = 1
                }, new System.Threading.CancellationToken());
            });
        }

        [Fact]
        public async void ValueMatchesTypesCheck()
        {
            Assert.True(false);
        }

        [Fact]
        public async void InvalidGlobalValueTypeCheck()
        {
            Assert.True(false);
        }
    }
}
