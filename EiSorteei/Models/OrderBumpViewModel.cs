﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EiSorteei.Models
{
    public class OrderBumpViewModel
    {
        [Required(ErrorMessage ="Campo Obrigatório")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório")]
        [Display(Name ="Descrição")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório")]
        public HttpPostedFileBase Imagem { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório")]
        public string Valor { get; set; }
    }
}