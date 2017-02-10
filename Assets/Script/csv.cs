using System;
using System.Collections.Generic;

namespace CSV
{
    public class Item
    {
        private object m_Object;
        public object Value
        {
            get
            {
                return m_Object;
            }
        }
        
        public Item(object obj)
        {
            m_Object = obj;
        }

        public T To<T>()
        {
            return (T)m_Object;
        }
    }

    public class Collection
    {
        private Item[] m_Items;
        private Collection m_Header;
        
        public int Length 
        {
            get 
            {
                return m_Items == null ? 0 : m_Items.Length;
            }
        }

        public Collection(Item[] items, Collection header)
        {
            m_Items = items;
            m_Header = header;
        }

        public Item this[int index]
        {
            get
            {
                if (m_Items == null)
                {
                    throw new NullReferenceException();
                }

                if (index >= m_Items.Length)
                {
                    throw new IndexOutOfRangeException();
                }

                return m_Items[index];
            }
        }

        public Item this[string key]
        {
            get
            {
                if (m_Items == null || m_Header == null)
                {
                    throw new NullReferenceException();
                }

                var index = 0;

                for (var i = 0; i < m_Header.Length; ++i)
                {
                    if ((string)(m_Header[i].Value) == key)
                    {
                        index = i;
                        break;
                    }
                }

                if (index >= m_Items.Length)
                {
                    throw new IndexOutOfRangeException();
                }

                return m_Items[index];
            }
        }
    }

    public class Content
    {
        private Collection[] m_Collections;

        public int Length 
        {
            get 
            {
                return m_Collections == null ? 0 : m_Collections.Length;
            }
        }

        public Content(Collection[] collections)
        {
            m_Collections = collections;
        }

        public Collection this[int index]
        {
            get
            {
                if (m_Collections == null)
                {
                    throw new NullReferenceException();
                }

                if (index >= m_Collections.Length)
                {
                    throw new IndexOutOfRangeException();
                }

                return m_Collections[index];
            }
        }
    }

    public static class Parser
    {
        public enum CollectionType
        {
            Data,
            Header,
            Type,
        }

        private static Collection DoCollection(string text, char separator, int beginPos, int endPos, Collection headerCollection, CollectionType collectionType, Collection typeCollection)
        {
            var itemList = new List<Item>();
            var pos = beginPos;
            var quotationCount = 0;

            for (int i = beginPos; i < endPos; ++i)
            {
                var c = text[i];

                quotationCount += (c == '"' ? 1 : 0);
                if (((quotationCount & 0x1) == 0) && (c == separator || i == endPos - 1))
                {
                    var subPos = pos;
                    var subLength = (i - pos + (c == separator ? 0 : 1));

                    if (text[pos] == '"')
                    {
                        subPos++;
                        subLength--;
                    }
                    if (text[subPos + subLength - 1] == '"')
                    {
                        subLength--;
                    }

                    var itemText = text.Substring(subPos, subLength);

                    switch (collectionType)
                    {
                        case CollectionType.Header:
                            {
                                itemList.Add(new Item(itemText));
                            }
                            break;

                        case CollectionType.Type:
                            {
                                var type = Type.GetType(itemText);

                                if (type == null)
                                {
                                    throw new NullReferenceException("error type:" + itemText);
                                }
                                itemList.Add(new Item(type));
                            }
                            break;

                        case CollectionType.Data:
                            {
                                var index = itemList.Count;

                                if (index >= typeCollection.Length)
                                {
                                    throw new IndexOutOfRangeException();
                                }

                                Type type = typeCollection[index].To<Type>();

                                if (type == null)
                                {
                                    throw new NullReferenceException();
                                }
                                if (type == typeof(int))
                                {
                                    itemList.Add(new Item(itemText == "" ? 0 : int.Parse(itemText)));
                                }
                                else if (type == typeof(float))
                                {
                                    itemList.Add(new Item(itemText == "" ? 0 : float.Parse(itemText)));
                                }
                                else if (type == typeof(double))
                                {
                                    itemList.Add(new Item(itemText == "" ? 0 : double.Parse(itemText)));
                                }
                                else if (type == typeof(string))
                                {
                                    itemList.Add(new Item(itemText));
                                }
                                else
                                {
                                    if (type.IsEnum)
                                    {
                                        itemList.Add(new Item(Enum.Parse(type, itemText)));
                                    }
                                    else
                                    {
                                        itemList.Add(new Item(Activator.CreateInstance(type, itemText)));
                                    }
                                }
                            }
                            break;
                    }

                    pos = i + 1;
                }
            }

            return new Collection(itemList.ToArray(), headerCollection);
        }

        private static Content DoContent(string text, char separator)
        {
            var collectionList = new List<Collection>();
            Collection headerCollection = null;
            Collection typeCollection = null;
            var pos = 0;
            var quotationCount = 0;
            var collectionCount = 0;

            for (int i = 0, li = text.Length; i < li; ++i)
            {
                var c = text[i];

                quotationCount += (c == '"' ? 1 : 0);
                if (((quotationCount & 0x1) == 0) && (c == '\n' || i == li - 1))
                {
                    var endPos = (i >= 2 && text[i - 1] == '\r') ? i - 1 : i;

                    endPos += (c == '\n' ? 0 : 1);
                    if (collectionCount == 0)
                    {
                        headerCollection = DoCollection(text, separator, pos, endPos, headerCollection, CollectionType.Header, null);
                    }
                    else if (collectionCount == 1)
                    {
                        typeCollection = DoCollection(text, separator, pos, endPos, headerCollection, CollectionType.Type, null);
                    }
                    else
                    {
                        collectionList.Add(DoCollection(text, separator, pos, endPos, headerCollection, CollectionType.Data, typeCollection));
                    }
                    pos = i + 1;
                    collectionCount++;
                }
            }

            return new Content(collectionList.ToArray());
        }

        public static Content Run(string text, char separator)
        {
            return DoContent(text, separator);
        }
    }
}