using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EiSorteei.Models
{
    public class ProdutoHomeViewModel
    {
        public long Id { get; set; }
        public string Nome { get; set; }
        public List<string> Imagem { get; set; }
        public decimal ValorRifa { get; set; }
    }
}