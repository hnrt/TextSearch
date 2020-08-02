using System;
using System.Collections.Generic;

namespace com.hideakin.textsearch.model
{
    public class MatchList : List<(int StartPos, int EndPos, int StartCol, int EndCol)>
    {
        public MatchList()
            : base()
        {
        }

        public MatchList(IEnumerable<(int StartPos, int EndPos, int StartCol, int EndCol)> collection)
            : base(collection)
        {
        }

        public MatchList(int capacity)
            : base(capacity)
        {
        }

        public void AddRange(MatchList subject, int startIndex)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            for (int index = startIndex; index < subject.Count; index++)
            {
                Add(subject[index]);
            }
        }

        /// <summary>
        /// Collects only the Match entries where an entry of m1 is followed by an entry of m2.
        /// </summary>
        /// <param name="m1">Match list that holds preceding entries.</param>
        /// <param name="m2">Match list that holds following entries.</param>
        /// <returns>Match list that holds the entries that meet the conditions.</returns>
        public static MatchList Append(MatchList m1, MatchList m2)
        {
            var m = new MatchList();
            var index = 0;
            foreach (var (startPos, endPos, startCol, endCol) in m1)
            {
                while (true)
                {
                    if (index >= m2.Count)
                    {
                        return m;
                    }
                    if (m2[index].StartPos > endPos + 1)
                    {
                        break;
                    }
                    else if (m2[index].StartPos == endPos + 1)
                    {
                        m.Add((startPos, m2[index].EndPos, startCol, m2[index].EndCol));
                        index++;
                        break;
                    }
                    else
                    {
                        index++;
                    }
                }
            }
            return m;
        }
    }
}
