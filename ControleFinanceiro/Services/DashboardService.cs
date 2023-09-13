using ControleFinanceiro.Models;

namespace ControleFinanceiro.Services
{
    public class DashboardService : BaseService
    {
        private readonly MovimentacaoService _movimentacaoService;
        private readonly MovimentacaoTipoService _movimentacaoTipoService;

        public DashboardService(IConfiguration config, MovimentacaoService movimentacaoService, MovimentacaoTipoService movimentacaoTipoService) : base(config)
        {
            _movimentacaoService = movimentacaoService;
            _movimentacaoTipoService = movimentacaoTipoService;

        }

        public DashBoardViewModel GerarDashboard()
        {
            DashBoardViewModel r = new();

            var m = _movimentacaoService.Obter(codigoTipo: _movimentacaoTipoService.ObterSaida().Codigo, dataMaiorOuIgualA: DateTime.Now);
            r.DividaTotal = m.Select(x => x.Valor).Sum();
            var categorias = m.Select(x => x.MovimentacaoCategoria.Descricao).Distinct();

            foreach (var item in categorias)
            {
                r.DividaPorCategoria.Add(new DashboardDividaPorCategoriaViewModel
                {
                    Categoria = item,
                    Valor = m.Where(x => x.MovimentacaoCategoria.Descricao == item).Select(x => x.Valor).Sum()
                });
            }
            r.DividaPorCategoria = r.DividaPorCategoria.OrderBy(x => x.Valor).ToList();

            return r;
        }
    }
}
