namespace TSP_CS
{
    partial class Tsp
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlWorld = new Panel();
            rtbxHELP = new RichTextBox();
            pictureWorld = new PictureBox();
            btnOk = new Button();
            btnToggleRun = new Button();
            btnDrawShape = new Button();
            cboShape = new ComboBox();
            lblShape = new Label();
            txtPorts = new TextBox();
            lblPorts = new Label();
            nudNodeSize = new NumericUpDown();
            lblNodeSize = new Label();
            nudMargin = new NumericUpDown();
            lblMargin = new Label();
            chkDisplay = new CheckBox();
            nudTrials = new NumericUpDown();
            lblTrials = new Label();
            txtDistance = new TextBox();
            lblDistance = new Label();
            txtTime = new TextBox();
            lblTime = new Label();
            btnHELP = new Button();
            btnTest = new Button();
            btnTune = new Button();
            pnlWorld.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureWorld).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudNodeSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudMargin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTrials).BeginInit();
            SuspendLayout();
            // 
            // pnlWorld
            // 
            pnlWorld.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlWorld.AutoScroll = true;
            pnlWorld.BackColor = SystemColors.ControlDarkDark;
            pnlWorld.BorderStyle = BorderStyle.FixedSingle;
            pnlWorld.Controls.Add(rtbxHELP);
            pnlWorld.Controls.Add(pictureWorld);
            pnlWorld.Location = new Point(11, 7);
            pnlWorld.Margin = new Padding(3, 2, 3, 2);
            pnlWorld.Name = "pnlWorld";
            pnlWorld.Size = new Size(533, 458);
            pnlWorld.TabIndex = 0;
            // 
            // rtbxHELP
            // 
            rtbxHELP.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbxHELP.BackColor = Color.LightYellow;
            rtbxHELP.Font = new Font("Segoe UI", 9F);
            rtbxHELP.Location = new Point(0, 0);
            rtbxHELP.Margin = new Padding(3, 2, 3, 2);
            rtbxHELP.Name = "rtbxHELP";
            rtbxHELP.Size = new Size(528, 456);
            rtbxHELP.TabIndex = 18;
            rtbxHELP.Text = "";
            rtbxHELP.Visible = false;
            // 
            // pictureWorld
            // 
            pictureWorld.BackColor = Color.Black;
            pictureWorld.BorderStyle = BorderStyle.FixedSingle;
            pictureWorld.Location = new Point(0, 0);
            pictureWorld.Margin = new Padding(3, 2, 3, 2);
            pictureWorld.Name = "pictureWorld";
            pictureWorld.Size = new Size(524, 450);
            pictureWorld.TabIndex = 0;
            pictureWorld.TabStop = false;
            // 
            // btnOk
            // 
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOk.Location = new Point(546, 444);
            btnOk.Margin = new Padding(3, 2, 3, 2);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(66, 21);
            btnOk.TabIndex = 1;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // btnToggleRun
            // 
            btnToggleRun.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnToggleRun.Location = new Point(549, 309);
            btnToggleRun.Margin = new Padding(3, 2, 3, 2);
            btnToggleRun.Name = "btnToggleRun";
            btnToggleRun.Size = new Size(66, 21);
            btnToggleRun.TabIndex = 2;
            btnToggleRun.Text = "TOUR";
            btnToggleRun.UseVisualStyleBackColor = true;
            btnToggleRun.Click += btnToggleRun_Click;
            // 
            // btnDrawShape
            // 
            btnDrawShape.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDrawShape.Location = new Point(549, 287);
            btnDrawShape.Margin = new Padding(3, 2, 3, 2);
            btnDrawShape.Name = "btnDrawShape";
            btnDrawShape.Size = new Size(66, 21);
            btnDrawShape.TabIndex = 4;
            btnDrawShape.Text = "DRAW";
            btnDrawShape.UseVisualStyleBackColor = true;
            btnDrawShape.Click += btnDRAW_Click;
            // 
            // cboShape
            // 
            cboShape.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cboShape.FormattingEnabled = true;
            cboShape.Items.AddRange(new object[] { "RANDOM", "SQUARE", "SPIRAL", "CIRCLE" });
            cboShape.Location = new Point(549, 264);
            cboShape.Margin = new Padding(3, 2, 3, 2);
            cboShape.Name = "cboShape";
            cboShape.Size = new Size(66, 23);
            cboShape.TabIndex = 5;
            // 
            // lblShape
            // 
            lblShape.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblShape.AutoSize = true;
            lblShape.Location = new Point(549, 246);
            lblShape.Name = "lblShape";
            lblShape.Size = new Size(42, 15);
            lblShape.TabIndex = 20;
            lblShape.Text = "Shape:";
            // 
            // txtPorts
            // 
            txtPorts.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtPorts.BorderStyle = BorderStyle.FixedSingle;
            txtPorts.Location = new Point(549, 222);
            txtPorts.Margin = new Padding(3, 2, 3, 2);
            txtPorts.Name = "txtPorts";
            txtPorts.Size = new Size(65, 23);
            txtPorts.TabIndex = 6;
            // 
            // lblPorts
            // 
            lblPorts.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblPorts.AutoSize = true;
            lblPorts.Location = new Point(549, 205);
            lblPorts.Name = "lblPorts";
            lblPorts.Size = new Size(39, 15);
            lblPorts.TabIndex = 21;
            lblPorts.Text = "Cities:";
            // 
            // nudNodeSize
            // 
            nudNodeSize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudNodeSize.Location = new Point(549, 184);
            nudNodeSize.Margin = new Padding(3, 2, 3, 2);
            nudNodeSize.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            nudNodeSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudNodeSize.Name = "nudNodeSize";
            nudNodeSize.Size = new Size(64, 23);
            nudNodeSize.TabIndex = 24;
            nudNodeSize.Value = new decimal(new int[] { 3, 0, 0, 0 });
            nudNodeSize.ValueChanged += nudNodeSize_ValueChanged;
            // 
            // lblNodeSize
            // 
            lblNodeSize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblNodeSize.AutoSize = true;
            lblNodeSize.Location = new Point(549, 169);
            lblNodeSize.Name = "lblNodeSize";
            lblNodeSize.Size = new Size(62, 15);
            lblNodeSize.TabIndex = 25;
            lblNodeSize.Text = "Node Size:";
            // 
            // nudMargin
            // 
            nudMargin.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudMargin.Location = new Point(549, 144);
            nudMargin.Margin = new Padding(3, 2, 3, 2);
            nudMargin.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudMargin.Minimum = new decimal(new int[] { 3, 0, 0, 0 });
            nudMargin.Name = "nudMargin";
            nudMargin.Size = new Size(64, 23);
            nudMargin.TabIndex = 26;
            nudMargin.Value = new decimal(new int[] { 10, 0, 0, 0 });
            nudMargin.ValueChanged += nudMargin_ValueChanged;
            // 
            // lblMargin
            // 
            lblMargin.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblMargin.AutoSize = true;
            lblMargin.Location = new Point(549, 126);
            lblMargin.Name = "lblMargin";
            lblMargin.Size = new Size(52, 15);
            lblMargin.TabIndex = 27;
            lblMargin.Text = "Spacing:";
            // 
            // chkDisplay
            // 
            chkDisplay.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            chkDisplay.AutoSize = true;
            chkDisplay.Checked = true;
            chkDisplay.CheckState = CheckState.Checked;
            chkDisplay.Location = new Point(555, 370);
            chkDisplay.Margin = new Padding(3, 2, 3, 2);
            chkDisplay.Name = "chkDisplay";
            chkDisplay.Size = new Size(64, 19);
            chkDisplay.TabIndex = 15;
            chkDisplay.Text = "Display";
            chkDisplay.UseVisualStyleBackColor = true;
            chkDisplay.CheckedChanged += chkDisplay_CheckedChanged;
            // 
            // nudTrials
            // 
            nudTrials.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudTrials.Location = new Point(549, 63);
            nudTrials.Margin = new Padding(3, 2, 3, 2);
            nudTrials.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            nudTrials.Name = "nudTrials";
            nudTrials.Size = new Size(66, 23);
            nudTrials.TabIndex = 16;
            nudTrials.ValueChanged += nudTrials_ValueChanged;
            // 
            // lblTrials
            // 
            lblTrials.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTrials.AutoSize = true;
            lblTrials.Location = new Point(549, 46);
            lblTrials.Name = "lblTrials";
            lblTrials.Size = new Size(31, 15);
            lblTrials.TabIndex = 22;
            lblTrials.Text = "Trial:";
            // 
            // txtDistance
            // 
            txtDistance.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtDistance.BorderStyle = BorderStyle.FixedSingle;
            txtDistance.Location = new Point(549, 22);
            txtDistance.Margin = new Padding(3, 2, 3, 2);
            txtDistance.Name = "txtDistance";
            txtDistance.ReadOnly = true;
            txtDistance.Size = new Size(70, 23);
            txtDistance.TabIndex = 11;
            // 
            // lblDistance
            // 
            lblDistance.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblDistance.AutoSize = true;
            lblDistance.Cursor = Cursors.Hand;
            lblDistance.Location = new Point(549, 7);
            lblDistance.Name = "lblDistance";
            lblDistance.Size = new Size(55, 15);
            lblDistance.TabIndex = 23;
            lblDistance.Text = "Distance:";
            lblDistance.MouseDown += lblDistance_MouseDown;
            lblDistance.MouseUp += lblDistance_MouseUp;
            // 
            // txtTime
            // 
            txtTime.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtTime.BorderStyle = BorderStyle.FixedSingle;
            txtTime.Location = new Point(549, 102);
            txtTime.Margin = new Padding(3, 2, 3, 2);
            txtTime.Name = "txtTime";
            txtTime.ReadOnly = true;
            txtTime.Size = new Size(65, 23);
            txtTime.TabIndex = 28;
            // 
            // lblTime
            // 
            lblTime.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTime.AutoSize = true;
            lblTime.Location = new Point(549, 84);
            lblTime.Name = "lblTime";
            lblTime.Size = new Size(36, 15);
            lblTime.TabIndex = 29;
            lblTime.Text = "Time:";
            // 
            // btnHELP
            // 
            btnHELP.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnHELP.Location = new Point(546, 418);
            btnHELP.Margin = new Padding(3, 2, 3, 2);
            btnHELP.Name = "btnHELP";
            btnHELP.Size = new Size(66, 21);
            btnHELP.TabIndex = 17;
            btnHELP.Text = "HELP";
            btnHELP.UseVisualStyleBackColor = true;
            btnHELP.Click += btnHELP_Click;
            // 
            // btnTest
            // 
            btnTest.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnTest.Location = new Point(545, 393);
            btnTest.Margin = new Padding(3, 2, 3, 2);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(66, 21);
            btnTest.TabIndex = 18;
            btnTest.Text = "TEST";
            btnTest.UseVisualStyleBackColor = true;
            // 
            // btnTune
            // 
            btnTune.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTune.Location = new Point(549, 332);
            btnTune.Margin = new Padding(3, 2, 3, 2);
            btnTune.Name = "btnTune";
            btnTune.Size = new Size(66, 21);
            btnTune.TabIndex = 19;
            btnTune.Text = "TUNE";
            btnTune.UseVisualStyleBackColor = true;
            btnTune.Click += btnTune_Click;
            // 
            // Tsp
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(622, 466);
            Controls.Add(btnTune);
            Controls.Add(btnTest);
            Controls.Add(btnHELP);
            Controls.Add(txtTime);
            Controls.Add(txtDistance);
            Controls.Add(nudTrials);
            Controls.Add(chkDisplay);
            Controls.Add(nudMargin);
            Controls.Add(nudNodeSize);
            Controls.Add(txtPorts);
            Controls.Add(cboShape);
            Controls.Add(btnDrawShape);
            Controls.Add(btnToggleRun);
            Controls.Add(btnOk);
            Controls.Add(pnlWorld);
            Controls.Add(lblDistance);
            Controls.Add(lblTrials);
            Controls.Add(lblTime);
            Controls.Add(lblMargin);
            Controls.Add(lblNodeSize);
            Controls.Add(lblPorts);
            Controls.Add(lblShape);
            Name = "Tsp";
            Text = "Traveling Salesman's Problem - Mazumdar";
            pnlWorld.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureWorld).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudNodeSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudMargin).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTrials).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlWorld;
        private PictureBox pictureWorld;
        private Button btnOk;
        private Button btnToggleRun;
        private Button btnDrawShape;
        private ComboBox cboShape;
        private Label lblShape;
        private TextBox txtPorts;
        private Label lblPorts;
        private NumericUpDown nudNodeSize;
        private Label lblNodeSize;
        private NumericUpDown nudMargin;
        private Label lblMargin;
        private CheckBox chkDisplay;
        private NumericUpDown nudTrials;
        private Label lblTrials;
        private TextBox txtDistance;
        private Label lblDistance;
        private TextBox txtTime;
        private Label lblTime;
        private Button btnHELP;
        private RichTextBox rtbxHELP;
        private Button btnTest;
        private Button btnTune;
    }
}
