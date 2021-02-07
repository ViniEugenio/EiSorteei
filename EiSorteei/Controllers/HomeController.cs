using EiSorteei.Data;
using EiSorteei.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace EiSorteei.Controllers
{
    public class HomeController : Controller
    {
        private readonly EiSorteeiEntities _Context;

        public HomeController()
        {
            _Context = Context.GetContext();
        }
        public ActionResult Index()
        {
            var Produtos = _Context.Produto.Where(p => p.Status).ToList();

            List<ProdutoHomeViewModel> model = new List<ProdutoHomeViewModel>();

            foreach (var x in Produtos)
            {
                ProdutoHomeViewModel data = new ProdutoHomeViewModel()
                {
                    Id = x.Id,
                    Imagem = _Context.Multimidia.Where(p => p.IdProduto.Equals(x.Id) && p.Status).Select(p => p.Caminho).ToList(),
                    Nome = x.Nome,
                    ValorRifa = x.ValorRifa,
                    DataSorteio = Convert.ToDateTime(x.DataSorteio)
                };

                model.Add(data);
            }

            ViewBag.Produtos = model;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Pagamento()
        {
            return View();
        }

        public ActionResult Regulamento()
        {
            return View();
        }
    }
}