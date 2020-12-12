using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

using VigenereCode.Models;
using VigenereCode.ViewModels;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace VigenereCode.Controllers
{
    [ValidateModel]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(new HomeIndexViewModel());
        }

        public IActionResult OpenFile(IFormFile file, HomeIndexViewModel homeIndexViewModel)
        {
            if (file == null)
            {
                homeIndexViewModel.Warning = "No file selected!";
                ModelState.Clear();
                return View("Index", homeIndexViewModel);
            }

            var extension = Path.GetExtension(file.FileName);

            switch (extension)
            {
                case ".txt":
                    homeIndexViewModel.SourceText = ReadTxtFile(file);
                    break;
                case ".docx":
                    homeIndexViewModel.SourceText = ReadDocxFile(file);
                    break;
                default:
                    throw new NotSupportedException();
            }

            ModelState.Clear();
            return View("Index", homeIndexViewModel);
        }

        public string ReadTxtFile(IFormFile file)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using MemoryStream ms = new MemoryStream();
            file.CopyTo(ms);
            var arr = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);

            int symbolsCounter = 0;
            int lettersCount = 0;

            var textFromFile = Encoding.UTF8.GetString(arr);

            foreach (var c in textFromFile)
            {
                if (char.IsSymbol(c))
                {
                    symbolsCounter++;
                }
                else if (char.IsLetter(c))
                {
                    lettersCount++;
                }
            }

            if (symbolsCounter > lettersCount)
            {
                textFromFile = Encoding.GetEncoding("windows-1251").GetString(arr);
            }

            return textFromFile.Replace("\n", string.Empty).Replace("\r", string.Empty);
        }

        public string ReadDocxFile(IFormFile file)
        {
            using MemoryStream ms = new MemoryStream();
            file.CopyTo(ms);

            using WordprocessingDocument doc = WordprocessingDocument.Open(ms, false);
            var wordDocumentText = new StringBuilder();
            var paragraphElements = doc.MainDocumentPart.Document.Body.Descendants<Paragraph>();

            foreach (var p in paragraphElements)
            {
                var textFromFile = string.Join("", p.Descendants<Text>().Select(a => a.Text));
                wordDocumentText.AppendLine(textFromFile);
            }

            return wordDocumentText.Replace("\n", string.Empty).Replace("\r", string.Empty).ToString();
        }

        public IActionResult Convert(HomeIndexViewModel homeIndexViewModel, string command)
        {
            if (string.IsNullOrEmpty(homeIndexViewModel.SourceText))
            {
                homeIndexViewModel.Warning = "The source text is empty!";
                return View("Index", homeIndexViewModel);
            }

            if (string.IsNullOrEmpty(homeIndexViewModel.Key) || !MainModel.ContainsOnlyRussianLetters(homeIndexViewModel.Key.Trim()))
            {
                homeIndexViewModel.Warning = "The key must not be empty, must contain only Russian letters and be without spaces!";
                return View("Index", homeIndexViewModel);
            }

            var operation = command == "Encrypt"
                 ? MainModel.Operations.Encrypt
                 : MainModel.Operations.Decrypt;

            homeIndexViewModel.Result = MainModel.Convert(homeIndexViewModel.SourceText, homeIndexViewModel.Key, operation); ModelState.Clear();
            return View("Index", homeIndexViewModel);
        }

        public IActionResult DownloadFile(HomeIndexViewModel homeIndexViewModel, string command)
        {
            if (string.IsNullOrEmpty(homeIndexViewModel.Result))
            {
                homeIndexViewModel.Warning = "Result is empty!";
                return View("Index", homeIndexViewModel);
            }

            var fileName = string.IsNullOrEmpty(homeIndexViewModel.DownloadFileName)
                ? "Result"
                : homeIndexViewModel.DownloadFileName;

            switch (command)
            {
                case "downloadTxtFile":
                    return DownloadTxtFile(homeIndexViewModel.Result, fileName);

                case "downloadDocxFile":
                    return DownloadDocxFile(homeIndexViewModel.Result, fileName);

                default:
                    throw new NotSupportedException();
            }

        }

        public FileContentResult DownloadTxtFile(string text, string fileName)
        {
            using MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
            return File(ms.ToArray(), "text/plain; charset=utf-8", fileName + ".txt");
        }

        public FileContentResult DownloadDocxFile(string text, string fileName)
        {
            using MemoryStream ms = new MemoryStream();

            using WordprocessingDocument wordDocument = WordprocessingDocument.Create(ms, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);

            MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            Body body = mainPart.Document.AppendChild(new Body());
            Paragraph para = body.AppendChild(new Paragraph());
            Run run = para.AppendChild(new Run());
            run.AppendChild(new Text(text));
            wordDocument.Close();
            ms.Position = 0;

            return File(ms.ToArray(), "application/vnd.openxmlformats", fileName + ".docx");
        }
    }
}

