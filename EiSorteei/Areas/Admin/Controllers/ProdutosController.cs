using EiSorteei.Data;
using EiSorteei.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
            LoadViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(ProdutoCreateViewModel model)
        {
            LoadViewBags();

            if (model.Imagem.Count()==1)
            {
                if(model.Imagem[0]==null)
                {
                    ModelState.AddModelError("Imagem", "Por favor selecione pelo menos uma imagem para o Produto");
                }
            }

            if (ModelState.IsValid)
            {
                Usuario UsuarioLogado = (Usuario)Session["Usuario"];

                Produto NovoProduto = new Produto()
                {
                    IdCategoria = model.CategoriaProduto,
                    DataAtualizacao = DateTime.Now,
                    DataCadastro = DateTime.Now,
                    Descricao = model.Descricao,
                    Nome = model.Nome,
                    RangeCodigo = model.RangeCodigo,
                    Status = true,
                    ValorRifa = model.ValorRifa,
                    IdUsuario = UsuarioLogado.Id
                };

                _Context.Produto.Add(NovoProduto);
                _Context.SaveChanges();

                string Caminho = Server.MapPath("~/Content/ImagemProdutos/");
                foreach (var imagem in model.Imagem)
                {
                    string NomeArquivo = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + imagem.FileName;
                    using (var stream = new FileStream(Caminho + NomeArquivo, FileMode.Create))
                    {
                        using (Stream inputStream = imagem.InputStream)
                        {
                            inputStream.CopyTo(stream);
                        }
                    }

                    Multimidia NovaMultimidia = new Multimidia()
                    {
                        Caminho = NomeArquivo,
                        DataAtualizacao = DateTime.Now,
                        DataCadastro = DateTime.Now,
                        IdProduto = NovoProduto.Id,
                        Status = true
                    };

                    _Context.Multimidia.Add(NovaMultimidia);
                    _Context.SaveChanges();

                }

                return RedirectToAction("Index");

            }

            return View(model);
        }


        public void LoadViewBags()
        {
            ViewBag.Categorias = _Context.CategoriaProduto.Where(c => c.Status).ToList();
        }
    }
}