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
    
    public partial class Multimidia
    {
        public long Id { get; set; }
        public long IdProduto { get; set; }
        public string Caminho { get; set; }
        public System.DateTime DataCadastro { get; set; }
        public System.DateTime DataAtualizacao { get; set; }
        public bool Status { get; set; }
    
        public virtual Produto Produto { get; set; }
    }
}