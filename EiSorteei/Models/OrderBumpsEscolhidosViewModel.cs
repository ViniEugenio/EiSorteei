using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EiSorteei.Models
{
    public class OrderBumpsEscolhidosViewModel
    {
        public long Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public List<int> NumerosRifas { get; set; }
        public string Imagem { get; set; }
    }
}