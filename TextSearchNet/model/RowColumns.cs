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
    }
}
