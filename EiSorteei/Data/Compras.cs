//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EiSorteei.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Compras
    {
        public long Id { get; set; }
        public long CarrinhoId { get; set; }
        public System.DateTime DataCompra { get; set; }
        public string Status { get; set; }
        public string IdCompra { get; set; }
        public string CodigoVendedor { get; set; }
        public string UrlBoleto { get; set; }
        public string ValorCompra { get; set; }
    
        public virtual Carrinho Carrinho { get; set; }
    }
}
