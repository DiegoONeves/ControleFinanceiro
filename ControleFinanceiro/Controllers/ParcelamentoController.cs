using ControleFinanceiro.Models;
using ControleFinanceiro.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleFinanceiro.Controllers
{
    public class ParcelamentoController : Controller
    {
        private readonly ParcelamentoService _service;
        private readonly MovimentacaoCategoriaService _categoriaService;
        private readonly MovimentacaoTipoService _tipoService;
        public ParcelamentoController(ParcelamentoService service, MovimentacaoCategoriaService categoriaService, MovimentacaoTipoService tipoService)
        {
            _service = service;
            _tipoService = tipoService;
            _categoriaService = categoriaService;
        }
        public IActionResult Index()
        {
            return View(_service.Obter());
        }

        public IActionResult Cadastrar()
        {
            var m = new ParcelamentoNovoViewModel
            {
                DataPrimeiraParcela = DateOnly.FromDateTime(DateTime.Now),
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

        [HttpPut]
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
        }
        private void CarregarListagensEdicao(ParcelamentoEdicaoViewModel model)
        {
            model.Categorias = CarregarCategorias();
        }
        private List<SelectListItem> CarregarCategorias()
        {
            return _categoriaService.Obter().Select(c => new SelectListItem()
            {
                Text = $"{c.Descricao}",
                Value = c.Codigo.ToString()
            }).ToList();
        }
    }
}
