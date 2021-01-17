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
                    Status = x.DataSorteio > DateTime.Now ? "Ativo" : "Encerrado"
                };

                model.Add(data);
            }

            return View(model);
        }
    }
}