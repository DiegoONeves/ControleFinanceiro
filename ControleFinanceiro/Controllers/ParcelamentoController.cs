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
        private readonly MovimentacaoTipoService _tipoService;
        private readonly CartaoDeCreditoService _cartaoDeCreditoService;
        public ParcelamentoController(ParcelamentoService service,
            CategoriaService categoriaService,
            MovimentacaoTipoService tipoService,
            CartaoDeCreditoService cartaoDeCreditoService)
        {
            _service = service;
            _tipoService = tipoService;
            _categoriaService = categoriaService;
            _cartaoDeCreditoService = cartaoDeCreditoService;
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

        private void CarregarListagensNovo(ParcelamentoNovoViewModel model)
        {
            model.Categorias = CarregarCategorias();
            model.CartoesDeCredito = CarregarCartoesDeCredito();
        }
        private void CarregarListagensEdicao(ParcelamentoEdicaoViewModel model)
        {
            model.Categorias = CarregarCategorias();
            model.CartoesDeCredito = CarregarCartoesDeCredito();
        }
        private List<SelectListItem> CarregarCategorias()
        {
            return _categoriaService.SelectSQL(ativo: true).Select(c => new SelectListItem()
            {
                Text = $"{c.Descricao}",
                Value = c.Codigo.ToString()
            }).ToList();
        }
        private List<SelectListItem> CarregarCartoesDeCredito()
        {
            return _cartaoDeCreditoService.Obter().Select(c => new SelectListItem()
            {
                Text = $"Crédito/{c.BandeiraCartao.Descricao}/{c.NumeroCartao.Substring(12, 4)}/{(c.Virtual ? "Virtual" : "Físico")}",
                Value = c.Codigo.ToString()
            }).ToList();
        }
    }
}
