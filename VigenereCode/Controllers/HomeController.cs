using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

        public async Task<IActionResult> OpenFile(IFormFile file, HomeIndexViewModel homeIndexViewModel)
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
                    homeIndexViewModel.SourceText = await Task.Run(() => ReadTxtFile(file));
                    break;
                case ".docx":
                    homeIndexViewModel.SourceText = await Task.Run(() => ReadDocxFile(file));
                    break;
                default:
                    throw new NotSupportedException();
            }

            ModelState.Clear();
            return View("Index", homeIndexViewModel);
        }

        public async Task<string> ReadTxtFile(IFormFile file)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string textFromFile;
            byte[] arr;

            using (MemoryStream ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
            }

            int symbolsCounter = 0;
            int lettersCount = 0;

            textFromFile = Encoding.UTF8.GetString(arr);

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

        public async Task<string> ReadDocxFile(IFormFile file)
        {
            string textFromFile;

            using (MemoryStream ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);

                using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, false))
                {
                    StringBuilder wordDocumentText = new StringBuilder();
                    IEnumerable<Paragraph> paragraphElements =
                        doc.MainDocumentPart.Document.Body.Descendants<Paragraph>();

                    foreach (Paragraph p in paragraphElements)
                    {
                        IEnumerable<Text> textElements = p.Descendants<Text>();

                        foreach (Text t in textElements)
                        {
                            wordDocumentText.Append(t.Text);
                        }

                        wordDocumentText.AppendLine();
                    }
                    textFromFile = wordDocumentText.ToString();
                }
            }

            return textFromFile.Replace("\n", string.Empty).Replace("\r", string.Empty);
        }

        public async Task<IActionResult> Convert(HomeIndexViewModel homeIndexViewModel, string command)
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

            MainModel.Operations operation;

            if (command == "Encrypt")
            {
                operation = MainModel.Operations.Encrypt;
            }
            else
            {
                operation = MainModel.Operations.Decrypt;
            }

            homeIndexViewModel.Result = await Task.Run(() => MainModel.Convert(homeIndexViewModel.SourceText.ToCharArray(), homeIndexViewModel.Key.ToCharArray(), operation));
            ModelState.Clear();
            return View("Index", homeIndexViewModel);
        }

        public async Task<IActionResult> DownloadFile(HomeIndexViewModel homeIndexViewModel, string command)
        {
            if (string.IsNullOrEmpty(homeIndexViewModel.Result))
            {
                homeIndexViewModel.Warning = "Result is empty!";
                return View("Index", homeIndexViewModel);
            }

            string fileName = string.IsNullOrEmpty(homeIndexViewModel.DownloadFileName) ? "Result" : homeIndexViewModel.DownloadFileName;

            switch (command)
            {
                case "downloadTxtFile":
                    return await Task.Run(() => DownloadTxtFile(homeIndexViewModel.Result, fileName));

                case "downloadDocxFile":
                    return await Task.Run(() => DownloadDocxFile(homeIndexViewModel.Result, fileName));

                default:
                    throw new NotSupportedException();
            }

        }

        public FileContentResult DownloadTxtFile(string text, string fileName)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                return File(ms.ToArray(), "text/plain; charset=utf-8", fileName + ".txt");
            }
        }

        public FileContentResult DownloadDocxFile(string text, string fileName)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(ms, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
                {
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
    }
}
