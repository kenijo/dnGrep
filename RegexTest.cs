using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace nGREP
{
	public partial class RegexTest : Form
	{
		private string searchRegex = "";
		private string replaceRegex = "";

		public RegexTest(string search, string replace)
		{
			InitializeComponent();
			searchRegex = search;
			replaceRegex = replace;
			tbSearchFor.Text = searchRegex;
			tbReplaceWith.Text = replaceRegex;
		}

		private void btnSearch_Click(object sender, EventArgs e)
		{
			try
			{
				Regex regex = new Regex(tbSearchFor.Text);
				StringBuilder sb = new StringBuilder();
				foreach (Match match in regex.Matches(tbInputText.Text))
				{
					sb.AppendLine(match.Value);
					sb.AppendLine("=================================");
				}
				tbOutputText.Text = sb.ToString();
			}
			catch (Exception ex)
			{
				tbOutputText.Text = "Error: " + ex.Message;
			}
		}

		private void formKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
		}

		private void btnReplace_Click(object sender, EventArgs e)
		{
			try
			{
				Regex search = new Regex(tbSearchFor.Text);
				string replace = tbReplaceWith.Text;

				tbOutputText.Text = search.Replace(tbInputText.Text, replace);
			}
			catch (Exception ex)
			{
				tbOutputText.Text = "Error: " + ex.Message;
			}
		}
	}
}