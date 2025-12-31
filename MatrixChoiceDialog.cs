using System;
using System.Windows.Forms;

namespace TSP_CS
{
    /// <summary>
    /// Dialog box allowing user to choose between using a cached distance matrix
    /// or recalculating a new one when city coordinates match an existing cache.
    /// 
    /// This dialog appears when Pattern 2 (square grid) is selected and a matching
    /// cache file exists with the same N-K-Pattern combination but potentially
    /// different city ordering (randomization option).
    /// </summary>
    public partial class MatrixChoiceDialog : Form
    {
        /// <summary>
        /// Returns true if user chose to use the existing cached matrix,
        /// false if user chose to recalculate a new matrix.
        /// </summary>
        public bool UseOldMatrix { get; private set; } = false;

        /// <summary>
        /// Initialize the matrix choice dialog.
        /// </summary>
        public MatrixChoiceDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// User clicked "Use Old Matrix" button - reuse existing cache.
        /// </summary>
        private void btnOldMatrix_Click(object sender, EventArgs e)
        {
            UseOldMatrix = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// User clicked "Calculate New Matrix" button - recalculate distance matrix.
        /// </summary>
        private void btnNewMatrix_Click(object sender, EventArgs e)
        {
            UseOldMatrix = false;
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// User clicked "Cancel" button - abort operation.
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Label click handler (no-op, required by designer).
        /// </summary>
        private void lblMessage_Click(object sender, EventArgs e)
        {
            // No action required
        }
    }
}

