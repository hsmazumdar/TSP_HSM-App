namespace TSP_CS
{
    /// <summary>
    /// Main entry point for the TSP Solver application.
    /// High-performance Traveling Salesman Problem solver supporting up to 1,000,000 cities
    /// with k-nearest neighbors optimization and adaptive virtual distance learning.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Application entry point. Initializes WinForms application and starts the main TSP solver form.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Initialize application with high DPI support and modern font rendering
            // For customization options, see: https://aka.ms/applicationconfiguration
            ApplicationConfiguration.Initialize();
            Application.Run(new Tsp());
        }
    }
}

