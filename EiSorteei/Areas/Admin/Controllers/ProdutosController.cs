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

            if (model.Imagem.Count() == 0)
            {
                if (model.Imagem[0] == null)
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
                    if (imagem != null)
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

                }

                return RedirectToAction("Index");

            }

            return View(model);
        }


        public void LoadViewBags()
        {
            ViewBag.Categorias = _Context.CategoriaProduto.Where(c => c.Status).ToList();
        }

        public ActionResult Excluir(long Id)
        {

            try
            {
                var produto = _Context.Produto.FirstOrDefault(p => p.Id.Equals(Id));
                produto.Status = false;
                _Context.Entry(produto).State = System.Data.Entity.EntityState.Modified;
                _Context.SaveChanges();

                return Json(new
                {
                    Status = true,
                    Mensagem = "Prêmio excluído com sucesso!"
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
            var produto = _Context.Produto.FirstOrDefault(p => p.Id.Equals(Id));
            ViewBag.Imagem = _Context.Multimidia.FirstOrDefault(p => p.IdProduto.Equals(Id) && p.Status).Caminho;
            ViewBag.Categoria = _Context.CategoriaProduto.FirstOrDefault(p => p.Id.Equals(produto.IdCategoria));
            return View(produto);

        }

        public ActionResult Alterar(long Id)
        {
            var produto = _Context.Produto.FirstOrDefault(p => p.Id.Equals(Id));
            ProdutoCreateViewModel model = new ProdutoCreateViewModel()
            {
                CategoriaProduto = produto.IdCategoria,
                Descricao = produto.Descricao,
                Nome = produto.Nome,
                RangeCodigo = produto.RangeCodigo,
                ValorRifa = produto.ValorRifa,
                Id = produto.Id
            };

            ViewBag.Imagens = _Context.Multimidia.Where(m => m.IdProduto.Equals(Id) && m.Status).ToList();

            LoadViewBags();
            return View(model);
        }

        public ActionResult ExcluirImagem(long Id)
        {
            var Imagem = _Context.Multimidia.FirstOrDefault(m => m.Id.Equals(Id));
            Imagem.Status = false;
            _Context.Entry(Imagem).State = System.Data.Entity.EntityState.Modified;
            _Context.SaveChanges();

            return new EmptyResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Alterar(ProdutoCreateViewModel model)
        {
            LoadViewBags();

            if (model.Imagem.Count() == 0 && _Context.Multimidia.Where(m => m.IdProduto.Equals(model.Id)).Count() == 0)
            {
                if (model.Imagem[0] == null)
                {
                    ModelState.AddModelError("Imagem", "Por favor selecione pelo menos uma imagem para o Produto");
                }
            }

            if (ModelState.IsValid)
            {
                Produto AlterarProduto = _Context.Produto.FirstOrDefault(p => p.Id.Equals(model.Id));
                AlterarProduto.Nome = model.Nome;
                AlterarProduto.Descricao = model.Descricao;
                AlterarProduto.IdCategoria = model.CategoriaProduto;
                AlterarProduto.DataAtualizacao = DateTime.Now;
                AlterarProduto.RangeCodigo = model.RangeCodigo;
                AlterarProduto.ValorRifa = model.ValorRifa;

                _Context.Entry(AlterarProduto).State = System.Data.Entity.EntityState.Modified;
                _Context.SaveChanges();


                string Caminho = Server.MapPath("~/Content/ImagemProdutos/");
                foreach (var imagem in model.Imagem)
                {
                    if (imagem != null)
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
                            IdProduto = AlterarProduto.Id,
                            Status = true
                        };

                        _Context.Multimidia.Add(NovaMultimidia);
                        _Context.SaveChanges();
                    }
                }



                return RedirectToAction("Index");

            }

            return View(model);
        }


    }
}