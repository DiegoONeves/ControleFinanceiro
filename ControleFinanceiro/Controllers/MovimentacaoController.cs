using ControleFinanceiro.Models;
using ControleFinanceiro.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleFinanceiro.Controllers
{
    public class MovimentacaoController : Controller
    {
        private readonly MovimentacaoService _service;
        private readonly MovimentacaoCategoriaService _categoriaService;
        private readonly MovimentacaoTipoService _tipoService;
        public MovimentacaoController(MovimentacaoService service, MovimentacaoCategoriaService categoriaService, MovimentacaoTipoService tipoService)
        {
            _service = service;
            _tipoService = tipoService;
            _categoriaService = categoriaService;
        }

        public IActionResult Index()
        {
            return View(_service.BuscarMovimentacoes());
        }

        [HttpPost]
        public IActionResult Index(MovimentacaoPesquisaViewModel model)
        {
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                return View(_service.BuscarMovimentacoes(model));
            }

            return View(model);
        }

        public IActionResult Cadastrar()
        {
            MovimentacaoNovaViewModel model = new()
            {
                DataMovimentacao = DateOnly.FromDateTime(DateTime.Now),
            };
            CarregarListagens(model);
            return View(model);
        }

        [HttpPost]
        public IActionResult Cadastrar(MovimentacaoNovaViewModel model)
        {
            if (ModelState.IsValid)
            {
                _service.AbrirTransacaoParaInserirNovaMovimentacao(model);
                return RedirectToAction("Index");
            }

            CarregarListagens(model);
            return View(model);
        }

        [HttpGet]
        [Route("/{controller}/{action}/{codigo}")]
        public IActionResult Editar(Guid codigo)
        {
            var m = _service.ObterParaEditar(codigo);
            CarregarListagens(m);
            return View(m);
        }

        [HttpPost]
        public IActionResult Editar(MovimentacaoEdicaoViewModel model)
        {
            if (ModelState.IsValid)
            {
                _service.AbrirTransacaoParaAtualizarMovimentacao(model);
                return RedirectToAction("Index");
            }
            CarregarListagens(model);
            return View(model);
        }

        private void CarregarListagens(MovimentacaoNovaViewModel model)
        {
            model.Tipos = _tipoService.Obter().Select(c => new SelectListItem()
            {
                Text = $"{c.Descricao}",
                Value = c.Codigo.ToString()
            }).ToList();

            model.Categorias = _categoriaService.Obter().Select(c => new SelectListItem()
            {
                Text = $"{c.Descricao}",
                Value = c.Codigo.ToString()
            }).ToList();
        }

        private void CarregarListagens(MovimentacaoEdicaoViewModel model)
        {
            model.Tipos = _tipoService.Obter().Select(c => new SelectListItem()
            {
                Text = $"{c.Descricao}",
                Value = c.Codigo.ToString()
            }).ToList();

            model.Categorias = _categoriaService.Obter().Select(c => new SelectListItem()
            {
                Text = $"{c.Descricao}",
                Value = c.Codigo.ToString()
            }).ToList();
        }
    }
}
