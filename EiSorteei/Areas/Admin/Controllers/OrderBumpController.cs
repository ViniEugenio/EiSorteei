using EiSorteei.Data;
using EiSorteei.Helpers;
using EiSorteei.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EiSorteei.Areas.Admin.Controllers
{
    [FilterLogin]
    public class OrderBumpController : Controller
    {
        private readonly EiSorteeiEntities _Context;

        public OrderBumpController()
        {
            _Context = new EiSorteeiEntities();
        }

        public ActionResult Index()
        {
            var model = _Context.OrderBump.Where(o => o.Status).ToList();
            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(OrderBumpViewModel model)
        {
            if (ModelState.IsValid)
            {
                Decimal ValorFormatado = Convert.ToDecimal(model.Valor, new CultureInfo("en-Us"));

                OrderBump NovoOrderBump = new OrderBump()
                {
                    DataCadastro = DateTime.Now,
                    Descricao = model.Descricao,
                    Nome = model.Nome,
                    Status = true,
                    Valor = Math.Round(ValorFormatado, 2)
                };

                string Caminho = Server.MapPath("~/Content/ImagensOrderBumps/");
                string NomeArquivo = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + model.Imagem.FileName;
                using (var stream = new FileStream(Caminho + NomeArquivo, FileMode.Create))
                {
                    using (Stream inputStream = model.Imagem.InputStream)
                    {
                        inputStream.CopyTo(stream);
                    }
                }

                NovoOrderBump.Imagem = NomeArquivo;

                _Context.OrderBump.Add(NovoOrderBump);
                _Context.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.Descricao = model.Descricao;
            return View(model);
        }

        public ActionResult Excluir(long Id)
        {

            try
            {
                var order = _Context.OrderBump.FirstOrDefault(p => p.Id.Equals(Id));
                order.Status = false;
                _Context.Entry(order).State = System.Data.Entity.EntityState.Modified;
                _Context.SaveChanges();

                return Json(new
                {
                    Status = true,
                    Mensagem = "Order Bump excluído com sucesso!"
                });
            }

            catch (Exception e)
            {
                return Json(new
                {
                    Status = false,
                    Mensagem = "Erro!"
                });
            }

        }

        public ActionResult Details(long Id)
        {
            var model = _Context.OrderBump.FirstOrDefault(o => o.Id.Equals(Id));

            if (model != null)
            {
                return View(model);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Alterar(long Id)
        {
            var model = _Context.OrderBump.FirstOrDefault(o => o.Id.Equals(Id));

            if (model != null)
            {
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Alterar(OrderBump model, HttpPostedFileBase Imagem, string ValorOrder)
        {
            if (ModelState.IsValid)
            {
                if (Imagem != null)
                {
                    string Caminho = Server.MapPath("~/Content/ImagensOrderBumps/");

                    FileInfo ImagemAtual = new FileInfo(Caminho + model.Imagem);
                    if (ImagemAtual.Exists)
                    {
                        ImagemAtual.Delete();
                    }

                    string NomeArquivo = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + Imagem.FileName;
                    using (var stream = new FileStream(Caminho + NomeArquivo, FileMode.Create))
                    {
                        using (Stream inputStream = Imagem.InputStream)
                        {
                            inputStream.CopyTo(stream);
                        }
                    }

                    model.Imagem = NomeArquivo;
                }

                Decimal ValorFormatado = Convert.ToDecimal(ValorOrder, new CultureInfo("en-Us"));
                model.Valor = ValorFormatado;
                _Context.Entry(model).State = System.Data.Entity.EntityState.Modified;
                _Context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
            
        }

    }
}
