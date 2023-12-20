using ControleFinanceiro.Models;
using ControleFinanceiro.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleFinanceiro.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly CategoriaService _service;
        public CategoriaController(CategoriaService service)
        {
            _service = service;
        }
        public IActionResult Index()
        {
            return View(_service.ObterParaListar());
        }

        public IActionResult Cadastrar()
        {
            CategoriaCadastroViewModel model = new();
            CarregarCategoriasPais(model);
            return View(model);
        }

        [HttpPost]
        public IActionResult Cadastrar(CategoriaCadastroViewModel model)
        {
            try
            {
                _service.AbrirTransacaoParaInserirCategoria(model);
            }
            catch (ValidationException validationException)
            {
                ModelState.AddModelError("", validationException.Message);
                return View(model);
            }
            return RedirectToAction("Index");
        }


        [HttpGet]
        [Route("/{controller}/{action}/{codigo}")]
        public IActionResult Editar(Guid codigo)
        {
            var model = _service.ObterParaEditar(codigo);
            CarregarCategoriasPais(model);
            return View(model);
        }

        [HttpPut]
        [HttpPost]
        public IActionResult Editar(CategoriaEdicaoViewModel model)
        {
            try
            {
                _service.AbrirTransacaoParaEditarCategoria(model);
                return RedirectToAction("Index");
            }
            catch (ValidationException validationException)
            {
                ModelState.AddModelError("", validationException.Message);
                return View(model);
            }
        }

        private void CarregarCategoriasPais(dynamic model)
        {
            model.CategoriasPais = _service.SelectSQL(somentePais: true, ativo: true).Select(c => new SelectListItem()
            {
                Text = $"{c.Descricao}",
                Value = c.Codigo.ToString()
            }).ToList();
        }


    }
}
