using EiSorteei.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            ViewBag.Imagens = _Context.Multimidia.Where(p => p.IdProduto.Equals(IdProduto) && p.Status).ToList();
            return View(Produto);
        }
    }
}