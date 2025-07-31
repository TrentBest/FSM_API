using System;
using System.Threading; // Essential for Thread.Sleep

using TheSingularityWorkshop.FSM_API;

namespace MyReviewerShowcaseFSM
{
    public class Program
    {
        public static string ReviewProcessGroup { get; set; } = "ReviewPG";
        static void Main(string[] args)
        {
            PortfolioViewer portfolioViewer = new PortfolioViewer(ReviewProcessGroup);

            // The loop should continue as long as the PortfolioViewer FSM is NOT in the "Quitting" state.
            // When it transitions to "Quitting", its OnEnterQuit method calls Environment.Exit(0).
            while (portfolioViewer.Status.CurrentState != "Quitting")
            {
                FSM_API.Interaction.Update(ReviewProcessGroup);
            }
        }
    }
}