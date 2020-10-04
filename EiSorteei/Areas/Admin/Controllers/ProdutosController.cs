using EiSorteei.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EiSorteei.Areas.Admin.Controllers
{

    public class ProdutosController : Controller
    {
        private readonly EiSorteeiEntities _Context;

        public ProdutosController()
        {
            _Context = Context.GetContext();
        }

        // GET: Admin/Produtos
        public ActionResult Index()
        {
            var Produtos = _Context.Produto.Where(p => p.Status).ToList();
            return View(Produtos);
        }

        public ActionResult Create()
        {
            return View();
        }
    }
}