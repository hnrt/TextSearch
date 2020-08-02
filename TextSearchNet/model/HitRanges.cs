using System;
using System.Collections.Generic;
using System.Linq;

namespace com.hideakin.textsearch.model
{
    public class HitRanges
    {
        public int Fid { get; set; }

        public List<(int Start, int End)> Ranges { get; private set; }

        public HitRanges()
        {
            Fid = -1;
            Ranges = new List<(int Start, int End)>();
        }

        public HitRanges(int fid)
        {
            Fid = fid;
            Ranges = new List<(int Start, int End)>();
        }

        public HitRanges(TextDistribution td)
        {
            Fid = td.Fid;
            Ranges = new List<(int Start, int End)>(td.Positions.Length);
            foreach (int position in td.Positions)
            {
                Ranges.Add((position, position));
            }
        }

        public HitRanges Merge(TextDistribution td)
        {
            if (td.Positions.Length == 0)
            {
                return this;
            }
            if (Ranges.Count == 0)
            {
                Ranges.AddRange(td.Positions.Select(x => (x, x)).ToList());
                return this;
            }
            var r = new List<(int Start, int End)>(Ranges.Count + td.Positions.Length);
            var a = Ranges[0].Start;
            int b = td.Positions[0];
            int i = 1;
            int j = 1;
            while (true)
            {
                if (a < b)
                {
                    r.Add((a, a));
                    if (i < Ranges.Count)
                    {
                        a = Ranges[i++].Start;
                    }
                    else
                    {
                        r.Add((b, b));
                        while (j < td.Positions.Length)
                        {
                            b = td.Positions[j++];
                            r.Add((b, b));
                        }
                        break;
                    }
                }
                else if (a > b)
                {
                    r.Add((b, b));
                    if (j < td.Positions.Length)
                    {
                        b = td.Positions[j++];
                    }
                    else
                    {
                        r.Add((a, a));
                        while (i < Ranges.Count)
                        {
                            a = Ranges[i++].Start;
                            r.Add((a, a));
                        }
                        break;
                    }
                }
                else
                {
                    r.Add((a, a));
                    if (i < Ranges.Count)
                    {
                        a = Ranges[i++].Start;
                    }
                    else
                    {
                        while (j < td.Positions.Length)
                        {
                            b = td.Positions[j++];
                            r.Add((b, b));
                        }
                        break;
                    }
                    if (j < td.Positions.Length)
                    {
                        b = td.Positions[j++];
                    }
                    else
                    {
                        r.Add((a, a));
                        while (i < Ranges.Count)
                        {
                            a = Ranges[i++].Start;
                            r.Add((a, a));
                        }
                        break;
                    }
                }
            }
            Ranges = r;
            return this;
        }

        public HitRanges Merge(HitRanges other)
        {
            if (other == null || other.Ranges.Count == 0)
            {
                return this;
            }
            if (Ranges.Count == 0)
            {
                Ranges.AddRange(other.Ranges);
                return this;
            }
            var r = new List<(int Start, int End)>(Ranges.Count + other.Ranges.Count);
            var a = Ranges[0];
            var b = other.Ranges[0];
            int i = 1;
            int j = 1;
            while (true)
            {
                if (a.Start < b.Start)
                {
                    r.Add(a);
                    if (i < Ranges.Count)
                    {
                        a = Ranges[i++];
                    }
                    else
                    {
                        r.Add(b);
                        while (j < other.Ranges.Count)
                        {
                            b = other.Ranges[j++];
                            r.Add(b);
                        }
                        break;
                    }
                }
                else if (a.Start > b.Start)
                {
                    r.Add(b);
                    if (j < other.Ranges.Count)
                    {
                        b = other.Ranges[j++];
                    }
                    else
                    {
                        r.Add(a);
                        while (i < Ranges.Count)
                        {
                            a = Ranges[i++];
                            r.Add(a);
                        }
                        break;
                    }
                }
                else
                {
                    if (a.End >= b.End)
                    {
                        r.Add(a);
                    }
                    else
                    {
                        r.Add(b);
                    }
                    if (i < Ranges.Count)
                    {
                        a = Ranges[i++];
                    }
                    else
                    {
                        while (j < other.Ranges.Count)
                        {
                            b = other.Ranges[j++];
                            r.Add(b);
                        }
                        break;
                    }
                    if (j < other.Ranges.Count)
                    {
                        b = other.Ranges[j++];
                    }
                    else
                    {
                        r.Add(a);
                        while (i < Ranges.Count)
                        {
                            a = Ranges[i++];
                            r.Add(a);
                        }
                        break;
                    }
                }
            }
            Ranges = r;
            return this;
        }

        public HitRanges Append(TextDistribution td)
        {
            int index = 0;
            while (index < Ranges.Count)
            {
                var (start, end) = Ranges[index];
                if (td.Positions.Contains(end + 1))
                {
                    Ranges[index] = (start, end + 1);
                    index++;
                }
                else
                {
                    Ranges.RemoveAt(index);
                }
            }
            return this;
        }

        public HitRanges Append(HitRanges other)
        {
            int index = 0;
            while (index < Ranges.Count)
            {
                var (start, end) = Ranges[index];
                var end2 = other.Ranges.Where(x => x.Start == end + 1).Select(x => x.End).FirstOrDefault();
                if (end2 >= end + 1)
                {
                    Ranges[index] = (start, end2);
                    index++;
                }
                else
                {
                    Ranges.RemoveAt(index);
                }
            }
            return this;
        }
    }
}
