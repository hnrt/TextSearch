using System.Collections.Generic;
using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class FindTextResponse
    {
        [JsonProperty("hits")]
        public PathPositions[] Hits { get; set; }

        public void Merge(int offset, FindTextResponse subject)
        {
            if (Hits != null && Hits.Length > 0)
            {
                if (subject != null)
                {
                    Merge(offset, subject.Hits);
                }
                else
                {
                    Hits = new PathPositions[0];
                }
            }
        }

        private void Merge(int offset, PathPositions[] pp)
        {
            var ppList = new List<PathPositions>();
            for (int i = 0; i < Hits.Length; i++)
            {
                Merge(ppList, Hits[i], offset, pp);
            }
            Hits = ppList.ToArray();
        }

        private void Merge(List<PathPositions> ppList, PathPositions pp, int offset, PathPositions[] ppArray)
        {
            for (int i = 0; i < ppArray.Length; i++)
            {
                if (ppArray[i].Path == pp.Path)
                {
                    Merge(ppList, pp, offset, ppArray[i].Positions);
                    break;
                }
            }
        }

        private void Merge(List<PathPositions> ppList, PathPositions pp, int offset, int[] positions)
        {
            var pList = new List<int>();
            for (int i = 0; i < pp.Positions.Length; i++)
            {
                for (int j = 0; j < positions.Length; j++)
                {
                    if (positions[j] == pp.Positions[i] + offset)
                    {
                        pList.Add(pp.Positions[i]);
                        break;
                    }
                }
            }
            if (pList.Count > 0)
            {
                pp.Positions = pList.ToArray();
                ppList.Add(pp);
            }
        }
    }
}
