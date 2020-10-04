using EiSorteei.Data;
using EiSorteei.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EiSorteei.Areas.Admin.Controllers
{
    public class LoginController : Controller
    {

        private readonly EiSorteeiEntities _Context;

        public LoginController()
        {
            _Context = Context.GetContext();
        }

        // GET: Admin/Login
        public ActionResult Index()
        {

            if (_Context.Usuario.Count() == 0)
            {
                Usuario NovoUsuario = new Usuario()
                {
                    Bairro = "Administrador",
                    Cidade = "Administrador",
                    Cpf = "Administrador",
                    DataAtualizacao = DateTime.Now,
                    DataCadastro = DateTime.Now,
                    Email = "admin@admin.com",
                    Senha = Helpers.HashPassword.PasswordHash("123"),
                    Estado = "Administrador",
                    Nome = "Administrador",
                    Numero = "Administrador",
                    Rua = "Administrador",
                    SobreNome = "Administrador",
                    Status = true,
                };

                _Context.Usuario.Add(NovoUsuario);
                _Context.SaveChanges();

                long IdPermissao = 0;

                if (!_Context.Permissao.Any(p => p.Nome == "Administrador"))
                {
                    Permissao NovaPermissao = new Permissao()
                    {
                        Nome = "Administrador"
                    };

                    _Context.Permissao.Add(NovaPermissao);
                    _Context.SaveChanges();

                    IdPermissao = NovaPermissao.Id;
                }


                else
                {
                    IdPermissao = _Context.Permissao.FirstOrDefault(p => p.Nome.Equals("Administrador")).Id;
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
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string HashPassword = Helpers.HashPassword.PasswordHash(model.Senha);

                if (_Context.Usuario.Any(u => u.Email.Equals(model.Email) && u.Senha.Equals(HashPassword)))
                {
                    return RedirectToAction("Index", "DashBoard");
                }

                else
                {
                    ModelState.AddModelError("Senha", "Email ou Senha Inválidos");
                    return View(model);
                }
            }
            return View(model);
        }

    }
}
