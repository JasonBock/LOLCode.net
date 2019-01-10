using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework; 

namespace LOLCode.net.Tests
{
    [TestFixture]    
    public class SampleHelperTest
    {

        [Test]
        public void GetCodeFromSampleNoBlocks()
        {
            var value = SampleHelper.GetCodeFromSample("fulltest.lol");
            Assert.IsTrue(value.Contains("BTW")); 
        }

        [Test]
        public void GetCodeFromSampleCodeBlock()
        {
            var value = SampleHelper.GetCodeFromSample("visible.lol");
            Assert.IsTrue(value.Contains("BTW")); 
        }

        [Test]
        public void GetBaselineFromSampleBlock()
        {
            var value = SampleHelper.GetBaselineFromSample("visible.lol");
            Assert.IsTrue(value.Contains("HELLO")); 
        }

    }
}
