using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping.CSV
{
    /// <summary>
    /// A column header for an CSV Object
    /// </summary>
    public class ColumnHeader : IColumnHeader
    {
        public ColumnHeader(int index, string title, int length)
        {
            Index = index;
            Title = title;
            Length = length;
        }

        public int Index { get; private set; }

        public string Title { get; private set; }

        public int Length { get; private set; }

        public override string ToString()
        {
            return $"ColumnHeader({Index},{Title},{Length})";
        }
    }
}
