using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyMap
{
    public class CashOutFlow
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Note { get; set; } = string.Empty;
        public string Tags {  get; set; } = string.Empty ;
    }
}
