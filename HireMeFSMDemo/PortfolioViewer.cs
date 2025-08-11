using System;

using TheSingularityWorkshop.FSM_API;

namespace MyReviewerShowcaseFSM
{
    /// <summary>
    /// 
    /// </summary>
    public class PortfolioViewer : IStateContext
    {

        private readonly string COMMAND_REVIEW_COVER_LETTER = "c";
        private readonly string COMMAND_REVIEW_RESUME = "r";
        private readonly string COMMAND_REVIEW_GIT_PAGES = "gp";
        private readonly string COMMAND_REVIEW_GIT_REPO = "g";
        private readonly string COMMAND_REVIEW_NUGET_PACKAGE = "n";
        private readonly string COMMAND_REVIEW_PATREON_PAGE = "p";
        private readonly string COMMAND_REVIEW_LINKED_IN_PROFILE = "l";
        private readonly string COMMAND_QUIT = "q";

        /// <summary>
        /// 
        /// </summary>
        public FSMHandle Status { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string ReviewProcessGroup { get; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsValid { get; set; } = false;
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsPresenting { get; private set; } = false;
        /// <summary>
        /// 
        /// </summary>
        public CandidateProfile CandidateProfile { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public bool CoverLetterReviewed { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public bool ResumeReviewed { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public bool GitRepoReviewed { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public bool GitPagesReviewed { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public bool NugetPackageReviewed { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public bool PatreonPageReviewed { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Complete { get; private set; } = false;
        /// <summary>
        /// 
        /// </summary>
        public string? Input { get; private set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public bool LinkedInProfileReviewed { get; private set; }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reviewProcessGroup"></param>
        public PortfolioViewer(string reviewProcessGroup)
        {
            ReviewProcessGroup = reviewProcessGroup;
            Name = "PortfolioViewerFSM";
            CandidateProfile = new CandidateProfile();
            if (!FSM_API.Interaction.Exists(Name))
            {
                FSM_API.Create.CreateFiniteStateMachine(Name, -1, reviewProcessGroup)
                    .State("Initializing", OnEnterInitializing, null, OnExitInitializing)
                    .State("PresentingUserInterface", OnEnterPresentingUserInterface, OnUpdatePresentingUserInterface, OnExitPresentingUserInterface)

                    .State("ReviewingResume", OnEnterReviewingResume, null, OnExitReviewingResume) // No OnUpdate for reviewing states
                    .State("ReviewingCoverLetter", OnEnterReviewingCoverLetter, null, OnExitReviewingCoverLetter)
                    .State("ReviewingGitPages", OnEnterReviewingGitPages, null, OnExitReviewingGitPages)
                    .State("ReviewingGitRepo", OnEnterReviewingGitRepo, null, OnExitReviewingGitRepo)
                    .State("ReviewingNugetPackage", OnEnterReviewingNugetPackage, null, OnExitReviewingNugetPackage)
                    .State("ReviewingPatreonPage", OnEnterReviewingPatreonPage, null, OnExitReviewingPatreonPage) // Ensure this is null
                    .State("ReviewingLinkedInProfile", OnEnterReviewingLinkedInProfile, null, OnExitReviewingLinkedInProfile)

                    .State("Quitting", OnEnterQuit, null, null)

                    .Transition("Initializing", "PresentingUserInterface", ShouldBeginPresentUI)

                    .Transition("PresentingUserInterface", "ReviewingCoverLetter", ShouldReviewCoverLetter)
                    .Transition("PresentingUserInterface", "ReviewingResume", ShouldReviewResume)
                    .Transition("PresentingUserInterface", "ReviewingGitPages", ShouldReviewGitPages)
                    .Transition("PresentingUserInterface", "ReviewingGitRepo", ShouldReviewGitRepo)
                    .Transition("PresentingUserInterface", "ReviewingNugetPackage", ShouldReviewNugetPackage)
                    .Transition("PresentingUserInterface", "ReviewingPatreonPage", ShouldReviewPatreonPage)
                    .Transition("PresentingUserInterface", "ReviewingLinkedInProfile", ShouldReviewLinkedInProfile)

                    // Transitions from reviewing states back to presenting UI
                    .Transition("ReviewingCoverLetter", "PresentingUserInterface", ShouldPresentUI)
                    .Transition("ReviewingResume", "PresentingUserInterface", ShouldPresentUI)
                    .Transition("ReviewingGitPages", "PresentingUserInterface", ShouldPresentUI)
                    .Transition("ReviewingGitRepo", "PresentingUserInterface", ShouldPresentUI)
                    .Transition("ReviewingNugetPackage", "PresentingUserInterface", ShouldPresentUI)
                    .Transition("ReviewingPatreonPage", "PresentingUserInterface", ShouldPresentUI)
                    .Transition("ReviewingLinkedInProfile", "PresentingUserInterface", ShouldPresentUI)

                    .Transition("PresentingUserInterface", "Quitting", ShouldQuit)

                    .BuildDefinition();
            }
            CandidateProfile = new CandidateProfile();
            Status = FSM_API.Create.CreateInstance(Name, this, reviewProcessGroup);
            IsValid = true;
        }

        private void OnExitInitializing(IStateContext context)
        {
            // No specific actions needed on exit from Initializing
        }

        private bool ShouldBeginPresentUI(IStateContext context)
        {
            return true;
        }

        private void OnEnterInitializing(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                CandidateProfile candidate = new CandidateProfile();
                viewer.CandidateProfile = candidate;
                Console.WriteLine($"Welcome to the Portfolio Reviewer Tool Created Exclusively for your viewing pleasure! " +
                    $"\nLoading candidate profile for {viewer.CandidateProfile.CandidateName}");
                Console.WriteLine($"");
            }
        }

        private void OnEnterPresentingUserInterface(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                viewer.IsPresenting = false; 
                
            }
        }

        private void OnUpdatePresentingUserInterface(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                DisplayOptions(viewer);
                string? input = Console.ReadLine();

                if (input != null)
                {
                    input.ToLower();
                    viewer.Input = input; // Set input for transitions
                }

            }
        }

        private void OnExitPresentingUserInterface(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                viewer.IsPresenting = true;
            }
        }

        // --- Reviewing States: OnEnter calls the actual view method, OnExit handles post-viewing prompt ---

        private void OnEnterReviewingResume(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                viewer.IsPresenting = true;
                viewer.CandidateProfile.ViewResume();
                viewer.IsPresenting = false;
            }
        }

        private void OnExitReviewingResume(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
               
                viewer.ResumeReviewed = true; // Track that it was reviewed
                
            }
        }

        private void OnEnterReviewingCoverLetter(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                viewer.IsPresenting = true;
                viewer.CandidateProfile.ViewCoverLetter();
                viewer.IsPresenting = false;
            }
        }

        private void OnExitReviewingCoverLetter(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
              
                viewer.CoverLetterReviewed = true;
               
            }
        }

        private void OnEnterReviewingGitPages(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                viewer.IsPresenting = true;
                viewer.CandidateProfile.ViewGitPages();
                viewer.IsPresenting = false;
            }
        }

        private void OnExitReviewingGitPages(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
              
                viewer.GitPagesReviewed = true;
                
            }
        }

        private void OnEnterReviewingGitRepo(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                viewer.IsPresenting = true;
                viewer.CandidateProfile.ViewGitRepo();
                viewer.IsPresenting = false;
            }
        }

        private void OnExitReviewingGitRepo(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                
                viewer.GitRepoReviewed = true;
              
            }
        }

        private void OnEnterReviewingNugetPackage(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                viewer.IsPresenting = true;
                viewer.CandidateProfile.ViewNugetPackage();
                viewer.IsPresenting = false;
            }
        }

        private void OnExitReviewingNugetPackage(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
              
                viewer.NugetPackageReviewed = true;
               
            }
        }

        private void OnEnterReviewingPatreonPage(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                viewer.IsPresenting = true;
                viewer.CandidateProfile.ViewPatreonPage();
                viewer.IsPresenting = false;
            }
        }

        private void OnExitReviewingPatreonPage(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                
                viewer.PatreonPageReviewed = true;
                
            }
        }

        private void OnEnterReviewingLinkedInProfile(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                viewer.IsPresenting = true;
                viewer.CandidateProfile.ViewLinkedInProfile();
                viewer.IsPresenting = false;

            }
        }

        private void OnExitReviewingLinkedInProfile(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                
                viewer.LinkedInProfileReviewed = true; // Track that LinkedIn was reviewed
             
            }
        }

        private void OnEnterQuit(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                Console.WriteLine($"Thank you!");
                viewer.IsValid = false;
                Environment.Exit(0); // This will terminate the application
            }
        }

        // --- Transition Conditions ---

        private bool ShouldPresentUI(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                return viewer.IsPresenting == false;
            }
            return false;
        }

        private bool ShouldReviewCoverLetter(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                return viewer.Input == COMMAND_REVIEW_COVER_LETTER;
            }
            return false;
        }

        private bool ShouldReviewResume(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                return viewer.Input == COMMAND_REVIEW_RESUME;
            }
            return false;
        }

        private bool ShouldReviewGitPages(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                return viewer.Input == COMMAND_REVIEW_GIT_PAGES;
            }
            return false;
        }

        private bool ShouldReviewGitRepo(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                return viewer.Input == COMMAND_REVIEW_GIT_REPO;
            }
            return false;
        }

        private bool ShouldReviewNugetPackage(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                return viewer.Input == COMMAND_REVIEW_NUGET_PACKAGE;
            }
            return false;
        }

        private bool ShouldReviewPatreonPage(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                return viewer.Input == COMMAND_REVIEW_PATREON_PAGE;
            }
            return false;
        }

        private bool ShouldReviewLinkedInProfile(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                return viewer.Input == COMMAND_REVIEW_LINKED_IN_PROFILE;
            }
            return false;
        }


        private bool ShouldQuit(IStateContext context)
        {
            if (context is PortfolioViewer viewer)
            {
                return viewer.Input == COMMAND_QUIT;
            }
            return false;
        }

        // Helper method to display UI options, adapted to PortfolioViewer's FSM states.
        private static void DisplayOptions(PortfolioViewer viewer)
        {
            Console.WriteLine($"----------------{viewer.Status.CurrentState}--------------------------");
            Console.WriteLine("Available actions:");

            switch (viewer.Status.CurrentState)
            {
                case "PresentingUserInterface":
                    if (!viewer.CoverLetterReviewed) Console.WriteLine("  'c' - View Cover Letter");
                    if (!viewer.ResumeReviewed) Console.WriteLine("  'r' - View Resume");
                    if (!viewer.GitRepoReviewed) Console.WriteLine("  'g' - View GitHub Repo");
                    if (!viewer.GitPagesReviewed) Console.WriteLine("  'gp' - View GitHub Pages (Portfolio)");
                    if (!viewer.NugetPackageReviewed) Console.WriteLine("  'n' - View NuGet Package");
                    if (!viewer.PatreonPageReviewed) Console.WriteLine("  'p' - View Patreon Page");
                    if (!viewer.LinkedInProfileReviewed) Console.WriteLine("  'l' - View Linked In Profile");
                    Console.WriteLine("  'q' - Quit");
                    break;
                case "Quitting":
                    // Final message is displayed in OnEnterQuit. No further actions here.
                    break;
                default:
                    // If we are in another state, don't display options immediately
                    Console.WriteLine($"Here");
                    break;
            }
            Console.Write("Your action: ");
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasEnteredCurrentState { get; set; }
    }
}