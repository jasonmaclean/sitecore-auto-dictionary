using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Eventing;
using SitecoreFundamentals.AutoDictionary.EventHandlers;
using SitecoreFundamentals.AutoDictionary.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SitecoreFundamentals.AutoDictionary
{
    public static class Translate
    {
        internal static List<DictionaryReportItem> DictionaryItemsNotFound = new List<DictionaryReportItem>();

        public static string Text(string key)
            => Text(key, "", "");

        public static string Text(string key, string createPath, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(key))
                return "";

            var result = Sitecore.Globalization.Translate.Text(key);

            if (!string.IsNullOrWhiteSpace(result))
                return result;

            var dictionaryDomain = Sitecore.Context.Site.SiteInfo.DictionaryDomain;

            var dictionaryFolder = dictionaryDomain;

            if (!string.IsNullOrWhiteSpace(dictionaryFolder))
                dictionaryFolder = $"{dictionaryFolder}/";

            if (!string.IsNullOrWhiteSpace(createPath) && !createPath.EndsWith("/"))
                createPath = $"{createPath}/";

            createPath = $"{Constants.ItemPaths.DictionaryRoot}{dictionaryFolder}{createPath}";

            var itemName = ItemUtil.ProposeValidItemName(key.Replace(".", " "));

            var item = Sitecore.Context.Database.GetItem($"{createPath}{itemName}");
            if (item == null || item.Versions.Count == 0)
            {
                var itemPath = createPath + itemName;

                if (!DictionaryItemsNotFound.Any(x => x.Path.ToLowerInvariant() == itemPath.ToLowerInvariant() && x.LanguageName == Sitecore.Context.Language.Name))
                {
                    DictionaryItemsNotFound.Add(new DictionaryReportItem()
                    {
                        Path = itemPath,
                        LanguageName = Sitecore.Context.Language.Name
                    });

                    Log.Info($"{typeof(Translate).FullName}.{nameof(Text)} => Cannot find item \"{itemName}\" in path \"{createPath}\". Returning default value of {defaultValue} and raising creation event.", $"{typeof(Translate).FullName}.{nameof(Text)}");

                    AddItemSaveEvent evt = new AddItemSaveEvent();

                    evt.DictionaryDomain = dictionaryDomain;
                    evt.Path = itemPath;
                    evt.Key = key;
                    evt.Phrase = defaultValue;
                    evt.ContextUrl = HttpContext.Current?.Request.Url.AbsoluteUri;
                    evt.Language = Sitecore.Context.Language;

                    EventManager.RaiseEvent(evt);
                }

                if (string.IsNullOrWhiteSpace(defaultValue))
                    return itemName;
            }

            return defaultValue;
        }
    }
}