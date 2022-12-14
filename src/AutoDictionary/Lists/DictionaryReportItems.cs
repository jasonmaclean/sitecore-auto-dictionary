using SitecoreFundamentals.AutoDictionary.Models;
using System.Collections.Generic;
using System.Linq;

namespace SitecoreFundamentals.AutoDictionary.Lists
{
    public class DictionaryReportItems
    {
        private static List<DictionaryReportItem> _dictionaryReportItems = new List<DictionaryReportItem>();
        private object _sync = new object();

        public List<DictionaryReportItem> GetAll()
        {
            lock (_sync)
            {
                return _dictionaryReportItems.ToList();
            }
        }

        public void Add(DictionaryReportItem value)
        {
            lock (_sync)
            {
                _dictionaryReportItems.Add(value);
            }
        }
        public void Remove(DictionaryReportItem value)
        {
            lock (_sync)
            {
                _dictionaryReportItems.Remove(value);
            }
        }
    }
}