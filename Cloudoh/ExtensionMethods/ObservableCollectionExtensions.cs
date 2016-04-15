using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Cloudoh.ExtensionMethods
{
    public static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> currentCollection, IEnumerable<T> newItems)
        {
            foreach (var item in newItems)
                currentCollection.Add(item);
        }
    }
}
