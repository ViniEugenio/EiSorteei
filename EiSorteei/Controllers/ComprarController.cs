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

        public ActionResult Pagar()
        {
            MercadoPago.SDK.AccessToken = "APP_USR-5017428128263404-072015-ddd04542ab6288891aebac5789addf33-247787081";

            Preference preference = new Preference();

            preference.Items.Add(
              new Item()
              {
                  Title = "Meu produto",
                  Quantity = 1,
                  CurrencyId = CurrencyId.BRL,
                  UnitPrice = (decimal)75.56
              }
            );
            preference.Save();

            return new EmptyResult();
        }
    }
}