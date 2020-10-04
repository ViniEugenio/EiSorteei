using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EiSorteei.Models
{
    public class ProdutoCreateViewModel
    {
        [DisplayName("Nome do Produto")]
        [Required(ErrorMessage ="Por favor insira o nome do Produto")]
        public string Nome { get; set; }

        [DisplayName("Descrição")]
        [Required(ErrorMessage = "Por favor insira o nome do Produto")]
        public string Descricao { get; set; }

        [DisplayName("Range das Rifas")]
        [Required(ErrorMessage ="Por favor insira o Range dos números das Rifas")]
        public int RangeCodigo { get; set; }

        [DisplayName("Valor das Rifas")]
        [Required(ErrorMessage ="Por favor insira o Valor das Rifas")]
        public Decimal ValorRifa { get; set; }

    }
}