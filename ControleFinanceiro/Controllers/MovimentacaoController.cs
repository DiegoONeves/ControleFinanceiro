using ControleFinanceiro.Models;
using ControleFinanceiro.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleFinanceiro.Controllers
{
    public class MovimentacaoController : Controller
    {
        private readonly MovimentacaoService _service;
        private readonly CategoriaService _categoriaService;
        private readonly TipoService _tipoService;
        private readonly CartaoService _cartaoService;
        public MovimentacaoController(MovimentacaoService service,
            CategoriaService categoriaService,
            TipoService tipoService,
            CartaoService cartaoService)
        {
            _service = service;
            _tipoService = tipoService;
            _categoriaService = categoriaService;
            _cartaoService = cartaoService;
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
                DataDaCompra = DateOnly.FromDateTime(DateTime.Now),
            };
            CarregarListagens(model, cartaoAtivo: true, categoriaAtiva: true);
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

            CarregarListagens(model, cartaoAtivo: true, categoriaAtiva: true);
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

        [HttpGet]
        [Route("/{controller}/{action}/{codigo}")]
        public IActionResult BaixarOuReverter(Guid codigo)
        {
            _service.BaixarOuReverter(codigo);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("/{controller}/{action}/{codigo}")]
        public IActionResult Excluir(Guid codigo)
        {
            _service.AbrirTransacaoParaExcluirMovimentacao(codigo);
            return RedirectToAction("Index");
        }

        private void CarregarListagens(dynamic model, bool? cartaoAtivo = null, bool? categoriaAtiva = null)
        {
            model.Tipos = _tipoService.Obter().Select(c => new SelectListItem()
            {
                Text = $"{c.Descricao}",
                Value = c.Codigo.ToString()
            }).ToList();

            model.Categorias = _categoriaService.SelectSQL(ativo: categoriaAtiva).OrderBy(x => x.Descricao).Select(c => new SelectListItem()
            {
                Text = $"{c.Descricao}",
                Value = c.Codigo.ToString()
            }).ToList();

            model.CartoesDePagamento = _cartaoService.Obter(ativo: cartaoAtivo).Select(c => new SelectListItem()
            {
                Text = CommonHelper.FormatarDescricaoCartao(c),
                Value = c.Codigo.ToString()
            }).ToList();
        }
    }
}
