namespace PDFapi.Resources
{
    internal static class Validation
    {
        /// <summary>
        /// Check if a page range is valid
        /// </summary>
        /// <param name="pageRanges"></param>
        /// <returns></returns>
        internal static bool IsValidPageRanges(string pageRanges)
        {
            int startPage = 0;
            int finishPage = 0;

            // Remove all whitespace from the string
            pageRanges = pageRanges.Trim().Replace(" ", "");

            // Check if page ranges is default blank string
            if (pageRanges.Length == 0)
            {
                return true;
            }

            // Split string into page ranges
            var pageRangesArray = pageRanges.Split(',');

            foreach (string pageRange in pageRangesArray)
            {
                // Check for individual page in page range
                if (pageRange.Length == 1)
                {
                    // Check if page is an int and is >= 0
                    if (!int.TryParse(pageRange, out startPage) && startPage >= 0)
                    {
                        return false;
                    }
                    else if (pageRangesArray.Length == 1)
                    {
                        return true;
                    }
                    // Individual page range verified so move onto the next page range
                    break;
                }

                // Check page range
                var pages = pageRange.Split('-');
                if (pages == null)
                {
                    break;
                }
                if (pages.Length != 2)
                {
                    return false;
                }

                // Check if start page is an int and is >= 0
                if (!(int.TryParse(pages[0], out startPage)) && startPage >= 0)
                {
                    return false;
                }

                // Check if finish page is an int and is >= 0
                if (!(int.TryParse(pages[1], out finishPage)) && finishPage >= 0)
                {
                    return false;
                }

                // Check if start page is before finish page
                if (startPage > finishPage)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
