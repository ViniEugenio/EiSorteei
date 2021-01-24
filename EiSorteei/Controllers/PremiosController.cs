using EiSorteei.Data;
using EiSorteei.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EiSorteei.Controllers
{
    public class PremiosController : Controller
    {
        private readonly EiSorteeiEntities _Context;

        public PremiosController()
        {
            _Context = Context.GetContext();
        }
        // GET: Premios
        public ActionResult Index(int status = 0, string titulo = "", string datasorteio = "")
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
                    Status = x.DataSorteio > DateTime.Now ? "Ativo" : "Finalizado",
                    DataSorteio = x.DataSorteio.Value
                };

                model.Add(data);
            }

            if (!string.IsNullOrEmpty(titulo))
            {
                model = model.Where(p => p.Nome.Contains(titulo)).ToList();
            }

            if (!string.IsNullOrEmpty(datasorteio))
            {
                DateTime DataFormatada = Convert.ToDateTime(datasorteio);
                model = model.Where(p => p.DataSorteio.Equals(DataFormatada)).ToList();
            }

            if (status != 0)
            {
                if (status == 1)
                {
                    model = model.Where(p => p.DataSorteio >= DateTime.Now).ToList();
                }

                else
                {
                    model = model.Where(p => p.DataSorteio < DateTime.Now).ToList();
                }
            }

            ViewBag.Status = status;
            ViewBag.Titulo = titulo;
            ViewBag.DataSorteio = datasorteio;

            return View(model);
        }
    }
}