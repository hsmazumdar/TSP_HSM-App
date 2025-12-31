namespace TSP_CS
{
    partial class MatrixChoiceDialog
    {
        private System.ComponentModel.IContainer? components = null;
        private System.Windows.Forms.Button btnOldMatrix;
        private System.Windows.Forms.Button btnNewMatrix;
        private System.Windows.Forms.Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            btnOldMatrix = new Button();
            btnNewMatrix = new Button();
            btnCancel = new Button();
            textBox1 = new TextBox();
            label2 = new Label();
            SuspendLayout();
            // 
            // btnOldMatrix
            // 
            btnOldMatrix.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnOldMatrix.DialogResult = DialogResult.OK;
            btnOldMatrix.Location = new Point(12, 306);
            btnOldMatrix.Margin = new Padding(3, 0, 3, 10);
            btnOldMatrix.MinimumSize = new Size(0, 35);
            btnOldMatrix.Name = "btnOldMatrix";
            btnOldMatrix.Size = new Size(557, 59);
            btnOldMatrix.TabIndex = 1;
            btnOldMatrix.Text = "Use Old Matrix";
            btnOldMatrix.UseVisualStyleBackColor = true;
            btnOldMatrix.Click += btnOldMatrix_Click;
            // 
            // btnNewMatrix
            // 
            btnNewMatrix.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnNewMatrix.DialogResult = DialogResult.OK;
            btnNewMatrix.Location = new Point(12, 239);
            btnNewMatrix.Margin = new Padding(3, 0, 3, 10);
            btnNewMatrix.MinimumSize = new Size(0, 35);
            btnNewMatrix.Name = "btnNewMatrix";
            btnNewMatrix.Size = new Size(557, 59);
            btnNewMatrix.TabIndex = 2;
            btnNewMatrix.Text = "Calculate New Matrix (Slow)";
            btnNewMatrix.UseVisualStyleBackColor = true;
            btnNewMatrix.Click += btnNewMatrix_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(12, 172);
            btnCancel.Margin = new Padding(3, 0, 3, 0);
            btnCancel.MinimumSize = new Size(0, 35);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(557, 59);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel DRAW";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.Location = new Point(12, 12);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(557, 78);
            textBox1.TabIndex = 5;
            textBox1.Text = "A cached distance matrix exists for this configuration.";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 112);
            label2.Name = "label2";
            label2.Size = new Size(209, 32);
            label2.TabIndex = 6;
            label2.Text = "Choose an option:";
            // 
            // MatrixChoiceDialog
            // 
            AcceptButton = btnOldMatrix;
            AutoScaleMode = AutoScaleMode.None;
            CancelButton = btnCancel;
            ClientSize = new Size(581, 393);
            ControlBox = false;
            Controls.Add(label2);
            Controls.Add(textBox1);
            Controls.Add(btnNewMatrix);
            Controls.Add(btnOldMatrix);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(400, 240);
            Name = "MatrixChoiceDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Distance Matrix Options";
            ResumeLayout(false);
            PerformLayout();
        }
        private TextBox textBox1;
        private Label label2;
    }
}

