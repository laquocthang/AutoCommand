using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AutoCommand
{
	public partial class Form1 : Form
	{
		private string logPath, backupPath;
		private bool canReplace = true;
		private char[] prohibitedChars = { '\\', '/', ':', '*', '?', '\"', '<', '>', '|' };

		public Form1()
		{
			InitializeComponent();
		}

		private void aboutAuthorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show("CHƯƠNG TRÌNH HỖ TRỢ ĐỔI TÊN ĐA TẬP TIN VÀ THÊM ẢNH MỜ (WATERMARK) VÀO ẢNH\nTác giả: LA QUỐC THẮNG\nChi tiết liên hệ: quocthang0507@gmail.com", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			dataGridView1.RowHeadersVisible = false;
			GetFont();
		}

		#region Tab1

		private void btn_Browse1_Click(object sender, EventArgs e)
		{
			openFileDialog1.Title = "Browse for one or more files";
			openFileDialog1.RestoreDirectory = true;
			openFileDialog1.Multiselect = true;
			openFileDialog1.Filter = "All files (*.*)|*.*";
			ShowSelectedfiles();
		}

		void Reset()
		{
			tbx_fileName.Text = "[N]";
			tbx_Extension.Text = "[E]";
			tbx_Find.Text = "";
			tbx_Replace.Text = "";
			cbx_Case.SelectedIndex = 0;
			nud_Start.Value = 1;
			nud_Step.Value = 1;
			cbx_Digits.SelectedIndex = 0;
			chk_Diacritical.Checked = false;
		}

		void ShowSelectedfiles()
		{
			DialogResult r = openFileDialog1.ShowDialog();
			if (r == DialogResult.OK)
			{
				dataGridView1.Rows.Clear();
				Reset();
				int count = openFileDialog1.FileNames.Length;
				int step = 1 / count;
				progressBar1.Value = 0;
				foreach (var file in openFileDialog1.FileNames)
				{
					dataGridView1.Rows.Add(Converter.GetFileInfoToRow(file, !chk_Format.Checked));
					progressBar1.Value += step;
				}
				logPath = Path.Combine(dataGridView1.Rows[0].Cells[4].Value.ToString(), "log.txt");
				backupPath = Path.Combine(dataGridView1.Rows[0].Cells[4].Value.ToString(), "backup.txt");
				progressBar1.Value = 100;
			}
		}

		private void btn_Name_Click(object sender, EventArgs e)
		{
			tbx_fileName.Text += "[N]";
		}

		private void btn_Counter_Click(object sender, EventArgs e)
		{
			tbx_fileName.Text += "[C]";
		}

		private void btn_Time_Click(object sender, EventArgs e)
		{
			tbx_fileName.Text += "[T]";
		}

		private void btn_Range_Click(object sender, EventArgs e)
		{
			tbx_fileName.Text += "[N#-#]";
		}

		private void btn_Date_Click(object sender, EventArgs e)
		{
			tbx_fileName.Text += "[D]";
		}

		private void btn_eExtension_Click(object sender, EventArgs e)
		{
			tbx_Extension.Text += "[E]";
		}

		private void btn_eRange_Click(object sender, EventArgs e)
		{
			tbx_Extension.Text += "[E#-#]";
		}

		private void btn_eCounter_Click(object sender, EventArgs e)
		{
			tbx_Extension.Text += "[C]";
		}

		private void label11_Click(object sender, EventArgs e)
		{
			if (!File.Exists(logPath))
				File.CreateText(logPath);
			Process.Start(logPath);
		}

		private void label16_Click(object sender, EventArgs e)
		{
			string[] names = openFileDialog1.FileNames;
			if (names.Count() == 0)
				MessageBox.Show("You have to browse for file(s) first", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			else
			{
				StreamWriter sw = new StreamWriter(backupPath, true);
				if (!chk_Format.Checked)
					sw.WriteLine(DateTime.Now.ToString("yyyy/M/dd"));
				else sw.WriteLine(DateTime.Now.ToString("dd/M/yyyy"));
				foreach (var item in names)
					sw.WriteLine(item);
				sw.WriteLine();
				sw.Close();
				Process.Start(backupPath);
			}
		}

		private void tbx_fileName_DoubleClick(object sender, EventArgs e)
		{
			tbx_fileName.SelectAll();
		}

		private void tbx_Extension_DoubleClick(object sender, EventArgs e)
		{
			tbx_Extension.SelectAll();
		}

		//The name without extension, and it is original name
		string processFileName(string name, int id, int count, int total, int digit)
		{
			List<string> mask = Converter.DecomposeMask(tbx_fileName.Text);
			string n = "";
			if (mask.Count == 0)
				return n;
			else
				foreach (var item in mask)
				{
					string path = Path.Combine(dataGridView1.Rows[id].Cells[4].Value.ToString(), dataGridView1.Rows[id].Cells[0].Value.ToString());
					DateTime modification = File.GetLastWriteTime(path);
					switch (item)
					{
						case "[N]":
						case "[n]":
							n += name;
							break;
						case "[C]":
						case "[c]":
							n += count.ToString("D" + digit);
							break;
						case "[T]":
						case "[t]":
							n += modification.ToString("hhmmss");
							break;
						case "[N#-#]":
						case "[n#-#]":
							n += string.Concat(id + 1, "-", total - 1);
							break;
						case "[D]":
						case "[d]":
							if (!chk_Format.Checked)
								n += modification.ToString("yyyyMdd");
							else n += modification.ToString("ddMyyyy");
							break;
						default:
							n += item;
							break;
					}
					if (chk_Diacritical.Checked)
						n = Converter.Retitle(n);
				}
			return n;
		}

		string processFileExt(string ext, int id, int count, int total, int digit)
		{
			List<string> mask = Converter.DecomposeMask(tbx_Extension.Text);
			string e = "";
			if (mask.Count == 0)
				return e;
			else
				foreach (var item in mask)
				{
					switch (item)
					{
						case "[C]":
						case "[c]":
							e += count.ToString("D" + digit);
							break;
						case "[E]":
						case "[e]":
							e += ext;
							break;
						case "[E#-#]":
						case "[e#-#]":
							e += string.Concat(id + 1, "-", total - 1);
							break;
						default:
							e += item;
							break;
					}
				}
			return e;
		}

		void processMask()
		{
			int count = Convert.ToInt32(nud_Start.Value), id = 0;
			foreach (DataGridViewRow row in dataGridView1.Rows)
			{
				if (row.Cells[0].Value != null)
				{
					string[] t = Converter.SplitByDot(row.Cells[0].Value.ToString());
					string r = string.Concat(processFileName(t[0], id, count, dataGridView1.RowCount, int.Parse(cbx_Digits.SelectedItem.ToString())), '.', processFileExt(t[1], id, count, dataGridView1.RowCount, int.Parse(cbx_Digits.SelectedItem.ToString())));
					r = Converter.ChangeCase(r, cbx_Case.SelectedIndex);
					if (tbx_Find.Text != "" && canReplace)
					{
						t = Converter.SplitByDot(r);
						t[0] = t[0].Replace(tbx_Find.Text, tbx_Replace.Text);
						t[1] = t[1].Replace(tbx_Find.Text, tbx_Replace.Text);
						row.Cells[1].Value = string.Concat(t[0], ".", t[1]);
					}
					else row.Cells[1].Value = r;
					count += Convert.ToInt32(nud_Step.Value);
					id++;
				}
			}
		}

		void ShowProhibitCharacters(TextBox textBox)
		{
			ToolTip toolTip = new ToolTip();
			toolTip.Show("A file name can't contain any of the following characters: \\ / : * ? \" < > |", textBox, 10000);
		}

		private void tbx_fileName_TextChanged(object sender, EventArgs e)
		{
			processMask();
		}

		private void tbx_Extension_TextChanged(object sender, EventArgs e)
		{
			processMask();
		}

		void ReloadMask()
		{
			processMask();
		}

		private void cbx_Case_SelectedValueChanged(object sender, EventArgs e)
		{
			if (cbx_Case.SelectedIndex == 0)
				ReloadMask();
			else
				foreach (DataGridViewRow row in dataGridView1.Rows)
					if (row.Cells[0].Value != null)
						row.Cells[1].Value = Converter.ChangeCase(row.Cells[1].Value.ToString(), cbx_Case.SelectedIndex);
		}

		private void tbx_Find_TextChanged(object sender, EventArgs e)
		{
			canReplace = false;
			ReloadMask();
			if (tbx_Find.Text != "")
			{
				foreach (DataGridViewRow row in dataGridView1.Rows)
				{
					if (row.Cells[0].Value != null)
					{
						string[] t = Converter.SplitByDot(row.Cells[1].Value.ToString());
						t[0] = t[0].Replace(tbx_Find.Text, tbx_Replace.Text);
						t[1] = t[1].Replace(tbx_Find.Text, tbx_Replace.Text);
						row.Cells[1].Value = string.Concat(t[0], ".", t[1]);
					}
				}
			}
			canReplace = true;
		}

		private void tbx_Replace_TextChanged(object sender, EventArgs e)
		{
			if (tbx_Find.Text != "")
			{
				canReplace = false;
				ReloadMask();
				foreach (DataGridViewRow row in dataGridView1.Rows)
				{
					if (row.Cells[0].Value != null)
					{
						string[] t = Converter.SplitByDot(row.Cells[1].Value.ToString());
						t[0] = t[0].Replace(tbx_Find.Text, tbx_Replace.Text);
						t[1] = t[1].Replace(tbx_Find.Text, tbx_Replace.Text);
						row.Cells[1].Value = string.Concat(t[0], ".", t[1]);
					}
				}
				canReplace = true;
			}
		}

		private void btn_Start1_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.FileNames[0] == "openFileDialog1")
				MessageBox.Show("Please firstly browse file(s). Then change the file name and file extenion. Finally press this button", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			else
			{
				int count = dataGridView1.Rows.Count - 1;
				int step = 1 / count;
				progressBar1.Value = 0;
				StreamWriter sw = new StreamWriter(logPath, true);
				sw.WriteLine(DateTime.Now.ToString());
				foreach (DataGridViewRow row in dataGridView1.Rows)
					if (row.Cells[0].Value != null)
					{
						string oldPath = string.Concat(row.Cells[4].Value, row.Cells[0].Value);
						string newPath = string.Concat(row.Cells[4].Value, row.Cells[1].Value);
						File.Move(oldPath, newPath);
						sw.WriteLine(string.Concat("\"", oldPath, "\" -> \"", newPath, "\""));
						row.Cells[0].Value = row.Cells[1].Value;
						progressBar1.Value += step;
					}
				progressBar1.Value = 100;
				sw.WriteLine();
				sw.Close();
				Reset();
			}
		}

		private void label11_MouseHover(object sender, EventArgs e)
		{
			label11.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Underline, GraphicsUnit.Point, ((byte)(163)));
		}

		private void label11_MouseLeave(object sender, EventArgs e)
		{
			label11.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(163)));
		}

		private void label16_MouseHover(object sender, EventArgs e)
		{
			label16.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Underline, GraphicsUnit.Point, ((byte)(163)));
		}

		private void label16_MouseLeave(object sender, EventArgs e)
		{
			label16.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(163)));
		}

		private void nud_Start_ValueChanged(object sender, EventArgs e)
		{
			if (tbx_fileName.Text.Contains("[C]") || tbx_fileName.Text.Contains("[c]"))
				tbx_fileName_TextChanged(sender, e);
			if (tbx_Extension.Text.Contains("[C]") || tbx_fileName.Text.Contains("[c]"))
				tbx_Extension_TextChanged(sender, e);
		}

		private void nud_Step_ValueChanged(object sender, EventArgs e)
		{
			nud_Start_ValueChanged(sender, e);
		}

		private void cbx_Digits_SelectedIndexChanged(object sender, EventArgs e)
		{
			nud_Start_ValueChanged(sender, e);
		}

		private void tbx_fileName_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (prohibitedChars.Contains(e.KeyChar))
			{
				ShowProhibitCharacters((TextBox)sender);
				e.Handled = true;
			}
		}

		private void tbx_Extension_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (prohibitedChars.Contains(e.KeyChar))
			{
				ShowProhibitCharacters((TextBox)sender);
				e.Handled = true;
			}
		}

		private void chk_Diacritical_CheckedChanged(object sender, EventArgs e)
		{
			ReloadMask();
		}

		private void chk_Format_CheckedChanged(object sender, EventArgs e)
		{
			foreach (DataGridViewRow item in dataGridView1.Rows)
				if (item.Cells[3].Value != null)
				{
					item.Cells[3].Value = Converter.ConvertDate(item.Cells[3].Value.ToString(), ((CheckBox)sender).Checked);
				}
		}

		#endregion

		#region Tab2

		int GetFontStyle()
		{
			if (cbx_FontStyle.SelectedIndex == 0)
				return 0;
			else if (cbx_FontStyle.SelectedIndex == 1)
				return 1;
			else if (cbx_FontStyle.SelectedIndex == 2)
				return 2;
			else if (cbx_FontStyle.SelectedIndex == 4)
				return 4;
			else return 8;
		}

		private void listFont_DrawItem(object sender, DrawItemEventArgs e)
		{
			e.DrawBackground();
			e.Graphics.DrawString(listFont.Items[e.Index].ToString(), new Font(listFont.Items[e.Index].ToString(), 16), Brushes.Black, e.Bounds);
		}

		private void btn_Start2_Click(object sender, EventArgs e)
		{
			Image img = null;
			string path = tbx_Path2.Text;
			string fullPath = string.Empty;
			try
			{
				string[] imgExtension = { "*.jpg", "*.jpeg", ".gif", "*.bmp", "*.png" };
				List<FileInfo> files = new List<FileInfo>();
				DirectoryInfo dir = new DirectoryInfo(path);
				foreach (string ext in imgExtension)
				{
					FileInfo[] folder = dir.GetFiles(ext, SearchOption.AllDirectories);
					foreach (FileInfo file in folder)
					{
						FileStream fs = file.OpenRead();
						fullPath = path + @"\" + file.Name;
						Stream outputStream = new MemoryStream();
						AddWaterMark.AddWatermark(fs, int.Parse(tbx_PX.Text), int.Parse(tbx_PY.Text), tbx_Text.Text, outputStream, listFont.SelectedItem.ToString(), Convert.ToInt32(cbx_Size.Text), (FontStyle)GetFontStyle(), btn_ChoosenColor.BackColor);
						fs.Close();
						img = Image.FromStream(outputStream);
						using (Bitmap savingImage = new Bitmap(img.Width, img.Height, img.PixelFormat))
						{
							using (Graphics g = Graphics.FromImage(savingImage))
								g.DrawImage(img, new Point(0, 0));
							string[] t = file.Name.Split('.');
							savingImage.Save(string.Concat(path, @"\", t[0], "_new.", t[1]), ImageFormat.Jpeg);
						}
						img.Dispose();
					}
				}
				MessageBox.Show("Processing Completed", "Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show("There was an error during processing...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				if (img != null)
					img.Dispose();
			}
		}

		void GetFont()
		{
			listFont.ItemHeight = 30;
			using (InstalledFontCollection col = new InstalledFontCollection())
			{
				foreach (FontFamily fa in col.Families)
					listFont.Items.Add(fa.Name);
				listFont.DrawMode = DrawMode.OwnerDrawFixed;
			}
			listFont.SelectedIndex = 0;
		}

		private void btn_Browse2_Click(object sender, EventArgs e)
		{
			folderBrowserDialog1.Description = "Browse for image folder";
			folderBrowserDialog1.ShowNewFolderButton = false;
			ReviewPicture();
		}

		string GetTheFirstPic()
		{
			string[] files = Directory.GetFiles(tbx_Path2.Text);
			foreach (var item in files)
				if (item.ToLower().EndsWith(".jpg") || item.ToLower().EndsWith(".jpeg") ||
					item.ToLower().EndsWith(".gif") || item.ToLower().EndsWith(".bmp") ||
					item.ToLower().EndsWith(".png"))
					return item;
			return "";
		}

		void ReviewPicture()
		{
			DialogResult result = folderBrowserDialog1.ShowDialog();
			if (result == DialogResult.OK)
			{
				tbx_Path2.Text = folderBrowserDialog1.SelectedPath;
				string r = GetTheFirstPic();
				if (r == "")
					MessageBox.Show("Can't find any pictures on this folder", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Information);
				else
					pbx_Before.Image = Image.FromFile(r);
			}
			else tbx_Path2.Text = "";
		}

		private void btn_ChoosenColor_Click(object sender, EventArgs e)
		{
			colorDialog1.ShowDialog();
			btn_ChoosenColor.BackColor = colorDialog1.Color;
		}

		private void btn_Review_Click(object sender, EventArgs e)
		{
			string fullPath = GetTheFirstPic();
			FileInfo file = new FileInfo(fullPath);
			FileStream fs = file.OpenRead();
			Stream outputStream = new MemoryStream();
			AddWaterMark.AddWatermark(fs, int.Parse(tbx_PX.Text), int.Parse(tbx_PY.Text), tbx_Text.Text, outputStream, listFont.SelectedItem.ToString(), Convert.ToInt32(cbx_Size.Text), (FontStyle)GetFontStyle(), btn_ChoosenColor.BackColor);
			pbx_After.Image = Image.FromStream(outputStream);
			fs.Close();
		}
	}
	#endregion
}
