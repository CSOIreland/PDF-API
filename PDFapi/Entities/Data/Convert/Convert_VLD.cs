using API;
using FluentValidation;
using PDFapi.Resources;
using PDFapi.Security;
using System;
using System.Text.RegularExpressions;

namespace PDFapi.Data
{
    internal class Convert_VLD_Create : AbstractValidator<Convert_DTO_Create>
    {
        internal Convert_VLD_Create()
        {
            RuleForEach(x => x.Urls).SetValidator(new UrlValidator());

            // Validation for page ranges print options
            RuleFor(x => Validation.IsValidPageRanges(x.PrintOptions.PageRanges)).Equal(true).WithMessage("page ranges are not valid").WithName("PrintOptionsValidation");
        }
    }

    internal class UrlValidator : AbstractValidator<String>
    {
        public UrlValidator()
        {
            string customDomain = Configuration_BSO.GetCustomConfig(ConfigType.server, "domain");

            // Escape '.' character for regular expression
            var customDomainEscaped = Regex.Escape(customDomain);
            string customDomainRegex = Utility.GetCustomConfig("APP_REGEX_URL_DOMAIN").Replace(Constants.C_DOMAIN_PLACEHOLDER, customDomainEscaped);

            RuleFor(url => Uri.IsWellFormedUriString(url, UriKind.Absolute)).Equal(true).WithMessage("url is not well formed").WithName("UrlValidation");
            RuleFor(url => ((new Uri(url).Scheme).Equals(Uri.UriSchemeHttp) || (new Uri(url).Scheme).Equals(Uri.UriSchemeHttps))).Equal(true).WithMessage("url scheme must be http or https").WithName("UrlValidation");
            RuleFor(url => new Uri(url).Host.Contains(customDomain)).Equal(true).WithMessage("url must contain valid domain").WithName("UrlValidation");
            RuleFor(url => Regex.Match(new Uri(url).Host, customDomainRegex, RegexOptions.IgnoreCase).Success).Equal(true).WithMessage("url must have valid domain").WithName("UrlValidation");
        }
    }
}
