using NUnit.Framework;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    /// <summary>
    /// Provides the permanent incremental package-version ledger for FSM_API development commits.
    /// </summary>
    /// <remarks>
    /// AI DEVELOPMENT CONTRACT:
    ///
    /// This class is intentionally simple. Each development commit made while the package is
    /// evolving must add exactly one new NUnit test method to this class. The test records the
    /// package version represented by that commit.
    ///
    /// For every new commit:
    /// 1. Increment the package version in FSM_API/FSM_API.csproj.
    /// 2. Add one new test method here using the new package version as the static value.
    /// 3. The test must assert the static version string against itself:
    ///        Assert.AreEqual(staticVersionString, staticVersionString);
    /// 4. Do not rewrite, remove, or renumber historical version tests.
    /// 5. The version in the new test and the package version in the same commit must match.
    ///
    /// The assertion is deliberately trivial. Its purpose is provenance, not behavioral
    /// verification: it leaves an immutable, executable marker showing which package version
    /// was associated with each development commit. This lets a future reader, AI agent, or
    /// maintainer correlate source history with package history without guessing.
    ///
    /// This ledger is especially important during the v2 development line, where breaking
    /// changes are being developed deliberately. Semantic-version compatibility still matters:
    /// the major version identifies the compatibility boundary, while the development suffix
    /// records the precise progression toward the eventual release.
    /// </remarks>
    [TestFixture]
    public sealed class IncrementalVersionTests
    {
        /// <summary>
        /// Package-version provenance marker for commit 1.0.15.
        /// </summary>
        [Test]
        public void IncrementalVersion_001_RecordsPackageVersion_1_0_15()
        {
            const string staticVersionString = "1.0.15";
            Assert.AreEqual(staticVersionString, staticVersionString);
        }
    }
}
