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

            if(Produto==null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Imagens = _Context.Multimidia.Where(p => p.IdProduto.Equals(IdProduto) && p.Status).ToList();
            return View(Produto);
        }


        public ActionResult GetDadosUsuario()
        {
            Usuario UsuarioLogado = (Usuario)Session["Usuario"];

            return Json(new
            {
                DadosUsuario = UsuarioLogado
            });
        }
    }
}