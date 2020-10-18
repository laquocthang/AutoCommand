using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AutoCommand
{
	public enum TextCase
	{
		None,
		ToLower,
		ToUpper,
		ToFirstUpper,
		ToTitle,
		ToCamelCase,
		ToReverseCase
	}

	public class Converter
	{
		public static string ChangeCase(string text, int @case)
		{
			switch ((TextCase)@case)
			{
				case TextCase.None:
					return text;
				case TextCase.ToLower:
					return text.ToLower();
				case TextCase.ToUpper:
					return text.ToUpper();
				case TextCase.ToFirstUpper:
					return char.ToUpper(text.First()) + text.Substring(1).ToLower();
				case TextCase.ToTitle:
					TextInfo t = new CultureInfo("en-US", false).TextInfo;
					return t.ToTitleCase(text);
				case TextCase.ToCamelCase:
					return ChangeCase(text, (int)TextCase.ToTitle).Replace(" ", "");
				case TextCase.ToReverseCase:
					return new string(text.Select(c => char.IsLetter(c) ? (char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)) : c).ToArray());
				default:
					return null;
			}
		}

		public static string ConvertFileSize(double size)
		{
			string[] sizes = { "B", "KB", "MB", "GB", "TB" };
			int order = 0;
			while (size >= 1024 && order < sizes.Length - 1)
			{
				order++;
				size = size / 1024;
			}
			return string.Format("{0:0.##} {1}", size, sizes[order]);
		}

		public static string ConvertDate(string strAsDate, bool en2VI)
		{
			if (en2VI)
			{
				DateTime d = DateTime.ParseExact(strAsDate, "yyyy/M/dd", System.Globalization.CultureInfo.InvariantCulture);
				return d.ToString("dd/M/yyyy");
			}
			else
			{
				DateTime d = DateTime.ParseExact(strAsDate, "dd/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
				return d.ToString("yyyy/M/dd");
			}
		}

		public static object[] GetFileInfoToRow(string fullFileName, bool isEN)
		{
			object[] row = new object[5];
			DateTime modification = File.GetLastWriteTime(fullFileName);
			string name = Path.GetFileName(fullFileName).TrimStart(' ');
			row[0] = name;
			row[1] = name;
			row[2] = ConvertFileSize(new FileInfo(fullFileName).Length);
			if (isEN)
				row[3] = modification.ToString("yyyy/M/dd");
			else row[3] = modification.ToString("dd/M/yyyy");
			row[4] = fullFileName.Replace(name, "");
			return row;
		}

		public static List<string> DecomposeMask(string text)
		{
			List<string> mask = new List<string>();
			string pattern = "\\[.*?\\]|.";
			Regex rgx = new Regex(pattern);
			foreach (Match m in rgx.Matches(text, 0))
				mask.Add(m.Value);
			return mask;
		}

		public static string[] SplitByDot(string fileName)
		{
			string[] r = new string[2];
			int i = fileName.LastIndexOf('.');
			r[0] = fileName.Remove(i);
			r[1] = fileName.Substring(i + 1);
			return r;
		}

		public static string Retitle(string vietnameseText)
		{
			var from = "àáảãạăằắẳẵặâầấẩẫậđèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵ·/_,:;";
			var to = "aaaaaaaaaaaaaaaaadeeeeeeeeeeeiiiiiooooooooooooooooouuuuuuuuuuuyyyyy------";
			var r = vietnameseText;
			for (int i = 0, l = from.Length; i < l; i++)
				r = r.Replace(from[i].ToString(), to[i].ToString()).Replace(from[i].ToString().ToUpper(), to[i].ToString().ToUpper());
			return r;
		}

	}
}
