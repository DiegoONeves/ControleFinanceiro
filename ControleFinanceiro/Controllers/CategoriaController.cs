using ControleFinanceiro.Models;
using ControleFinanceiro.Services;
using Microsoft.AspNetCore.Mvc;

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
            return View(_service.Obter());
        }

        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar(MovimentacaoCategoriaCadastroViewModel model)
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
            return View(_service.ObterParaEditar(codigo));
        }

        [HttpPut]
        [HttpPost]
        public IActionResult Editar(MovimentacaoCategoriaEdicaoViewModel model)
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

        
    }
}
