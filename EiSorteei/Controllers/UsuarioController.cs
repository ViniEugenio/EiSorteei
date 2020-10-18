using EiSorteei.Data;
using EiSorteei.Helpers;
using EiSorteei.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EiSorteei.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly EiSorteeiEntities _Context;

        public UsuarioController()
        {
            _Context = new EiSorteeiEntities();
        }

        // GET: Login
        public ActionResult Index(long IdProduto=0)
        {
            ViewBag.Produto = IdProduto;
            return View();
        }

        public ActionResult CadastrarUsuario(long IdProduto=0)
        {
            ViewBag.Estados = Estados.GetAllStates();
            ViewBag.Produto = IdProduto;
            return View();
        }


        public bool Valida(string cpf)

        {

            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf;

            string digito;

            int soma;

            int resto;

            cpf = cpf.Trim();

            cpf = cpf.Replace(".", "").Replace("-", "");

            if (cpf.Length != 11)

                return false;

            tempCpf = cpf.Substring(0, 9);

            soma = 0;

            for (int i = 0; i < 9; i++)

                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            resto = soma % 11;

            if (resto < 2)

                resto = 0;

            else

                resto = 11 - resto;

            digito = resto.ToString();

            tempCpf = tempCpf + digito;

            soma = 0;

            for (int i = 0; i < 10; i++)

                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;

            if (resto < 2)

                resto = 0;

            else

                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cpf.EndsWith(digito);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CadastrarUsuario(UsuarioViewModel model,long IdProduto=0)
        {
            ViewBag.Estados = Estados.GetAllStates();

            if (ModelState.IsValid)
            {

                if (!Valida(model.Cpf))
                {
                    ModelState.AddModelError("Cpf", "O cpf digitado não é válido!");
                    return View(model);
                }

                Usuario NovoUsuario = new Usuario()
                {
                    Bairro = model.Bairro,
                    Cidade = model.Cidade,
                    Cpf = model.Cpf,
                    DataAtualizacao = DateTime.Now,
                    DataCadastro = DateTime.Now,
                    Email = model.Email,
                    Estado = model.Estado,
                    Nome = model.Nome,
                    Numero = model.Numero,
                    Rua = model.Rua,
                    Senha = Helpers.HashPassword.PasswordHash(model.Senha),
                    SobreNome = model.SobreNome,
                    Status = true
                };

                _Context.Usuario.Add(NovoUsuario);
                _Context.SaveChanges();

                long IdPermissao = 0;

                if (!_Context.Permissao.Any(p => p.Nome == "Usuário"))
                {
                    Permissao NovaPermissao = new Permissao()
                    {
                        Nome = "Usuário"
                    };

                    _Context.Permissao.Add(NovaPermissao);
                    _Context.SaveChanges();

                    IdPermissao = NovaPermissao.Id;
                }


                else
                {
                    IdPermissao = _Context.Permissao.FirstOrDefault(p => p.Nome.Equals("Usuário")).Id;
                }

                PermissaoUsuario Permissao = new PermissaoUsuario()
                {
                    IdPermissao = IdPermissao,
                    IdUsuario = NovoUsuario.Id,
                    Status = true,
                    DataCadastro = DateTime.Now
                };

                _Context.PermissaoUsuario.Add(Permissao);
                _Context.SaveChanges();

                Session["Usuario"] = NovoUsuario;
                long Id = _Context.PermissaoUsuario.FirstOrDefault(p => p.IdUsuario.Equals(NovoUsuario.Id)).IdPermissao;
                Session["Permissao"] = _Context.Permissao.FirstOrDefault(p => p.Id.Equals(Id)).Nome;

                if(IdProduto!=0)
                {
                    return RedirectToAction("Index", "Comprar", new { IdProduto = IdProduto });
                }

                return RedirectToAction("Index", "Home");
            }

            return View(model);

        }

        public ActionResult Logout()
        {
            Session.RemoveAll();
            return RedirectToAction("Index", "Home");
        }
    }
}