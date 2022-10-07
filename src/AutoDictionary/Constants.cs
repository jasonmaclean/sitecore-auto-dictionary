using Sitecore.Data;

namespace SitecoreFundamentals.AutoDictionary
{
    internal struct Constants
    {
        internal static class ItemPaths
        {
            internal const string DictionaryRoot = "/sitecore/system/dictionary/";
        }

        internal static class ItemIDs
        {
            internal static readonly ID Settings = new ID("{9F358296-CDD3-496E-8B1F-ADFC9FCAD97A}");
        }

        internal static class Templates
        {
            internal static class Dictionary
            {
                internal static readonly ID DictionaryDomain = new ID("{0A2847E6-9885-450B-B61E-F9E6528480EF}");
                internal static readonly ID DictionaryFolder = new ID("{267D9AC7-5D85-4E9D-AF89-99AB296CC218}");
                internal static readonly ID DictionaryEntry = new ID("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}");
            }

            internal static class Settings
            {
                internal static readonly ID ID = new ID("{7A006290-8CAD-4F84-BD2F-564F1D862FA9}");
                internal static class Fields
                {
                    internal const string SendEmailNotifications = "{BC4B03F7-671D-4CEF-98E1-EA50BC08B663}";
                    internal const string EmailFrom = "{0AFA674D-2B90-4234-91E0-08EFA80FAA09}";
                    internal const string EmailTo = "{C6F2B6FB-5A3F-4680-A2BD-BCE37FA5CF80}";
                    internal const string EmailBcc = "{E68C59FA-270A-4D83-BFDA-FADCFEB9C425}";
                    internal const string Subject = "{F8B1054C-CE13-41E1-9269-17A0365DFFF8}";
                    internal const string BeginningOfEmail = "{6438399B-8BC1-4402-8747-1D3C9973BEF3}";
                    internal const string EndOfEmail = "{9BFC5700-B1CE-4D34-8084-4960B476C9B1}";
                }
            }
        }
    }
}