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
            GetAllOrders();
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
        
        public void GetAllOrders()
        {
            var client = new RestClient("https://accounts.cartx.io/api/ei-sorteei/orders?created_at_min&created_at_max&updated_at_min&updated_at_max&status");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("AUTHORIZATION", "Bearer NRw70HKslPmlUkmd8omZkGahmVdYowrg8gUnZn55BPxSefz32AavjL5i13Dl");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }

    }
}