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

namespace EiSorteei.Controllers
{
    public class ComprarController : Controller
    {
        private readonly EiSorteeiEntities _Context;

        public ComprarController()
        {
            _Context = Context.GetContext();
        }

        public ActionResult Index(long IdProduto)
        {
            if (IdProduto != null)
            {
                var Produto = _Context.Produto.FirstOrDefault(p => p.Id == IdProduto);
                if (Produto == null)
                {
                    return RedirectToAction("Index");
                }

                ViewBag.Imagens = _Context.Multimidia.Where(p => p.IdProduto.Equals(IdProduto) && p.Status).ToList();
                ViewBag.Usuario = _Context.Usuario.FirstOrDefault(u => u.Id.Equals(Produto.IdUsuario));
                ViewBag.Categoria = _Context.CategoriaProduto.FirstOrDefault(c => c.Id.Equals(Produto.IdCategoria)).Nome;
                string[] Comprados = { "41" };
                ViewBag.Comprados = Comprados;

                return View(Produto);
            }

            return RedirectToAction("Index", "Home");
        }


        public ActionResult GetDadosUsuario(long IdProduto)
        {

            Produto oProduto = _Context.Produto.FirstOrDefault(p => p.Id.Equals(IdProduto));
            Usuario UsuarioLogado = (Usuario)Session["Usuario"];

            return Json(new
            {
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

        //public ActionResult RegistrarCompra(long IdProduto, string CodigoVendedor, string IdCompra, string StatusPagamento, string NumeroRifa[])
        //{
        //    try
        //    {
        //        Usuario UsuarioLogado = (Usuario)Session["Usuario"];

        //        _Context.SaveChanges();

        //        return Json(new
        //        {
        //            Status = true,
        //        });
        //    }

        //    catch (Exception e)
        //    {
        //        return Json(new
        //        {
        //            Status = false,
        //        });
        //    }


        //}




        public ActionResult Pagamento(long Id)
        {
            HttpCookie FindedCookie = Request.Cookies["Carrinho"];

            if (FindedCookie == null)
            {
                return RedirectToAction("Index", "Home");
            }

            //else
            //{
            //    if (Convert.ToDateTime(FindedCookie["Validade"]) <= DateTime.Now)
            //    {
            //        return RedirectToAction("Index", "Home");
            //    }
            //}

            Usuario UsuarioLogado = (Usuario)Session["Usuario"];
            long IdProduto = Convert.ToInt64(FindedCookie["IdProduto"]);
            Produto FindedProduto = _Context.Produto.FirstOrDefault(p => p.Id.Equals(IdProduto));

            PagamentoViewModel model = new PagamentoViewModel()
            {
                Bilhetes = FindedCookie["Bilhetes"],
                Usuario = UsuarioLogado,
                Produto = FindedProduto,
                Imagens = _Context.Multimidia.Where(m => m.IdProduto.Equals(FindedProduto.Id)).ToList(),
                ValorTotal = FindedCookie["ValorTotal"]
            };

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
                        DateTime DataBilhete = Convert.ToDateTime(FindedBilhete.DataCadastro);

                        var DiferencaDatas = DateTime.Now.Subtract(DataBilhete);
                        if (DiferencaDatas.Hours < 1)
                        {
                            BilhetesRepetidos.Add(Bilhete[x]);
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
                }

                if (BilhetesRepetidos.Count > 0)
                {
                    return Json(new
                    {
                        Status = false,
                        Mensagem = "Bilhetes Inválidos",
                        BilhetesInvalidosMessage,
                        BilhetesInvalidos = BilhetesRepetidos.ToArray()
                    });
                }

                BilhetesRepetidos = null;
                List<Carrinho> Carrinhos = _Context.Carrinho.Where(c => c.IdProduto.Equals(IdProduto) && c.Status).ToList();
                foreach (var carrinho in Carrinhos)
                {

                    List<BilhetesCarrinho> BilhetesDoCarrinho = _Context.BilhetesCarrinho.Where(b => b.IdCarrinho.Equals(carrinho.Id)).ToList();
                    foreach (var bilhete in BilhetesDoCarrinho)
                    {
                        if (Bilhete.Contains(bilhete.NumeroBilhete))
                        {
                            Compras FindedCompra = _Context.Compras.FirstOrDefault(c => c.CarrinhoId.Equals(bilhete.IdCarrinho));
                            if (FindedCompra.Status == "Compra Fechada")
                            {
                                BilhetesRepetidos.Append(bilhete.NumeroBilhete);
                            }
                        }
                    }
                }

                if (BilhetesRepetidos.Count > 0)
                {
                    foreach (var x in BilhetesRepetidos)
                    {
                        BilhetesInvalidosMessage = BilhetesRepetidos.IndexOf(x) == BilhetesRepetidos.Count - 1 ? BilhetesInvalidosMessage + x : BilhetesInvalidosMessage + ", " + x;
                    }
                }
                if (BilhetesRepetidos.Count > 0)
                {
                    return Json(new
                    {
                        Status = false,
                        Mensagem = "Bilhetes Inválidos",
                        BilhetesInvalidosMessage
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


        public void GetAllOrders(long IdProduto)
        {

            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            WebRequest request = WebRequest.Create("https://accounts.cartx.io/api/ei-sorteei/orders");
            request.Headers.Add("AUTHORIZATION", "Bearer NRw70HKslPmlUkmd8omZkGahmVdYowrg8gUnZn55BPxSefz32AavjL5i13Dl");
            request.Method = "GET";
            var response = (HttpWebResponse)request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream());
            var result = streamReader.ReadToEnd();

            var Objeto = JObject.Parse(result);
            var JOrders = Objeto["orders"]["data"];

            List<OrderCartX> Orders = JsonConvert.DeserializeObject<List<OrderCartX>>(JOrders.ToString());



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

    }
}