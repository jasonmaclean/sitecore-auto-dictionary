using Sitecore.Diagnostics;
using Sitecore.Pipelines.GetTranslation;


namespace SitecoreFundamentals.AutoDictionary.Pipelines.GetTranslation
{
    public class AutoCreate
    {
        public void Process(GetTranslationArgs args)
        {
            if (!ShouldSetAsEmpty(args))
                return;

            args.Result = string.Empty;
        }

        protected virtual bool ShouldSetAsEmpty(GetTranslationArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

           if (args.Result != null)
                return false;

            var result = Sitecore.Context.Site.Properties["SitecoreFundamentalsAutoDictionaryEnabled"]?.ToLowerInvariant() == "true";

            return result;
        }
    }
}