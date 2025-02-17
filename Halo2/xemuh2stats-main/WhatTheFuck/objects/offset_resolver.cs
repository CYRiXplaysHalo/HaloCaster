using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xemuh2stats.objects
{
    public class offset_resolver : System.Collections.CollectionBase
    {
        public offset_resolver_item this[int index] =>
            (offset_resolver_item)List[index];

        public offset_resolver_item this[string name] =>
            List.Cast<offset_resolver_item>().FirstOrDefault(offsetResolverItem => offsetResolverItem.name == name);

        public void Add(offset_resolver_item item)
        {
            List.Add(item);
        }
    }
}
