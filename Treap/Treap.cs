using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treap
{
    public class Treap<TKey, TValue> where TKey : IComparable<TKey>
    {
        private TreapNode<TKey, TValue>? _root;
        private readonly Random _random;
        private readonly HashSet<int> _usedPriorities;

        public Treap()
        {
            _root = null;
            _random = new Random();
            _usedPriorities = new HashSet<int>();
        }

        protected int GeneratePriority()
        {
            int priority;
            do
            {
                priority = _random.Next();
            }
            while (!_usedPriorities.Add(priority));

            return priority;
        }

        private TreapNode<TKey, TValue> RotateRight(TreapNode<TKey, TValue> y)
        {
            TreapNode<TKey, TValue> x = y.Left!;
            TreapNode<TKey, TValue>? T2 = x.Right;

            x.Right = y;
            y.Left = T2;

            return x;
        }
        private TreapNode<TKey, TValue> RotateLeft(TreapNode<TKey, TValue> x)
        {
            TreapNode<TKey, TValue> y = x.Right!;
            TreapNode<TKey, TValue>? T2 = y.Left;

            y.Left = x;
            x.Right = T2;

            return y;
        }
        public bool Contains(TKey key)
        {
            return SearchNode(_root, key) != null;
        }

        public TValue? GetValue(TKey key)
        {
            var node = SearchNode(_root, key);
            return node != null ? node.Value : default;
        }

        private TreapNode<TKey, TValue>? SearchNode(TreapNode<TKey, TValue>? node, TKey key)
        {
            if (node == null) return null;

            int cmp = key.CompareTo(node.Key);

            if (cmp == 0) return node; 
            if (cmp < 0) return SearchNode(node.Left, key); 

            return SearchNode(node.Right, key);
        }
        public bool Insert(TKey key, TValue value)
        {
            if (Contains(key))
            {
                return false;
            }

            int priority = GeneratePriority(); 
            _root = InsertNode(_root, key, value, priority);
            return true;
        }

        private TreapNode<TKey, TValue> InsertNode(TreapNode<TKey, TValue>? node, TKey key, TValue value, int priority)
        {
            if (node == null)
            {
                return new TreapNode<TKey, TValue>(key, value, priority);
            }

            int cmp = key.CompareTo(node.Key);

            if (cmp < 0)
            {
                node.Left = InsertNode(node.Left, key, value, priority);

                if (node.Left.Priority > node.Priority)
                {
                    node = RotateRight(node);
                }
            }
            else if (cmp > 0)
            {
                node.Right = InsertNode(node.Right, key, value, priority);

                if (node.Right.Priority > node.Priority)
                {
                    node = RotateLeft(node);
                }
            }

            return node; 
        }
        public List<TreapNode<TKey, TValue>> GetElementsInOrder()
        {
            var result = new List<TreapNode<TKey, TValue>>();
            InOrderTraversal(_root, result);
            return result;
        }

        private void InOrderTraversal(TreapNode<TKey, TValue>? node, List<TreapNode<TKey, TValue>> result)
        {
            if (node != null)
            {
                InOrderTraversal(node.Left, result);  
                result.Add(node);                    
                InOrderTraversal(node.Right, result); 
            }
        }

        public TreapNode<TKey, TValue>? Root => _root;
        public bool Delete(TKey key)
        {
            if (!Contains(key))
            {
                return false; 
            }

            _root = DeleteNode(_root, key);
            return true;
        }

        private TreapNode<TKey, TValue>? DeleteNode(TreapNode<TKey, TValue>? node, TKey key)
        {
            if (node == null) return null;

            int cmp = key.CompareTo(node.Key);

            if (cmp < 0)
            {
                node.Left = DeleteNode(node.Left, key);
            }
            else if (cmp > 0)
            {
                node.Right = DeleteNode(node.Right, key);
            }
            else
            {
                if (node.Left == null && node.Right == null)
                {
                    return null; 
                }

                else if (node.Left == null)
                {
                    return node.Right; 
                }
                else if (node.Right == null)
                {
                    return node.Left; 
                }

                else
                {
                    if (node.Left.Priority > node.Right.Priority)
                    {
                        node = RotateRight(node);

                        node.Right = DeleteNode(node.Right, key);
                    }
                    else
                    {
                        node = RotateLeft(node);

                        node.Left = DeleteNode(node.Left, key);
                    }
                }
            }

            return node;
        }
        public TreapNode<TKey, TValue>? GetPredecessor(TKey key)
        {
            TreapNode<TKey, TValue>? current = _root;
            TreapNode<TKey, TValue>? predecessor = null;

            while (current != null)
            {
                if (key.CompareTo(current.Key) > 0)
                {
                    predecessor = current;
                    current = current.Right;
                }
                else
                {
                    current = current.Left;
                }
            }
            return predecessor;
        }

        public TreapNode<TKey, TValue>? GetSuccessor(TKey key)
        {
            TreapNode<TKey, TValue>? current = _root;
            TreapNode<TKey, TValue>? successor = null;

            while (current != null)
            {
                if (key.CompareTo(current.Key) < 0)
                {
                    successor = current;
                    current = current.Left;
                }
                else
                {
                    current = current.Right;
                }
            }
            return successor;
        }
    }
}
