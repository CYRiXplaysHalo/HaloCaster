using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;

namespace xemuh2stats.objects
{
    public class spread_sheet_template
    {
        public string name { get; set; }
        public List<string> headers { get; set; }
        public List<string> cells { get; set; }
        public List<CellValues> types { get; set; }

        public int column_count;

        public void add_column(string name, string cell_format, CellValues type)
        {
            headers.Add(name);
            cells.Add(cell_format);
            types.Add(type);
            column_count++;
        }

        public bool isValid()
        {
            return headers.Count == cells.Count;
        }
    }
}
