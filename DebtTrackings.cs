using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyMap
{
    public class DebtTrackings
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public decimal ClearedAmount { get; set; }
        public DateTime DueDate { get; set; }
    }
}
