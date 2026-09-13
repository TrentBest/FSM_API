using NUnit.Framework;

#pragma warning disable NUnit2009
namespace TheSingularityWorkshop.FSM_API.Tests
{
    /// <summary>
    /// Development provenance ledger for FSM_API package revisions.
    /// </summary>
    /// <remarks>
    /// Each development commit made by the AI development workflow records the package version
    /// associated with that commit as a new test. Historical entries are intentionally never
    /// rewritten or removed.
    /// </remarks>
    [TestFixture]
    public sealed class IncrementalVersionTests
    {
        [Test]
        public void IncrementalVersion_001_RecordsPackageVersion_1_0_14()
        {
            const string staticVersionString = "1.0.14";
            Assert.That(staticVersionString, Is.EqualTo(staticVersionString));
        }

        [Test]
        public void IncrementalVersion_002_RecordsPackageVersion_1_0_15()
        {
            const string staticVersionString = "1.0.15";
            Assert.That(staticVersionString, Is.EqualTo(staticVersionString));
        }

        [Test]
        public void IncrementalVersion_003_RecordsPackageVersion_1_0_16()
        {
            const string staticVersionString = "1.0.16";
            Assert.That(staticVersionString, Is.EqualTo(staticVersionString));
        }

        [Test]
        public void IncrementalVersion_004_RecordsPackageVersion_1_0_17()
        {
            const string staticVersionString = "1.0.17";
            Assert.That(staticVersionString, Is.EqualTo(staticVersionString));
        }

        [Test]
        public void IncrementalVersion_005_RecordsPackageVersion_1_0_18()
        {
            const string staticVersionString = "1.0.18";
            Assert.That(staticVersionString, Is.EqualTo(staticVersionString));
        }

        [Test]
        public void IncrementalVersion_006_RecordsPackageVersion_1_0_19()
        {
            const string staticVersionString = "1.0.19";
            Assert.That(staticVersionString, Is.EqualTo(staticVersionString));
        }

        [Test]
        public void IncrementalVersion_007_RecordsPackageVersion_1_0_20()
        {
            const string staticVersionString = "1.0.20";
            Assert.That(staticVersionString, Is.EqualTo(staticVersionString));
        }

        [Test]
        public void IncrementalVersion_008_RecordsPackageVersion_1_0_21()
        {
            const string staticVersionString = "1.0.21";
            Assert.That(staticVersionString, Is.EqualTo(staticVersionString));
        }

        [Test]
        public void IncrementalVersion_009_RecordsPackageVersion_1_0_22()
        {
            const string staticVersionString = "1.0.22";
            Assert.That(staticVersionString, Is.EqualTo(staticVersionString));
        }
    }
}
#pragma warning restore NUnit2009
