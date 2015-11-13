using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wired.Caching
{
    /// <summary>
    /// Holds meta details about the cache item
    /// </summary>
    public sealed class CacheItemDetail
    {
        /// <summary>
        /// The time the item was added to the cache.
        /// </summary>
        public DateTime AddedOn { get; set; }

        /// <summary>
        /// How many seconds the item was set to remain in the cache.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// The time the item is due to expire from the cache.
        /// </summary>
        public DateTime ExpiresOn => AddedOn.AddSeconds(Duration);
    }
}
