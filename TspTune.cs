namespace TSP_CS
{
    /// <summary>
    /// Tuning Parameters Dialog
    /// Allows user to configure all TSP algorithm parameters in a dedicated form
    /// Persists settings to Tune.txt between sessions
    /// </summary>
    public partial class TspTune : Form
    {
        // Default values (install-time recommended settings) - optimized for 1000 cities
        private const int DEFAULT_CITIES = 1000;
        private const string DEFAULT_DISTRIBUTION = "Square";
        private const int DEFAULT_BOUNDING_THRESHOLD = 200000;
        private const int DEFAULT_MINIMUM_THRESHOLD = 80000;
        private const int DEFAULT_ITERATION_FACTOR = 100;
        private const int DEFAULT_NORMALIZATION = 160000;  // Matches ASPX scale for better precision
        private const int DEFAULT_NODE_SIZE = 4;  // Match ASPX default
        private const int DEFAULT_MARGIN = 20;    // Match ASPX default
        private const int DEFAULT_K_NEIGHBORS = 500;  // k-nearest neighbors for O(N×k) optimization
        private const int DEFAULT_FULL_SCAN_THRESHOLD = 200;  // Use full scan if city count < this value (matches distance matrix threshold for consistency)

        // Settings file path (in executable directory for backward compatibility)
        private static string SettingsFilePath => Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Tune.txt");
        
        // External TUNE file path (outside codebase, in parent directory)
        private static string ExternalTuneFilePath
        {
            get
            {
                // Get project root directory
                string projectRoot = GetProjectRootDirectory();
                // Go to parent directory (outside codebase)
                string parentDir = Directory.GetParent(projectRoot)?.FullName ?? projectRoot;
                return Path.Combine(parentDir, "TUNE");
            }
        }
        
        /// <summary>
        /// Get project root directory (parent of bin/obj folders)
        /// Falls back to executable directory if project structure not found
        /// </summary>
        private static string GetProjectRootDirectory()
        {
            // Start from executable directory
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Navigate up from bin/Debug/net8.0-windows/ or bin/Release/net8.0-windows/
            // to find project root (where .csproj file is)
            DirectoryInfo dir = new DirectoryInfo(currentDir);
            
            // Look for project root by checking for .csproj file or .sln file
            while (dir != null && dir.Parent != null)
            {
                // Check if this directory contains .csproj or .sln (project root indicators)
                if (dir.GetFiles("*.csproj").Length > 0 || dir.GetFiles("*.sln").Length > 0)
                {
                    return dir.FullName;
                }
                
                // Also check if we've gone too far up (reached drive root)
                if (dir.Parent.FullName == dir.FullName)
                    break;
                    
                dir = dir.Parent;
            }
            
            // Fallback: Use executable directory if project root not found
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        // Public properties to get/set values
        public int Cities { get; set; }
        public string CityDistribution { get; set; }
        public int BoundingThreshold { get; set; }
        public int MinimumThreshold { get; set; }
        public int IterationFactor { get; set; }
        public int Normalization { get; set; }
        public int NodeSize { get; set; }
        public int MarginPixels { get; set; }
        public int KNeighbors { get; set; }  // k-nearest neighbors parameter
        public int FullScanThreshold { get; set; } = DEFAULT_FULL_SCAN_THRESHOLD;  // City count threshold for full scan vs k-NN scan
        public string CachePath { get; set; } = "";  // Cache folder path (outside codebase)
        public string SelectedDistanceMatrix { get; private set; } = "";  // Selected distance matrix file name

        public TspTune()
        {
            InitializeComponent();
            CityDistribution = DEFAULT_DISTRIBUTION;
            FullScanThreshold = DEFAULT_FULL_SCAN_THRESHOLD;  // Initialize with default value
        }

        /// <summary>
        /// Load form with current values from main form
        /// </summary>
        private void TspTune_Load(object sender, EventArgs e)
        {
            // Load cache path from external TUNE file if not already set
            if (string.IsNullOrEmpty(CachePath))
            {
                CachePath = LoadCachePathFromExternalTune();
            }
            
            // Populate controls with current values
            txtPorts.Text = Cities.ToString();
            cboShape.SelectedItem = CityDistribution;
            txtBoundingThreshold.Text = BoundingThreshold.ToString();
            txtMinimumThreshold.Text = MinimumThreshold.ToString();
            txtIterationFactor.Text = IterationFactor.ToString();
            txtNormalization.Text = Normalization.ToString();
            // Clamp NodeSize to valid range (1-20) before setting
            nudNodeSize.Value = Math.Max(nudNodeSize.Minimum, Math.Min(nudNodeSize.Maximum, NodeSize));
            // Clamp MarginPixels to valid range (5-50) before setting
            nudMargin.Value = Math.Max(nudMargin.Minimum, Math.Min(nudMargin.Maximum, MarginPixels));
            txtKNeighbors.Text = KNeighbors.ToString();
            txtFullScanThreshold.Text = FullScanThreshold.ToString();
            txtCachePath.Text = CachePath;
            
            // Display number of parallel threads available (read-only)
            // Matches TspMain calculation: cores - 1, minimum 1
            int cores = System.Environment.ProcessorCount;
            int parallelThreads = System.Math.Max(1, cores - 1);
            txtParallelThreads.Text = parallelThreads.ToString();
            
            // Populate distance matrix combo box from cache directory
            PopulateDistanceMatrixCombo();
        }
        
        /// <summary>
        /// Populate combo box with distance matrix files from D:\_December2025\cache
        /// </summary>
        private void PopulateDistanceMatrixCombo()
        {
            cboDistMatrix.Items.Clear();
            
            string cacheDir = @"D:\_December2025\cache";
            
            try
            {
                if (!Directory.Exists(cacheDir))
                {
                    cboDistMatrix.Items.Add("(Cache directory not found)");
                    cboDistMatrix.Enabled = false;
                    return;
                }
                
                // Get all .gz files matching the pattern N*_K*_P*.gz
                // Order by file size (smallest first)
                var files = Directory.GetFiles(cacheDir, "N*_K*_P*.gz")
                    .Select(f => new FileInfo(f))
                    .OrderBy(fi => fi.Length)
                    .Select(fi => Path.GetFileName(fi.FullName))
                    .ToList();
                
                if (files.Count == 0)
                {
                    cboDistMatrix.Items.Add("(No distance matrix files found)");
                    cboDistMatrix.Enabled = false;
                    return;
                }
                
                // Update label to show count
                lblDistMatrix.Text = $"Distance Matrix Files ({files.Count}):";
                
                // Add files to combo box with readable format
                // Store filename in custom class for parsing
                foreach (var file in files)
                {
                    // Parse filename: N{n}_K{k}_P{pattern}.gz
                    // Display as: N={n}, K={k}, P={pattern}
                    string displayText = file;
                    if (file.StartsWith("N") && file.EndsWith(".gz"))
                    {
                        string nameWithoutExt = file.Substring(0, file.Length - 3); // Remove .gz
                        displayText = nameWithoutExt.Replace("_", ", ");
                    }
                    // Store original filename in a custom class
                    cboDistMatrix.Items.Add(new DistanceMatrixFileItem(displayText, file));
                }
                
                cboDistMatrix.Enabled = true;
            }
            catch (Exception ex)
            {
                cboDistMatrix.Items.Add($"(Error: {ex.Message})");
                cboDistMatrix.Enabled = false;
            }
        }
        
        /// <summary>
        /// Distance Matrix combo box selection changed - update Cities and K-Neighbors
        /// </summary>
        private void cboDistMatrix_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboDistMatrix.SelectedItem == null || !cboDistMatrix.Enabled)
                return;
            
            try
            {
                // Get the selected item
                if (cboDistMatrix.SelectedItem is DistanceMatrixFileItem item)
                {
                    // Parse filename: N{n}_K{k}_P{pattern}.gz
                    string fileName = item.FileName;
                    if (fileName.StartsWith("N") && fileName.EndsWith(".gz"))
                    {
                        string nameWithoutExt = fileName.Substring(0, fileName.Length - 3); // Remove .gz
                        string[] parts = nameWithoutExt.Split('_');
                        
                        if (parts.Length >= 3)
                        {
                            // Extract N value (cities)
                            if (parts[0].StartsWith("N") && int.TryParse(parts[0].Substring(1), out int nValue))
                            {
                                txtPorts.Text = nValue.ToString();
                            }
                            
                            // Extract K value (neighbors)
                            if (parts[1].StartsWith("K") && int.TryParse(parts[1].Substring(1), out int kValue))
                            {
                                txtKNeighbors.Text = kValue.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail - don't interrupt user workflow
            }
        }
        
        /// <summary>
        /// Helper class to store distance matrix file info for combo box
        /// </summary>
        private class DistanceMatrixFileItem
        {
            public string Display { get; }
            public string FileName { get; }
            
            public DistanceMatrixFileItem(string display, string fileName)
            {
                Display = display;
                FileName = fileName;
            }
            
            public override string ToString()
            {
                return Display;
            }
        }

        /// <summary>
        /// OK button - validate and save values
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate and parse values
                Cities = int.Parse(txtPorts.Text);
                CityDistribution = cboShape.SelectedItem?.ToString() ?? "RANDOM";
                BoundingThreshold = int.Parse(txtBoundingThreshold.Text);
                MinimumThreshold = int.Parse(txtMinimumThreshold.Text);
                IterationFactor = int.Parse(txtIterationFactor.Text);
                Normalization = int.Parse(txtNormalization.Text);
                NodeSize = (int)nudNodeSize.Value;
                MarginPixels = (int)nudMargin.Value;
                KNeighbors = int.Parse(txtKNeighbors.Text);
                FullScanThreshold = int.Parse(txtFullScanThreshold.Text);
                CachePath = txtCachePath.Text.Trim();
                
                // Get selected distance matrix file
                if (cboDistMatrix.SelectedItem != null && cboDistMatrix.Enabled)
                {
                    if (cboDistMatrix.SelectedItem is DistanceMatrixFileItem item)
                    {
                        SelectedDistanceMatrix = item.FileName;
                    }
                    else
                    {
                        SelectedDistanceMatrix = cboDistMatrix.SelectedItem.ToString() ?? "";
                    }
                }
                else
                {
                    SelectedDistanceMatrix = "";
                }

                // Validate cache path
                if (!string.IsNullOrEmpty(CachePath))
                {
                    try
                    {
                        // Validate path format
                        if (!Path.IsPathRooted(CachePath))
                        {
                            MessageBox.Show("Cache path must be an absolute path (e.g., D:\\Cache or C:\\MyCache)", 
                                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Invalid cache path format", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Validate k-neighbors
                if (KNeighbors < 1 || KNeighbors > 10000)
                {
                    MessageBox.Show("K-Neighbors must be between 1 and 10,000", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Validate full scan threshold
                if (FullScanThreshold < 1 || FullScanThreshold > 100000)
                {
                    MessageBox.Show("Full Scan Threshold must be between 1 and 100,000", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Basic validation
                if (Cities < 1 || Cities > 1000000)
                {
                    MessageBox.Show("Cities must be between 1 and 1000,000", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Save settings to Tune.txt
                SaveSettings();

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter valid numeric values for all parameters.",
                    "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Cancel button - discard changes
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Default button - reset to install-time recommended values
        /// </summary>
        private void btnDefault_Click(object sender, EventArgs e)
        {
            txtPorts.Text = DEFAULT_CITIES.ToString();
            cboShape.SelectedItem = DEFAULT_DISTRIBUTION;
            txtBoundingThreshold.Text = DEFAULT_BOUNDING_THRESHOLD.ToString();
            txtMinimumThreshold.Text = DEFAULT_MINIMUM_THRESHOLD.ToString();
            txtIterationFactor.Text = DEFAULT_ITERATION_FACTOR.ToString();
            txtNormalization.Text = DEFAULT_NORMALIZATION.ToString();
            nudNodeSize.Value = DEFAULT_NODE_SIZE;
            nudMargin.Value = DEFAULT_MARGIN;
            txtKNeighbors.Text = DEFAULT_K_NEIGHBORS.ToString();
            txtFullScanThreshold.Text = DEFAULT_FULL_SCAN_THRESHOLD.ToString();
            txtCachePath.Text = "";  // Default: empty (use project root/cache)
        }

        /// <summary>
        /// Save current settings to Tune.txt
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(SettingsFilePath))
                {
                    writer.WriteLine($"Cities={Cities}");
                    writer.WriteLine($"CityDistribution={CityDistribution}");
                    writer.WriteLine($"BoundingThreshold={BoundingThreshold}");
                    writer.WriteLine($"MinimumThreshold={MinimumThreshold}");
                    writer.WriteLine($"IterationFactor={IterationFactor}");
                    writer.WriteLine($"Normalization={Normalization}");
                    writer.WriteLine($"NodeSize={NodeSize}");
                    writer.WriteLine($"Margin={MarginPixels}");
                    writer.WriteLine($"KNeighbors={KNeighbors}");
                    writer.WriteLine($"FullScanThreshold={FullScanThreshold}");
                }
                
                // Also save CachePath to external TUNE file (outside codebase)
                SaveCachePathToExternalTune();
            }
            catch (Exception)
            {
                // Silently fail - don't interrupt user workflow if settings file cannot be written
            }
        }
        
        /// <summary>
        /// Save cache path to external TUNE file (outside codebase)
        /// </summary>
        private void SaveCachePathToExternalTune()
        {
            try
            {
                string tunePath = ExternalTuneFilePath;
                string tuneDir = Path.GetDirectoryName(tunePath);
                if (!string.IsNullOrEmpty(tuneDir) && !Directory.Exists(tuneDir))
                {
                    Directory.CreateDirectory(tuneDir);
                }
                
                using (StreamWriter writer = new StreamWriter(tunePath))
                {
                    writer.WriteLine($"CachePath={CachePath}");
                }
            }
            catch (Exception)
            {
                // Silently fail - don't interrupt user workflow
            }
        }
        
        /// <summary>
        /// Load cache path from external TUNE file (outside codebase)
        /// Returns empty string if file doesn't exist or path not found
        /// </summary>
        public static string LoadCachePathFromExternalTune()
        {
            try
            {
                string tunePath = ExternalTuneFilePath;
                if (!File.Exists(tunePath))
                    return "";

                foreach (string line in File.ReadAllLines(tunePath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split('=', 2);
                    if (parts.Length == 2 && parts[0].Trim() == "CachePath")
                    {
                        return parts[1].Trim();
                    }
                }
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// Load settings from Tune.txt (static method for main form to call on startup)
        /// Returns dictionary with settings, or null if file doesn't exist/is invalid
        /// </summary>
        public static Dictionary<string, string>? LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                    return null;

                var settings = new Dictionary<string, string>();
                foreach (string line in File.ReadAllLines(SettingsFilePath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        settings[parts[0].Trim()] = parts[1].Trim();
                    }
                }
                return settings.Count > 0 ? settings : null;
            }
            catch (Exception)
            {
                // Silently fail - return null if settings file cannot be read
                return null;
            }
        }

        /// <summary>
        /// Get default value for a specific setting
        /// </summary>
        public static string GetDefault(string settingName)
        {
            return settingName switch
            {
                "Cities" => DEFAULT_CITIES.ToString(),
                "CityDistribution" => DEFAULT_DISTRIBUTION,
                "BoundingThreshold" => DEFAULT_BOUNDING_THRESHOLD.ToString(),
                "MinimumThreshold" => DEFAULT_MINIMUM_THRESHOLD.ToString(),
                "IterationFactor" => DEFAULT_ITERATION_FACTOR.ToString(),
                "Normalization" => DEFAULT_NORMALIZATION.ToString(),
                "NodeSize" => DEFAULT_NODE_SIZE.ToString(),
                "Margin" => DEFAULT_MARGIN.ToString(),
                "KNeighbors" => DEFAULT_K_NEIGHBORS.ToString(),
                "FullScanThreshold" => DEFAULT_FULL_SCAN_THRESHOLD.ToString(),
                "CachePath" => "",
                _ => ""
            };
        }
        //*******************************************************
    }
}
