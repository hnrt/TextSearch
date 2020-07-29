using System.Collections.Generic;

namespace com.hideakin.textsearch.model
{
    public class RowColumns
    {
        public int Row { get; }

        public List<(int Start, int End)> Columns { get; }

        public RowColumns(int row, List<(int Start, int End)> columns)
        {
            Row = row;
            Columns = columns;
        }

        public RowColumns(int row, List<(int Start, int End)> columns1, List<(int Start, int End)> columns2)
        {
            Row = row;
            Columns = new List<(int Start, int End)>();
            int index1 = 0;
            int index2 = 0;
            if (columns1.Count == 0)
            {
                while (index2 < columns2.Count)
                {
                    Columns.Add(columns2[index2++]);
                }
            }
            else if (columns2.Count == 0)
            {
                do
                {
                    Columns.Add(columns1[index1++]);
                }
                while (index1 < columns1.Count);
            }
            else
            {
                while (true)
                {
                    if (columns1[index1].Start < columns2[index2].Start)
                    {
                        Columns.Add(columns1[index1++]);
                        if (index1 >= columns1.Count)
                        {
                            do
                            {
                                Columns.Add(columns2[index2++]);
                            }
                            while (index2 < columns2.Count);
                            break;
                        }
                    }
                    else if (columns1[index1].Start > columns2[index2].Start)
                    {
                        Columns.Add(columns2[index2++]);
                        if (index2 >= columns2.Count)
                        {
                            do
                            {
                                Columns.Add(columns1[index1++]);
                            }
                            while (index1 < columns1.Count);
                            break;
                        }
                    }
                    else if (columns1[index1].End >= columns2[index2].End)
                    {
                        Columns.Add(columns1[index1++]);
                        if (index1 >= columns1.Count)
                        {
                            index2++;
                            while (index2 < columns2.Count)
                            {
                                Columns.Add(columns2[index2++]);
                            }
                            break;
                        }
                        index2++;
                        if (index2 >= columns2.Count)
                        {
                            do
                            {
                                Columns.Add(columns1[index1++]);
                            }
                            while (index1 < columns1.Count);
                            break;
                        }
                    }
                    else //if (columns1[index1].End < columns2[index2].End)
                    {
                        Columns.Add(columns2[index2++]);
                        if (index2 >= columns2.Count)
                        {
                            index1++;
                            while (index1 < columns1.Count)
                            {
                                Columns.Add(columns1[index1++]);
                            }
                            break;
                        }
                        index1++;
                        if (index1 >= columns1.Count)
                        {
                            do
                            {
                                Columns.Add(columns2[index2++]);
                            }
                            while (index2 < columns2.Count);
                            break;
                        }
                    }
                }
            }
        }
    }
}
