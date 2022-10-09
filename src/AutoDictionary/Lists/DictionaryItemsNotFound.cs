using SitecoreFundamentals.AutoDictionary.Models;
using System.Collections.Generic;
using System.Linq;

namespace SitecoreFundamentals.AutoDictionary.Lists
{
    public class DictionaryItemsNotFound
    {
        private static List<DictionaryReportItem> _dictionaryItemsNotFound = new List<DictionaryReportItem>();
        private object _sync = new object();

        public List<DictionaryReportItem> GetAll()
        {
            lock (_sync)
            {
                return _dictionaryItemsNotFound.ToList();
            }
        }

        public void Add(DictionaryReportItem value)
        {
            lock (_sync)
            {
                _dictionaryItemsNotFound.Add(value);
            }
        }
        public void Remove(DictionaryReportItem value)
        {
            lock (_sync)
            {
                _dictionaryItemsNotFound.Remove(value);
            }
        }
    }
}