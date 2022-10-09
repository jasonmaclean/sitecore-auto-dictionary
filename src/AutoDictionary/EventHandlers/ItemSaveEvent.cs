
using Sitecore.Configuration.KnownSettings;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Eventing;
using Sitecore.Events;
using Sitecore.Events.Hooks;
using Sitecore.Globalization;
using Sitecore.Publishing;
using Sitecore.SecurityModel;
using Sitecore.Web.UI.WebControls.Presentation;
using SitecoreFundamentals.AutoDictionary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SitecoreFundamentals.AutoDictionary.EventHandlers
{
    public class AddItemSaveEventHandler
    {
        public virtual void OnAddItemSaveRemote(object sender, EventArgs e)
        {
            Log.Info($"{typeof(AddItemSaveEventHandler).FullName}.{nameof(OnAddItemSaveRemote)} => Event Queue has picked up a new Item Save Event. Creating or updating item in the Master database content tree now.", this);
            
            if (e is AddItemSaveEventArgs)
            {
                var args = e as AddItemSaveEventArgs;

                if (args == null)
                {
                    Log.Error($"{typeof(AddItemSaveEventHandler).FullName}.{nameof(OnAddItemSaveRemote)} => args == null.", this);
                    return;
                }

                using (new SecurityDisabler())
                {
                    Database masterDb = Sitecore.Configuration.Factory.GetDatabase("master");
                    Database webDb = Sitecore.Configuration.Factory.GetDatabase("web");

                    var itemsInPath = args.SaveItemPath.Split('/');
                    var itemPaths = new List<string>();

                    using (new LanguageSwitcher(args.Language))
                    {
                        // Get all items in the desired path that do not exist in this language
                        for (int i = 0; i < itemsInPath.Count(); i++)
                        {
                            var itemPath = String.Join("/", itemsInPath.Take(itemsInPath.Count() - i).Select(p => p.ToString()).ToArray());

                            if (string.IsNullOrWhiteSpace(itemPath) || $"{itemPath.ToLowerInvariant()}/" == Constants.ItemPaths.DictionaryRoot)
                                break;

                            var item = masterDb.GetItem(itemPath);

                            if (item != null && item.Versions.Count > 0)
                                break;

                            itemPaths.Add(itemPath);
                        }

                        // Work down the path to create it, figuring out what needs to be added or versioned. The last item would be the dictionary with fields in it.
                        if (itemPaths.Any())
                        {
                            itemPaths.Reverse();

                            Sitecore.Data.Items.Item itemToPublish = null;

                            string logAction = ""; 

                            foreach (var itemPath in itemPaths)
                            {
                                var commitFields = false;

                                var itemName = ItemUtil.ProposeValidItemName(itemPath.Split('/').Last().Replace(".", " "));

                                Sitecore.Data.Items.Item parentItem = masterDb.GetItem(itemPath.Replace($"/{itemName}", ""));

                                // Make a folder, unless this is a site root or last item in the collection of item paths.
                                var itemTemplateId = Constants.Templates.Dictionary.DictionaryFolder;
                                if (itemPaths.Last() == itemPath)
                                {
                                    itemTemplateId = Constants.Templates.Dictionary.DictionaryEntry;
                                    commitFields = true;
                                }
                                else if (!string.IsNullOrWhiteSpace(args.DictionaryDomain)
                                    && itemPath == $"{Constants.ItemPaths.DictionaryRoot}{args.DictionaryDomain}".ToLowerInvariant())
                                {
                                    itemTemplateId = Constants.Templates.Dictionary.DictionaryDomain;
                                }

                                TemplateItem itemTemplate = masterDb.GetItem(itemTemplateId);

                                if (itemTemplate == null || parentItem == null)
                                {
                                    Log.Error($"{typeof(AddItemSaveEventHandler).FullName}.{nameof(OnAddItemSaveRemote)} => Parent Item ({$"/{itemName}"}) or Save Item Template ({itemTemplateId}) not found in Master database. Aborting", this);
                                    return;
                                }

                                try
                                {
                                    var dictionaryReportItems = new Lists.DictionaryReportItems();

                                    var saveItem = masterDb.GetItem(itemPath);

                                    if (saveItem == null)
                                    {
                                        logAction = "been saved into";
                                        saveItem = parentItem.Add(itemName, itemTemplate);
                                    }
                                    else
                                    {
                                        logAction = $"had the {args.Language.Name} version added to";
                                        saveItem.Versions.AddVersion();
                                    }

                                    if (commitFields)
                                    {
                                        saveItem.Editing.BeginEdit();
                                        saveItem.Fields["key"].Value = args.Key;
                                        saveItem.Fields["phrase"].Value = !string.IsNullOrEmpty(args.Phrase) ? args.Phrase : itemName;
                                        saveItem.Editing.EndEdit();
                                    }

                                    if (itemToPublish == null)
                                        itemToPublish = saveItem;

                                    if (saveItem.TemplateID == Constants.Templates.Dictionary.DictionaryEntry 
                                        && !dictionaryReportItems.GetAll().Any(x => x.Path.ToLowerInvariant() == itemPath.ToLowerInvariant() 
                                        && x.LanguageName == args.Language.Name))
                                    {
                                        var reportItem = new DictionaryReportItem()
                                        {
                                            Path = itemPath,
                                            Key = args.Key,
                                            DefaultValue = args.Phrase,
                                            ContextUrl = args.ContextUrl,
                                            LanguageName = args.Language.Name
                                        };

                                        dictionaryReportItems.Add(reportItem);
                                    }

                                    Log.Info($"{typeof(AddItemSaveEventHandler).FullName}.{nameof(OnAddItemSaveRemote)} => Item \"{itemName}\" with ID {saveItem.ID.ToShortID()} has {logAction} the Master database.", this);
                                }
                                catch (Exception ex)
                                {
                                    Log.Error($"{typeof(AddItemSaveEventHandler).FullName}.{nameof(OnAddItemSaveRemote)} => {ex}", this);
                                }
                            }

                            if (itemToPublish != null)
                            {
                                Database[] databases = new Database[1] { webDb };
                                Language[] language = new Language[1] { itemToPublish.Language };

                                Sitecore.Handle publishHandle = PublishManager.PublishItem(itemToPublish, databases, language, true, false);

                                Log.Info($"{typeof(AddItemSaveEventHandler).FullName}.{nameof(OnAddItemSaveRemote)} => Item \"{itemToPublish.Name}\" with ID {itemToPublish.ID.ToShortID()} and its children are being published.", this);
                            }
                        }
                    }
                }
            }
        }

        public static void Run(AddItemSaveEvent evt)
        {
            AddItemSaveEventArgs args = new AddItemSaveEventArgs(evt);
            Event.RaiseEvent("additemsave:remote", new object[] { args });
        }
    }

    public class AddItemSaveEvent
    {
        [DataMember]
        public string DictionaryDomain { get; set; }
        [DataMember]
        public string Path { get; set; }
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string Phrase { get; set; }
        [DataMember]
        public string ContextUrl { get; set; }
        [DataMember]
        public Language Language { get; set; }
    }

    public class AddItemSaveEventArgs : EventArgs, IPassNativeEventArgs
    {
        private AddItemSaveEvent _evt;

        public AddItemSaveEventArgs(AddItemSaveEvent evt)
        {
            _evt = evt;
        }

        public string DictionaryDomain => _evt.DictionaryDomain;
        public string SaveItemPath => _evt.Path;
        public string Key => _evt.Key;
        public string Phrase => _evt.Phrase;
        public string ContextUrl => _evt.ContextUrl;
        public Language Language => _evt.Language;
    }

    public class AddItemSaveHook : IHook
    {
        public void Initialize()
        {
            EventManager.Subscribe<AddItemSaveEvent>(new Action<AddItemSaveEvent>(AddItemSaveEventHandler.Run));

            Log.Info("{Constants.Configuration.BrandName}.{Constants.Configuration.ModuleName}.Initialize => This instance is now subscribed to the AddItemSaveHook", this);
        }
    }
}