using NUnit.Framework;

namespace LOLCode.net.Tests.Runtime
{
	[TestFixture]
	public class SimpleRuntimeTest
	{
		[Test]
		public void VisibleKeywordRuntime()
		{
			var sources = SampleHelper.GetCodeFromSample("visible.lol");
			var baseline = SampleHelper.GetBaselineFromSample("visible.lol");
			RuntimeTestHelper.TestExecuteSourcesNoInput(sources, baseline,
				 "VisibleKeywordRuntime", ExecuteMethod.ExternalProcess);
		}

		[Test]
		public void HaiWorldRuntime()
		{
			var sources = SampleHelper.GetCodeFromSample("haiworld.lol");
			var baseline = SampleHelper.GetBaselineFromSample("haiworld.lol");
			RuntimeTestHelper.TestExecuteSourcesNoInput(sources, baseline,
				 "HaiWorldRuntime", ExecuteMethod.ExternalProcess);
		}

		[Test]
		public void Simple1Runtime()
		{
			var sources = SampleHelper.GetCodeFromSample("simple1.lol");
			var baseline = SampleHelper.GetBaselineFromSample("simple1.lol");
			RuntimeTestHelper.TestExecuteSourcesNoInput(sources, baseline,
				 "Simple1Runtime", ExecuteMethod.ExternalProcess);
		}

	}
}
