namespace TSP_CS
{
    partial class TspTune
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            grpGeneral = new GroupBox();
            cboDistMatrix = new ComboBox();
            lblDistMatrix = new Label();
            lblCachePath = new Label();
            txtCachePath = new TextBox();
            lblParallelThreads = new Label();
            txtParallelThreads = new TextBox();
            lblMargin = new Label();
            nudMargin = new NumericUpDown();
            lblNodeSize = new Label();
            nudNodeSize = new NumericUpDown();
            lblShape = new Label();
            cboShape = new ComboBox();
            lblPorts = new Label();
            txtPorts = new TextBox();
            grpAlgorithm = new GroupBox();
            lblFullScanThreshold = new Label();
            txtFullScanThreshold = new TextBox();
            lblKNeighbors = new Label();
            txtKNeighbors = new TextBox();
            lblNormalization = new Label();
            txtNormalization = new TextBox();
            lblIterationFactor = new Label();
            txtIterationFactor = new TextBox();
            lblMinimumThreshold = new Label();
            txtMinimumThreshold = new TextBox();
            lblBoundingThreshold = new Label();
            txtBoundingThreshold = new TextBox();
            btnOK = new Button();
            btnCancel = new Button();
            btnDefault = new Button();
            grpGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMargin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudNodeSize).BeginInit();
            grpAlgorithm.SuspendLayout();
            SuspendLayout();
            // 
            // grpGeneral
            // 
            grpGeneral.Controls.Add(cboDistMatrix);
            grpGeneral.Controls.Add(lblDistMatrix);
            grpGeneral.Controls.Add(lblCachePath);
            grpGeneral.Controls.Add(txtCachePath);
            grpGeneral.Controls.Add(lblParallelThreads);
            grpGeneral.Controls.Add(txtParallelThreads);
            grpGeneral.Controls.Add(lblMargin);
            grpGeneral.Controls.Add(nudMargin);
            grpGeneral.Controls.Add(lblNodeSize);
            grpGeneral.Controls.Add(nudNodeSize);
            grpGeneral.Controls.Add(lblShape);
            grpGeneral.Controls.Add(cboShape);
            grpGeneral.Controls.Add(lblPorts);
            grpGeneral.Controls.Add(txtPorts);
            grpGeneral.Location = new Point(13, 12);
            grpGeneral.Name = "grpGeneral";
            grpGeneral.Size = new Size(271, 329);
            grpGeneral.TabIndex = 0;
            grpGeneral.TabStop = false;
            grpGeneral.Text = "General Settings";
            // 
            // cboDistMatrix
            // 
            cboDistMatrix.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDistMatrix.FormattingEnabled = true;
            cboDistMatrix.Location = new Point(6, 231);
            cboDistMatrix.Name = "cboDistMatrix";
            cboDistMatrix.Size = new Size(245, 28);
            cboDistMatrix.TabIndex = 13;
            cboDistMatrix.SelectedIndexChanged += cboDistMatrix_SelectedIndexChanged;
            // 
            // lblDistMatrix
            // 
            lblDistMatrix.AutoSize = true;
            lblDistMatrix.Location = new Point(6, 209);
            lblDistMatrix.Name = "lblDistMatrix";
            lblDistMatrix.Size = new Size(148, 20);
            lblDistMatrix.TabIndex = 14;
            lblDistMatrix.Text = "Distance Matrix Files:";
            // 
            // lblCachePath
            // 
            lblCachePath.AutoSize = true;
            lblCachePath.Location = new Point(6, 265);
            lblCachePath.Name = "lblCachePath";
            lblCachePath.Size = new Size(84, 20);
            lblCachePath.TabIndex = 13;
            lblCachePath.Text = "Cache Path:";
            // 
            // txtCachePath
            // 
            txtCachePath.Location = new Point(6, 287);
            txtCachePath.Name = "txtCachePath";
            txtCachePath.Size = new Size(245, 27);
            txtCachePath.TabIndex = 12;
            // 
            // lblParallelThreads
            // 
            lblParallelThreads.AutoSize = true;
            lblParallelThreads.Location = new Point(14, 179);
            lblParallelThreads.Name = "lblParallelThreads";
            lblParallelThreads.Size = new Size(116, 20);
            lblParallelThreads.TabIndex = 11;
            lblParallelThreads.Text = "Parallel Threads:";
            // 
            // txtParallelThreads
            // 
            txtParallelThreads.Location = new Point(140, 179);
            txtParallelThreads.Name = "txtParallelThreads";
            txtParallelThreads.ReadOnly = true;
            txtParallelThreads.Size = new Size(104, 27);
            txtParallelThreads.TabIndex = 10;
            txtParallelThreads.TabStop = false;
            // 
            // lblMargin
            // 
            lblMargin.AutoSize = true;
            lblMargin.Location = new Point(14, 140);
            lblMargin.Name = "lblMargin";
            lblMargin.Size = new Size(89, 20);
            lblMargin.TabIndex = 9;
            lblMargin.Text = "Margin (px):";
            // 
            // nudMargin
            // 
            nudMargin.Location = new Point(140, 133);
            nudMargin.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudMargin.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            nudMargin.Name = "nudMargin";
            nudMargin.Size = new Size(111, 27);
            nudMargin.TabIndex = 8;
            nudMargin.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // lblNodeSize
            // 
            lblNodeSize.AutoSize = true;
            lblNodeSize.Location = new Point(18, 104);
            lblNodeSize.Name = "lblNodeSize";
            lblNodeSize.Size = new Size(110, 20);
            lblNodeSize.TabIndex = 7;
            lblNodeSize.Text = "Node Size (px):";
            // 
            // nudNodeSize
            // 
            nudNodeSize.Location = new Point(140, 100);
            nudNodeSize.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            nudNodeSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudNodeSize.Name = "nudNodeSize";
            nudNodeSize.Size = new Size(111, 27);
            nudNodeSize.TabIndex = 6;
            nudNodeSize.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // lblShape
            // 
            lblShape.AutoSize = true;
            lblShape.Location = new Point(18, 65);
            lblShape.Name = "lblShape";
            lblShape.Size = new Size(119, 20);
            lblShape.TabIndex = 5;
            lblShape.Text = "City Distribution:";
            // 
            // cboShape
            // 
            cboShape.DropDownStyle = ComboBoxStyle.DropDownList;
            cboShape.FormattingEnabled = true;
            cboShape.Items.AddRange(new object[] { "Random", "Square", "Spiral", "Circle" });
            cboShape.Location = new Point(140, 63);
            cboShape.Name = "cboShape";
            cboShape.Size = new Size(113, 28);
            cboShape.TabIndex = 2;
            // 
            // lblPorts
            // 
            lblPorts.AutoSize = true;
            lblPorts.Location = new Point(18, 31);
            lblPorts.Name = "lblPorts";
            lblPorts.Size = new Size(94, 20);
            lblPorts.TabIndex = 1;
            lblPorts.Text = "Cities (Ports):";
            // 
            // txtPorts
            // 
            txtPorts.Location = new Point(140, 28);
            txtPorts.Name = "txtPorts";
            txtPorts.Size = new Size(113, 27);
            txtPorts.TabIndex = 0;
            // 
            // grpAlgorithm
            // 
            grpAlgorithm.Controls.Add(lblFullScanThreshold);
            grpAlgorithm.Controls.Add(txtFullScanThreshold);
            grpAlgorithm.Controls.Add(lblKNeighbors);
            grpAlgorithm.Controls.Add(txtKNeighbors);
            grpAlgorithm.Controls.Add(lblNormalization);
            grpAlgorithm.Controls.Add(txtNormalization);
            grpAlgorithm.Controls.Add(lblIterationFactor);
            grpAlgorithm.Controls.Add(txtIterationFactor);
            grpAlgorithm.Controls.Add(lblMinimumThreshold);
            grpAlgorithm.Controls.Add(txtMinimumThreshold);
            grpAlgorithm.Controls.Add(lblBoundingThreshold);
            grpAlgorithm.Controls.Add(txtBoundingThreshold);
            grpAlgorithm.Location = new Point(12, 347);
            grpAlgorithm.Name = "grpAlgorithm";
            grpAlgorithm.Size = new Size(271, 237);
            grpAlgorithm.TabIndex = 1;
            grpAlgorithm.TabStop = false;
            grpAlgorithm.Text = "Algorithm Parameters";
            // 
            // lblFullScanThreshold
            // 
            lblFullScanThreshold.AutoSize = true;
            lblFullScanThreshold.Location = new Point(18, 206);
            lblFullScanThreshold.Name = "lblFullScanThreshold";
            lblFullScanThreshold.Size = new Size(139, 20);
            lblFullScanThreshold.TabIndex = 11;
            lblFullScanThreshold.Text = "Full Scan Threshold:";
            // 
            // txtFullScanThreshold
            // 
            txtFullScanThreshold.Location = new Point(171, 202);
            txtFullScanThreshold.Name = "txtFullScanThreshold";
            txtFullScanThreshold.Size = new Size(87, 27);
            txtFullScanThreshold.TabIndex = 10;
            // 
            // lblKNeighbors
            // 
            lblKNeighbors.AutoSize = true;
            lblKNeighbors.Location = new Point(18, 174);
            lblKNeighbors.Name = "lblKNeighbors";
            lblKNeighbors.Size = new Size(96, 20);
            lblKNeighbors.TabIndex = 9;
            lblKNeighbors.Text = "K-Neighbors:";
            // 
            // txtKNeighbors
            // 
            txtKNeighbors.Location = new Point(171, 171);
            txtKNeighbors.Name = "txtKNeighbors";
            txtKNeighbors.Size = new Size(87, 27);
            txtKNeighbors.TabIndex = 8;
            // 
            // lblNormalization
            // 
            lblNormalization.AutoSize = true;
            lblNormalization.Location = new Point(18, 142);
            lblNormalization.Name = "lblNormalization";
            lblNormalization.Size = new Size(107, 20);
            lblNormalization.TabIndex = 7;
            lblNormalization.Text = "Normalization:";
            // 
            // txtNormalization
            // 
            txtNormalization.Location = new Point(171, 139);
            txtNormalization.Name = "txtNormalization";
            txtNormalization.Size = new Size(87, 27);
            txtNormalization.TabIndex = 6;
            // 
            // lblIterationFactor
            // 
            lblIterationFactor.AutoSize = true;
            lblIterationFactor.Location = new Point(18, 108);
            lblIterationFactor.Name = "lblIterationFactor";
            lblIterationFactor.Size = new Size(112, 20);
            lblIterationFactor.TabIndex = 5;
            lblIterationFactor.Text = "Iteration Factor:";
            // 
            // txtIterationFactor
            // 
            txtIterationFactor.Location = new Point(171, 104);
            txtIterationFactor.Name = "txtIterationFactor";
            txtIterationFactor.Size = new Size(87, 27);
            txtIterationFactor.TabIndex = 5;
            // 
            // lblMinimumThreshold
            // 
            lblMinimumThreshold.AutoSize = true;
            lblMinimumThreshold.Location = new Point(18, 72);
            lblMinimumThreshold.Name = "lblMinimumThreshold";
            lblMinimumThreshold.Size = new Size(144, 20);
            lblMinimumThreshold.TabIndex = 3;
            lblMinimumThreshold.Text = "Minimum Threshold:";
            // 
            // txtMinimumThreshold
            // 
            txtMinimumThreshold.Location = new Point(171, 71);
            txtMinimumThreshold.Name = "txtMinimumThreshold";
            txtMinimumThreshold.Size = new Size(87, 27);
            txtMinimumThreshold.TabIndex = 4;
            // 
            // lblBoundingThreshold
            // 
            lblBoundingThreshold.AutoSize = true;
            lblBoundingThreshold.Location = new Point(18, 39);
            lblBoundingThreshold.Name = "lblBoundingThreshold";
            lblBoundingThreshold.Size = new Size(145, 20);
            lblBoundingThreshold.TabIndex = 1;
            lblBoundingThreshold.Text = "Bounding Threshold:";
            // 
            // txtBoundingThreshold
            // 
            txtBoundingThreshold.Location = new Point(171, 36);
            txtBoundingThreshold.Name = "txtBoundingThreshold";
            txtBoundingThreshold.Size = new Size(87, 27);
            txtBoundingThreshold.TabIndex = 3;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnOK.Location = new Point(140, 590);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(67, 28);
            btnOK.TabIndex = 7;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(217, 590);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(67, 28);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnDefault
            // 
            btnDefault.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDefault.Location = new Point(15, 590);
            btnDefault.Name = "btnDefault";
            btnDefault.Size = new Size(67, 28);
            btnDefault.TabIndex = 9;
            btnDefault.Text = "Default";
            btnDefault.UseVisualStyleBackColor = true;
            btnDefault.Click += btnDefault_Click;
            // 
            // TspTune
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(298, 628);
            Controls.Add(btnDefault);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(grpAlgorithm);
            Controls.Add(grpGeneral);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TspTune";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "TSP Tuning Parameters";
            Load += TspTune_Load;
            grpGeneral.ResumeLayout(false);
            grpGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudMargin).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudNodeSize).EndInit();
            grpAlgorithm.ResumeLayout(false);
            grpAlgorithm.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpGeneral;
        private GroupBox grpAlgorithm;
        private TextBox txtPorts;
        private Label lblPorts;
        private ComboBox cboShape;
        private Label lblShape;
        private NumericUpDown nudNodeSize;
        private Label lblNodeSize;
        private NumericUpDown nudMargin;
        private Label lblMargin;
        private Label lblParallelThreads;
        private TextBox txtParallelThreads;
        private TextBox txtBoundingThreshold;
        private Label lblBoundingThreshold;
        private TextBox txtMinimumThreshold;
        private Label lblMinimumThreshold;
        private TextBox txtIterationFactor;
        private Label lblIterationFactor;
        private TextBox txtNormalization;
        private Label lblNormalization;
        private TextBox txtKNeighbors;
        private Label lblKNeighbors;
        private TextBox txtFullScanThreshold;
        private Label lblFullScanThreshold;
        private ComboBox cboDistMatrix;
        private Label lblDistMatrix;
        private Label lblCachePath;
        private TextBox txtCachePath;
        private Button btnOK;
        private Button btnCancel;
        private Button btnDefault;
    }
}

