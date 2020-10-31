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
using RestSharp;
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
            if (IdProduto!=null)
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

        public ActionResult RegistrarCompra(long IdProduto, string CodigoVendedor,string IdCompra,string NumeroRifa)
        {
            try
            {
                Usuario UsuarioLogado = (Usuario)Session["Usuario"];

                Compra NovaCompra = new Compra()
                {
                    CodigoVendedor = CodigoVendedor,
                    DataCompra = DateTime.Now,
                    ProdutoId = IdProduto,
                    UsuarioId = UsuarioLogado.Id,
                    IdCompra = IdCompra,
                    NumeroRifa = NumeroRifa
                };

                _Context.Compra.Add(NovaCompra);
                _Context.SaveChanges();

                return Json(new
                {
                    Status = true,
                });
            }

            catch (Exception e)
            {
                return Json(new
                {
                    Status = false,
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

    }
}