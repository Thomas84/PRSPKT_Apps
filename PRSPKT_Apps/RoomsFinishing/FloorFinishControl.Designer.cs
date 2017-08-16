namespace PRSPKT_Apps.RoomsFinishing
{
    partial class FloorFinishControl
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
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.allrooms_radio = new System.Windows.Forms.RadioButton();
            this.selectedrooms_radio = new System.Windows.Forms.RadioButton();
            this.selectfloor_label = new System.Windows.Forms.GroupBox();
            this.FloorTypeListBox = new System.Windows.Forms.ListBox();
            this.groupBoxName = new System.Windows.Forms.GroupBox();
            this.paramSelector = new System.Windows.Forms.ComboBox();
            this.Height_TextBox = new System.Windows.Forms.TextBox();
            this.heightparam_radio = new System.Windows.Forms.RadioButton();
            this.floorheight_radio = new System.Windows.Forms.RadioButton();
            this.selectfloor_label.SuspendLayout();
            this.groupBoxName.SuspendLayout();
            this.SuspendLayout();
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(116, 426);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 0;
            this.OKButton.Text = "OKButton";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.Ok_Button_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(197, 426);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 1;
            this.CancelButton.Text = "CancelButton";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.Cancel_Button_Click);
            // 
            // allrooms_radio
            // 
            this.allrooms_radio.AutoSize = true;
            this.allrooms_radio.Location = new System.Drawing.Point(12, 12);
            this.allrooms_radio.Name = "allrooms_radio";
            this.allrooms_radio.Size = new System.Drawing.Size(92, 17);
            this.allrooms_radio.TabIndex = 2;
            this.allrooms_radio.Text = "allrooms_radio";
            this.allrooms_radio.UseVisualStyleBackColor = true;
            // 
            // selectedrooms_radio
            // 
            this.selectedrooms_radio.AutoSize = true;
            this.selectedrooms_radio.Checked = true;
            this.selectedrooms_radio.Location = new System.Drawing.Point(12, 35);
            this.selectedrooms_radio.Name = "selectedrooms_radio";
            this.selectedrooms_radio.Size = new System.Drawing.Size(122, 17);
            this.selectedrooms_radio.TabIndex = 3;
            this.selectedrooms_radio.TabStop = true;
            this.selectedrooms_radio.Text = "selectedrooms_radio";
            this.selectedrooms_radio.UseVisualStyleBackColor = true;
            // 
            // selectfloor_label
            // 
            this.selectfloor_label.Controls.Add(this.FloorTypeListBox);
            this.selectfloor_label.Location = new System.Drawing.Point(12, 58);
            this.selectfloor_label.Name = "selectfloor_label";
            this.selectfloor_label.Size = new System.Drawing.Size(260, 259);
            this.selectfloor_label.TabIndex = 4;
            this.selectfloor_label.TabStop = false;
            this.selectfloor_label.Text = "selectfloor_label";
            // 
            // FloorTypeListBox
            // 
            this.FloorTypeListBox.FormattingEnabled = true;
            this.FloorTypeListBox.Location = new System.Drawing.Point(6, 19);
            this.FloorTypeListBox.Name = "FloorTypeListBox";
            this.FloorTypeListBox.Size = new System.Drawing.Size(254, 238);
            this.FloorTypeListBox.TabIndex = 0;
            // 
            // groupBoxName
            // 
            this.groupBoxName.Controls.Add(this.paramSelector);
            this.groupBoxName.Controls.Add(this.Height_TextBox);
            this.groupBoxName.Controls.Add(this.heightparam_radio);
            this.groupBoxName.Controls.Add(this.floorheight_radio);
            this.groupBoxName.Location = new System.Drawing.Point(18, 321);
            this.groupBoxName.Name = "groupBoxName";
            this.groupBoxName.Size = new System.Drawing.Size(254, 99);
            this.groupBoxName.TabIndex = 5;
            this.groupBoxName.TabStop = false;
            this.groupBoxName.Text = "groupBoxName";
            // 
            // paramSelector
            // 
            this.paramSelector.FormattingEnabled = true;
            this.paramSelector.Location = new System.Drawing.Point(115, 42);
            this.paramSelector.Name = "paramSelector";
            this.paramSelector.Size = new System.Drawing.Size(139, 21);
            this.paramSelector.TabIndex = 3;
            // 
            // Height_TextBox
            // 
            this.Height_TextBox.Location = new System.Drawing.Point(115, 19);
            this.Height_TextBox.Name = "Height_TextBox";
            this.Height_TextBox.Size = new System.Drawing.Size(139, 20);
            this.Height_TextBox.TabIndex = 2;
            this.Height_TextBox.Text = "0";
            this.Height_TextBox.Leave += new System.EventHandler(this.Height_Text_LostFocus);
            // 
            // heightparam_radio
            // 
            this.heightparam_radio.AutoSize = true;
            this.heightparam_radio.Location = new System.Drawing.Point(6, 43);
            this.heightparam_radio.Name = "heightparam_radio";
            this.heightparam_radio.Size = new System.Drawing.Size(112, 17);
            this.heightparam_radio.TabIndex = 1;
            this.heightparam_radio.Text = "heightparam_radio";
            this.heightparam_radio.UseVisualStyleBackColor = true;
            // 
            // floorheight_radio
            // 
            this.floorheight_radio.AutoSize = true;
            this.floorheight_radio.Checked = true;
            this.floorheight_radio.Location = new System.Drawing.Point(6, 19);
            this.floorheight_radio.Name = "floorheight_radio";
            this.floorheight_radio.Size = new System.Drawing.Size(103, 17);
            this.floorheight_radio.TabIndex = 0;
            this.floorheight_radio.TabStop = true;
            this.floorheight_radio.Text = "floorheight_radio";
            this.floorheight_radio.UseVisualStyleBackColor = true;
            // 
            // FloorFinishControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 461);
            this.Controls.Add(this.groupBoxName);
            this.Controls.Add(this.selectfloor_label);
            this.Controls.Add(this.selectedrooms_radio);
            this.Controls.Add(this.allrooms_radio);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Name = "FloorFinishControl";
            this.Text = "Создание полов";
            this.selectfloor_label.ResumeLayout(false);
            this.groupBoxName.ResumeLayout(false);
            this.groupBoxName.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OKButton;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.RadioButton allrooms_radio;
        private System.Windows.Forms.RadioButton selectedrooms_radio;
        private System.Windows.Forms.GroupBox selectfloor_label;
        private System.Windows.Forms.ListBox FloorTypeListBox;
        private System.Windows.Forms.GroupBox groupBoxName;
        private System.Windows.Forms.TextBox Height_TextBox;
        private System.Windows.Forms.RadioButton heightparam_radio;
        private System.Windows.Forms.RadioButton floorheight_radio;
        private System.Windows.Forms.ComboBox paramSelector;
    }
}