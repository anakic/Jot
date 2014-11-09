namespace TestWinForms
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.colorPicker2 = new TestWinForms.ColorPickerUC();
            this.colorPicker1 = new TestWinForms.ColorPickerUC();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // colorPicker2
            // 
            this.colorPicker2.Blue = ((byte)(0));
            this.colorPicker2.Green = ((byte)(0));
            this.colorPicker2.Location = new System.Drawing.Point(347, 73);
            this.colorPicker2.Name = "colorPicker2";
            this.colorPicker2.Red = ((byte)(0));
            this.colorPicker2.Size = new System.Drawing.Size(314, 357);
            this.colorPicker2.TabIndex = 1;
            // 
            // colorPicker1
            // 
            this.colorPicker1.Blue = ((byte)(0));
            this.colorPicker1.Green = ((byte)(0));
            this.colorPicker1.Location = new System.Drawing.Point(15, 73);
            this.colorPicker1.Name = "colorPicker1";
            this.colorPicker1.Red = ((byte)(0));
            this.colorPicker1.Size = new System.Drawing.Size(314, 357);
            this.colorPicker1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoEllipsis = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(660, 61);
            this.label1.TabIndex = 2;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 442);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.colorPicker2);
            this.Controls.Add(this.colorPicker1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private ColorPickerUC colorPicker1;
        private ColorPickerUC colorPicker2;
        private System.Windows.Forms.Label label1;
    }
}

