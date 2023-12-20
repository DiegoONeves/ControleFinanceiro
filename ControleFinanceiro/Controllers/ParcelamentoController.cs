using ControleFinanceiro.Models;
using ControleFinanceiro.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleFinanceiro.Controllers
{
    public class ParcelamentoController : Controller
    {
        private readonly ParcelamentoService _service;
        private readonly CategoriaService _categoriaService;
        private readonly TipoService _tipoService;
        private readonly CartaoService _cartaoService;
        public ParcelamentoController(ParcelamentoService service,
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
            return View(_service.ObterParaListar());
        }

        public IActionResult Cadastrar()
        {
            var m = new ParcelamentoNovoViewModel
            {
                DataDaCompra = DateOnly.FromDateTime(DateTime.Now),
                QuantidadeParcela = 1
            };
            CarregarListagensNovo(m);
            return View(m);
        }

        [HttpPost]
        public IActionResult Cadastrar(ParcelamentoNovoViewModel model)
        {
            if (ModelState.IsValid)
            {
                _service.AbrirTransacaoParaInserirNovoParcelamento(model);
                return RedirectToAction("Index");
            }
            CarregarListagensNovo(model);

            return View(model);
        }

        [HttpGet]
        [Route("/{controller}/{action}/{codigo}")]
        public IActionResult Editar(Guid codigo)
        {
            var m = _service.ObterParaEditar(codigo);
            CarregarListagensEdicao(m);
            return View(m);
        }

        [HttpPost]
        public IActionResult Editar(ParcelamentoEdicaoViewModel model)
        {
            if (ModelState.IsValid)
            {
                _service.AbrirTransacaoParaAtualizarParcelamento(model);
                return RedirectToAction("Index");
            }
            CarregarListagensEdicao(model);
            return View(model);
        }


        [HttpGet]
        [Route("/{controller}/{action}/{codigo}")]
        public IActionResult Excluir(Guid codigo)
        {
            _service.AbrirTransacaoParaExcluirParcelamento(codigo);
            return RedirectToAction("Index");
        }

        private void CarregarListagensNovo(ParcelamentoNovoViewModel model)
        {
            model.Categorias = CarregarCategorias(ativo: true);
            model.CartoesDePagamento = CarregarCartoesDePagamento(ativo: true);
        }
        private void CarregarListagensEdicao(ParcelamentoEdicaoViewModel model)
        {
            model.Categorias = CarregarCategorias();
            model.CartoesDePagamento = CarregarCartoesDePagamento();
        }
        private List<SelectListItem> CarregarCategorias(bool? ativo = null)
        {
            return _categoriaService.SelectSQL(ativo: ativo).OrderBy(x => x.Descricao).Select(c => new SelectListItem()
            {
                Text = $"{c.Descricao}",
                Value = c.Codigo.ToString()
            }).ToList();
        }
        private List<SelectListItem> CarregarCartoesDePagamento(bool? ativo = null)
        {
            return _cartaoService.Obter(ativo: ativo).Select(c => new SelectListItem()
            {
                Text = CommonHelper.FormatarDescricaoCartao(c),
                Value = c.Codigo.ToString()
            }).ToList();
        }
    }
}
