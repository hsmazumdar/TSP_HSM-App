using System.Collections.Generic;
using System.IO;

namespace TSP_CS
{
    /// <summary>
    /// Main Windows Forms UI for the TSP Solver application.
    /// 
    /// This form provides the user interface for configuring and running TSP solver instances.
    /// It manages the interaction between the user and the TspMain engine, handles visualization,
    /// and coordinates background algorithm execution with UI updates.
    /// 
    /// Key Responsibilities:
    /// - UI initialization and settings management (loads from Tune.txt)
    /// - Button event handling (DRAW, START, STOP, TUNE)
    /// - Real-time display updates via timers
    /// - Route visualization on PictureBox
    /// - Thread-safe communication with background algorithm thread
    /// </summary>
    public partial class Tsp : Form
    {
        // ====================================================================
        // Private Fields: Core Components and State
        // ====================================================================
        
        /// <summary>Core TSP solver engine instance</summary>
        private readonly TspMain tsp;
        
        /// <summary>Timer for algorithm progress updates (1ms interval)</summary>
        private readonly System.Windows.Forms.Timer algoTimer;
        
        /// <summary>Timer for display refresh (1 second interval)</summary>
        private readonly System.Windows.Forms.Timer displayTimer;
        
        /// <summary>Flag indicating whether route display is enabled</summary>
        private bool dispFlg;
        
        /// <summary>Track if help text was successfully loaded from HelpText.rtf</summary>
        private bool helpTextLoaded = false;
        
        /// <summary>Prevents route loading when timer updates nudTrials control</summary>
        private bool updatingTrialsFromTimer = false;
        
        /// <summary>Current display mode: "Distance" or "Gap%" (toggled by clicking distance label)</summary>
        private string distnow = "";

        // ====================================================================
        // Constructor and Initialization
        // ====================================================================
        
        /// <summary>
        /// Initialize the main TSP solver form with UI, engine, timers, and default values.
        /// Loads settings from Tune.txt if available, otherwise uses install-time defaults.
        /// </summary>
        public Tsp()
        {
            InitializeComponent();
            // Align combo options with legacy C++ dialog
            cboShape.Items.Clear();
            cboShape.Items.AddRange(new object[] { "Random", "Square", "Spiral", "Circle" });
            cboShape.SelectedIndex = 1; // Default to Square (index 1)

            // initialize core engine and timers
            tsp = new TspMain();
            
            // Set callback for matrix choice dialog (allows Core to request user input without UI dependency)
            tsp.MatrixChoiceCallback = () =>
            {
                using (var dialog = new MatrixChoiceDialog())
                {
                    var parentForm = pictureWorld?.FindForm();
                    var result = dialog.ShowDialog(parentForm);
                    if (result == DialogResult.OK)
                    {
                        return dialog.UseOldMatrix;
                    }
                    return false; // Default to new matrix if cancelled
                }
            };
            
            algoTimer = new System.Windows.Forms.Timer();
            displayTimer = new System.Windows.Forms.Timer();
            algoTimer.Interval = 1;
            displayTimer.Interval = 1000;
            algoTimer.Tick += AlgoTimer_Tick;
            displayTimer.Tick += DisplayTimer_Tick;

            // Wire form events (shape selection change handler removed to prevent double-draw)
            this.Shown += Tsp_Shown;
            this.FormClosing += Tsp_FormClosing;
            pictureWorld.Resize += PictureWorld_Resize;

            //*******************************************************
            // Populate initial global variables - load from Tune.txt or use defaults
            dispFlg = false;

            // Try to load saved settings from Tune.txt
            var savedSettings = TspTune.LoadSettings();

            if (savedSettings != null)
            {
                // Load from Tune.txt (previous session)
                txtPorts.Text = savedSettings.GetValueOrDefault("Cities", TspTune.GetDefault("Cities"));
                cboShape.SelectedItem = savedSettings.GetValueOrDefault("CityDistribution", TspTune.GetDefault("CityDistribution"));

                // Load algorithm parameters directly into engine (removed from main form UI)
                tsp.boundingthreshold = int.TryParse(savedSettings.GetValueOrDefault("BoundingThreshold", TspTune.GetDefault("BoundingThreshold")), out int bt) ? bt : 200000;
                tsp.minimumthreshold = int.TryParse(savedSettings.GetValueOrDefault("MinimumThreshold", TspTune.GetDefault("MinimumThreshold")), out int mt) ? mt : 80000;
                tsp.iterationfactor = int.TryParse(savedSettings.GetValueOrDefault("IterationFactor", TspTune.GetDefault("IterationFactor")), out int it) ? it : 100;
                tsp.normalization = int.TryParse(savedSettings.GetValueOrDefault("Normalization", TspTune.GetDefault("Normalization")), out int norm) ? norm : 160000;

                // Load visualization parameters
                tsp.NodeRadiusPixels = int.TryParse(savedSettings.GetValueOrDefault("NodeSize", TspTune.GetDefault("NodeSize")), out int ns) ? ns : 3;
                tsp.MarginPixels = int.TryParse(savedSettings.GetValueOrDefault("Margin", TspTune.GetDefault("Margin")), out int mg) ? mg : 10;

                // Load k-neighbors parameter for O(N×k) optimization
                tsp.kNeighbors = int.TryParse(savedSettings.GetValueOrDefault("KNeighbors", TspTune.GetDefault("KNeighbors")), out int kn) ? kn : 500;

                // Load full scan threshold parameter
                tsp.fullScanThreshold = int.TryParse(savedSettings.GetValueOrDefault("FullScanThreshold", TspTune.GetDefault("FullScanThreshold")), out int fst) ? fst : 200;
            }
            else
            {
                // Use install-time defaults (first run or Tune.txt missing)
                txtPorts.Text = TspTune.GetDefault("Cities");
                cboShape.SelectedItem = TspTune.GetDefault("CityDistribution");

                // Load algorithm defaults directly into engine (removed from main form UI)
                tsp.boundingthreshold = int.Parse(TspTune.GetDefault("BoundingThreshold"));
                tsp.minimumthreshold = int.Parse(TspTune.GetDefault("MinimumThreshold"));
                tsp.iterationfactor = int.Parse(TspTune.GetDefault("IterationFactor"));
                tsp.normalization = int.Parse(TspTune.GetDefault("Normalization"));

                // Load visualization defaults - match ASPX version
                tsp.NodeRadiusPixels = 4;  // Match ASPX default
                tsp.MarginPixels = 20;     // Match ASPX default

                // Load k-neighbors default
                tsp.kNeighbors = int.Parse(TspTune.GetDefault("KNeighbors"));

                // Load full scan threshold default
                tsp.fullScanThreshold = int.Parse(TspTune.GetDefault("FullScanThreshold"));
            }

            // Fixed display values (not in Tune.txt)
            txtDistance.Text = "0.00";               // m_dist
            nudTrials.Value = 0;                 // m_trials (0 = no solution)
            txtTime.Text = "0.00";                 // elapsed time

            // Initialize engine with base values
            tsp.trials = 0;
            tsp.rout = 0;
            tsp.maxToursWithoutImprovement = 3; // 0 = disabled, N = auto-stop after N tours without improvement

            // Initialize NumericUpDown controls with engine values
            nudNodeSize.Value = tsp.NodeRadiusPixels;
            nudMargin.Value = tsp.MarginPixels;

            // Ensure normalization is set before any operations (critical for Restart/PopulateCityes)
            if (tsp.normalization == 0) tsp.normalization = 160000; // Safety default (matches ASPX scale)

            // Note: Graphics and bitmap are now created in PopulateCityes when DRAW is pressed
        }
        //*******************************************************
        // On form shown: fit form to screen height and stretch picture box to panel bounds
        private void Tsp_Shown(object? sender, EventArgs e)
        {
            // Get screen working area (excludes taskbar)
            var screen = Screen.FromControl(this);
            var workingArea = screen.WorkingArea;
            this.Height = screen.WorkingArea.Height;
            this.Location = new Point(0, 0);
            var fw = this.Width;
            var gapx = fw - pictureWorld.Width;
            this.Width = gapx + pictureWorld.Height;
            // Do NOT dock pictureWorld - it needs to match bitmap size for scroll bars to work
            // pictureWorld.Dock = DockStyle.Fill; // REMOVED - prevents scroll bars
            tsp.pictureWorld = pictureWorld;
            // Note: Graphics and bitmap are created in PopulateCityes when DRAW is pressed
            // pictureWorld size will be set to match bitmap size, scroll bars will appear if needed
        }
        //*******************************************************
        // OK button: close application
        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }
        //*******************************************************
        // Draw button: populate cities based on selected shape (toggle DRAW/STOP)
        private void btnDRAW_Click(object sender, EventArgs e)
        {
            if (btnDrawShape.Text == "DRAW")
            {
                // Stop existing draw thread if running
                if (tsp.drawThread != null && tsp.drawThread.IsAlive)
                {
                    tsp.StopDrawThread();
                    tsp.drawThread.Join(1000); // Wait up to 1 second for thread to stop
                }

                // Start DRAW operation in background thread
                btnDrawShape.Text = "STOP";
                var shape = (string?)cboShape.SelectedItem ?? "Random";
                tsp.pictureWorld = pictureWorld;

                // Read UI values on UI thread before starting background thread
                int ports = int.TryParse(txtPorts.Text, out var p) ? p : 100;

                // Reset distance display immediately when DRAW is pressed
                txtDistance.Text = "0.00";
                txtTime.Text = "0.00";
                updatingTrialsFromTimer = true; // Prevent route loading during reset
                try
                {
                    nudTrials.Value = 0;  // 0 = no solution
                    nudTrials.Maximum = 9999; // Reset maximum for new run
                }
                finally
                {
                    updatingTrialsFromTimer = false;
                }

                // Reset cancellation flag
                tsp.shouldStopDraw = false;

                // Initialize DRAW progress tracking
                tsp.drawStartTime = System.DateTime.Now;
                tsp.drawProgressMessage = "Initializing...";

                // Set wait cursor on UI thread BEFORE starting background thread
                System.Windows.Forms.Cursor previousCursor = System.Windows.Forms.Cursor.Current;
                this.UseWaitCursor = true;
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                Application.DoEvents(); // Process cursor change immediately

                // Start display timer to show DRAW progress (isolated from GO operation)
                displayTimer.Start();

                // Create and start background thread for DRAW operation
                tsp.drawThread = new System.Threading.Thread(() =>
                {
                    try
                    {
                        OnShape(shape, ports);
                    }
                    finally
                    {
                        // Reset button text and cursor on UI thread after operation completes
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() =>
                            {
                                btnDrawShape.Text = "DRAW";
                                this.UseWaitCursor = false;
                                System.Windows.Forms.Cursor.Current = previousCursor;
                                // Stop display timer when DRAW completes
                                displayTimer.Stop();
                                tsp.drawProgressMessage = "";
                                if (!tsp.shouldStopDraw)
                                {
                                    txtDistance.Text = "wait";
                                    txtTime.Text = "0.00";
                                    // Focus nudTrials after DRAW completes
                                    nudTrials.Focus();
                                }
                            }));
                        }
                        else
                        {
                            btnDrawShape.Text = "DRAW";
                            this.UseWaitCursor = false;
                            System.Windows.Forms.Cursor.Current = previousCursor;
                            // Stop display timer when DRAW completes
                            displayTimer.Stop();
                            tsp.drawProgressMessage = "";
                            if (!tsp.shouldStopDraw)
                            {
                                txtDistance.Text = "wait";
                                txtTime.Text = "0.00";
                                // Focus nudTrials after DRAW completes
                                nudTrials.Focus();
                            }
                        }
                    }
                })
                {
                    IsBackground = true,
                    Name = "DRAW Thread"
                };
                tsp.drawThread.Start();
            }
            else
            {
                // STOP DRAW operation
                tsp.StopDrawThread();
                btnDrawShape.Text = "DRAW";
                // Stop display timer when DRAW is stopped
                displayTimer.Stop();
                tsp.drawProgressMessage = "";
                txtTime.Text = "0.00";
                // Restore cursor when stopping
                this.UseWaitCursor = false;
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
        }
        //*******************************************************
        // Toggle GO/STOP button
        private void btnToggleRun_Click(object sender, EventArgs e)
        {
            if (btnToggleRun.Text == "TOUR")
            {
                // Reset distance display immediately when GO is pressed
                txtDistance.Text = "0.00";
                txtTime.Text = "0.00";
                updatingTrialsFromTimer = true; // Prevent route loading during reset
                try
                {
                    nudTrials.Value = 0;  // 0 = no solution
                    nudTrials.Maximum = 9999; // Reset maximum for new run
                }
                finally
                {
                    updatingTrialsFromTimer = false;
                }

                // Only allow GO if DRAW has been pressed (cities exist)
                // We check by looking at tsp.rect - if it's empty, no cities have been drawn
                if (tsp.rect.Width == 0 || tsp.rect.Height == 0)
                {
                    // No cities drawn yet - don't allow GO
                    System.Windows.Forms.MessageBox.Show("Please press DRAW first to generate cities.",
                        "Draw Required", System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Information);
                    return;
                }
                OnGo();
                btnToggleRun.Text = "STOP";
                txtDistance.Text = "wait";
            }
            else
            {
                OnStop();
                btnToggleRun.Text = "TOUR";
            }
        }
        //*******************************************************
        // Checkbox: toggle display updates (replaces DISPLAY/MINIMUM buttons)
        private void chkDisplay_CheckedChanged(object? sender, EventArgs e)
        {
            dispFlg = chkDisplay.Checked;
        }
        //*******************************************************

        // Shape combo: DO NOT auto-draw - only DRAW button triggers drawing
        // private void cboShape_SelectedIndexChanged(object? sender, EventArgs e)
        // {
        //     var shape = (string?)cboShape.SelectedItem ?? "Random";
        //     OnShape(shape);
        // }
        //*******************************************************
        // Algorithm timer tick - REMOVED: Algorithm now runs in background thread
        // This method kept for compatibility but no longer used
        private void AlgoTimer_Tick(object? sender, EventArgs e)
        {
            // Algorithm now runs in background thread - timer no longer needed
            // Check if algorithm auto-stopped and update UI
            if (tsp.autoStopped)
            {
                OnStop(); // stop timers and thread
                tsp.autoStopped = false; // reset flag
                btnToggleRun.Text = "TOUR"; // reset button text
            }
        }
        //*******************************************************
        // Display timer tick (legacy 1235): refresh stats and drawing
        // FLAG ISOLATION: Shows DRAW progress when DRAW is running, otherwise shows GO/algorithm stats
        private void DisplayTimer_Tick(object? sender, EventArgs e)
        {
            // FLAG ISOLATION: Check DRAW operation first (isolated from GO)
            if (tsp.IsDrawRunning)
            {
                // DRAW operation is running - show DRAW progress
                var elapsed = System.DateTime.Now - tsp.drawStartTime;
                txtTime.Text = elapsed.TotalSeconds.ToString("F2");  // Show elapsed time for DRAW

                // Show DRAW progress message in distance field
                if (!string.IsNullOrEmpty(tsp.drawProgressMessage))
                {
                    txtDistance.Text = tsp.drawProgressMessage;
                }
                else
                {
                    txtDistance.Text = "Drawing...";
                }

                // Clear other fields during DRAW
                nudTrials.Value = 0;

                // Don't draw routes during DRAW (cities are shown but routes are not ready yet)
                return;  // Exit early - don't process algorithm stats when DRAW is running
            }

            // GO operation (algorithm) - show algorithm stats (original logic from old version)
            // Update elapsed time (always show, even if algorithm not running)
            if (tsp.IsAlgorithmRunning || tsp.trials > 0)
            {
                var elapsed = System.DateTime.Now - tsp.algorithmStartTime;
                txtTime.Text = elapsed.TotalSeconds.ToString("F2");  // 2 decimal places
            }
            else
            {
                txtTime.Text = "0.00";
            }

            // update stats and optionally plot
            // txtDistance = current/best route length (tsp.rout) or Gap%
            // nudTrials = count of completed tours (tsp.trials increments on each successful tour), 0 = no solution
            // Only display distance if at least one route has been found (trials > 0)
            // This prevents displaying partial/wrong values before rout_found() is called
            if (tsp.trials > 0)
            {
                // Route has been found - display valid distance with 2 decimal places
                // NOTE: For square grids (dtyp == 2), distances are already in normalized grid units
                // No conversion needed - distances are normalized at calculation time
                txtDistance.Text = ((float)tsp.rout).ToString("F2");

            }
            else
            {
                // No routes found yet - show defaults
                txtDistance.Text = "0.00";
            }

            // If trials == 0, keep showing "wait" (set by GO button) until first route is found
            try
            {
                updatingTrialsFromTimer = true; // Prevent route loading when timer updates
                // Update maximum to match actual trials (lock to available trials)
                // During execution, allow growth; after stop, lock to actual count
                int maxTrial = tsp.trials;
                if (tsp.IsAlgorithmRunning)
                {
                    // During execution: allow some buffer for upcoming trials, but cap reasonably
                    maxTrial = Math.Max(maxTrial, (int)nudTrials.Maximum);
                    nudTrials.Maximum = Math.Min(9999, maxTrial + 50); // Small buffer during execution
                }
                else
                {
                    // Algorithm stopped: lock maximum to actual trials (trial 0 = best is always available)
                    nudTrials.Maximum = Math.Max(0, tsp.trials); // Lock to actual trials
                }
                // Only update value if it's different to avoid unnecessary ValueChanged events
                if ((int)nudTrials.Value != tsp.trials)
                {
                    nudTrials.Value = tsp.trials;
                }
            }
            finally
            {
                updatingTrialsFromTimer = false; // Always reset flag, even if exception occurs
            }
            if (dispFlg)
            {
                // Update bitmap and PictureBox on UI thread (safe here)
                if (tsp.bmpWorld != null)
                {
                    // Only draw routes if at least one complete route has been found (trials > 0)
                    // This prevents displaying partial routes before rout_found() is called
                    if (tsp.trials > -1)
                    {
                        // Check if algorithm has stopped - show last route if stopped
                        bool algorithmStopped = !tsp.IsAlgorithmRunning;
                        int displayMode;

                        if (algorithmStopped && tsp.trials > 0)
                        {
                            // Algorithm stopped - show last completed route (mode 2)
                            displayMode = 2;
                        }
                        else
                        {
                            // Algorithm running - check if route was improved - display best route for 5 seconds
                            bool showBestRoute = false;
                            if (tsp.routeImproved)
                            {
                                var elapsed = System.DateTime.Now - tsp.routeImprovedTime;
                                if (elapsed.TotalSeconds < 5.0)
                                {
                                    showBestRoute = true;  // Show best route for 5 seconds
                                }
                                else
                                {
                                    tsp.routeImproved = false;  // Reset flag after 5 seconds
                                }
                            }
                            // Mode 1 = best route (mincityroute), Mode 0 = current route (cityroute)
                            displayMode = showBestRoute ? 1 : 0;
                        }

                        // Update bitmap drawing (thread-safe - bitmap updates are fine from any thread)
                        tsp.plot_rout(displayMode);

                        // Update PictureBox on UI thread (we're already on UI thread in timer)
                        if (pictureWorld.Image != tsp.bmpWorld)
                        {
                            pictureWorld.Image = tsp.bmpWorld;
                        }
                        pictureWorld.Refresh();
                    }
                    // If trials == 0, don't update display - cities are already shown from Restart()
                    // This prevents crashes and interference with algorithm thread
                }
            }

            // Check if algorithm auto-stopped (from background thread)
            if (tsp.autoStopped)
            {
                OnStop(); // stop thread and timer
                tsp.autoStopped = false; // reset flag
                btnToggleRun.Text = "TOUR"; // reset button text
            }
        }
        //*******************************************************
        // Keep graphics and rect updated on resize
        private void PictureWorld_Resize(object? sender, EventArgs e)
        {
            // Note: Bitmap size is determined by city count and pattern, not by pictureWorld size
            // If bitmap exists, refresh the display (scroll bars will appear automatically if needed)
            if (tsp.bmpWorld != null)
            {
                // Set Image first, then Size to avoid parameter validation errors
                pictureWorld.Image = tsp.bmpWorld;
                pictureWorld.Size = tsp.bmpWorld.Size; // Maintain bitmap size for scrolling
                pictureWorld.Refresh();
            }
        }
        //*******************************************************
        // Start training: sync UI to engine, reset, and start background thread
        // NOTE: Algorithm runs in background thread until STOP button is pressed
        private void OnGo()
        {
            // C++: ptsp->Restart() is called FIRST, before parameters
            tsp.Restart();

            // Algorithm parameters are now in tsp engine (set via TUNE dialog or loaded from Tune.txt)
            // No need to sync from UI - parameters are already in the engine

            // reset and start (C++: ptsp->trials=m_trials=0; followed by StartTour)
            tsp.trials = 0;
            tsp.autoStopped = false; // reset auto-stop flag
            tsp.shouldStop = false;  // reset stop flag
            tsp.StartTour();
            dispFlg = chkDisplay.Checked; // sync with checkbox

            // Start algorithm in background thread (replaces timer-based execution)
            tsp.StartAlgorithmThread();

            // Keep display timer for UI updates only
            displayTimer.Start();
        }
        //*******************************************************
        // Stop training: stop background algorithm thread and display timer
        private void OnStop()
        {
            // Stop background algorithm thread (sets shouldStop flag)
            tsp.StopAlgorithmThread();

            // Stop display timer
            displayTimer.Stop();

            // Lock maximum to actual trials after algorithm stops
            updatingTrialsFromTimer = true; // Prevent route loading during update
            try
            {
                nudTrials.Maximum = Math.Max(0, tsp.trials); // Lock to actual trials
            }
            finally
            {
                updatingTrialsFromTimer = false;
            }

            // After algorithm completes, copy best trial to trial 0 and navigate to it
            // Only save trial 0 if we have a valid complete solution
            if (tsp.trials > 0 && tsp.bestTrial > 0 && tsp.minrout != int.MaxValue)
            {
                // Load the best trial's route from file
                var (bestRoute, _, bestSolutionTime) = tsp.LoadRouteSolution(tsp.bestTrial);

                if (bestRoute != null)
                {
                    // Validate route completeness: ensure all cities are present exactly once
                    var uniqueCities = new HashSet<int>();
                    bool routeIsComplete = true;
                    
                    if (bestRoute.Length == tsp.numcity + 1)
                    {
                        foreach (int cityIndex in bestRoute)
                        {
                            if (cityIndex < 0 || cityIndex > tsp.numcity)
                            {
                                routeIsComplete = false;
                                break;
                            }
                            if (!uniqueCities.Add(cityIndex))
                            {
                                routeIsComplete = false; // Duplicate city found
                                break;
                            }
                        }
                        routeIsComplete = routeIsComplete && (uniqueCities.Count == tsp.numcity + 1);
                    }
                    else
                    {
                        routeIsComplete = false;
                    }

                    if (routeIsComplete)
                    {
                        // FIX: Use minrout (current best distance in memory) instead of loading from file
                        // This ensures trial 0 uses the correct normalized distance, not an old buggy file
                        // minrout is already in normalized grid units for square grids
                        int distanceToSave = tsp.minrout;

                        // Save best route to trial 0 with correct distance (only if complete)
                        tsp.SaveRouteSolution(0, bestRoute, distanceToSave, bestSolutionTime);

                        // Navigate to trial 0 to show the best solution
                        updatingTrialsFromTimer = true; // Prevent route loading during update
                        try
                        {
                            nudTrials.Value = 0;
                        }
                        finally
                        {
                            updatingTrialsFromTimer = false;
                        }

                        // Load and display the best route (trial 0)
                        LoadAndDisplayRoute(0);
                    }
                    // If route is incomplete, skip saving to trial 0 (prevents negative gaps)
                }
            }

            // Display last route when algorithm stops (mode 2 = last route)
            if (tsp.bmpWorld != null && tsp.trials > 0 && dispFlg)
            {
                tsp.plot_rout(2); // Display last completed route
                pictureWorld.Image = tsp.bmpWorld;
                pictureWorld.Refresh();
            }
        }
        //*******************************************************
        // Shape dispatcher: populate cities based on selected label
        // Note: Can be called from background thread, so UI operations must be marshalled
        private void OnShape(string shape, int ports)
        {
            int pattern = 1; // default Random
            if (string.Equals(shape, "Random", StringComparison.OrdinalIgnoreCase)) pattern = 1;
            else if (string.Equals(shape, "Square", StringComparison.OrdinalIgnoreCase)) pattern = 2;
            else if (string.Equals(shape, "Circle", StringComparison.OrdinalIgnoreCase)) pattern = 3;
            else if (string.Equals(shape, "Spiral", StringComparison.OrdinalIgnoreCase)) pattern = 4;

            // Populate cities - this creates the bitmap and draws cities
            // plot_ap() is called inside PopulateCityes, so we refresh immediately after
            tsp.PopulateCityes(ports, pattern);

            // Refresh pictureWorld immediately to show cities before distance matrix processing
            // Must marshal to UI thread if called from background thread
            if (tsp.bmpWorld != null)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        // Set Image first, then SizeMode to avoid parameter validation errors
                        pictureWorld.Image = tsp.bmpWorld;
                        pictureWorld.Size = tsp.bmpWorld.Size;
                        pictureWorld.SizeMode = PictureBoxSizeMode.Normal; // Use Normal to show actual bitmap size
                        pictureWorld.Refresh();
                        // Force immediate UI update - process pending paint messages
                        Application.DoEvents();
                        // Also invalidate to ensure repaint
                        pictureWorld.Invalidate();
                        Application.DoEvents();
                        // Focus nudTrials after DRAW completes
                        nudTrials.Focus();
                    }));
                }
                else
                {
                    // Set Image first, then SizeMode to avoid parameter validation errors
                    pictureWorld.Image = tsp.bmpWorld;
                    pictureWorld.Size = tsp.bmpWorld.Size;
                    pictureWorld.SizeMode = PictureBoxSizeMode.Normal; // Use Normal to show actual bitmap size
                    pictureWorld.Refresh();
                    // Force immediate UI update - process pending paint messages
                    Application.DoEvents();
                    // Also invalidate to ensure repaint
                    pictureWorld.Invalidate();
                    Application.DoEvents();
                    // Focus nudTrials after DRAW completes
                    nudTrials.Focus();
                }
            }
        }
        //*******************************************************
        // One algorithm iteration (legacy Toure)
        // UNUSED METHOD - COMMENTED OUT (algorithm now runs in background thread)
        /*
        private void OnAlgoTimer()
        {
            // run one algorithm iteration
            tsp.Toure();
        }
        */
        //*******************************************************
        // HELP button: toggle RichTextBox visibility and populate with formatted help
        private void btnHELP_Click(object sender, EventArgs e)
        {
            if (rtbxHELP.Visible)
            {
                // Hide help
                rtbxHELP.Visible = false;
                btnHELP.Text = "HELP";
            }
            else
            {
                // Show help with formatted content
                PopulateHelp();
                rtbxHELP.Visible = true;
                btnHELP.Text = "CLOSE";
            }
        }
        //*******************************************************
        // Populate RichTextBox with formatted help text from external file
        private void PopulateHelp()
        {
            string helpFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelpText.rtf");

            try
            {
                if (File.Exists(helpFilePath))
                {
                    // Load from external file
                    rtbxHELP.LoadFile(helpFilePath, RichTextBoxStreamType.RichText);
                    // Verify content was actually loaded
                    if (!string.IsNullOrWhiteSpace(rtbxHELP.Text) && rtbxHELP.Rtf.Length > 200)
                    {
                        helpTextLoaded = true;
                        // DEBUG/LOG CODE - COMMENTED OUT
                        //System.Diagnostics.Debug.WriteLine("Help text loaded successfully from file");
                    }
                    else
                    {
                        // DEBUG/LOG CODE - COMMENTED OUT
                        //System.Diagnostics.Debug.WriteLine("Help file exists but appears empty, creating default content");
                        CreateDefaultHelpContent();
                        helpTextLoaded = true;
                    }
                }
                else
                {
                    // File doesn't exist - create default content and save it
                    CreateDefaultHelpFile(helpFilePath);
                    rtbxHELP.LoadFile(helpFilePath, RichTextBoxStreamType.RichText);
                    helpTextLoaded = true;
                    // DEBUG/LOG CODE - COMMENTED OUT
                    //System.Diagnostics.Debug.WriteLine("Default help file created and loaded");
                }
            }
            catch (Exception ex)
            {
                // If loading fails, create default content in memory
                // DEBUG/LOG CODE - COMMENTED OUT
                //System.Diagnostics.Debug.WriteLine($"Failed to load help file: {ex.Message}");
                CreateDefaultHelpContent();
                helpTextLoaded = true; // Still mark as loaded since we have default content
            }

            // Scroll to top
            rtbxHELP.SelectionStart = 0;
            rtbxHELP.SelectionLength = 0;
        }

        //*******************************************************
        // Create default help content in RichTextBox (fallback)
        private void CreateDefaultHelpContent()
        {
            rtbxHELP.Clear();

            // Title
            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 14, FontStyle.Bold);
            rtbxHELP.SelectionColor = System.Drawing.Color.Blue;
            rtbxHELP.AppendText("Traveling Salesman Problem\n");
            rtbxHELP.AppendText("Quick Start Guide\n\n");

            // Quick Start Section
            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 11, FontStyle.Bold);
            rtbxHELP.SelectionColor = System.Drawing.Color.DarkGreen;
            rtbxHELP.AppendText("QUICK START:\n");

            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 10, FontStyle.Regular);
            rtbxHELP.SelectionColor = System.Drawing.Color.Black;
            rtbxHELP.AppendText("1. Select city pattern (Random/Square/Circle/Spiral)\n");
            rtbxHELP.AppendText("2. Enter number of cities in txtPorts\n");
            rtbxHELP.AppendText("3. Press DRAW to generate cities\n");
            rtbxHELP.AppendText("4. Adjust parameters (optional)\n");
            rtbxHELP.AppendText("5. Press GO to start algorithm\n");
            rtbxHELP.AppendText("6. Watch txtDistance decrease\n");
            rtbxHELP.AppendText("7. Press STOP or wait for auto-stop\n\n");

            // Control Parameters Section
            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 11, FontStyle.Bold);
            rtbxHELP.SelectionColor = System.Drawing.Color.DarkGreen;
            rtbxHELP.AppendText("CONTROL PARAMETERS:\n\n");

            // txtPorts
            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
            rtbxHELP.SelectionColor = System.Drawing.Color.DarkBlue;
            rtbxHELP.AppendText("txtPorts:\n");
            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 9, FontStyle.Italic);
            rtbxHELP.SelectionColor = System.Drawing.Color.Black;
            rtbxHELP.AppendText("• Number of cities to generate\n");
            rtbxHELP.AppendText("• Range: 1 to 2500\n");
            rtbxHELP.AppendText("• Default: 100\n");
            rtbxHELP.AppendText("• Used when DRAW is pressed\n\n");

            // TUNE Button
            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
            rtbxHELP.SelectionColor = System.Drawing.Color.DarkBlue;
            rtbxHELP.AppendText("TUNE Button:\n");
            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 9, FontStyle.Italic);
            rtbxHELP.SelectionColor = System.Drawing.Color.Black;
            rtbxHELP.AppendText("• Opens Tuning Parameters Dialog\n");
            rtbxHELP.AppendText("• Configure all algorithm parameters:\n");
            rtbxHELP.AppendText("  - Bounding Threshold (default: 200000)\n");
            rtbxHELP.AppendText("  - Minimum Threshold (default: 80000)\n");
            rtbxHELP.AppendText("  - Iteration Factor (default: 100)\n");
            rtbxHELP.AppendText("  - Normalization (default: 160000)\n");
            rtbxHELP.AppendText("• Settings persist between sessions\n");
            rtbxHELP.AppendText("• Use Default button to restore factory settings\n\n");

            // Display Checkbox
            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 11, FontStyle.Bold);
            rtbxHELP.SelectionColor = System.Drawing.Color.DarkGreen;
            rtbxHELP.AppendText("DISPLAY:\n");
            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 10, FontStyle.Regular);
            rtbxHELP.SelectionColor = System.Drawing.Color.Black;
            rtbxHELP.AppendText("• Uncheck to disable drawing for speed\n");
            rtbxHELP.AppendText("• Shows current best route when checked\n");
            rtbxHELP.AppendText("• Default: Checked\n\n");

            // Auto-Stop Feature
            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 11, FontStyle.Bold);
            rtbxHELP.SelectionColor = System.Drawing.Color.DarkGreen;
            rtbxHELP.AppendText("AUTO-STOP:\n");
            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 10, FontStyle.Regular);
            rtbxHELP.SelectionColor = System.Drawing.Color.Black;
            rtbxHELP.AppendText("• Algorithm stops if no improvement for N tours\n");
            rtbxHELP.AppendText("• Default: 10 tours\n");
            rtbxHELP.AppendText("• Set to 0 to disable\n\n");

            // Output Fields
            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 11, FontStyle.Bold);
            rtbxHELP.SelectionColor = System.Drawing.Color.DarkGreen;
            rtbxHELP.AppendText("OUTPUT:\n");
            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 10, FontStyle.Regular);
            rtbxHELP.SelectionColor = System.Drawing.Color.Black;
            rtbxHELP.AppendText("• txtDistance: Best route length found\n");
            rtbxHELP.AppendText("• nudTrials: Trial number to view (0 = best solution after completion, use up/down arrows to navigate)\n\n");

            // Algorithm Info
            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 11, FontStyle.Bold);
            rtbxHELP.SelectionColor = System.Drawing.Color.DarkGreen;
            rtbxHELP.AppendText("ALGORITHM:\n");
            rtbxHELP.SelectionFont = new System.Drawing.Font("Arial", 9, FontStyle.Italic);
            rtbxHELP.SelectionColor = System.Drawing.Color.Black;
            rtbxHELP.AppendText("• Adaptive neural network approach\n");
            rtbxHELP.AppendText("• Dynamic threshold selection\n");
            rtbxHELP.AppendText("• 2-opt local optimization\n");
            rtbxHELP.AppendText("• Edge cost learning\n");
            rtbxHELP.AppendText("• Original algorithm by H.S. Mazumdar (1993)\n");
        }

        //*******************************************************
        // Create default help file with formatted content
        private void CreateDefaultHelpFile(string filePath)
        {
            // Create default content in memory first
            CreateDefaultHelpContent();

            // Save to file
            try
            {
                rtbxHELP.SaveFile(filePath, RichTextBoxStreamType.RichText);
            }
            catch (Exception ex)
            {
                // DEBUG/LOG CODE - COMMENTED OUT
                //System.Diagnostics.Debug.WriteLine($"Failed to create default help file: {ex.Message}");
            }
        }

        //*******************************************************
        // Save help text to external file (appends/saves on exit)
        private void SaveHelpText()
        {
            // Only save if help text was successfully loaded
            if (!helpTextLoaded)
            {
                // DEBUG/LOG CODE - COMMENTED OUT
                //System.Diagnostics.Debug.WriteLine("Skipping save: Help text was never loaded");
                return;
            }

            // Only save if help text has actual content (not just formatting)
            // Check both RTF property and Text property to ensure there's real content
            string rtfContent = rtbxHELP.Rtf ?? string.Empty;
            string textContent = rtbxHELP.Text ?? string.Empty;

            // Skip saving if RTF is empty or if text content is empty/whitespace
            // This prevents saving blank files when RichTextBox is cleared or not loaded
            if (string.IsNullOrWhiteSpace(rtfContent) || string.IsNullOrWhiteSpace(textContent))
            {
                // DEBUG/LOG CODE - COMMENTED OUT
                //System.Diagnostics.Debug.WriteLine("Skipping save: Help text has no content");
                return;
            }

            // Additional check: ensure RTF contains actual text (not just formatting codes)
            // RTF with only formatting but no text would be very short (< 200 chars typically)
            if (rtfContent.Length < 200)
            {
                // DEBUG/LOG CODE - COMMENTED OUT
                //System.Diagnostics.Debug.WriteLine($"Skipping save: RTF content too short ({rtfContent.Length} chars) - likely empty");
                return;
            }

            string helpFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelpText.rtf");

            try
            {
                rtbxHELP.SaveFile(helpFilePath, RichTextBoxStreamType.RichText);
                // DEBUG/LOG CODE - COMMENTED OUT
                //System.Diagnostics.Debug.WriteLine($"Help text saved successfully to {helpFilePath} ({rtfContent.Length} chars)");
            }
            catch (Exception ex)
            {
                // DEBUG/LOG CODE - COMMENTED OUT
                //System.Diagnostics.Debug.WriteLine($"Failed to save help text: {ex.Message}");
            }
        }
        //*******************************************************
        private void btnTune_Click(object sender, EventArgs e)
        {
            // Create tune dialog and populate with current values from UI and engine
            using (var tuneForm = new TspTune())
            {
                // Load current values from main form UI and engine
                tuneForm.Cities = int.TryParse(txtPorts.Text, out int ports) ? ports : 100;
                tuneForm.CityDistribution = (string?)cboShape.SelectedItem ?? "Random";

                // Load algorithm parameters from engine (no longer on main form)
                tuneForm.BoundingThreshold = tsp.boundingthreshold;
                tuneForm.MinimumThreshold = tsp.minimumthreshold;
                tuneForm.IterationFactor = tsp.iterationfactor;
                tuneForm.Normalization = tsp.normalization;

                // Load visualization parameters from engine
                tuneForm.NodeSize = tsp.NodeRadiusPixels;
                tuneForm.MarginPixels = tsp.MarginPixels;

                // Load k-neighbors parameter from engine
                tuneForm.KNeighbors = tsp.kNeighbors;

                // Load full scan threshold parameter from engine
                tuneForm.FullScanThreshold = tsp.fullScanThreshold;

                // Show dialog
                if (tuneForm.ShowDialog(this) == DialogResult.OK)
                {
                    // User clicked OK - update main form visible controls
                    txtPorts.Text = tuneForm.Cities.ToString();
                    cboShape.SelectedItem = tuneForm.CityDistribution;

                    // Update engine algorithm parameters (removed from main form UI)
                    tsp.boundingthreshold = tuneForm.BoundingThreshold;
                    tsp.minimumthreshold = tuneForm.MinimumThreshold;
                    tsp.iterationfactor = tuneForm.IterationFactor;
                    tsp.normalization = tuneForm.Normalization;
                    tsp.kNeighbors = tuneForm.KNeighbors;  // Update k-neighbors
                    tsp.fullScanThreshold = tuneForm.FullScanThreshold;  // Update full scan threshold

                    // Update visualization parameters in engine and UI
                    tsp.NodeRadiusPixels = tuneForm.NodeSize;
                    tsp.MarginPixels = tuneForm.MarginPixels;
                    nudNodeSize.Value = tuneForm.NodeSize;
                    nudMargin.Value = tuneForm.MarginPixels;
                }
                // If user clicked Cancel, dialog is dismissed and no changes are made
            }
        }
        //*******************************************************
        // Node Size changed: update engine value and save to Tune.txt
        private void nudNodeSize_ValueChanged(object sender, EventArgs e)
        {
            tsp.NodeRadiusPixels = (int)nudNodeSize.Value;
            SaveVisualizationSettings();
        }
        //*******************************************************
        // Margin changed: update engine value and save to Tune.txt
        private void nudMargin_ValueChanged(object sender, EventArgs e)
        {
            tsp.MarginPixels = (int)nudMargin.Value;
            SaveVisualizationSettings();
        }
        //*******************************************************
        // Save visualization settings to Tune.txt (preserving other settings)
        private void SaveVisualizationSettings()
        {
            try
            {
                // Load existing settings
                var savedSettings = TspTune.LoadSettings() ?? new Dictionary<string, string>();

                // Update visualization parameters
                savedSettings["NodeSize"] = tsp.NodeRadiusPixels.ToString();
                savedSettings["Margin"] = tsp.MarginPixels.ToString();

                // Ensure other required parameters exist
                if (!savedSettings.ContainsKey("Cities")) savedSettings["Cities"] = txtPorts.Text;
                if (!savedSettings.ContainsKey("CityDistribution")) savedSettings["CityDistribution"] = cboShape.SelectedItem?.ToString() ?? "Random";
                if (!savedSettings.ContainsKey("BoundingThreshold")) savedSettings["BoundingThreshold"] = tsp.boundingthreshold.ToString();
                if (!savedSettings.ContainsKey("MinimumThreshold")) savedSettings["MinimumThreshold"] = tsp.minimumthreshold.ToString();
                if (!savedSettings.ContainsKey("IterationFactor")) savedSettings["IterationFactor"] = tsp.iterationfactor.ToString();
                if (!savedSettings.ContainsKey("Normalization")) savedSettings["Normalization"] = tsp.normalization.ToString();

                // Write all settings back to file
                string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tune.txt");
                using (StreamWriter writer = new StreamWriter(settingsFilePath))
                {
                    foreach (var kvp in savedSettings)
                    {
                        writer.WriteLine($"{kvp.Key}={kvp.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                // DEBUG/LOG CODE - COMMENTED OUT
                //System.Diagnostics.Debug.WriteLine($"Failed to save visualization settings: {ex.Message}");
            }
        }
        //*******************************************************
        // Helper method to load and display route for a given trial number
        private void LoadAndDisplayRoute(int trialNumber)
        {
            if (trialNumber < 0) return; // Allow trial 0 (best solution)

            // Load route solution from file
            var (route, distance, solutionTime) = tsp.LoadRouteSolution(trialNumber);

            if (route == null)
            {
                // File not found or invalid - don't update display
                return;
            }

            // Update lastroute to display the loaded route
            if (tsp.lastroute != null && route.Length == tsp.lastroute.Length)
            {
                for (int i = 0; i < route.Length; i++)
                {
                    tsp.lastroute[i] = route[i];
                }
            }

            // Update distance display with 2 decimal places
            //float dist = (float)distance / tsp.MarginPixels;
            //if (tsp.dtyp == 2)
            //    txtDistance.Text = dist.ToString("F2");
            //else
            txtDistance.Text = ((float)distance).ToString("F2");

            // Display solution time for this route
            txtTime.Text = solutionTime.ToString("F2");

            // Display the loaded route (mode 2 = last route)
            if (tsp.bmpWorld != null && dispFlg)
            {
                tsp.plot_rout(2); // Display last route (which we just loaded)
                pictureWorld.Image = tsp.bmpWorld;
                pictureWorld.Refresh();
            }
        }

        //*******************************************************
        // nudTrials ValueChanged: Load and display route for selected trial number
        private void nudTrials_ValueChanged(object? sender, EventArgs e)
        {
            // Skip if this is an automatic update from the display timer
            if (updatingTrialsFromTimer) return;

            // Get trial number from NumericUpDown value
            int trialNumber = (int)nudTrials.Value;

            // Allow trial 0 (best solution after completion) or any trial >= 1
            if (trialNumber < 0)
            {
                return;
            }

            // Load and display route using helper method
            LoadAndDisplayRoute(trialNumber);
        }

        //*******************************************************
        // Form closing: save help text if it was edited
        private void Tsp_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Dispose bitmap and graphics resources
            tsp.graphics?.Dispose();
            tsp.bmpWorld?.Dispose();
            SaveHelpText();
        }

        //*******************************************************
        private void lblDistance_MouseDown(object sender, MouseEventArgs e)
        {
            lblDistance.Text = "Gap%:";
            float dstNow = float.Parse(txtDistance.Text);
            distnow = txtDistance.Text;
            float dstMin = float.Parse(txtPorts.Text);
            float gap = 100 * (dstNow - dstMin) / dstMin;
            txtDistance.Text = gap.ToString("F2");
        }
        //*******************************************************
        private void lblDistance_MouseUp(object sender, MouseEventArgs e)
        {
            lblDistance.Text = "Distance:";
            txtDistance.Text= distnow;
        }
        //*******************************************************
    }
}
