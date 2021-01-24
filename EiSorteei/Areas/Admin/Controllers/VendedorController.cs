using EiSorteei.Data;
using EiSorteei.Helpers;
using EiSorteei.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

            if (_Context.Usuario.Any(u => u.Email.Equals(model.Email)))
            {
                ModelState.AddModelError("Email", "O email informado já está sendo usado por outro usuário!");
            }

            if (!Valida(model.Cpf))
            {
                ModelState.AddModelError("Cpf", "O CPF digitado não é válido!");
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
                    Status = false,
                    CEP = model.CEP,
                    Telefone = model.Telefone
                };

                if (model.UserImage != null)
                {
                    string CaminhoVideo = Server.MapPath("~/Content/ImagensUsuario/");
                    string NomeArquivo = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + model.UserImage.FileName;

                    using (var stream = new FileStream(CaminhoVideo + NomeArquivo, FileMode.Create))
                    {
                        using (Stream inputStream = model.UserImage.InputStream)
                        {
                            inputStream.CopyTo(stream);
                        }
                    }

                    NovoUsuario.UserImage = NomeArquivo;
                }

                _Context.Usuario.Add(NovoUsuario);
                _Context.SaveChanges();

                //string Codigo = GerarProtocolo();

                //while (_Context.Vendedor.Any(v => v.Codigo.Equals(Codigo)))
                //{
                //    Codigo = GerarProtocolo();
                //}

                string Codigo = "";
                string[] Nomes = NovoUsuario.Nome.Split(' ');
                string[] SobreNomes = NovoUsuario.SobreNome.Split(' ');

                for (var x = 0; x < Nomes.Length; x++)
                {
                    Codigo = Codigo + Nomes[x][0];
                }

                for (var x = 0; x < SobreNomes.Length; x++)
                {
                    Codigo = Codigo + SobreNomes[x][0];
                }

                Codigo = Codigo + DateTime.Now.ToString("yy") + NovoUsuario.Id.ToString();

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

        public ActionResult MinhasVendas(string NomeUsuario = "", string Status = "")
        {
            Usuario UsuarioLogado = (Usuario)Session["Usuario"];
            string CodigoVendedor = _Context.Vendedor.FirstOrDefault(v => v.IdUsuario.Equals(UsuarioLogado.Id)).Codigo;

            List<Carrinho> Carrinhos = _Context.Carrinho.Join(_Context.Compras.Where(c => c.CodigoVendedor.Equals(CodigoVendedor)), ca => ca.Id, c => c.CarrinhoId, (ca, c) => ca).ToList();
            List<MinhasComprasViewModel> MinhasCompras = new List<MinhasComprasViewModel>();

            foreach (var carrinho in Carrinhos)
            {
                var IdProduto = carrinho.Compras.First().Carrinho.IdProduto;
                var FindedProduto = _Context.Produto.FirstOrDefault(p => p.Id == IdProduto);

                if (FindedProduto.DataSorteio > DateTime.Now)
                {
                    var Bilhetes = _Context.BilhetesCarrinho.Where(b => b.IdCarrinho.Equals(carrinho.Id)).ToList();

                    var OrderBumps = _Context.OrderBump.Join(_Context.OrderBumpsEscolhidos.
                        Join(_Context.BilhetesCarrinho.Where(b => b.IdCarrinho.Equals(carrinho.Id)), o => o.IdBilhete, b => b.Id, (o, b) => o), o => o.Id, oe => oe.IdOrderBump, (o, oe) => o).GroupBy(o => o.Id).ToList();

                    List<OrderBumpsEscolhidosViewModel> Orders = new List<OrderBumpsEscolhidosViewModel>();

                    foreach (IGrouping<long, OrderBump> order in OrderBumps)
                    {
                        OrderBump FindedOrder = _Context.OrderBump.FirstOrDefault(o => o.Id.Equals(order.Key));

                        OrderBumpsEscolhidosViewModel NewOrder = new OrderBumpsEscolhidosViewModel()
                        {
                            Id = FindedOrder.Id,
                            Nome = FindedOrder.Nome,
                            Descricao = FindedOrder.Descricao,
                            Imagem = FindedOrder.Imagem,
                            NumerosRifas = _Context.BilhetesCarrinho.Join(_Context.OrderBumpsEscolhidos.Where(o => o.IdOrderBump.Equals(FindedOrder.Id)), b => b.Id, o => o.IdBilhete, (b, o) => b).Where(b => b.IdCarrinho.Equals(carrinho.Id)).Select(b => b.NumeroBilhete).ToList()
                        };

                        Orders.Add(NewOrder);
                    }

                    MinhasComprasViewModel data = new MinhasComprasViewModel();

                    data.Id = carrinho.Id;
                    data.DadosUsuario = _Context.Usuario.First(u => u.Id.Equals(carrinho.IdUsuario));
                    data.CodigoVendedor = carrinho.Compras.First().CodigoVendedor;
                    data.DataCompra = carrinho.Compras.First().DataCompra;
                    data.Status = FormataStatus(carrinho.Compras.First().Status);
                    data.UrlBoleto = string.IsNullOrEmpty(carrinho.Compras.First().UrlBoleto) ? "" : carrinho.Compras.First().UrlBoleto;
                    data.ValorCompra = "R$ " + carrinho.Compras.First().ValorCompra.Replace('.', ',');
                    data.OrderBumps = Orders;
                    data.Bilhetes = Bilhetes;
                    data.Premio = FindedProduto;
                    data.NomeProduto = FindedProduto.Nome;


                    MinhasCompras.Add(data);
                }

            }


            if (!string.IsNullOrEmpty(Status) && Status != "TodasCompras")
            {
                MinhasCompras = MinhasCompras.Where(m => m.Status.Equals(Status)).ToList();
                ViewBag.Status = Status;
            }

            if (!string.IsNullOrEmpty(NomeUsuario))
            {
                MinhasCompras = MinhasCompras.Where(m => m.DadosUsuario.Nome.ToLower().Contains(NomeUsuario.ToLower())).ToList();
                ViewBag.NomeUsuario = NomeUsuario;
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

        public JsonResult GetLocation(string CEP)
        {
            string FormatedCep = "http://viacep.com.br/ws/" + CEP.Replace(".", string.Empty).Replace("-", string.Empty) + "/json/";
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            WebRequest request = WebRequest.Create(FormatedCep);
            request.Method = "GET";
            var response = (HttpWebResponse)request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream());
            var result = streamReader.ReadToEnd();

            return Json(JsonConvert.DeserializeObject<CEPViewModel>(result));
        }

        public ActionResult TodosVendedores()
        {
            var model = _Context.Vendedor.Include("Usuario").Where(v => v.Usuario.Status).ToList();
            return View(model);
        }

        public ActionResult MeuPerfil()
        {
            ViewBag.Estados = Estados.GetAllStates();
            Usuario UsuarioLogado = (Usuario)Session["Usuario"];

            if (UsuarioLogado == null)
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                ViewBag.Codigo = _Context.Vendedor.FirstOrDefault(v => v.IdUsuario.Equals(UsuarioLogado.Id)).Codigo;
            }

            catch
            {

            }

            return View(UsuarioLogado);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult MeuPerfil(Usuario model, HttpPostedFileBase foto)
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

                if (foto != null)
                {
                    string CaminhoVideo = Server.MapPath("~/Content/ImagensUsuario/");
                    string NomeArquivo = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + foto.FileName;

                    using (var stream = new FileStream(CaminhoVideo + NomeArquivo, FileMode.Create))
                    {
                        using (Stream inputStream = foto.InputStream)
                        {
                            inputStream.CopyTo(stream);
                        }
                    }

                    model.UserImage = NomeArquivo;
                }

                _Context.Entry(model).State = System.Data.Entity.EntityState.Modified;
                _Context.SaveChanges();
            }

            return View(model);
        }


        public ActionResult DetalhesVendedor(int id)
        {
            var Usuario = _Context.Usuario.FirstOrDefault(u => u.Id.Equals(id));

            if (Usuario == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Codigo = _Context.Vendedor.FirstOrDefault(v => v.IdUsuario.Equals(id)).Codigo;

            return View(Usuario);
        }

        public ActionResult Vendas(int IdVendedor, string Status = "", string NomeUsuario = "")
        {
            Usuario UsuarioLogado = _Context.Usuario.FirstOrDefault(u => u.Id.Equals(IdVendedor));
            ViewBag.Nome = UsuarioLogado.Nome + " " + UsuarioLogado.SobreNome;

            string CodigoVendedor = _Context.Vendedor.FirstOrDefault(v => v.IdUsuario == IdVendedor).Codigo;
            ViewBag.Codigo = CodigoVendedor;

            List<Carrinho> Carrinhos = _Context.Carrinho.Join(_Context.Compras.Where(c => c.CodigoVendedor.Equals(CodigoVendedor)), ca => ca.Id, c => c.CarrinhoId, (ca, c) => ca).ToList();
            List<MinhasComprasViewModel> MinhasCompras = new List<MinhasComprasViewModel>();

            foreach (var carrinho in Carrinhos)
            {
                var IdProduto = carrinho.Compras.First().Carrinho.IdProduto;
                var FindedProduto = _Context.Produto.FirstOrDefault(p => p.Id == IdProduto);

                if (FindedProduto.DataSorteio > DateTime.Now)
                {
                    var Bilhetes = _Context.BilhetesCarrinho.Where(b => b.IdCarrinho.Equals(carrinho.Id)).ToList();

                    var OrderBumps = _Context.OrderBump.Join(_Context.OrderBumpsEscolhidos.
                        Join(_Context.BilhetesCarrinho.Where(b => b.IdCarrinho.Equals(carrinho.Id)), o => o.IdBilhete, b => b.Id, (o, b) => o), o => o.Id, oe => oe.IdOrderBump, (o, oe) => o).GroupBy(o => o.Id).ToList();

                    List<OrderBumpsEscolhidosViewModel> Orders = new List<OrderBumpsEscolhidosViewModel>();

                    foreach (IGrouping<long, OrderBump> order in OrderBumps)
                    {
                        OrderBump FindedOrder = _Context.OrderBump.FirstOrDefault(o => o.Id.Equals(order.Key));

                        OrderBumpsEscolhidosViewModel NewOrder = new OrderBumpsEscolhidosViewModel()
                        {
                            Id = FindedOrder.Id,
                            Nome = FindedOrder.Nome,
                            Descricao = FindedOrder.Descricao,
                            Imagem = FindedOrder.Imagem,
                            NumerosRifas = _Context.BilhetesCarrinho.Join(_Context.OrderBumpsEscolhidos.Where(o => o.IdOrderBump.Equals(FindedOrder.Id)), b => b.Id, o => o.IdBilhete, (b, o) => b).Where(b => b.IdCarrinho.Equals(carrinho.Id)).Select(b => b.NumeroBilhete).ToList()
                        };

                        Orders.Add(NewOrder);
                    }

                    MinhasComprasViewModel data = new MinhasComprasViewModel();

                    data.Id = carrinho.Id;
                    data.DadosUsuario = _Context.Usuario.First(u => u.Id.Equals(carrinho.IdUsuario));
                    data.CodigoVendedor = carrinho.Compras.First().CodigoVendedor;
                    data.DataCompra = carrinho.Compras.First().DataCompra;
                    data.Status = FormataStatus(carrinho.Compras.First().Status);
                    data.UrlBoleto = string.IsNullOrEmpty(carrinho.Compras.First().UrlBoleto) ? "" : carrinho.Compras.First().UrlBoleto;
                    data.ValorCompra = "R$ " + carrinho.Compras.First().ValorCompra.Replace('.', ',');
                    data.OrderBumps = Orders;
                    data.Bilhetes = Bilhetes;
                    data.Premio = FindedProduto;
                    data.NomeProduto = FindedProduto.Nome;


                    MinhasCompras.Add(data);
                }

            }


            if (!string.IsNullOrEmpty(Status) && Status != "TodasCompras")
            {
                MinhasCompras = MinhasCompras.Where(m => m.Status.Equals(Status)).ToList();
                ViewBag.Status = Status;
            }

            if (!string.IsNullOrEmpty(NomeUsuario))
            {
                MinhasCompras = MinhasCompras.Where(m => m.DadosUsuario.Nome.ToLower().Contains(NomeUsuario.ToLower())).ToList();
                ViewBag.NomeUsuario = NomeUsuario;
            }

            return View(MinhasCompras);
        }


        public JsonResult StatusVendedor(int id)
        {
            var Vendendor = _Context.Vendedor.FirstOrDefault(v => v.Id.Equals(id));

            Vendendor.Status = Vendendor.Status ? false : true;

            _Context.Entry(Vendendor).State = System.Data.Entity.EntityState.Modified;
            _Context.SaveChanges();

            if (Vendendor.Status)
            {
                return Json(new
                {
                    classe = "btn btn-danger",
                    icone = "fa fa-ban",
                    text = "Bloquear",
                    status = "Ativo"
                });
            }

            return Json(new
            {
                classe = "btn btn-success",
                icone = "fa fa-check",
                text = "Ativar",
                status = "Bloqueado"
            });
        }


    }
}