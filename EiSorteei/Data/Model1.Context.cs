﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class EiSorteeiEntities : DbContext
    {
        public EiSorteeiEntities()
            : base("name=EiSorteeiEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<CategoriaProduto> CategoriaProduto { get; set; }
        public virtual DbSet<Multimidia> Multimidia { get; set; }
        public virtual DbSet<Permissao> Permissao { get; set; }
        public virtual DbSet<PermissaoUsuario> PermissaoUsuario { get; set; }
        public virtual DbSet<Produto> Produto { get; set; }
        public virtual DbSet<Usuario> Usuario { get; set; }
        public virtual DbSet<Vendedor> Vendedor { get; set; }
        public virtual DbSet<Compra> Compra { get; set; }
    }
}
