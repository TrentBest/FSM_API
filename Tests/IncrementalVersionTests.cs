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
            Assert.AreEqual(staticVersionString, staticVersionString);
        }
    }
}
