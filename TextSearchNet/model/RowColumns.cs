using System.Collections.Generic;
using System.Linq;

namespace com.hideakin.textsearch.model
{
    public class RowColumns
    {
        public int Row { get; }

        public MatchList Matches { get; }

        public RowColumns(int row, MatchList matches)
        {
            Row = row;
            Matches = matches;
        }

        public RowColumns(int row, MatchList matches1, MatchList matches2)
        {
            Row = row;
            Matches = new MatchList();
            if (matches1 == null)
            {
                if (matches2 != null)
                {
                    Matches.AddRange(matches2);
                }
            }
            else if (matches2 == null)
            {
                Matches.AddRange(matches1);
            }
            else
            {
                int index1 = 0;
                int index2 = 0;
                while (true)
                {
                    if (matches1[index1].StartCol < matches2[index2].StartCol)
                    {
                        Matches.Add(matches1[index1++]);
                        if (index1 >= matches1.Count)
                        {
                            Matches.AddRange(matches2, index2);
                            break;
                        }
                    }
                    else if (matches1[index1].StartCol > matches2[index2].StartCol)
                    {
                        Matches.Add(matches2[index2++]);
                        if (index2 >= matches2.Count)
                        {
                            Matches.AddRange(matches1, index1);
                            break;
                        }
                    }
                    else if (matches1[index1].EndCol >= matches2[index2].EndCol)
                    {
                        Matches.Add(matches1[index1++]);
                        index2++;
                        if (index1 >= matches1.Count)
                        {
                            Matches.AddRange(matches2, index2);
                            break;
                        }
                        if (index2 >= matches2.Count)
                        {
                            Matches.AddRange(matches1, index1);
                            break;
                        }
                    }
                    else //if (matches1[index1].EndCol < matches2[index2].EndCol)
                    {
                        index1++;
                        Matches.Add(matches2[index2++]);
                        if (index1 >= matches1.Count)
                        {
                            Matches.AddRange(matches2, index2);
                            break;
                        }
                        if (index2 >= matches2.Count)
                        {
                            Matches.AddRange(matches1, index1);
                            break;
                        }
                    }
                }
            }
        }

        public static List<RowColumns> Append(IEnumerable<RowColumns> c1, IEnumerable<RowColumns> c2)
        {
            var c = new List<RowColumns>();
            foreach (var e1 in c1)
            {
                var row = e1.Row;
                var e2 = c2.Where(x => x.Row == row).FirstOrDefault();
                if (e2 != null)
                {
                    var matches = MatchList.Append(e1.Matches, e2.Matches);
                    if (matches.Count > 0)
                    {
                        c.Add(new RowColumns(row, matches));
                    }
                }
            }
            return c;
        }

        public static List<RowColumns> And(IEnumerable<RowColumns> c1, IEnumerable<RowColumns> c2)
        {
            var c = new List<RowColumns>();
            var e1 = c1.GetEnumerator();
            var e2 = c2.GetEnumerator();
            RowColumns v1;
            if (e1.MoveNext())
            {
                v1 = e1.Current;
            }
            else
            {
                return c;
            }
            RowColumns v2;
            if (e2.MoveNext())
            {
                v2 = e2.Current;
            }
            else
            {
                return c;
            }
            while (true)
            {
                if (v1.Row < v2.Row)
                {
                    if (e1.MoveNext())
                    {
                        v1 = e1.Current;
                    }
                    else
                    {
                        return c;
                    }
                }
                else if (v1.Row > v2.Row)
                {
                    if (e2.MoveNext())
                    {
                        v2 = e2.Current;
                    }
                    else
                    {
                        return c;
                    }
                }
                else
                {
                    c.Add(new RowColumns(v1.Row, v1.Matches, v2.Matches));
                    if (e1.MoveNext())
                    {
                        v1 = e1.Current;
                    }
                    else
                    {
                        return c;
                    }
                    if (e2.MoveNext())
                    {
                        v2 = e2.Current;
                    }
                    else
                    {
                        return c;
                    }
                }
            }
        }

        public static List<RowColumns> Or(IEnumerable<RowColumns> c1, IEnumerable<RowColumns> c2)
        {
            var c = new List<RowColumns>();
            var e1 = c1.GetEnumerator();
            var e2 = c2.GetEnumerator();
            RowColumns v1;
            if (e1.MoveNext())
            {
                v1 = e1.Current;
            }
            else
            {
                while (e2.MoveNext())
                {
                    c.Add(e2.Current);
                }
                return c;
            }
            RowColumns v2;
            if (e2.MoveNext())
            {
                v2 = e2.Current;
            }
            else
            {
                c.Add(v1);
                while (e1.MoveNext())
                {
                    c.Add(e1.Current);
                }
                return c;
            }
            while (true)
            {
                if (v1.Row < v2.Row)
                {
                    c.Add(v1);
                    if (e1.MoveNext())
                    {
                        v1 = e1.Current;
                    }
                    else
                    {
                        c.Add(v2);
                        while (e2.MoveNext())
                        {
                            c.Add(e2.Current);
                        }
                        return c;
                    }
                }
                else if (v1.Row > v2.Row)
                {
                    c.Add(v2);
                    if (e2.MoveNext())
                    {
                        v2 = e2.Current;
                    }
                    else
                    {
                        c.Add(v1);
                        while (e1.MoveNext())
                        {
                            c.Add(e1.Current);
                        }
                        return c;
                    }
                }
                else
                {
                    c.Add(new RowColumns(v1.Row, v1.Matches, v2.Matches));
                    if (e1.MoveNext())
                    {
                        v1 = e1.Current;
                    }
                    else
                    {
                        while (e2.MoveNext())
                        {
                            c.Add(e2.Current);
                        }
                        return c;
                    }
                    if (e2.MoveNext())
                    {
                        v2 = e2.Current;
                    }
                    else
                    {
                        c.Add(v1);
                        while (e1.MoveNext())
                        {
                            c.Add(e1.Current);
                        }
                        return c;
                    }
                }
            }
        }
    }
}
