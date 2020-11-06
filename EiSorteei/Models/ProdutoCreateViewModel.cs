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
        public long Id { get; set; }
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
        public string ValorRifa { get; set; }

        [DisplayName("Imagens do Produto")]
        [Required(ErrorMessage ="Por favor selecione uma imagem do Produto")]
        public List<HttpPostedFileBase> Imagem { get; set; }

        [DisplayName("Categoria do Produto")]
        [Required(ErrorMessage ="Por favor selecione uma Categoria para o Produto")]
        public long CategoriaProduto { get; set; }

        [DisplayName("Data do Sorteio")]
        [Required(ErrorMessage ="Por favor selecione a Data de Realização do Sorteio")]
        public string DataSorteio { get; set; }

        [DisplayName("Vídeo do Produto")]
        public HttpPostedFileBase Video { get; set; }

        public string ActualyVideo { get; set; }
        public Decimal ActualyPrice { get; set; }

    }
}