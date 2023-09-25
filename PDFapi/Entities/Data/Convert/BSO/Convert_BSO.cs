using API;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using PDFapi.Resources;
using PDFapi.Security;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Web;
//using WebDriverManager;
//using WebDriverManager.DriverConfigs.Impl;
using C = System.Convert;

namespace PDFapi.Data.BSO
{
    public class Convert_BSO : IConvert_BSO
    {
        private const string WINDOW_WIDTH = "windowWidth";
        private const string WINDOW_HEIGHT = "windowHeight";
        private const string CREATE = "Create";
        private const string DATA = "data";
        private const string BODY = "body";
        private const string HEADER = "header";
        private const string FOOTER = "footer";

        /// <summary>
        /// Creates a PDF from a url or merges multiple PDFs from a list of urls
        /// using Chrome command line and print options
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="request"></param>
        /// <returns>Base64 encoded string or byte[] containing a PDF</returns>
        public dynamic Create(JSONRPC_API request, Convert_DTO_Create dto)
        {
            dynamic pdf = null;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Log.Instance.Debug("Starting Create");

            // Get url text to create SHA256 code
            using (WebClient client = new WebClient())
            {
                string urlText = client.DownloadString(dto.Urls[0]);
                dto.SHA512Code = GetSHA512(urlText);
            }

            // Do not use cache if merging PDF files
            if (dto.Urls.Count == 1)
            {
                // Use copy constructor to create a cacheDto that is unique for the url
                var cacheDto = new Convert_DTO_Create(dto, dto.Urls[0]);

                // Check if this request has cached data
                MemCachedD_Value cache = MemCacheD.Get_BSO<dynamic>("PDFapi.Data", "Convert", CREATE, cacheDto);
                if (cache.hasData)
                {
                    Log.Instance.Debug("Finishing Create");
                    stopWatch.Stop();
                    TimeSpan tsCache = stopWatch.Elapsed;
                    Log.Instance.Debug($"Run time for Create cached is {tsCache.TotalMilliseconds}ms");
                    return cache.data.Value;
                }
            }

            // Convert printOptions to a dictionary
            var json = JsonConvert.SerializeObject(dto.PrintOptions);
            var printOptionsDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            // Add Chrome command line options to the driver
            var driverOptions = new ChromeOptions();
            if (dto.ChromeCommandLineOptions != null)
            {
                foreach (var option in dto.ChromeCommandLineOptions)
                {
                    driverOptions.AddArgument(option);
                }
            }

            // Get AppData directory for example:
            // C:\Users\chapmand\AppData\Local\Temp\Temporary ASP.NET Files\vs\9571f56f\1d66a971\assembly\dl3\7a84530d\11454afc_50c4d901
            //string appDataDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Instantiating a new DriverManager does two things:
            // 1. It checks for the latest version of the WebDriver binary file
            // 2. It downloads the binary WebDriver, to the appDataDirectory, if it is not present in your system
            //new DriverManager(appDataDirectory).SetUpDriver(new ChromeConfig());

            // Instantiate the Chrome driver with using so that it will be disposed off correctly 
            // when the conversion is finished
            using (var driver = new ChromeDriver(driverOptions))
            {
                try
                {
                    var pdfDocument = new PdfDocument();
                    for (var i = 0; i < dto.Urls.Count; i++)
                    {
                        // Process the creation of the PDF
                        pdf = Process(dto, printOptionsDictionary, driver, dto.Urls[i], i);

                        // Check if creating rather than merging
                        if (dto.Urls.Count == 1)
                        {
                            Log.Instance.Debug("Finishing Create");
                            stopWatch.Stop();
                            Log.Instance.Debug($"Run time for Create is {stopWatch.Elapsed.TotalMilliseconds}ms");
                            return pdf;
                        }

                        // Convert pdf to a byte[] to pass to the MemoryStream constructor
                        if (pdf.GetType().Name.Equals(Constants.STRING))
                        {
                            pdf = C.FromBase64String(pdf);
                        }
                        // Read the PDF from a memory stream
                        PdfDocument inputPDFDocument = PdfReader.Open(new MemoryStream(pdf), PdfDocumentOpenMode.Import);
                        foreach (PdfPage page in inputPDFDocument.Pages)
                        {
                            pdfDocument.AddPage(page);
                        }
                    }

                    // Save PDF document as a memory stream
                    var stream = new MemoryStream();
                    pdfDocument.Save(stream, false);

                    Log.Instance.Debug("Finishing Merge");
                    stopWatch.Stop();
                    Log.Instance.Debug($"Run time for Merge is {stopWatch.Elapsed.TotalMilliseconds}ms");

                    // Return Base64 string or byte[]
                    switch (dto.ReturnType)
                    {
                        case Constants.BTYE_ARRAY:
                            return stream.ToArray();
                        case Constants.BASE_64_STRING:
                        default:
                            return C.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length);
                    }
                }
                catch (Exception e)
                {
                    Log.Instance.Error(e);
                    throw e;
                }
                finally
                {
                    // Close and quit the driver
                    if (driver != null)
                    {
                        driver.Close();
                        driver.Quit();
                    }
                }
            }
        }

        /// <summary>
        /// Process the conversion of a url to a PDF
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="printOptions"></param>
        /// <param name="driver"></param>
        /// <param name="url"></param>
        /// <returns>Base64 encoded string or byte[], containing a PDF</returns>
        private dynamic Process(Convert_DTO_Create dto, Dictionary<string, object> printOptions, ChromeDriver driver, string url, int index)
        {
            // Use copy constructor to create a cacheDto that is unique for the url
            var cacheDto = new Convert_DTO_Create(dto, url);

            // Check if this url has cached data
            MemCachedD_Value cache = MemCacheD.Get_BSO<dynamic>("PDFapi.Data", "Convert", CREATE, cacheDto);
            if (cache.hasData)
            {
                return cache.data.Value;
            }
            dynamic data = null;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Log.Instance.Debug("Starting Process");

            // Set up the window size for the Chrome driver
            var width = C.ToInt32(printOptions[WINDOW_WIDTH]);
            var height = C.ToInt32(printOptions[WINDOW_HEIGHT]);
            driver.Manage().Window.Size = new Size(width, height);
            driver.Url = url;

            // Wait for page to load
            bool waiting = true;
            long maxWaitTime = Convert.ToInt32(Utility.GetCustomConfig("APP_MAX_WAIT_TIME"));
            int waitTime = Convert.ToInt32(Utility.GetCustomConfig("APP_WAIT_TIME"));
            long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            string method = Utility.GetCustomConfig("METHOD_FOR_HIGH_CHART_FIX");

            object result = null;
            try
            {
                result = driver.ExecuteScript(method);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
            }

            if (result != null)
            {
                while (waiting && ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) < startTime + maxWaitTime))
                {
                    string previousState = driver.PageSource;
                    Thread.Sleep(waitTime);
                    if (previousState.Contains("export2pdf_completed"))
                    {
                        waiting = false;
                    }
                    else
                    {
                        Log.Instance.Debug("Waiting for export2pdf page to load...");
                    }
                }
            }
            else
            {
                Log.Instance.Debug("Javascript method " + method + " is not found in the page source of the url");
                while (waiting && ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) < startTime + maxWaitTime))
                {
                    string previousState = driver.PageSource;
                    Thread.Sleep(waitTime);
                    if (previousState.Equals(driver.PageSource))
                    {
                        waiting = false;
                    }
                    else
                    {
                        Log.Instance.Debug("Waiting for page to load...");
                    }
                }
            }

            if (dto.HtmlIdsToStrip.Count > 0)
            {
                foreach (string id in dto.HtmlIdsToStrip)
                {
                    ProcessInnerHTML(dto, driver, url, index, id);
                }
            }

            var printOutput = driver.ExecuteCdpCommand("Page.printToPDF", printOptions) as Dictionary<string, object>;

            // Return Base64 string or byte[]
            switch (dto.ReturnType)
            {
                case Constants.BTYE_ARRAY:
                    data = C.FromBase64String(printOutput[DATA] as string);
                    break;
                case Constants.BASE_64_STRING:
                default:
                    data = printOutput[DATA];
                    break;
            }

            // Store the data in the cache
            MemCacheD.Store_BSO<dynamic>("PDFapi.Data", "Convert", CREATE, cacheDto, data, default(DateTime));
            Log.Instance.Debug("Finishing Process");
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Log.Instance.Debug($"Run time for Process is {ts.TotalMilliseconds}ms");
            return data;
        }

        /// <summary>
        /// Process the inner HTML for the url
        /// This method uses the dto fields HtmlIdForMasterHeader and HtmlIdForMasterFooter and the id that is passed 
        /// into the method from a list of ids in the dto field HtmlIdsToStrip. For example,
        /// For
        /// dto.Urls = new List<string>{ "https://data.ie", "https://test-data.ie"}
        /// dto.HtmlIdsToStrip = new List<string>() { "header", "nav", "footer" }
        /// dto.HtmlIdForMasterHeader = "header"
        /// dto.HtmlIdForMasterHeader = "footer"
        /// 
        /// This method will not strip the header for "https://data.ie" or the footer for https://test-data.ie.
        /// This method will strip the footer for "https://data.ie" and the header for "https://test-data.ie
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="driver"></param>
        /// <param name="url"></param>
        /// <param name="index"></param>
        /// <param name="id"></param>
        private static void ProcessInnerHTML(Convert_DTO_Create dto, ChromeDriver driver, string url, int index, string id)
        {
            IWebElement webElement = FindElementById(id, driver, url);
            if (webElement != null)
            {

                if (id.Equals(dto.HtmlIdForMasterHeader) && index == 0)
                {
                    Log.Instance.Debug($"Skipping the master header stripping for the element with id {dto.HtmlIdForMasterHeader} " +
                        $"as we are processing the first page");
                }
                else if (id.Equals(dto.HtmlIdForMasterFooter) && index == dto.Urls.Count - 1)
                {
                    Log.Instance.Debug($"Skipping the master footer stripping for the element with id {dto.HtmlIdForMasterFooter} " +
                        $"as we are processing the last page");
                }
                else
                {
                    Log.Instance.Debug($"Executing script to strip {id} innerHTML: document.getElementById('{id}').innerHTML='';");
                    driver.ExecuteScript($"document.getElementById('{id}').innerHTML='';");
                }
            }
            else
            {
                Log.Instance.Debug($"The {id} element was not found in {url}");
                throw new HttpRequestValidationException($"The {id} element was not found in {url}");
            }
        }

        private static IWebElement FindElementByTagName(string name, ChromeDriver driver, string url)
        {
            IWebElement element = null;
            try
            {
                element = driver.FindElement(By.TagName(name));
            }
            catch (NoSuchElementException)
            {
                Log.Instance.Info($"The {name} tag was not found in {url}");
            }
            return element;
        }

        private static IWebElement FindElementById(string name, ChromeDriver driver, string url)
        {
            IWebElement element = null;
            try
            {
                element = driver.FindElement(By.Id(name));
            }
            catch (NoSuchElementException)
            {
                Log.Instance.Info($"The {name} Id was not found in {url}");
            }
            return element;
        }

        /// <summary>
        /// Generate the SHA512 hash of the input parameter
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetSHA512(string input)
        {
            Log.Instance.Info("Generate SHA512 hash");
            Log.Instance.Info("Input string: " + input);

            // Create a SHA512   
            using (SHA512 sha512Hash = SHA512.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                string hashSHA512 = builder.ToString();

                Log.Instance.Info("Output hash: " + hashSHA512);
                return hashSHA512;
            }
        }
    }
}
