using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EiSorteei.Models
{
    public class UsuarioViewModel
    {
        [DisplayName("Imagem de Usuário")]
        public HttpPostedFileBase UserImage { get; set; }

        [Required(ErrorMessage ="Campo Obrigatório")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório")]
        public string SobreNome { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório")]
        [DataType(DataType.EmailAddress,ErrorMessage ="Este não é um email válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório")]
        [DataType(DataType.Password)]
        [StringLength(16,ErrorMessage ="A senha deve conter no máximo 16 e no mínimo 8 caractéres",MinimumLength =8)]
        public string Senha { get; set; }

        [Required(ErrorMessage ="Campo Obrigatório")]
        [DataType(DataType.Password)]
        [Compare("Senha",ErrorMessage ="As senhas não são iguais")]
        [DisplayName("Confirmar Senha")]
        public string ConfirmarSenha { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório")]
        public string Estado { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório")]
        public string Cidade { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório")]
        public string Bairro { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório")]
        public string Rua { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório")]
        [DisplayName("Número")]
        public string Numero { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório")]
        public string Cpf { get; set; }

        [Required(ErrorMessage ="Campo Obrigatório")]
        public string CEP { get; set; }

        [Required(ErrorMessage ="Campo Obrigatório")]
        public string Telefone { get; set; }
    }
}