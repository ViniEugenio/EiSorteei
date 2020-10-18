using EiSorteei.Data;
using EiSorteei.Helpers;
using EiSorteei.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EiSorteei.Areas.Admin.Controllers
{
    public class VendedorController : Controller
    {
        private readonly EiSorteeiEntities _Context;

        public VendedorController()
        {
            _Context = new EiSorteeiEntities();
        }

        // GET: Admin/Vendedor
        public ActionResult Index()
        {
            ViewBag.Estados = Estados.GetAllStates();
            return View();
        }

        public string GerarProtocolo()
        {
            int Quantidade = 8;

            string validar = "abcdefghijklmnozABCDEFGHIJKLMNOZ1234567890";

            StringBuilder strbld = new StringBuilder(100);
            Random random = new Random();
            while (0 < Quantidade--)
            {
                strbld.Append(validar[random.Next(validar.Length)]);
            }
            return strbld.ToString();
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
        public ActionResult Index(UsuarioViewModel model)
        {
            ViewBag.Estados = Estados.GetAllStates();

            if (ModelState.IsValid)
            {

                if(!Valida(model.Cpf))
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

                string Codigo = GerarProtocolo();

                while (_Context.Vendedor.Any(v => v.Codigo.Equals(Codigo)))
                {
                    Codigo = GerarProtocolo();
                }


                Vendedor NovoVendedor = new Vendedor()
                {
                    Codigo = Codigo,
                    DataAtualizacao = DateTime.Now,
                    DataCadastro = DateTime.Now,
                    IdUsuario = NovoUsuario.Id,
                    Status = true,
                };

                _Context.Vendedor.Add(NovoVendedor);
                _Context.SaveChanges();

                long IdPermissao = 0;

                if (!_Context.Permissao.Any(p => p.Nome == "Vendedor"))
                {
                    Permissao NovaPermissao = new Permissao()
                    {
                        Nome = "Vendedor"
                    };

                    _Context.Permissao.Add(NovaPermissao);
                    _Context.SaveChanges();

                    IdPermissao = NovaPermissao.Id;
                }


                else
                {
                    IdPermissao = _Context.Permissao.FirstOrDefault(p => p.Nome.Equals("Vendedor")).Id;
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

                IdPermissao = 0;

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

                Permissao = new PermissaoUsuario()
                {
                    IdPermissao = IdPermissao,
                    IdUsuario = NovoUsuario.Id,
                    Status = true,
                    DataCadastro = DateTime.Now
                };

                _Context.PermissaoUsuario.Add(Permissao);
                _Context.SaveChanges();

                return RedirectToAction("Index", "Login");
            }

            return View(model);

        }
    }
}