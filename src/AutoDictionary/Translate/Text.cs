using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using SitecoreFundamentals.AutoDictionary.EventHandlers;
using SitecoreFundamentals.AutoDictionary.Models;
using System.Linq;
using System.Web;

namespace SitecoreFundamentals.AutoDictionary
{
    public static class Translate
    {
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
                var dictionaryReportItems = new Lists.DictionaryReportItems();

                var itemPath = createPath + itemName;
                var saveToMaster = false;

                if (!dictionaryReportItems.GetAll().Any(x => x.Path.ToLowerInvariant() == itemPath.ToLowerInvariant() && x.LanguageName == Sitecore.Context.Language.Name))
                {
                    dictionaryReportItems.Add(new DictionaryReportItem()
                    {
                        Path = itemPath,
                        LanguageName = Sitecore.Context.Language.Name
                    });

                    saveToMaster = true;
                }

                if (saveToMaster)
                {
                    Log.Info($"{typeof(Translate).FullName}.{nameof(Text)} => Cannot find item \"{itemName}\" in path \"{createPath}\". Returning default value of {defaultValue} and raising creation event.", $"{typeof(Translate).FullName}.{nameof(Text)}");

                    AddItemSaveEvent evt = new AddItemSaveEvent();

                    evt.DictionaryDomain = dictionaryDomain;
                    evt.Path = itemPath;
                    evt.Key = key;
                    evt.Phrase = defaultValue;
                    evt.ContextUrl = HttpContext.Current?.Request.Url.AbsoluteUri;
                    evt.Language = Sitecore.Context.Language;

                    Sitecore.Configuration.Factory.GetDatabase("web").RemoteEvents.EventQueue.QueueEvent<AddItemSaveEvent>(evt, true, true);
                }

                if (string.IsNullOrWhiteSpace(defaultValue))
                    return itemName;
            }

            return defaultValue;
        }
    }
}