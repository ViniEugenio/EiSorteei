using EiSorteei.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EiSorteei.Models
{
    public class MinhasComprasViewModel
    {
        public long Id { get; set; }
        public string NomeProduto { get; set; }
        public Usuario DadosUsuario { get; set; }
        public DateTime DataCompra { get; set; }
        public string UrlBoleto { get; set; }
        public string Status { get; set; }
        public string CodigoVendedor { get; set; }
        public List<OrderBumpsEscolhidosViewModel> OrderBumps { get; set; }
        public List<BilhetesCarrinho> Bilhetes { get; set; }
        public string ValorCompra { get; set; }
        public Produto Premio { get; set; }
        public List<Multimidia> Multimidia { get; set; }
    }
}