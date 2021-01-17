using EiSorteei.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MercadoPago;
using MercadoPago.Resources;
using MercadoPago.DataStructures.Preference;
using MercadoPago.Common;
using EiSorteei.Models;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Globalization;
using EiSorteei.Helpers;
using System.Threading;

namespace EiSorteei.Controllers
{
    public class ComprarController : Controller
    {
        private readonly EiSorteeiEntities _Context;

        public ComprarController()
        {
            _Context = Context.GetContext();
        }

        public ActionResult Index(long IdProduto, string CodVendedor = "")
        {

            if (IdProduto != 0)
            {
                var Produto = _Context.Produto.FirstOrDefault(p => p.Id == IdProduto);

                if (Produto == null)
                {
                    return RedirectToAction("Index");
                }

                ViewBag.Imagens = _Context.Multimidia.Where(p => p.IdProduto.Equals(IdProduto) && p.Status).ToList();
                ViewBag.Usuario = _Context.Usuario.FirstOrDefault(u => u.Id.Equals(Produto.IdUsuario));
                ViewBag.Categoria = _Context.CategoriaProduto.FirstOrDefault(c => c.Id.Equals(Produto.IdCategoria)).Nome;

                List<Compras> TodasCompras = _Context.Compras.Join(_Context.Carrinho.Where(carrinho => carrinho.IdProduto.Equals(IdProduto)),
                                                                        compra => compra.CarrinhoId, carrinho => carrinho.Id, (compra, carrinho) => compra).Where(c => c.Status != "cancelled").ToList();

                List<Compras> ComprasAprovadas = TodasCompras.Join(_Context.Carrinho.Where(carrinho => carrinho.IdProduto.Equals(IdProduto)),
                                                                        compra => compra.CarrinhoId, carrinho => carrinho.Id, (compra, carrinho) => compra)
                                                                        .Where(c => c.Status == "approved").ToList();

                Thread AtualizaCompras = new Thread(() =>
                {
                    List<string> NumeroComprados = new List<string>();

                    foreach (var x in ComprasAprovadas)
                    {
                        List<BilhetesCarrinho> Bilhetes = _Context.BilhetesCarrinho.Where(b => b.IdCarrinho.Equals(x.CarrinhoId)).ToList();
                        foreach (var bilhete in Bilhetes)
                        {
                            NumeroComprados.Add(bilhete.NumeroBilhete.ToString());
                        }
                    }

                    List<TempBilhetes> BilhetesReservados = _Context.TempBilhetes.Where(p => p.IdProduto == IdProduto).ToList();
                    List<string> NumerosReservados = new List<string>();

                    foreach (var x in BilhetesReservados)
                    {
                        DateTime DataBilhete = DateTime.ParseExact(x.DataCadastro, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime DataAtual = DateTime.Now;

                        var DiferencaDatas = DataAtual.Subtract(DataBilhete);

                        if (DiferencaDatas.Hours < 1)
                        {
                            NumerosReservados.Add(x.NumeroBilhete);
                        }

                        else
                        {
                            _Context.Entry(x).State = System.Data.Entity.EntityState.Deleted;
                            _Context.SaveChanges();
                        }
                    }

                    List<Compras> ComprasPendentes = TodasCompras.Join(_Context.Carrinho.Where(carrinho => carrinho.IdProduto.Equals(IdProduto)),
                                                                            compra => compra.CarrinhoId, carrinho => carrinho.Id, (compra, carrinho) => compra)
                                                                            .Where(c => c.Status == "pending" || c.Status == "in_process").ToList();
                    foreach (var x in ComprasPendentes)
                    {
                        string retorno = GetOrder(x.IdCompra);
                        if (retorno == "pending" || retorno == "in_process")
                        {
                            List<BilhetesCarrinho> Bilhetes = _Context.BilhetesCarrinho.Where(b => b.IdCarrinho.Equals(x.CarrinhoId)).ToList();
                            foreach (var bilhete in Bilhetes)
                            {
                                NumerosReservados.Add(bilhete.NumeroBilhete.ToString());
                            }
                        }

                        else if (retorno == "cancelled" || retorno == "rejected")
                        {
                            x.Status = retorno;
                            _Context.Entry(x).State = System.Data.Entity.EntityState.Modified;
                            _Context.SaveChanges();
                        }

                        else if (retorno == "approved")
                        {
                            List<BilhetesCarrinho> Bilhetes = _Context.BilhetesCarrinho.Where(b => b.IdCarrinho.Equals(x.CarrinhoId)).ToList();
                            foreach (var bilhete in Bilhetes)
                            {
                                NumeroComprados.Add(bilhete.NumeroBilhete.ToString());
                            }
                        }
                    }

                    ViewBag.BilhetesReservados = NumerosReservados.ToArray();
                    ViewBag.CountComprados = NumeroComprados.Count();
                    string[] Comprados = NumeroComprados.ToArray();
                    ViewBag.Comprados = Comprados;
                });

                AtualizaCompras.Start();
                AtualizaCompras.Join();

                ViewBag.CodVendedor = CodVendedor;

                return View(Produto);
            }

            return RedirectToAction("Index", "Home");
        }


        public ActionResult GetDadosUsuario(long IdProduto)
        {

            Produto oProduto = _Context.Produto.FirstOrDefault(p => p.Id.Equals(IdProduto));
            Usuario UsuarioLogado = (Usuario)Session["Usuario"];

            if (UsuarioLogado != null)
            {
                return Json(new
                {
                    Status = true,
                    UsuarioLogado.Nome,
                    UsuarioLogado.SobreNome,
                    UsuarioLogado.Email,
                    UsuarioLogado.Estado,
                    UsuarioLogado.Cidade,
                    UsuarioLogado.Bairro,
                    UsuarioLogado.Rua,
                    UsuarioLogado.Numero,
                    UsuarioLogado.Telefone,
                    UsuarioLogado.CEP,
                    IdProduto = oProduto.Id,
                    oProduto.ValorRifa,
                    NomeProduto = oProduto.Nome,
                    DataCadastro = DateTime.Now,
                    UsuarioLogado.Cpf
                });
            }

            return Json(new
            {
                Status = false
            });

        }

        public ActionResult Pagamento(string CodVendedor)
        {
            ViewBag.Estados = Estados.GetAllStates();

            HttpCookie FindedCookie = Request.Cookies["Carrinho"];

            if (FindedCookie == null)
            {
                return RedirectToAction("Index", "Home");
            }

            else
            {
                if (DateTime.ParseExact(FindedCookie["Validade"], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture) <= DateTime.Now)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            Usuario UsuarioLogado = (Usuario)Session["Usuario"];
            long IdProduto = Convert.ToInt64(FindedCookie["IdProduto"]);
            Produto FindedProduto = _Context.Produto.FirstOrDefault(p => p.Id.Equals(IdProduto));

            PagamentoViewModel model = new PagamentoViewModel()
            {
                Bilhetes = FindedCookie["Bilhetes"],
                Usuario = UsuarioLogado,
                Produto = FindedProduto,
                Imagens = _Context.Multimidia.Where(m => m.IdProduto.Equals(FindedProduto.Id) && m.Status).ToList(),
                ValorTotal = FindedCookie["ValorTotal"],
                OrderBumps = _Context.OrderBump.Join(_Context.OrderBumps_Produto.Where(o => o.Status && o.IdProduto.Equals(IdProduto)), o => o.Id, op => op.IdOrderBump, (o, op) => o).Where(o => o.Status).ToList()
            };


            ViewBag.CodVendedor = CodVendedor;

            return View(model);
        }


        public JsonResult RegistrarCompra(int[] Bilhete, int IdProduto, string ValorRifa)
        {
            try
            {
                List<int> BilhetesRepetidos = new List<int>();

                for (int x = 0; x < Bilhete.Length; x++)
                {
                    string BilheteIndex = Bilhete[x].ToString();
                    if (_Context.TempBilhetes.Any(b => b.IdProduto.Equals(IdProduto) && b.NumeroBilhete == BilheteIndex))
                    {
                        var FindedBilhete = _Context.TempBilhetes.FirstOrDefault(b => b.IdProduto == IdProduto && b.NumeroBilhete == BilheteIndex);

                        DateTime DataBilhete = DateTime.ParseExact(FindedBilhete.DataCadastro, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        DateTime DataAtual = DateTime.Now;

                        var DiferencaDatas = DataAtual.Subtract(DataBilhete);

                        if (DiferencaDatas.Hours < 1)
                        {
                            BilhetesRepetidos.Add(Bilhete[x]);
                        }

                        else
                        {
                            _Context.Entry(FindedBilhete).State = System.Data.Entity.EntityState.Deleted;
                            _Context.SaveChanges();
                        }
                    }
                }

                string BilhetesInvalidosMessage = "";
                if (BilhetesRepetidos.Count > 0)
                {
                    foreach (var x in BilhetesRepetidos)
                    {
                        BilhetesInvalidosMessage = BilhetesRepetidos.IndexOf(x) != BilhetesRepetidos.Count - 1 ? BilhetesInvalidosMessage + x + ", " : BilhetesInvalidosMessage + x.ToString();
                    }

                    return Json(new
                    {
                        Status = false,
                        Mensagem = "Bilhetes Inválidos",
                        BilhetesInvalidosMessage,
                        BilhetesInvalidos = BilhetesRepetidos.ToArray()
                    });
                }

                BilhetesRepetidos.Clear();

                List<Carrinho> Carrinhos = _Context.Carrinho.Where(c => c.IdProduto == IdProduto && c.Status).ToList();

                foreach (var x in Carrinhos)
                {
                    Compras FindedCompra = _Context.Compras.FirstOrDefault(c => c.CarrinhoId.Equals(x.Id));
                    if (FindedCompra != null)
                    {
                        if (FindedCompra.Status == "approved" || FindedCompra.Status == "pending" || FindedCompra.Status == "in_process")
                        {
                            List<BilhetesCarrinho> BilhetesDoCarrinho = _Context.BilhetesCarrinho.Where(b => b.IdCarrinho.Equals(x.Id)).ToList();
                            foreach (var bilhete in BilhetesDoCarrinho)
                            {
                                for (int y = 0; y < Bilhete.Length; y++)
                                {
                                    if (bilhete.NumeroBilhete == Bilhete[y])
                                    {
                                        BilhetesRepetidos.Add(bilhete.NumeroBilhete);
                                    }
                                }
                            }
                        }

                    }

                }

                BilhetesInvalidosMessage = "";
                if (BilhetesRepetidos.Count > 0)
                {
                    foreach (var x in BilhetesRepetidos)
                    {
                        BilhetesInvalidosMessage = BilhetesRepetidos.IndexOf(x) == BilhetesRepetidos.Count - 1 ? BilhetesInvalidosMessage + x : BilhetesInvalidosMessage + ", " + x;
                    }

                    return Json(new
                    {
                        Status = false,
                        Mensagem = "Bilhetes Inválidos",
                        BilhetesInvalidosMessage,
                        BilhetesInvalidos = BilhetesRepetidos.ToArray()
                    });
                }


                for (int x = 0; x < Bilhete.Length; x++)
                {
                    TempBilhetes NovoBilheteTemporario = new TempBilhetes()
                    {
                        DataCadastro = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                        IdProduto = IdProduto,
                        NumeroBilhete = Bilhete[x].ToString(),
                    };

                    _Context.TempBilhetes.Add(NovoBilheteTemporario);
                    _Context.SaveChanges();
                }

                HttpCookie FindedCookie = Request.Cookies["Carrinho"];

                if (FindedCookie != null)
                {
                    FindedCookie.Expires = DateTime.Now.AddYears(-1);
                    Response.Cookies.Set(FindedCookie);
                }

                string BilhetesFormatados = "";

                for (int x = 0; x < Bilhete.Length; x++)
                {
                    BilhetesFormatados = +x == Bilhete.Length - 1 ? BilhetesFormatados + Bilhete[x].ToString() : BilhetesFormatados + Bilhete[x].ToString() + ", ";
                }

                HttpCookie NovoCookie = new HttpCookie("Carrinho");
                NovoCookie.Values.Add("Bilhetes", BilhetesFormatados);
                NovoCookie.Values.Add("IdProduto", IdProduto.ToString());
                NovoCookie.Values.Add("Validade", DateTime.Now.AddMinutes(1).ToString("dd/MM/yyyy HH:mm:ss"));
                NovoCookie.Values.Add("ValorTotal", ValorRifa);
                Response.Cookies.Add(NovoCookie);


                return Json(new
                {
                    Status = true,
                    Mensagem = "Compra Válidada"
                });

            }
            catch (Exception e)
            {
                return Json(new
                {
                    Status = false,
                    Mensagem = "Erro no processamento"
                });
            }
        }


        public string GetOrder(string IdCompra)
        {

            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            WebRequest request = WebRequest.Create("https://api.mercadopago.com/v1/payments/search?id=" + IdCompra);
            request.Headers.Add("AUTHORIZATION", "Bearer APP_USR-5017428128263404-072015-ddd04542ab6288891aebac5789addf33-247787081");
            request.Method = "GET";
            var response = (HttpWebResponse)request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream());
            var result = streamReader.ReadToEnd();
            JObject Objeto = JObject.Parse(result);
            return (string)Objeto["results"][0]["status"];
        }


        public JsonResult LoginPagamento(string Email, string Senha)
        {
            try
            {
                string HashPassword = Helpers.HashPassword.PasswordHash(Senha);

                if (_Context.Usuario.Any(u => u.Email.Equals(Email) && u.Senha.Equals(HashPassword)))
                {
                    Usuario oUsuario = _Context.Usuario.FirstOrDefault(u => u.Email.Equals(Email) && u.Senha.Equals(HashPassword));
                    Session["Usuario"] = oUsuario;
                    long IdPermissao = _Context.PermissaoUsuario.FirstOrDefault(p => p.IdUsuario.Equals(oUsuario.Id)).IdPermissao;
                    Session["Permissao"] = _Context.Permissao.FirstOrDefault(p => p.Id.Equals(IdPermissao)).Nome;

                    return Json(new
                    {
                        Status = true,
                        Mensagem = "Usuário Logado",
                        NomeLogado = oUsuario.Nome + " " + oUsuario.SobreNome,
                        EmailLogado = oUsuario.Email,
                        TelefoneLogado = oUsuario.Telefone,
                        CPFLogado = oUsuario.Cpf,
                        CEPLogado = oUsuario.CEP,
                        EstadoLogado = oUsuario.Estado,
                        CidadeLogado = oUsuario.Cidade,
                        BairroLogado = oUsuario.Bairro,
                        RuaLogado = oUsuario.Rua,
                        NumeroLogado = oUsuario.Numero
                    });
                }

                return Json(new
                {
                    Status = true,
                    Mensagem = "Usuário não encontrado"
                });
            }

            catch (Exception e)
            {
                return Json(new
                {
                    Status = false,
                    Mensagem = "Erro"
                });
            }

        }

        public JsonResult GetInformacoesUsuario(long IdUsuario)
        {
            try
            {
                if (_Context.Usuario.Any(u => u.Id.Equals(IdUsuario)))
                {
                    Usuario oUsuario = _Context.Usuario.FirstOrDefault(u => u.Id.Equals(IdUsuario));

                    return Json(new
                    {
                        Status = true,
                        Mensagem = "Usuário Logado",
                        NomeLogado = oUsuario.Nome + " " + oUsuario.SobreNome,
                        EmailLogado = oUsuario.Email,
                        TelefoneLogado = oUsuario.Telefone,
                        CPFLogado = oUsuario.Cpf,
                        CEPLogado = oUsuario.CEP,
                        EstadoLogado = oUsuario.Estado,
                        CidadeLogado = oUsuario.Cidade,
                        BairroLogado = oUsuario.Bairro,
                        RuaLogado = oUsuario.Rua,
                        NumeroLogado = oUsuario.Numero
                    });
                }

                return Json(new
                {
                    Status = true,
                    Mensagem = "Usuário não encontrado"
                });
            }

            catch (Exception e)
            {
                return Json(new
                {
                    Status = false,
                    Mensagem = "Erro"
                });
            }
        }

        public JsonResult VerificaBilhetes(long IdProduto, string Bilhetes)
        {
            string[] BilhetesFormatados = Bilhetes.Split(',');
            List<int> BilhetesRepetidos = new List<int>();

            List<Carrinho> Carrinhos = _Context.Carrinho.Where(c => c.IdProduto == IdProduto && c.Status).ToList();
            foreach (var x in Carrinhos)
            {
                Compras FindedCompra = _Context.Compras.FirstOrDefault(c => c.CarrinhoId.Equals(x.Id));
                if (FindedCompra != null)
                {
                    if (FindedCompra.Status == "approved" || FindedCompra.Status == "pending" || FindedCompra.Status == "in_process")
                    {
                        List<BilhetesCarrinho> BilhetesDoCarrinho = _Context.BilhetesCarrinho.Where(b => b.IdCarrinho.Equals(x.Id)).ToList();
                        foreach (var bilhete in BilhetesDoCarrinho)
                        {
                            if (BilhetesFormatados.Contains(bilhete.NumeroBilhete.ToString()))
                            {
                                BilhetesRepetidos.Add(bilhete.NumeroBilhete);
                            }
                        }
                    }

                }

            }

            string BilhetesInvalidosMessage = "";
            if (BilhetesRepetidos.Count > 0)
            {
                foreach (var x in BilhetesRepetidos)
                {
                    BilhetesInvalidosMessage = BilhetesRepetidos.IndexOf(x) == BilhetesRepetidos.Count - 1 ? BilhetesInvalidosMessage + x : BilhetesInvalidosMessage + ", " + x;
                }

                return Json(new
                {
                    Status = false,
                    Mensagem = "Bilhetes Inválidos",
                    BilhetesInvalidosMessage,
                    JsonRequestBehavior.AllowGet
                });
            }

            return Json(new
            {
                Status = true
            });
        }


        public JsonResult CompraRealizada(string Status, long IdProduto, string IdCompra, string TipoPagamento, string Bilhetes, string CodigoVendedor, string UrlBoleto, List<Models.OrderBumpsEscolhidos> OrderBumps, string ValorCompra)
        {
            try
            {
                string[] BilhetesFormatados = Bilhetes.Split(',');

                Usuario UsuarioLogado = (Usuario)Session["Usuario"];
                Carrinho NovoCarrinho = new Carrinho()
                {
                    DataCadastro = DateTime.Now,
                    IdProduto = IdProduto,
                    IdUsuario = UsuarioLogado.Id,
                    Status = true
                };

                _Context.Carrinho.Add(NovoCarrinho);
                _Context.SaveChanges();

                for (int x = 0; x < BilhetesFormatados.Count(); x++)
                {
                    int NumeroBilhete = Convert.ToInt32(BilhetesFormatados[x]);
                    BilhetesCarrinho NovoBilhete = new BilhetesCarrinho()
                    {
                        DataCadastro = DateTime.Now,
                        IdCarrinho = NovoCarrinho.Id,
                        NumeroBilhete = NumeroBilhete,
                    };

                    _Context.BilhetesCarrinho.Add(NovoBilhete);

                    TempBilhetes FindedBilhete = _Context.TempBilhetes.FirstOrDefault(b => b.NumeroBilhete == NumeroBilhete.ToString() && b.IdProduto == IdProduto);
                    if (FindedBilhete != null)
                    {
                        _Context.Entry(FindedBilhete).State = System.Data.Entity.EntityState.Deleted;
                        _Context.SaveChanges();
                    }

                    if (OrderBumps != null)
                    {
                        var OrderBumpsFiltradas = OrderBumps.Where(o => o.NumeroBilhete.Trim() == NumeroBilhete.ToString()).ToList();
                        foreach (var order in OrderBumpsFiltradas)
                        {
                            Data.OrderBumpsEscolhidos NovoOrder = new Data.OrderBumpsEscolhidos()
                            {
                                DataCadastro = DateTime.Now,
                                IdOrderBump = Convert.ToInt64(order.IdOrderBump),
                                IdBilhete = NovoBilhete.Id,
                            };

                            _Context.OrderBumpsEscolhidos.Add(NovoOrder);
                            _Context.SaveChanges();
                        }
                    }

                }

                Compras NovaCompra = new Compras()
                {
                    CarrinhoId = NovoCarrinho.Id,
                    DataCompra = DateTime.Now,
                    Status = Status,
                    IdCompra = IdCompra,
                    CodigoVendedor = CodigoVendedor,
                    UrlBoleto = UrlBoleto,
                    ValorCompra = ValorCompra
                };

                _Context.Compras.Add(NovaCompra);
                _Context.SaveChanges();


                string Mensagem = "Obrigado por comprar conosco, seus Bilhetes foram reservados,";
                if (Status == "pending" && TipoPagamento == "Boleto")
                {
                    Mensagem = Mensagem + " efetue o pagamento do boleto para que o seus bilhetes fiquem válidos para o sorteio.";
                }

                else if (Status == "pending" && TipoPagamento != "Boleto")
                {
                    Mensagem = Mensagem + " sua compra está sendo analisada e em breve entraremos em contato com você para mais informações.";
                }


                HttpCookie FindedCookie = Response.Cookies["Carrinho"];
                FindedCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Set(FindedCookie);

                return Json(new
                {
                    Status = true,
                    Mensagem,
                    JsonRequestBehavior.AllowGet
                });
            }

            catch (Exception e)
            {
                return Json(new
                {
                    Status = false,
                    Mensagem = "Não foi possível realizar sua compra tente novamente mais tarde",
                    JsonRequestBehavior.AllowGet
                });
            }
        }

        public JsonResult CadastrarUsuario(UsuarioViewModel model)
        {

            try
            {

                if (_Context.Usuario.Any(u => u.Email.Equals(model.Email)))
                {
                    return Json(new
                    {
                        Status = false,
                        Mensagem = "O Email informado já está sendo usado por outro usuário."
                    });
                }

                if (_Context.Usuario.Any(u => u.Cpf.Equals(model.Cpf)))
                {
                    return Json(new
                    {
                        Status = false,
                        Mensagem = "O CPF informado já está sendo usado por outro usuário."
                    });
                }

                Usuario oUsuario = new Usuario()
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

                _Context.Usuario.Add(oUsuario);
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
                    IdUsuario = oUsuario.Id,
                    Status = true,
                    DataCadastro = DateTime.Now
                };

                _Context.PermissaoUsuario.Add(Permissao);
                _Context.SaveChanges();

                Usuario NovoUsuario = _Context.Usuario.FirstOrDefault(u => u.Id.Equals(oUsuario.Id));
                Session["Usuario"] = NovoUsuario;
                IdPermissao = _Context.PermissaoUsuario.FirstOrDefault(p => p.IdUsuario.Equals(oUsuario.Id)).IdPermissao;
                Session["Permissao"] = _Context.Permissao.FirstOrDefault(p => p.Id.Equals(IdPermissao)).Nome;

                return Json(new
                {
                    Status = true,
                    Mensagem = "Usuário Logado",
                    NomeLogado = oUsuario.Nome + " " + oUsuario.SobreNome,
                    EmailLogado = oUsuario.Email,
                    TelefoneLogado = oUsuario.Telefone,
                    CPFLogado = oUsuario.Cpf,
                    CEPLogado = oUsuario.CEP,
                    EstadoLogado = oUsuario.Estado,
                    CidadeLogado = oUsuario.Cidade,
                    BairroLogado = oUsuario.Bairro,
                    RuaLogado = oUsuario.Rua,
                    NumeroLogado = oUsuario.Numero
                });
            }

            catch (Exception e)
            {
                return Json(new
                {
                    Status = false,
                    Mensagem = "Não foi possível realizar o cadastro."
                });
            }

        }

    }
}