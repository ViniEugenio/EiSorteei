using EiSorteei.Data;
using EiSorteei.Helpers;
using EiSorteei.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        public ActionResult Index(long IdProduto = 0)
        {
            ViewBag.Produto = IdProduto;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginViewModel model, long IdProduto = 0)
        {
            if (ModelState.IsValid)
            {
                string HashPassword = Helpers.HashPassword.PasswordHash(model.Senha);

                if (_Context.Usuario.Any(u => u.Email.Equals(model.Email) && u.Senha.Equals(HashPassword)))
                {
                    Usuario oUsuario = _Context.Usuario.FirstOrDefault(u => u.Email.Equals(model.Email) && u.Senha.Equals(HashPassword));
                    Session["Usuario"] = oUsuario;
                    long IdPermissao = _Context.PermissaoUsuario.FirstOrDefault(p => p.IdUsuario.Equals(oUsuario.Id)).IdPermissao;
                    Session["Permissao"] = _Context.Permissao.FirstOrDefault(p => p.Id.Equals(IdPermissao)).Nome;

                    if (IdProduto != 0)
                    {
                        return RedirectToAction("Index", "Comprar", new { IdProduto = IdProduto });
                    }

                    return RedirectToAction("Index", "Home");
                }

                else
                {
                    ModelState.AddModelError("Senha", "Email ou Senha Inválidos");
                    return View(model);
                }
            }
            return View(model);
        }

        public ActionResult CadastrarUsuario(long IdProduto = 0)
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
        public ActionResult CadastrarUsuario(UsuarioViewModel model, long IdProduto = 0)
        {
            ViewBag.Estados = Estados.GetAllStates();

            if (!Valida(model.Cpf))
            {
                ModelState.AddModelError("Cpf", "O CPF digitado não é válido!");
            }

            if (_Context.Usuario.Any(u => u.Email.Equals(model.Email)))
            {
                ModelState.AddModelError("Email", "O Email digitado já está sendo usado por outro usuário!");
            }

            if (_Context.Usuario.Any(u => u.Cpf.Equals(model.Cpf)))
            {
                ModelState.AddModelError("Cpf", "O CPF digitado já está sendo usado por outro usuário!");
            }

            if (ModelState.IsValid)
            {
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
                    Status = true,
                    CEP = model.CEP,
                    Telefone = model.Telefone
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

                if (IdProduto != 0)
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
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult MeuPerfil()
        {
            ViewBag.Estados = Estados.GetAllStates();
            Usuario UsuarioLogado = (Usuario)Session["Usuario"];

            if (UsuarioLogado == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(UsuarioLogado);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult MeuPerfil(Usuario model)
        {
            ViewBag.Estados = Estados.GetAllStates();

            Usuario FindedUsuario = _Context.Usuario.AsNoTracking().FirstOrDefault(u => u.Id.Equals(model.Id));

            if (!Valida(model.Cpf))
            {
                ModelState.AddModelError("Cpf", "O CPF digitado não é válido!");
            }

            if (_Context.Usuario.Any(u => u.Cpf.Equals(model.Cpf)) && FindedUsuario.Cpf != model.Cpf)
            {
                ModelState.AddModelError("Cpf", "O CPF digitado já está sendo usado por outro usuário!");
            }

            if (ModelState.IsValid)
            {
                model.DataAtualizacao = DateTime.Now;
                _Context.Entry(model).State = System.Data.Entity.EntityState.Modified;
                _Context.SaveChanges();
            }

            return View(model);
        }

        public ActionResult MinhasCompras()
        {
            Usuario UsuarioLogado = (Usuario)Session["Usuario"];

            if (UsuarioLogado == null)
            {
                return RedirectToAction("Index", "Home");
            }

            List<Carrinho> Carrinhos = _Context.Carrinho.Include("BilhetesCarrinho").Include("Compras").Where(c => c.IdUsuario.Equals(UsuarioLogado.Id)).ToList();


            List<MinhasComprasViewModel> MinhasCompras = new List<MinhasComprasViewModel>();

            foreach (var carrinho in Carrinhos)
            {
                long IdProduto = carrinho.Compras.First().Carrinho.IdProduto;
                Produto FindedProduto = _Context.Produto.FirstOrDefault(p => p.Id.Equals(IdProduto));

                if (FindedProduto.DataSorteio > DateTime.Now)
                {
                    var Bilhetes = _Context.BilhetesCarrinho.Where(b => b.IdCarrinho.Equals(carrinho.Id)).ToList();

                    var OrderBumps = _Context.OrderBump.Join(_Context.OrderBumpsEscolhidos.
                        Join(_Context.BilhetesCarrinho.Where(b => b.IdCarrinho.Equals(carrinho.Id)), o => o.IdBilhete, b => b.Id, (o, b) => o), o => o.Id, oe => oe.IdOrderBump, (o, oe) => o).GroupBy(o => o.Id).ToList();

                    List<OrderBumpsEscolhidosViewModel> Orders = new List<OrderBumpsEscolhidosViewModel>();

                    foreach(IGrouping<long,OrderBump> order in OrderBumps)
                    {
                        OrderBump FindedOrder = _Context.OrderBump.FirstOrDefault(o => o.Id.Equals(order.Key));

                        OrderBumpsEscolhidosViewModel NewOrder = new OrderBumpsEscolhidosViewModel()
                        {
                            Id = FindedOrder.Id,
                            Nome = FindedOrder.Nome,
                            Descricao = FindedOrder.Descricao,
                            Imagem = FindedOrder.Imagem,
                            NumerosRifas = _Context.BilhetesCarrinho.Join(_Context.OrderBumpsEscolhidos.Where(o=>o.IdOrderBump.Equals(FindedOrder.Id)),b=>b.Id,o=>o.IdBilhete,(b,o)=>b).Where(b=>b.IdCarrinho.Equals(carrinho.Id)).Select(b=>b.NumeroBilhete).ToList()
                        };

                        Orders.Add(NewOrder);
                    }

                    MinhasComprasViewModel data = new MinhasComprasViewModel()
                    {
                        Id = carrinho.Id,
                        CodigoVendedor = carrinho.Compras.First().CodigoVendedor,
                        DataCompra = carrinho.Compras.First().DataCompra,
                        Status = FormataStatus(carrinho.Compras.First().Status),
                        UrlBoleto = string.IsNullOrEmpty(carrinho.Compras.First().UrlBoleto) ? "" : carrinho.Compras.First().UrlBoleto,
                        ValorCompra = "R$ " + carrinho.Compras.First().ValorCompra.Replace('.',','),
                        OrderBumps = Orders,
                        Bilhetes = Bilhetes,
                        Premio = FindedProduto,
                        Multimidia = _Context.Multimidia.Where(m => m.IdProduto.Equals(FindedProduto.Id) && m.Status).ToList(),                        
                    };

                    MinhasCompras.Add(data);
                }

            }

            return View(MinhasCompras);
        }


        public string FormataStatus(string Status)
        {

            if (Status == "pending")
            {
                return "Pendente";
            }

            else if (Status == "in_process")
            {
                return "Pedido em Análise";
            }

            else if (Status == "cancelled")
            {
                return "Pedido Cancelado";
            }

            else if (Status == "rejected")
            {
                return "Pedido Rejeitado";
            }

            return "Aprovado";

        }


    }
}