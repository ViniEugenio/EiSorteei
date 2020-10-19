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
            var Produto = _Context.Produto.FirstOrDefault(p => p.Id == IdProduto);
            ViewBag.Comprados = _Context.Compra.Select(c => c.NumeroRifa).ToList();

            if (Produto == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Imagens = _Context.Multimidia.Where(p => p.IdProduto.Equals(IdProduto) && p.Status).ToList();
            return View(Produto);
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
    }
}