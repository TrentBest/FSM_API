using NUnit.Framework;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    /// <summary>
    /// Development provenance ledger for FSM_API package revisions.
    /// </summary>
    /// <remarks>
    /// Each development commit made by the AI development workflow records the package version
    /// associated with that commit as a new test. Historical entries are intentionally never
    /// rewritten or removed.
    ///
    /// This ledger is a provenance mechanism, not a behavioral test. The assertion is deliberately
    /// trivial so that the version record remains independent of the implementation under test.
    /// The package version in FSM_API.csproj and the version recorded by the newest ledger test
    /// must match for the same development commit.
    /// </remarks>
    [TestFixture]
    public sealed class IncrementalVersionTests
    {
        /// <summary>Records package version 1.0.14 for this development commit.</summary>
        [Test]
        public void IncrementalVersion_001_RecordsPackageVersion_1_0_14()
        {
            const string staticVersionString = "1.0.14";
            Assert.That(staticVersionString, Is.EqualTo(staticVersionString));
        }

        /// <summary>Records package version 1.0.15 for this development commit.</summary>
        [Test]
        public void IncrementalVersion_002_RecordsPackageVersion_1_0_15()
        {
            const string staticVersionString = "1.0.15";
            Assert.That(staticVersionString, Is.EqualTo(staticVersionString));
        }

        /// <summary>Records package version 1.0.16 for this development commit.</summary>
        [Test]
        public void IncrementalVersion_003_RecordsPackageVersion_1_0_16()
        {
            const string staticVersionString = "1.0.16";
            Assert.That(staticVersionString, Is.EqualTo(staticVersionString));
        }

        /// <summary>Records package version 1.0.17 for this development commit.</summary>
        [Test]
        public void IncrementalVersion_004_RecordsPackageVersion_1_0_17()
        {
            const string staticVersionString = "1.0.17";
            Assert.That(staticVersionString, Is.EqualTo(staticVersionString));
        }

        /// <summary>Records package version 1.0.18 for this development commit.</summary>
        [Test]
        public void IncrementalVersion_005_RecordsPackageVersion_1_0_18()
        {
            const string staticVersionString = "1.0.18";
            Assert.That(staticVersionString, Is.EqualTo(staticVersionString));
        }
    }
}
