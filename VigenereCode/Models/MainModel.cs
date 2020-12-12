using System;
using System.Linq;

namespace VigenereCode.Models
{
	public static class MainModel
	{
		public static char[] AdjustKeyToInputText(char[] textFromUser, char[] keyFromUser)
		{
			var onlyRussianLettersArray = textFromUser.Where(ch => IsRussianLetter(ch)).ToArray();
			var keyAdjustedToInputString = new char[onlyRussianLettersArray.Length];
			var keyIndexer = 0;

			for (var i = 0; i < onlyRussianLettersArray.Length; i++)
			{
				if (keyIndexer > keyFromUser.Length - 1)
				{
					keyIndexer = 0;
				}

				keyAdjustedToInputString[i] = keyFromUser[keyIndexer];
				keyIndexer++;
			}
			return keyAdjustedToInputString;
		}

		private static char[][] MakeVigenereTable()
		{
			var alphabet = new char[] { 'а', 'б', 'в', 'г', 'д', 'е', 'ё', 'ж', 'з', 'и', 'й', 'к', 'л', 'м', 'н', 'о',
							 'п', 'р', 'с', 'т', 'у', 'ф', 'х', 'ц', 'ч', 'ш', 'щ', 'ъ', 'ы', 'ь', 'э', 'ю', 'я' };

			var vigenereTable = new char[alphabet.Length][];
			vigenereTable[0] = alphabet;

			for (int i = 1; i < alphabet.Length; i++)
			{
				var previousRow = vigenereTable[i - 1];
				var firstLetterFromPreviousRow = vigenereTable[i - 1][0];
				var currentRow = new char[alphabet.Length];
				Array.Copy(previousRow, 1, currentRow, 0, previousRow.Length - 1);
				currentRow[currentRow.Length - 1] = firstLetterFromPreviousRow;
				vigenereTable[i] = currentRow;
			}
			return vigenereTable;
		}

		public static string Convert(char[] textFromUser, char[] keyFromUser, Operations operation)
		{
			char[] adjustedKey = AdjustKeyToInputText(textFromUser, keyFromUser);
			char[][] vigenereTable = MakeVigenereTable();

			char[] convertedText = new char[textFromUser.Length];
			char[] alphabet = vigenereTable[0]; //first row of vigenereTable, where alphabet is
			int keyIndexer = 0;

			for (int i = 0; i < textFromUser.Length; i++)
			{
				if (!char.IsLetter(textFromUser[i]) || !IsRussianLetter(textFromUser[i]))
				{
					convertedText[i] = textFromUser[i]; //skip conversion
				}
				else
				{
					if (operation == Operations.Decrypt)
					{
						int rowIndexer = Array.IndexOf(alphabet, char.ToLower(adjustedKey[keyIndexer]));
						var rowWhereEncryptedLetterExist = vigenereTable[rowIndexer]; //the row corresponding to the letter of the key, in which you need to find the column in which it(the encrypted letter) is located and the header of this column will be the original letter
						int columnIndexer = Array.IndexOf(rowWhereEncryptedLetterExist, char.ToLower(textFromUser[i]));
						convertedText[i] = char.IsUpper(textFromUser[i]) ? char.ToUpper(alphabet[columnIndexer]) : alphabet[columnIndexer];
						keyIndexer++;
					}
					else
					{
						int rowIndexer = Array.IndexOf(alphabet, char.ToLower(adjustedKey[keyIndexer]));
						int columnIndexer = Array.IndexOf(alphabet, char.ToLower(textFromUser[i]));
						convertedText[i] = char.IsUpper(textFromUser[i]) ? char.ToUpper(vigenereTable[rowIndexer][columnIndexer]) : vigenereTable[rowIndexer][columnIndexer];
						keyIndexer++;
					}
				}
			}
			return new string(convertedText);
		}

		public enum Operations
		{
			Decrypt,
			Encrypt
		}

		private static bool IsRussianLetter(char c)
			=> (c >= 'А' && c <= 'я') || c == 'ё' || c == 'Ё';

		public static bool ContainsOnlyRussianLetters(string s)
		{
			foreach (var item in s)
			{
				if (!char.IsLetter(item) || !IsRussianLetter(item))
				{
					return false;
				}
			}
			return true;
		}
	}
}