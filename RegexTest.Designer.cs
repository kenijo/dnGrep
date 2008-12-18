namespace nGREP
{
	partial class RegexTest
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegexTest));
			this.tbReplaceWith = new System.Windows.Forms.TextBox();
			this.tbSearchFor = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.tbInputText = new System.Windows.Forms.TextBox();
			this.btnReplace = new System.Windows.Forms.Button();
			this.btnSearch = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tbOutputText = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.btnDone = new System.Windows.Forms.Button();
			this.btnReplaceHelp = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tbReplaceWith
			// 
			this.tbReplaceWith.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbReplaceWith.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::nGREP.Properties.Settings.Default, "ReplaceWith", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.tbReplaceWith.Location = new System.Drawing.Point(12, 64);
			this.tbReplaceWith.Name = "tbReplaceWith";
			this.tbReplaceWith.Size = new System.Drawing.Size(522, 20);
			this.tbReplaceWith.TabIndex = 1;
			this.tbReplaceWith.Text = global::nGREP.Properties.Settings.Default.ReplaceWith;
			// 
			// tbSearchFor
			// 
			this.tbSearchFor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbSearchFor.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::nGREP.Properties.Settings.Default, "SearchFor", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.tbSearchFor.Location = new System.Drawing.Point(12, 25);
			this.tbSearchFor.Name = "tbSearchFor";
			this.tbSearchFor.Size = new System.Drawing.Size(550, 20);
			this.tbSearchFor.TabIndex = 0;
			this.tbSearchFor.Text = global::nGREP.Properties.Settings.Default.SearchFor;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 13);
			this.label2.TabIndex = 7;
			this.label2.Text = "Replace with:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(59, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Search for:";
			// 
			// tbInputText
			// 
			this.tbInputText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tbInputText.Location = new System.Drawing.Point(3, 23);
			this.tbInputText.Multiline = true;
			this.tbInputText.Name = "tbInputText";
			this.tbInputText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.tbInputText.Size = new System.Drawing.Size(544, 112);
			this.tbInputText.TabIndex = 0;
			// 
			// btnReplace
			// 
			this.btnReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnReplace.Location = new System.Drawing.Point(406, 90);
			this.btnReplace.Name = "btnReplace";
			this.btnReplace.Size = new System.Drawing.Size(75, 23);
			this.btnReplace.TabIndex = 2;
			this.btnReplace.Text = "Replace";
			this.btnReplace.UseVisualStyleBackColor = true;
			this.btnReplace.Click += new System.EventHandler(this.btnReplace_Click);
			// 
			// btnSearch
			// 
			this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSearch.Location = new System.Drawing.Point(487, 90);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(75, 23);
			this.btnSearch.TabIndex = 3;
			this.btnSearch.Text = "Search";
			this.btnSearch.UseVisualStyleBackColor = true;
			this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.tableLayoutPanel1);
			this.groupBox1.Location = new System.Drawing.Point(8, 119);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(556, 296);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Regex test:";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Controls.Add(this.tbOutputText, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.tbInputText, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label4, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(550, 277);
			this.tableLayoutPanel1.TabIndex = 14;
			// 
			// tbOutputText
			// 
			this.tbOutputText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tbOutputText.Location = new System.Drawing.Point(3, 161);
			this.tbOutputText.Multiline = true;
			this.tbOutputText.Name = "tbOutputText";
			this.tbOutputText.ReadOnly = true;
			this.tbOutputText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.tbOutputText.Size = new System.Drawing.Size(544, 113);
			this.tbOutputText.TabIndex = 1;
			// 
			// label3
			// 
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(3, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(544, 20);
			this.label3.TabIndex = 13;
			this.label3.Text = "Sample input text:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label4
			// 
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Location = new System.Drawing.Point(3, 138);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(544, 20);
			this.label4.TabIndex = 14;
			this.label4.Text = "Output text:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// btnDone
			// 
			this.btnDone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDone.Location = new System.Drawing.Point(489, 424);
			this.btnDone.Name = "btnDone";
			this.btnDone.Size = new System.Drawing.Size(75, 23);
			this.btnDone.TabIndex = 5;
			this.btnDone.Text = "Done";
			this.btnDone.UseVisualStyleBackColor = true;
			// 
			// btnReplaceHelp
			// 
			this.btnReplaceHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnReplaceHelp.Location = new System.Drawing.Point(540, 62);
			this.btnReplaceHelp.Name = "btnReplaceHelp";
			this.btnReplaceHelp.Size = new System.Drawing.Size(24, 23);
			this.btnReplaceHelp.TabIndex = 8;
			this.btnReplaceHelp.Text = "?";
			this.btnReplaceHelp.UseVisualStyleBackColor = true;
			// 
			// RegexTest
			// 
			this.AcceptButton = this.btnDone;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(574, 456);
			this.Controls.Add(this.btnReplaceHelp);
			this.Controls.Add(this.btnDone);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnReplace);
			this.Controls.Add(this.btnSearch);
			this.Controls.Add(this.tbReplaceWith);
			this.Controls.Add(this.tbSearchFor);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.Name = "RegexTest";
			this.Text = "Regex Test";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formKeyDown);
			this.groupBox1.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox tbReplaceWith;
		private System.Windows.Forms.TextBox tbSearchFor;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbInputText;
		private System.Windows.Forms.Button btnReplace;
		private System.Windows.Forms.Button btnSearch;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tbOutputText;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button btnDone;
		private System.Windows.Forms.Button btnReplaceHelp;
	}
}