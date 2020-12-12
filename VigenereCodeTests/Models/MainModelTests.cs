using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace VigenereCode.Models.Tests
{
    [TestClass()]
    public class MainModelTests
    {
        [TestMethod()]
        public void AdjustKeyToInputTextTest1()
        {
            string inputText = "привет";
            string key = "скорпион";
            string expected = "скорпи";
            char[] adjustedkey = MainModel.AdjustKeyToInputText(inputText, key);
            string actual = new string(adjustedkey);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void AdjustKeyToInputTextTest2()
        {
            string inputText = "Зима winter, 2020, холодно!";
            string key = "скорпион";
            string expected = "скорпионско";
            char[] adjustedkey = MainModel.AdjustKeyToInputText(inputText, key);
            string actual = new string(adjustedkey);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ConvertTest1()
        {
            string inputText = "Америка";
            string key = "США";
            string expected = "Сеевбкс";
            string actual = MainModel.Convert(inputText, key, MainModel.Operations.Encrypt);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ConvertTest2()
        {
            string inputText = "Юиёнт БЩВ ъгтхмлж к Lamborgini ц пэобрьштл !\"№;%:?*()_ +";
            string key = "машина";
            string expected = "Синее БМВ въехало в Lamborgini и произошёл !\"№;%:?*()_ +";
            string actual = MainModel.Convert(inputText, key, MainModel.Operations.Decrypt);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ConvertTest3()
        {
            string inputText = null;
            string key = "машина";
            Assert.ThrowsException<ArgumentNullException>(() => MainModel.Convert(inputText, key, MainModel.Operations.Decrypt));
        }

        [TestMethod()]
        public void ConvertTest4()
        {
            string inputText = string.Empty;
            string key = "машина";
            string expected = string.Empty;
            string actual = MainModel.Convert(inputText, key, MainModel.Operations.Decrypt);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ConvertTest5()
        {
            string inputText = "Юиёнт БЩВ ъгтхмлж к Lamborgini ц пэобрьштл !\"№;%:?*()_ +";
            string key = string.Empty;
            Assert.ThrowsException<ArgumentException>(() => MainModel.Convert(inputText, key, MainModel.Operations.Decrypt));
        }

        [TestMethod()]
        public void ConvertTest6()
        {
            string inputText = "Д";
            string key = "б";
            string expected = "Г";
            string actual = MainModel.Convert(inputText, key, MainModel.Operations.Decrypt);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ContainsOnlyRussianLettersTest1()
        {
            string s = "ТолькоРусскиеБуквы";
            bool expected = true;
            bool actual = MainModel.ContainsOnlyRussianLetters(s);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ContainsOnlyRussianLettersTest2()
        {
            string s = "EnglishLetters";
            bool expected = false;
            bool actual = MainModel.ContainsOnlyRussianLetters(s);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ContainsOnlyRussianLettersTest3()
        {
            string s = "Рашин буквс с пробелами и символы ;:(№);_%  ";
            bool expected = false;
            bool actual = MainModel.ContainsOnlyRussianLetters(s);

            Assert.AreEqual(expected, actual);
        }
    }
}