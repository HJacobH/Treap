using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treap
{
    public class TreapNode<TKey, TValue> where TKey : IComparable<TKey>
    {
        public TKey Key { get; set; }   
        public TValue Value { get; set; }   
        public int Priority { get; set; }   

        public TreapNode<TKey, TValue>? Left { get; set; }
        public TreapNode<TKey, TValue>? Right { get; set; }

        public TreapNode(TKey key, TValue value, int priority)
        {
            Key = key;
            Value = value;
            Priority = priority;
        }
    }
}
