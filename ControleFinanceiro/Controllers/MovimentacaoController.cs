using ControleFinanceiro.Models;
using ControleFinanceiro.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Controllers
{
    public class MovimentacaoController : Controller
    {
        private readonly MovimentacaoService _service;
        public MovimentacaoController(MovimentacaoService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {      
            return View(_service.BuscarMovimentacoes());
        }
    }
}
