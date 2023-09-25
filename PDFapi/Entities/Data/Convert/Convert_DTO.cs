using Newtonsoft.Json;
using PDFapi.Resources;
using PDFapi.Security;
using System.Collections.Generic;
using System.Linq;

namespace PDFapi.Data
{

    public class Convert_DTO_Create
    {
        /// <summary>
        /// url
        /// </summary>
        public List<string> Urls { get; set; }

        /// <summary>
        /// Chrome command line options list
        /// See https://peter.sh/experiments/chromium-command-line-switches/
        /// </summary>
        public List<string> ChromeCommandLineOptions { get; set; }

        /// <summary>
        /// Print optionoption
        /// </summary>
        public PrintOptions PrintOptions { get; set; }

        /// <summary>
        /// returnType
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// SHA512 code
        /// </summary>
        public string SHA512Code { get; set; }

        /// <summary>
        /// HTML Ids to cut
        /// </summary>
        public List<string> HtmlIdsToStrip { get; set; }

        /// <summary>
        /// HTML for master header
        /// </summary>
        public string HtmlIdForMasterHeader { get; set; }

        /// <summary>
        /// HTML for master footer
        /// </summary>
        public string HtmlIdForMasterFooter { get; set; }



        private void AddDefaultChromeCommandLineOptions()
        {
            if (ChromeCommandLineOptions == null)
            {
                ChromeCommandLineOptions = new List<string>();
            }
            var defaults = new List<string>(Configuration_BSO.GetCustomConfig(ConfigType.server, Constants.CHROME_COMMAND_LINE_OPTIONS));

            // Concatenate ChromeCommandLineOptions with defaults
            ChromeCommandLineOptions = ChromeCommandLineOptions.Union(defaults).ToList();
        }

        public Convert_DTO_Create()
        {

        }

        public Convert_DTO_Create(dynamic parameters)
        {
            if (parameters.urls != null)
            {
                Urls = JsonConvert.DeserializeObject<List<string>>(parameters.urls.ToString());
            }
            if (parameters.chromeCommandLineOptions != null)
            {
                ChromeCommandLineOptions = JsonConvert.DeserializeObject<List<string>>(parameters.chromeCommandLineOptions.ToString());
            }
            AddDefaultChromeCommandLineOptions();

            if (parameters.printOptions != null)
            {
                PrintOptions = JsonConvert.DeserializeObject<PrintOptions>(parameters.printOptions.ToString());
            }
            else
            {
                PrintOptions = new PrintOptions();
            }
            if (parameters.returnType != null)
            {
                ReturnType = parameters.returnType;
            }
            else
            {
                ReturnType = Constants.BTYE_ARRAY;
            }

            if (parameters.htmlIdsToCut != null)
            {
                HtmlIdsToStrip = JsonConvert.DeserializeObject<List<string>>(parameters.htmlIdsToCut.ToString());
            }
            else
            {
                HtmlIdsToStrip = new List<string>();
            }

            if (parameters.htmlIdForMasterHeader != null)
            {
                HtmlIdForMasterHeader = parameters.htmlIdForMasterHeader;
            }
            else
            {
                HtmlIdForMasterHeader = "";
            }

            if (parameters.htmlIdForMasterFooter != null)
            {
                HtmlIdForMasterFooter = parameters.htmlIdForMasterFooter;
            }
            else
            {
                HtmlIdForMasterFooter = "";
            }

            // For merge testing purposes to see results using REST API
            // Add values for
            // HtmlIdsToStrip 
            // HtmlIdForMasterHeader
            // HtmlIdForMasterFooter
            // For example
            // List<string> ids = new List<string>() { "header", "footer" };
            // HtmlIdsToStrip = ids;
            // HtmlIdForMasterHeader = "header";
            // HtmlIdForMasterFooter = "footer";
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="url"></param>
        public Convert_DTO_Create(Convert_DTO_Create dto, string url)
        {
            ChromeCommandLineOptions = dto.ChromeCommandLineOptions;
            HtmlIdForMasterHeader = dto.HtmlIdForMasterHeader;
            HtmlIdForMasterFooter = dto.HtmlIdForMasterFooter;
            HtmlIdsToStrip = dto.HtmlIdsToStrip;
            ReturnType = dto.ReturnType;
            PrintOptions = dto.PrintOptions;
            SHA512Code = dto.SHA512Code;
            this.Urls = new List<string>
            {
                url
            };
        }
    }

    /// <summary>
    /// See  https://chromedevtools.github.io/devtools-protocol/tot/Page/#method-printToPDF
    /// </summary>
    public class PrintOptions
    {
        /// <summary>
        /// Landscape
        /// </summary>
        public bool Landscape { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.landscape");

        /// <summary>
        /// DisplayHeaderFooter
        /// </summary>
        public bool DisplayHeaderFooter { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.displayHeaderFooter");

        /// <summary>
        /// PrintBackground
        /// </summary>
        public bool PrintBackground { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.printBackground");

        /// <summary>
        /// Scale
        /// </summary>
        public double Scale { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.scale");

        /// <summary>
        /// Paper width
        /// </summary>
        public double PaperWidth { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.paperWidth");

        /// <summary>
        /// Paper height
        /// </summary>
        public double PaperHeight { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.paperHeight");

        /// <summary>
        /// Margin top
        /// </summary>
        public double MarginTop { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.marginTop");

        /// <summary>
        /// Margin bottom
        /// </summary>
        public double MarginBottom { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.marginBottom");

        /// <summary>
        /// Margin left
        /// </summary>
        public double MarginLeft { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.marginLeft");

        /// <summary>
        /// Margin right
        /// </summary>
        public double MarginRight { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.marginRight");

        /// <summary>
        /// Page Ranges
        /// </summary>
        public string PageRanges { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.pageRanges");

        /// <summary>
        /// Ignore invalid page ranges
        /// </summary>
        public bool IgnoreInvalidPageRanges { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.ignoreInvalidPageRanges");

        /// <summary>
        /// Header template
        /// </summary>
        [NoHtmlStrip]
        public string HeaderTemplate { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.headerTemplate");

        /// <summary>
        /// Footer template
        /// </summary>
        [NoHtmlStrip]
        public string FooterTemplate { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.footerTemplate");

        /// <summary>
        /// Prefer CSS page size
        /// </summary>
        public bool PreferCSSPageSize { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.preferCSSPageSize");

        /// <summary>
        /// Window width
        /// </summary>
        public int WindowWidth { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.windowWidth");

        /// <summary>
        /// Window height
        /// /// </summary>
        public int WindowHeight { get; set; } = Configuration_BSO.GetCustomConfig(ConfigType.server, "printOptions.windowHeight");
    }
}
