using EiSorteei.Data;
using EiSorteei.Helpers;
using EiSorteei.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EiSorteei.Areas.Admin.Controllers
{
    [FilterLogin]
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
            Usuario UsuarioLogado = (Usuario)Session["Usuario"];
            var Produtos = _Context.Produto.Where(p => p.Status && p.IdUsuario.Equals(UsuarioLogado.Id)).ToList();
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


            if (model.Imagem[0] == null)
            {
                ModelState.AddModelError("Imagem", "Por favor selecione pelo menos uma imagem para o Produto");
            }

            if (model.DataSorteio <= DateTime.Now)
            {
                ModelState.AddModelError("DataSorteio", "A data do sorteio deve ser uma data futura");
            }

            if (model.Video != null)
            {
                if (!model.Video.FileName.Contains("mp4"))
                {
                    ModelState.AddModelError("Video", "O arquivo de vídeo deve ser do tipo .mp4");
                }

                if (model.Video.ContentLength > 52428800)
                {
                    ModelState.AddModelError("Video", "O tamanho máximo do arquivo deve ser 50MB");
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
                    ValorRifa = Convert.ToDecimal(model.ValorRifa.ToString().Replace(".", ",")),
                    IdUsuario = UsuarioLogado.Id,
                    DataSorteio = model.DataSorteio
                };

                if (model.Video != null)
                {
                    string CaminhoVideo = Server.MapPath("~/Content/VideoProdutos/");
                    string NomeArquivo = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + model.Video.FileName;

                    using (var stream = new FileStream(CaminhoVideo + NomeArquivo, FileMode.Create))
                    {
                        using (Stream inputStream = model.Video.InputStream)
                        {
                            inputStream.CopyTo(stream);
                        }
                    }

                    NovoProduto.Video = NomeArquivo;
                }

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

            ViewBag.Descricao = model.Descricao;
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
            ViewBag.Imagens = _Context.Multimidia.Where(p => p.IdProduto.Equals(Id) && p.Status).ToList();
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
                ValorRifa = Math.Round(produto.ValorRifa, 2).ToString().Replace(",", "."),
                Id = produto.Id,
                DataSorteio = produto.DataSorteio.Value,
                ActualyVideo = produto.Video
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
            ViewBag.Imagens = _Context.Multimidia.Where(m => m.IdProduto.Equals(model.Id) && m.Status).ToList();

            if (model.Imagem.Count() == 0 && _Context.Multimidia.Where(m => m.IdProduto.Equals(model.Id)).Count() == 0)
            {
                if (model.Imagem[0] == null)
                {
                    ModelState.AddModelError("Imagem", "Por favor selecione pelo menos uma imagem para o Produto");
                }
            }

            if (model.DataSorteio <= DateTime.Now)
            {
                ModelState.AddModelError("DataSorteio", "A data do sorteio deve ser uma data futura");
            }

            if (model.Video != null)
            {
                if (!model.Video.FileName.Contains("mp4"))
                {
                    ModelState.AddModelError("Video", "O arquivo de vídeo deve ser do tipo .mp4");
                }

                if (model.Video.ContentLength > 52428800)
                {
                    ModelState.AddModelError("Video", "O tamanho máximo do arquivo deve ser 50MB");
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
                AlterarProduto.ValorRifa = Convert.ToDecimal(model.ValorRifa.ToString().Replace(".", ","));
                AlterarProduto.DataSorteio = model.DataSorteio;


                if (model.Video != null)
                {
                    string CaminhoVideo = Server.MapPath("~/Content/VideoProdutos/");

                    if (AlterarProduto.Video != null)
                    {
                        FileInfo VideoAtual = new FileInfo(CaminhoVideo + AlterarProduto.Video);
                        if (VideoAtual.Exists)
                        {
                            VideoAtual.Delete();
                        }
                    }

                    string NomeArquivo = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + model.Video.FileName;

                    using (var stream = new FileStream(CaminhoVideo + NomeArquivo, FileMode.Create))
                    {
                        using (Stream inputStream = model.Video.InputStream)
                        {
                            inputStream.CopyTo(stream);
                        }
                    }

                    AlterarProduto.Video = NomeArquivo;
                }

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