using Cindi.Domain.Entities.Steps;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Cindi.Domain.Tests.Utilities
{
    public class Journal_Tests
    {
        [Fact]
        public void Reflections()
        {
            var test = new Step();
            var fields = test.GetType().GetProperties();
        }
    }
}
