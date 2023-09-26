# PDF-API
An API Web Application to convert HTML to PDF based on the Open-Source Selenium engine 

The PDF-API uses the Selenium WebDriver(see https://www.selenium.dev/documentation/overview/#webdriver) to convert html page(s) to pdf.
It specifically uses the chromeDriver version of the WebDriver to do the conversion.

This simply uses the chromeDriver print functionality, to convert the html page, to be saved as a pdf. This is similiar to pressing the Alt-P on a html page in Chrome 
and selecting Save as PDF as the Destination.

The API can use the Chrome command line options (see https://peter.sh/experiments/chromium-command-line-switches). It also can use the Chrome print options (see https://chromedevtools.github.io/devtools-protocol/tot/Page/#method-printToPDF). Both options can be used as parameters to the API.

For more specific details of the API(see https://github.com/CSOIreland/PDF-API/wiki/API).
