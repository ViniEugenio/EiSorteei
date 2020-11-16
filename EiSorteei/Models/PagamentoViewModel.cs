using EiSorteei.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EiSorteei.Models
{
    public class PagamentoViewModel
    {
        public List<Multimidia> Imagens { get; set; }
        public Produto Produto { get; set; }
        public Usuario Usuario { get; set; }
        public string Bilhetes { get; set; }
        public string ValorTotal { get; set; }
        public List<OrderBump> OrderBumps { get; set; }
    }
}