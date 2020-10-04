using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EiSorteei.Models
{
    public class LoginViewModel
    {
        [DisplayName("Email")]
        [DataType(DataType.EmailAddress,ErrorMessage ="Por favor Digite um email válido")]
        [Required(ErrorMessage ="Por favor Insira seu Email")]
        public string Email { get; set; }

        [DisplayName("Senha")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage ="Por favor Insira sua Senha")]
        public string Senha { get; set; }
    }
}