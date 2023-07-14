using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Format
{
	public static string FormatSQL(string txt)
	{
		StringBuilder sb = new StringBuilder();
		string tmp = txt.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
		bool inText = false;
		bool inField = false;
		List<string> operationCars = new List<string>() { "!", "=", "<", ">", "(", ")", "+", "-", "/", "*", "," ,"&", ";" };
		string prevCar = "";
		for (int idx = 0; idx < tmp.Length; idx++)
		{
			string car1 = tmp.Substring(idx, 1);
			if (!inText && car1 == "\t")
			{
				car1 = " ";
			}

			if (car1 == "'")
			{
				inText = !inText;
				sb.Append(car1);
			}
			else if (!inText && car1 == "[")
			{
				inField = true;
				sb.Append(car1);
			}
			else if (!inText && car1 == "]")
			{
				inField = false;
				sb.Append(car1);
			}
			else if (!inText && !inField && operationCars.Contains(car1))
			{
				if (prevCar != "\r\n")
				{
					sb.Append("\r\n");
				}
				sb.Append(car1);
				sb.Append("\r\n");
				car1 = "\r\n";
			}
			else if (!inText && !inField && car1 == " ")
			{
				string nextCar = "";
				do
				{
					if (idx + 1 < tmp.Length)
					{
						nextCar = tmp.Substring(idx + 1, 1);
						if (nextCar == " ")
						{
							idx++;
						}
					}

				} while (nextCar == " " && idx + 1 < tmp.Length);
				if (prevCar != "\r\n")
				{
					sb.Append("\r\n");
				}
				car1 = "\r\n";
			}
			else
			{
				sb.Append(car1);
			}
			prevCar = car1;
		}
		string[] lines = sb.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
		sb = new StringBuilder();
		int inSelect = 0;
		int inFrom = 0;
		int indent = 0;
		int inFunction = 0;
		int inFunctionSelect = 0;
		string lastWord = "";
		List<string> joins = new List<string> { "left", "right", "inner" };
		for (int idx = 0; idx < lines.Length; idx++)
		{
			string word = lines[idx];
			string next_word = "";
			if (idx + 1 < lines.Length)
			{
				next_word = lines[idx + 1];
			}
			if (word.ToLower() == "select")
			{
				inSelect++;
				indent++;
				sb.Append("\r\n" + GetTabs(indent) + word + "\r\n" + GetTabs(indent));
				if (inFunction != 0)
				{
					inFunctionSelect++;
				}
			}
			else if (word.ToLower() == "union" && next_word.ToLower() == "all")
			{
				idx++;
				sb.Append("\r\n\r\n" + GetTabs(indent) + word + " " + next_word + "\r\n");
				indent--;
			}
			else if (word.ToLower() == "union")
			{
				sb.Append("\r\n\r\n" + GetTabs(indent) + word + "\r\n");
				indent--;
			}
			else if (word.ToLower() == "from")
			{
				inFrom++;
				sb.Append("\r\n" + GetTabs(indent) + word + " ");
			}
			else if (lastWord.ToLower() == "from" && word == "(")
			{
				inFrom++;
				sb.Append("\r\n" + GetTabs(indent) + word + "\r\n");
			}
			else if (inFunction == 0 && word == ")")
			{
				indent--;
				sb.Append("\r\n" + GetTabs(indent) + word + "\r\n" + GetTabs(indent));
			}
			else if (word == ")" && next_word == "as")
			{
				inFunction--;
				indent--;
				sb.Append("\r\n" + GetTabs(indent) + word + "\r\n" + GetTabs(indent));
			}
			else if (inFunction == 0 && word == ",")
			{
				sb.Append(word + "\r\n" + GetTabs(indent));
			}
			else if (lastWord.ToLower() != "nocount" && word.ToLower() == "on")
			{
				sb.Append("\r\n" + GetTabs(indent) + word + " ");
			}
			else if (word.ToLower() == "where")
			{
				sb.Append("\r\n" + GetTabs(indent) + word + " ");
			}
			else if (word.ToLower() == "for" && next_word.ToLower() == "json")
			{
				sb.Append("\r\n" + GetTabs(indent) + word + " ");
			}
			else if ((word.ToLower() == "group" || word.ToLower() == "order") && next_word.ToLower() == "by")
			{
				idx++;
				sb.Append("\r\n" + GetTabs(indent) + word + " " + next_word + " ");
			}
			else if (lastWord.ToLower() == "join" && word == "(")
			{
				sb.Append(word + " ");
			}
			else if (word == "(")
			{
				inFunction++;
				sb.Append(word + " ");
			}
			else if (word == ")")
			{
				inFunction--;
				if (inFunctionSelect != 0 && inFunction == 0  && inSelect == inFrom)
				{
					inFunctionSelect--;
					indent--;
					sb.Append("\r\n" + GetTabs(indent) + word + " ");
				}
				else
				{
					sb.Append(word + " ");
				}
			}
			else if ((word.ToLower() == "and" || word.ToLower() == "or") && inSelect == inFrom)
			{
				sb.Append("\r\n" + GetTabs(indent) + word + " ");
			}
			else if (word == ";")
			{
				inSelect = 0;
				inFrom = 0;
				indent = 0;
				inFunction = 0;
				inFunctionSelect = 0;
				lastWord = "";
				sb.Append(word + "\r\n" + GetTabs(1));
			}
			else if (joins.Contains(word.ToLower()))
			{
				sb.Append("\r\n" + GetTabs(indent) + word + " ");
			}
			else
			{
				sb.Append(word + " ");
			}
			lastWord = word;
		}
		return sb.ToString();
	}

	private static string GetTabs(int indent)
	{
		return new string('\t', indent);

	}
}