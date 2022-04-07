namespace Book_Analysis
{
    partial class Book_Analysis
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Book_Analysis));
            this.tabSelector = new MaterialSkin.Controls.MaterialTabSelector();
            this.tabControl = new MaterialSkin.Controls.MaterialTabControl();
            this.tabLearn = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.tbBookFile = new System.Windows.Forms.TextBox();
            this.btnBrowsBookFile = new System.Windows.Forms.Button();
            this.tabAnalysis = new System.Windows.Forms.TabPage();
            this.tabControl.SuspendLayout();
            this.tabLearn.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabSelector
            // 
            this.tabSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabSelector.BaseTabControl = this.tabControl;
            this.tabSelector.Depth = 0;
            this.tabSelector.Location = new System.Drawing.Point(4, 2);
            this.tabSelector.MouseState = MaterialSkin.MouseState.HOVER;
            this.tabSelector.Name = "tabSelector";
            this.tabSelector.Size = new System.Drawing.Size(793, 68);
            this.tabSelector.TabIndex = 1;
            this.tabSelector.Text = "materialTabSelector1";
            // 
            // tabControl
            // 
            this.tabControl.AllowDrop = true;
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabLearn);
            this.tabControl.Controls.Add(this.tabAnalysis);
            this.tabControl.Depth = 0;
            this.tabControl.Location = new System.Drawing.Point(4, 76);
            this.tabControl.MouseState = MaterialSkin.MouseState.HOVER;
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(793, 376);
            this.tabControl.TabIndex = 2;
            // 
            // tabLearn
            // 
            this.tabLearn.AllowDrop = true;
            this.tabLearn.Controls.Add(this.label1);
            this.tabLearn.Controls.Add(this.tbBookFile);
            this.tabLearn.Controls.Add(this.btnBrowsBookFile);
            this.tabLearn.Location = new System.Drawing.Point(4, 24);
            this.tabLearn.Name = "tabLearn";
            this.tabLearn.Padding = new System.Windows.Forms.Padding(3);
            this.tabLearn.Size = new System.Drawing.Size(785, 348);
            this.tabLearn.TabIndex = 0;
            this.tabLearn.Text = "Learn";
            this.tabLearn.UseVisualStyleBackColor = true;
            this.tabLearn.DragDrop += new System.Windows.Forms.DragEventHandler(this.tabLearn_DragDrop);
            this.tabLearn.DragEnter += new System.Windows.Forms.DragEventHandler(this.tabLearn_DragEnter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Book File:";
            // 
            // tbBookFile
            // 
            this.tbBookFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbBookFile.Location = new System.Drawing.Point(67, 14);
            this.tbBookFile.Name = "tbBookFile";
            this.tbBookFile.Size = new System.Drawing.Size(669, 23);
            this.tbBookFile.TabIndex = 1;
            // 
            // btnBrowsBookFile
            // 
            this.btnBrowsBookFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowsBookFile.Location = new System.Drawing.Point(742, 14);
            this.btnBrowsBookFile.Name = "btnBrowsBookFile";
            this.btnBrowsBookFile.Size = new System.Drawing.Size(37, 23);
            this.btnBrowsBookFile.TabIndex = 0;
            this.btnBrowsBookFile.Text = "...";
            this.btnBrowsBookFile.UseVisualStyleBackColor = true;
            this.btnBrowsBookFile.Click += new System.EventHandler(this.btnBrowsBookFile_Click);
            // 
            // tabAnalysis
            // 
            this.tabAnalysis.Location = new System.Drawing.Point(4, 24);
            this.tabAnalysis.Name = "tabAnalysis";
            this.tabAnalysis.Padding = new System.Windows.Forms.Padding(3);
            this.tabAnalysis.Size = new System.Drawing.Size(785, 348);
            this.tabAnalysis.TabIndex = 1;
            this.tabAnalysis.Text = "Analysis";
            this.tabAnalysis.UseVisualStyleBackColor = true;
            // 
            // Book_Analysis
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.tabSelector);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Book_Analysis";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Book Analysis";
            this.tabControl.ResumeLayout(false);
            this.tabLearn.ResumeLayout(false);
            this.tabLearn.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private MaterialSkin.Controls.MaterialTabSelector tabSelector;
        private MaterialSkin.Controls.MaterialTabControl tabControl;
        private TabPage tabLearn;
        private TabPage tabAnalysis;
        private Label label1;
        private TextBox tbBookFile;
        private Button btnBrowsBookFile;
    }
}