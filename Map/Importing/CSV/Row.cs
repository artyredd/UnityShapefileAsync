using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping.CSV
{
    public class Row : IRow
    {
        public Row(int index)
        {
            Index = index;
        }

        /// <summary>
        /// Position of the row within the CSV
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// This Rows Cells
        /// </summary>
        public IEnumerable<ICell> Cells => _cells;
        private List<ICell> _cells { get; set; } = new List<ICell>();

        public void AddCell(ICell cell) {
            _cells.Add(cell);
        }

        public override string ToString()
        {
            string cells = string.Empty;
            foreach (var item in Cells)
            {
                cells += item.ToString() + ',';
            }
            return $"Row({Index})[{cells}]";
        }
    }
}
