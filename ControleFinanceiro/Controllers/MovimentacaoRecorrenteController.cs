using ControleFinanceiro.Models;
using ControleFinanceiro.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleFinanceiro.Controllers
{
    public class MovimentacaoRecorrenteController : Controller
    {
        private readonly MovimentacaoRecorrenteService _service;
        private readonly CategoriaService _categoriaService;
        private readonly TipoService _tipoService;
        public MovimentacaoRecorrenteController(MovimentacaoRecorrenteService service,
            CategoriaService categoriaService,
            TipoService tipoService,
            CartaoService CartaoService)
        {
            _service = service;
            _tipoService = tipoService;
            _categoriaService = categoriaService;
        }

        public IActionResult Index()
        {
            return View(_service.BuscarMovimentacoesRecorrentes());
        }

        public IActionResult Cadastrar()
        {
            MovimentacaoRecorrenteNovaViewModel model = new()
            {
                DataDaPrimeiraMovimentacao = DateOnly.FromDateTime(DateTime.Now),
            };
            CarregarListagens(model, categoriaAtiva: true);
            return View(model);
        }

        [HttpPost]
        public IActionResult Cadastrar(MovimentacaoRecorrenteNovaViewModel model)
        {
            if (ModelState.IsValid)
            {
                _service.AbrirTransacaoParaInserirNovaMovimentacaoRecorrente(model);
                return RedirectToAction("Index");
            }

            CarregarListagens(model, categoriaAtiva: true);
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
        public IActionResult Editar(MovimentacaoRecorrenteEdicaoViewModel model)
        {
            if (ModelState.IsValid)
            {
                _service.AbrirTransacaoParaAtualizarMovimentacaoRecorrente(model);
                return RedirectToAction("Index");
            }
            CarregarListagens(model);
            return View(model);
        }

        [HttpGet]
        [Route("/{controller}/{action}/{codigo}")]
        public IActionResult Excluir(Guid codigo)
        {
            _service.AbrirTransacaoParaExcluirRecorrencias(codigo);
            return RedirectToAction("Index");
        }

        private void CarregarListagens(dynamic model, bool? categoriaAtiva = null)
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

        }
    }
}
