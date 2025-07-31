using System;
using System.Diagnostics; // Required for Process.Start

namespace MyReviewerShowcaseFSM
{
    // This class acts as the context, holding all the data
    // related to my professional profile that a reviewer would examine.
    public class CandidateProfile
    {
        public string CandidateName { get; set; } = "Trent Best";

        // Flags to track which review items have been explored by the reviewer
        public bool CoverLetterReviewed { get; private set; } = false;
        public bool ResumeReviewed { get; private set; } = false;
        public bool GitRepoReviewed { get; private set; } = false;
        public bool GitPagesReviewed { get; private set; } = false;
        public bool NugetPackageReviewed { get; private set; } = false;
        public bool PatreonPageReviewed { get; private set; } = false;

        public bool InitialReviewCompleted => CoverLetterReviewed && ResumeReviewed && GitPagesReviewed && NugetPackageReviewed && PatreonPageReviewed && GitRepoReviewed;
        public bool LinkedInReviewed { get; private set; }

        // Constructor to initialize the candidate's name and context properties
        public CandidateProfile()
        {

        }

        // --- Methods for Reviewer Actions (Viewing Content) ---

        // Displays the candidate's resume with pagination
        public void ViewResume()
        {
            // Define resume sections manually for controlled pagination
            string[] resumeSections = new string[]
            {
                @"Trent Best
1202 Sequalish Street, Steilacoom, WA, 98388, USA | 1+512.296.0434 | TheSingularityWorkshop@gmail.com

Software Development Engineer | Technical Leader | UI/UX Innovator

HIGHLIGHTS
• Expert in C#, C++, Python, Java with extensive experience in architecting and developing robust software solutions.
• Proven track record in designing user-centric interfaces and automation tools, significantly improving efficiency (e.g., 7x efficiency gain at Amazon).
• Creator of the FSM_API, a pure C# Finite State Machine API with a NuGet package and CI/CD pipeline (GitHub: TrentBest/FSM_API).
• Passionate about game development UI/UX, including ""GUI-less"" and in-game UI paradigms, and innovative VR interfaces.
• Strong leader and mentor, skilled in enabling engineering and design teams to build state-of-the-art systems.",

                @"EXPERIENCE

Software Development Engineer | Amazon, Seattle, WA | 2020 – 2024
• Led discovery and initial development of ""One GUI"" concept: compositional UI for reducing data overload and providing flexible, user-modifiable components.
• Developed Revit External application for rapid integration of external commands via attribute-defined commands, accelerating development.
• Delivered software solutions resulting in a 7x efficiency gain, streamlining project timelines from two months to one week.

Senior VDC Engineer / Software Developer | BluHomes, Seattle, WA | 2018 – 2020
• Developed automation tools for architectural/construction workflows (Revit API, Dynamo), improving data consistency and project delivery.
• Implemented modular and reusable code practices, enhancing scalability and reducing technical debt.

VDC Engineer / Software Developer | Skanska USA Building, Seattle, WA | 2016 – 2018
• Created custom scripts/applications to automate data extraction/reporting from BIM models, reducing manual effort by 40%.
• Mentored junior VDC engineers on software best practices and efficient model management.",

                @"PROJECTS
• FSM_API (GitHub: https://github.com/TrentBest/FSM_API): Pure C# Finite State Machine API with NuGet package and CI/CD. (Currently developing more demos)
• FSM_API_Unity: In-progress Unity Asset Package demonstrating FSM_API integration.
• Conceptual VR Keyboard: Gaze-detection based VR keyboard with optimized layout.
• DirectX Windows Map Editor: College project with derivative for Hero Quest.

EDUCATION
• USN Nuclear Electrician's Mate Training (18 month program, Qualified in 10)
• USN Prototype Qualification Training
• USN Nuclear Naval Power Training Command 'B' - School
• USN Nuclear Naval Power Training Command 'A' - School
• California State University Chico, Computer Science studies (1997 - 2007)
• Nuclear Electricians Mate 3rd Class, US Navy (2004 – 2007) – USS Nimitz (CVN-68)"
            };

            Console.WriteLine("\n--- Displaying Trent Best's Resume ---");
            foreach (string section in resumeSections)
            {
                Console.WriteLine(section);
                // The WaitForAnyKey() call should remain inside the loop for section-based pagination
                WaitForAnyKey();
            }
            Console.WriteLine("\n--- End of Resume ---\n\n");
        }

        // Displays the candidate's cover letter with pagination
        public void ViewCoverLetter()
        {
            string[] coverLetterSections = new string[]
            {
                @"--- A Note from Trent Best ---

Welcome to my interactive portfolio showcase! Thank you for taking the time to explore my work.

As a software development engineer, I am passionate about crafting elegant, efficient systems and
empowering users through intuitive design. My career, while diverse, has consistently revolved
around complex software development, automation, and most importantly, designing user-centric
tools and interfaces. I thrive on translating intricate processes into streamlined, user-friendly
solutions, always with a focus on enhancing efficiency and the overall experience.",

@"You'll find that a core theme in my work is a deep understanding of user experience and a
proven history of creating powerful tools. From early projects like a DirectX Windows Map
Editor to leveraging modern APIs like Revit within Dynamo for workflow automation, I've
consistently built custom UI tools that transform how teams operate.

At Amazon, I was involved in the discovery and initial development of a ""One GUI"" concept.
This involved creating a compositional GUI where workflows and data streams could be wrapped
into flexible, user-modifiable components. The aim was to reduce data overload by delivering
precisely what users needed, allowing them to create and share team-specific components. This
work emphasized connecting local models, propagating data through the GUI system, and injecting
actions back into the system for seamless, modifiable interaction. I also developed a Revit
External application to rapidly integrate external commands, significantly accelerating
development cycles.",

@"My interest in UI/UX extends deeply into game development. As a hobbyist, I constantly explore
innovative paradigms within Unity, including ""GUI-less"" or in-game UI where the environment
itself serves as the interface. Imagine a game where environment navigation and in-world
elements replace traditional menus, providing a more immersive experience.

I've also conceptualized a working prototype idea for a VR keyboard. This design leverages
gaze detection with an optimized layout that places frequently used characters centrally,
surrounded by others in a tight, invisible circle for efficient input. Characters animate upon
gaze to confirm selection, with an option to cancel by looking away. Essential non-letter keys
are strategically placed for ergonomic interaction. This exemplifies the creative, problem-solving
approach to UI that I bring to complex challenges.",

@"My technical skillset is robust, encompassing C++, C#, Python, and Java, underpinned by a strong
foundation in software design and architecture. I've engineered and developed a powerful,
pure C# Finite State Machine (FSM) API, available on GitHub at https://github.com/TrentBest/FSM_API.
This project includes a corresponding NuGet package and an established CI workflow that publishes the
NuGet package, pushes the correct .dll to my FSM_API_Unity repository, and also to my private raWWar
repository (my current hobby project). I am also actively developing an in-progress Unity Asset
Package for the FSM_API, further demonstrating my commitment to creating versatile, reusable systems.
I am adept at navigating large codebases, familiar with multi-threading principles, and consistently
focus on optimizing performance.",

@"Beyond technical execution, I am a strong advocate for mentorship and coaching. My drive is to
provide the tools, building blocks, and user-friendly, configurable systems that empower teams to
bring visually and aesthetically pleasing UI to life. I coordinate closely with UX to define clear
acceptance criteria for menus and HUDs. My track record includes leading teams and delivering
significant efficiency gains, such as a 7x efficiency gain achieved at Amazon by streamlining
project timelines from two months to one week.

I hope this showcase provides a clear understanding of my capabilities and my approach to software
development and UI/UX. Please explore the other sections of my portfolio to see more examples of my work.

Sincerely,

Trent Best"
            };

            Console.WriteLine($"\n\n--- Displaying {CandidateName}'s Cover Letter ---");
            foreach (string section in coverLetterSections)
            {
                Console.WriteLine(section);
                WaitForAnyKey();
            }
            Console.WriteLine("\n\n--- End of Cover Letter ---\n\n");
        }

        // Opens the GitHub Repository URL in the default browser
        public void ViewGitRepo()
        {
            const string url = "https://github.com/TrentBest/FSM_API";
            Console.WriteLine($"Opening GitHub Repository: {url}");
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening browser: {ex.Message}");
            }
            //WaitForAnyKey();
        }

        // Opens the GitHub Pages URL in the default browser
        public void ViewGitPages()
        {
            const string url = "https://trentbest.github.io/";
            Console.WriteLine($"Opening GitHub Pages (Trent Best's Portfolio): {url}");
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening browser: {ex.Message}");
            }
            //WaitForAnyKey();
        }

        // Opens the NuGet Package URL in the default browser
        public void ViewNugetPackage()
        {
            const string url = "https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API";
            Console.WriteLine($"Opening FSM_API NuGet Package: {url}");
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening browser: {ex.Message}");
            }
            //WaitForAnyKey();
        }

        // Opens the Patreon Page URL in the default browser
        public void ViewPatreonPage()
        {
            const string url = "https://www.patreon.com/TheSingularityWorkshop";
            Console.WriteLine($"Opening Patreon Page: {url}");
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening browser: {ex.Message}");
            }
            //WaitForAnyKey();
        }

        public void ViewLinkedInProfile()
        {
            const string url = "https://www.linkedin.com/in/trent-best-3432a365/";
            Console.WriteLine($"Opening LinkedIn Profile: {url}");
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening browser: {ex.Message}");
            }
            //WaitForAnyKey();
        }

        public void WaitForAnyKey()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n\nPress any key to continue...\n\n");
            Console.ResetColor();
            Console.ReadKey(true);
        }
    }
}