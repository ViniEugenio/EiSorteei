using EiSorteei.Data;
using EiSorteei.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EiSorteei.Areas.Admin.Controllers
{
    [FilterLogin]
    public class CategoriasProdutosController : Controller
    {
        private readonly EiSorteeiEntities _Context;

        public CategoriasProdutosController()
        {
            _Context = Context.GetContext();
        }

        // GET: Admin/CategoriasProdutos
        public ActionResult Index()
        {
            var Categorias = _Context.CategoriaProduto.Where(c => c.Status);
            return View(Categorias);
        }


        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CategoriaProduto model)
        {
            if (ModelState.IsValid)
            {
                model.DataCadastro = DateTime.ParseExact(DateTime.Now.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                model.DataAtualizacao = DateTime.ParseExact(DateTime.Now.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                model.Status = true;

                _Context.CategoriaProduto.Add(model);
                _Context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult Excluir(long Id)
        {

            var Categoria = _Context.CategoriaProduto.FirstOrDefault(c => c.Id.Equals(Id));
            Categoria.Status = false;
            _Context.Entry(Categoria).State = System.Data.Entity.EntityState.Modified;
            _Context.SaveChanges();
            return Json(new
            {
                Status = true,
                Mensagem = "Categoria excluída com sucesso!"
            });
        }

        public ActionResult Edit(long Id)
        {
            var Categoria = _Context.CategoriaProduto.FirstOrDefault(c => c.Id.Equals(Id));
            return View(Categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CategoriaProduto model)
        {
            if(ModelState.IsValid)
            {
                model.DataAtualizacao = DateTime.ParseExact(DateTime.Now.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);

                _Context.Entry(model).State = System.Data.Entity.EntityState.Modified;
                _Context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }

    }
}