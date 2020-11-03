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
            GetAllOrders(IdProduto);
            if (IdProduto != null)
            {
                var Produto = _Context.Produto.FirstOrDefault(p => p.Id == IdProduto);


                if (Produto == null)
                {
                    return RedirectToAction("Index");
                }

                ViewBag.Imagens = _Context.Multimidia.Where(p => p.IdProduto.Equals(IdProduto) && p.Status).ToList();
                ViewBag.Usuario = _Context.Usuario.FirstOrDefault(u => u.Id.Equals(Produto.IdUsuario));
                ViewBag.Comprados = _Context.Compra.Select(c => c.NumeroRifa).ToList();
                ViewBag.Categoria = _Context.CategoriaProduto.FirstOrDefault(c => c.Id.Equals(Produto.IdCategoria)).Nome;

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
            });
        }

        //public ActionResult RegistrarCompra(long IdProduto, string CodigoVendedor,string IdCompra,string NumeroRifa)
        //{
        //    try
        //    {
        //        Usuario UsuarioLogado = (Usuario)Session["Usuario"];

        //        Compra NovaCompra = new Compra()
        //        {
        //            CodigoVendedor = CodigoVendedor,
        //            DataCompra = DateTime.Now,
        //            ProdutoId = IdProduto,
        //            UsuarioId = UsuarioLogado.Id,
        //            IdCompra = IdCompra,
        //            NumeroRifa = NumeroRifa
        //        };

        //        _Context.Compra.Add(NovaCompra);
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


        public JsonResult RegistrarCompra(int[] Bilhete, int IdProduto,string ValorRifa)
        {
            try
            {

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

                return Json(true);

            }
            catch (Exception e)
            {
                return Json(false);
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


        public JsonResult LoginPagamento(string Email,string Senha)
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
                        NomeLogado = oUsuario.Nome+" "+oUsuario.SobreNome,
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

            catch(Exception e)
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