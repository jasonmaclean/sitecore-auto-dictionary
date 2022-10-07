using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using SitecoreFundamentals.AutoDictionary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace SitecoreFundamentals.AutoDictionary.Tasks
{
    public class CreationReport
    {
        internal static List<DictionaryReportItem> DictionaryReportItems = new List<DictionaryReportItem>();

        public void Run()
        {
            var dictionaryReportItems = DictionaryReportItems.ToList();

            if (!dictionaryReportItems.Any())
                return;

            Database masterDb = Sitecore.Configuration.Factory.GetDatabase("master");

            var settingsItem = masterDb.GetItem(Constants.ItemIDs.Settings);

            if (settingsItem == null)
            {
                Log.Warn($"{typeof(CreationReport).FullName}.{nameof(Run)} => Settings item ({Constants.ItemIDs.Settings}) not found.", this);
                return;
            }

            var sb = new StringBuilder();

            foreach (var reportItem in dictionaryReportItems)
            {
                sb.Append($"<strong>{Settings.GetSetting("SitecoreFundamentals.AutoDictionary.CreationReportEmail.NewItem")}:</strong> {reportItem.Path}<br />");
                sb.Append($"<strong>{Settings.GetSetting("SitecoreFundamentals.AutoDictionary.CreationReportEmail.Key")}:</strong> {reportItem.Key}<br />");
                sb.Append($"<strong>{Settings.GetSetting("SitecoreFundamentals.AutoDictionary.CreationReportEmail.ValueText")}:</strong> {reportItem.DefaultValue}<br />");
                sb.Append($"<strong>{Settings.GetSetting("SitecoreFundamentals.AutoDictionary.CreationReportEmail.Language")}:</strong> {reportItem.LanguageName}<br />");
                sb.Append($"<strong>{Settings.GetSetting("SitecoreFundamentals.AutoDictionary.CreationReportEmail.PageUrl")}:</strong> <a href=\"{reportItem.ContextUrl}\">{reportItem.ContextUrl}</a><br />");

                if (reportItem != dictionaryReportItems.Last())
                    sb.Append("<br />");

                DictionaryReportItems.Remove(reportItem);
            }

            if (settingsItem.Fields[Constants.Templates.Settings.Fields.SendEmailNotifications].Value != "1")
            {
                Log.Info($"{typeof(CreationReport).FullName}.{nameof(Run)} => Dictionary creation emails are disabled. Nothing will be sent.", this);
                return;
            }

            var emailFrom = settingsItem.Fields[Constants.Templates.Settings.Fields.EmailFrom].Value;
            var emailTo = settingsItem.Fields[Constants.Templates.Settings.Fields.EmailTo].Value;
            var emailBcc = settingsItem.Fields[Constants.Templates.Settings.Fields.EmailBcc].Value;
            var subject = settingsItem.Fields[Constants.Templates.Settings.Fields.Subject].Value;
            var beginningOfEmail = settingsItem.Fields[Constants.Templates.Settings.Fields.BeginningOfEmail].Value;
            var endOfEmail = settingsItem.Fields[Constants.Templates.Settings.Fields.EndOfEmail].Value;

            if (string.IsNullOrWhiteSpace(emailFrom) || string.IsNullOrWhiteSpace(emailFrom))
            {
                Log.Warn($"{typeof(CreationReport).FullName}.{nameof(Run)} => Dictionary creation emails are enabled but the From or To address is empty.", this);
                return;
            }

            var message = new MailMessage();

            message.From = new MailAddress(emailFrom);

            if (!emailTo.Contains(","))
            {
                message.To.Add(new MailAddress(emailTo));
            }
            else
            {
                foreach (string email in emailTo.Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(email))
                        message.To.Add(new MailAddress(email.Trim()));
                }
            }

            if (!string.IsNullOrWhiteSpace(emailBcc))
            {
                if (!emailBcc.Contains(","))
                {
                    message.To.Add(new MailAddress(emailBcc));
                }
                else
                {
                    foreach (string email in emailBcc.Split(','))
                    {
                        if (!string.IsNullOrWhiteSpace(email))
                            message.To.Add(new MailAddress(email.Trim()));
                    }
                }
            }

            message.Subject = subject;
            message.Body = $"{beginningOfEmail}{sb}{endOfEmail}";
            message.IsBodyHtml = true;

            Log.Info($"{typeof(CreationReport).FullName}.{nameof(Run)} => Creation email sent to {emailTo} (Bcc: {emailBcc}).", this);

            try
            {
                MainUtil.SendMail(message);
            }
            catch(Exception ex)
            {
                Log.Error($"{typeof(CreationReport).FullName}.{nameof(Run)} => {ex}", this);
            }
        }
    }
}