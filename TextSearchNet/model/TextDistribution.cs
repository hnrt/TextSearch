using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace com.hideakin.textsearch.model
{
    public class TextDistribution
    {
        [JsonProperty("fid")]
        public int Fid { get; set; }

        [JsonProperty("positions")]
        public int[] Positions { get; set; }

        public void Merge(TextDistribution src)
        {
            if (src == null)
            {
                //NOP
            }
            else if (src.Fid != Fid)
            {
                throw new Exception("TextDistribution.Add: FID mismatch.");
            }
            else if (src.Positions == null || src.Positions.Length == 0)
            {
                //NOP
            }
            else if (Positions == null || Positions.Length == 0)
            {
                Positions = src.Positions;
            }
            else
            {
                var dst = new List<int>(Positions.Length + src.Positions.Length);
                int a = Positions[0];
                int b = src.Positions[0];
                int i = 1;
                int j = 1;
                while (true)
                {
                    if (a < b)
                    {
                        dst.Add(a);
                        if (i < Positions.Length)
                        {
                            a = Positions[i++];
                        }
                        else
                        {
                            dst.Add(b);
                            while (j < src.Positions.Length)
                            {
                                dst.Add(src.Positions[j++]);
                            }
                            break;
                        }
                    }
                    else if (a > b)
                    {
                        dst.Add(b);
                        if (j < src.Positions.Length)
                        {
                            b = src.Positions[j++];
                        }
                        else
                        {
                            dst.Add(a);
                            while (i < Positions.Length)
                            {
                                dst.Add(Positions[i++]);
                            }
                            break;
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("TextDistribution.Add: Duplicate position({0}).", a));
                    }
                }
                Positions = dst.ToArray();
            }
        }
    }
}
