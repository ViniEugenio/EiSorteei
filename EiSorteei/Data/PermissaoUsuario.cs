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
    
    public partial class PermissaoUsuario
    {
        public long Id { get; set; }
        public long IdPermissao { get; set; }
        public long IdUsuario { get; set; }
        public System.DateTime DataCadastro { get; set; }
        public bool Status { get; set; }
    
        public virtual Permissao Permissao { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
