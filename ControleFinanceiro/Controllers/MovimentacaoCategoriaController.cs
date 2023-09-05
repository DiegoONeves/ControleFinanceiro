using ControleFinanceiro.Models;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Controllers
{
    public class MovimentacaoCategoriaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar(MovimentacaoCategoriaCadastroViewModel model)
        {
            return View();
        }

        public IActionResult Editar(Guid codigo)
        {
            return View();
        }

        [HttpPut]
        public IActionResult Editar(MovimentacaoCategoriaEdicaoViewModel model)
        {
            return View();
        }

        public IActionResult Excluir(Guid codigo)
        {
            return View();
        }

        public IActionResult ConfirmarExcluir()
        {
            return View();
        }
    }
}
