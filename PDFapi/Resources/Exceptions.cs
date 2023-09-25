using System;


namespace PDFapi.Resources
{
    internal class UnmatchedParametersException : Exception
    {
        internal UnmatchedParametersException(Exception innerException) : base(Label.Get("unmatched-parameters"), innerException)
        {

        }


    }


}
