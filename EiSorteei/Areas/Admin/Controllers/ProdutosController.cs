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

            if (model.DataSorteio != null)
            {
                DateTime DataConvertida = DateTime.ParseExact(model.DataSorteio, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                if (DataConvertida <= DateTime.Now)
                {
                    ModelState.AddModelError("DataSorteio", "A data do sorteio deve ser uma data futura");
                }
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
                DateTime DataConvertida = DateTime.ParseExact(model.DataSorteio, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                Usuario UsuarioLogado = (Usuario)Session["Usuario"];

                Decimal ValorFormatado = Convert.ToDecimal(model.ValorRifa, new CultureInfo("en-Us"));

                Produto NovoProduto = new Produto()
                {
                    IdCategoria = model.CategoriaProduto,
                    DataAtualizacao = DateTime.Now,
                    DataCadastro = DateTime.Now,
                    Descricao = model.Descricao,
                    Nome = model.Nome,
                    RangeCodigo = model.RangeCodigo,
                    Status = true,
                    ValorRifa = Math.Round(ValorFormatado, 2),
                    IdUsuario = UsuarioLogado.Id,
                    DataSorteio = DataConvertida
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


                foreach(var OrderBump in model.OrderBumps)
                {
                    OrderBumps_Produto NovoOrderBump = new OrderBumps_Produto()
                    {
                        DataCadastro = DateTime.Now,
                        IdOrderBump = OrderBump,
                        IdProduto = NovoProduto.Id,
                        Status = true
                    };

                    _Context.OrderBumps_Produto.Add(NovoOrderBump);
                    _Context.SaveChanges();
                }


                return RedirectToAction("Index");

            }

            if(model.OrderBumps!=null)
            {
                ViewBag.OrderBumps = _Context.OrderBump.Where(o => model.OrderBumps.Contains(o.Id)).ToList();
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
                DataSorteio = produto.DataSorteio.Value.ToString("dd/MM/yyyy"),                               
            };

            ViewBag.OrderBumps = _Context.OrderBump.Join(_Context.OrderBumps_Produto.Where(o=>o.Status && o.IdProduto.Equals(Id)), o => o.Id, op => op.IdOrderBump, (o, op) => o).Where(o => o.Status).ToList();
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
            ViewBag.OrderBumps = _Context.OrderBump.Join(_Context.OrderBumps_Produto.Where(o => o.Status && o.IdProduto.Equals(model.Id)), o => o.Id, op => op.IdOrderBump, (o, op) => o).Where(o => o.Status).ToList();

            DateTime DataConvertida = DateTime.ParseExact(model.DataSorteio, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            if (model.Imagem.Count() == 0 && _Context.Multimidia.Where(m => m.IdProduto.Equals(model.Id)).Count() == 0)
            {
                if (model.Imagem[0] == null)
                {
                    ModelState.AddModelError("Imagem", "Por favor selecione pelo menos uma imagem para o Produto");
                }
            }

            if (DataConvertida <= DateTime.Now)
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
                Decimal ValorFormatado = Convert.ToDecimal(model.ValorRifa, new CultureInfo("en-Us"));
                AlterarProduto.ValorRifa = Math.Round(ValorFormatado, 2);
                AlterarProduto.DataSorteio = DataConvertida;


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

                List<OrderBumps_Produto> ActualyOrderBumps = _Context.OrderBumps_Produto.Where(o => o.IdProduto.Equals(model.Id) && o.Status).ToList();
                foreach(var x in ActualyOrderBumps)
                {
                    if(!model.OrderBumps.Contains(x.IdOrderBump))
                    {
                        x.Status = false;
                        _Context.Entry(x).State = System.Data.Entity.EntityState.Modified;
                    }
                }

                foreach (var x in model.OrderBumps)
                {
                    if (!ActualyOrderBumps.Any(o=>o.IdOrderBump.Equals(x)))
                    {
                        OrderBumps_Produto order = new OrderBumps_Produto()
                        {
                            DataCadastro = DateTime.Now,
                            IdOrderBump = x,
                            IdProduto = model.Id,
                            Status = true
                        };

                        _Context.OrderBumps_Produto.Add(order);
                    }
                }

                _Context.SaveChanges();

                return RedirectToAction("Index");

            }

            return View(model);
        }



        public JsonResult GetOrderBumps()
        {
            var Dados = _Context.OrderBump.Where(o => o.Status).OrderBy(o => o.Nome).Select(o => new
            {
                o.Nome,
                o.Id,
                o.Imagem
            });

            return Json(new
            {
                OrderBumps = Dados
            },JsonRequestBehavior.AllowGet);
        }


        public ActionResult Bilhetes(long Id, string NomeUsuario="",string Status="")
        {
            Produto FindedProduto = _Context.Produto.FirstOrDefault(p => p.Id.Equals(Id));
            ViewBag.NomeProduto = FindedProduto.Nome;
            ViewBag.Multimidia = _Context.Multimidia.Where(m => m.IdProduto.Equals(FindedProduto.Id) && m.Status).ToList();
            ViewBag.Id = Id;

            List<Carrinho> Carrinhos = _Context.Carrinho.Include("BilhetesCarrinho").Include("Compras").Where(c => c.IdProduto.Equals(Id)).ToList();
            List<MinhasComprasViewModel> MinhasCompras = new List<MinhasComprasViewModel>();

            foreach (var carrinho in Carrinhos)
            {
                long IdProduto = carrinho.Compras.First().Carrinho.IdProduto;
               

                if (FindedProduto.DataSorteio > DateTime.Now)
                {
                    var Bilhetes = _Context.BilhetesCarrinho.Where(b => b.IdCarrinho.Equals(carrinho.Id)).ToList();

                    var OrderBumps = _Context.OrderBump.Join(_Context.OrderBumpsEscolhidos.
                        Join(_Context.BilhetesCarrinho.Where(b => b.IdCarrinho.Equals(carrinho.Id)), o => o.IdBilhete, b => b.Id, (o, b) => o), o => o.Id, oe => oe.IdOrderBump, (o, oe) => o).GroupBy(o => o.Id).ToList();

                    List<OrderBumpsEscolhidosViewModel> Orders = new List<OrderBumpsEscolhidosViewModel>();

                    foreach (IGrouping<long, OrderBump> order in OrderBumps)
                    {
                        OrderBump FindedOrder = _Context.OrderBump.FirstOrDefault(o => o.Id.Equals(order.Key));

                        OrderBumpsEscolhidosViewModel NewOrder = new OrderBumpsEscolhidosViewModel()
                        {
                            Id = FindedOrder.Id,
                            Nome = FindedOrder.Nome,
                            Descricao = FindedOrder.Descricao,
                            Imagem = FindedOrder.Imagem,
                            NumerosRifas = _Context.BilhetesCarrinho.Join(_Context.OrderBumpsEscolhidos.Where(o => o.IdOrderBump.Equals(FindedOrder.Id)), b => b.Id, o => o.IdBilhete, (b, o) => b).Where(b=>b.IdCarrinho.Equals(carrinho.Id)).Select(b => b.NumeroBilhete).ToList()
                        };

                        Orders.Add(NewOrder);
                    }

                    MinhasComprasViewModel data = new MinhasComprasViewModel()
                    {
                        Id = carrinho.Id,
                        DadosUsuario = _Context.Usuario.First(u=>u.Id.Equals(carrinho.IdUsuario)),
                        CodigoVendedor = carrinho.Compras.First().CodigoVendedor,
                        DataCompra = carrinho.Compras.First().DataCompra,
                        Status = FormataStatus(carrinho.Compras.First().Status),
                        UrlBoleto = string.IsNullOrEmpty(carrinho.Compras.First().UrlBoleto) ? "" : carrinho.Compras.First().UrlBoleto,
                        ValorCompra = "R$ " + carrinho.Compras.First().ValorCompra.Replace('.', ','),
                        OrderBumps = Orders,
                        Bilhetes = Bilhetes,
                        Premio = FindedProduto,                        
                    };

                    MinhasCompras.Add(data);
                }

            }


            if(!string.IsNullOrEmpty(Status) && Status!= "TodasCompras")
            {
                MinhasCompras = MinhasCompras.Where(m => m.Status.Equals(Status)).ToList();
                ViewBag.Status = Status;
            }

            if(!string.IsNullOrEmpty(NomeUsuario))
            {
                MinhasCompras = MinhasCompras.Where(m => m.DadosUsuario.Nome.ToLower().Contains(NomeUsuario.ToLower())).ToList();
                ViewBag.NomeUsuario = NomeUsuario;
            }
            
            return View(MinhasCompras);
        }


        public string FormataStatus(string Status)
        {

            if (Status == "pending")
            {
                return "Pendente";
            }

            else if (Status == "in_process")
            {
                return "Pedido em Análise";
            }

            else if (Status == "cancelled")
            {
                return "Pedido Cancelado";
            }

            else if (Status == "rejected")
            {
                return "Pedido Rejeitado";
            }

            return "Aprovado";

        }


    }
}