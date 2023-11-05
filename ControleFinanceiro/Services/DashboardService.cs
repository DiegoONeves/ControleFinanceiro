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

            var movimentacoesFuturas = _movimentacaoService.SelectSQL(codigoTipo: _movimentacaoTipoService.ObterSaida().Codigo, dataMaiorOuIgualA: DateTime.Now, baixado: false, somenteParcelamentos: true);
            r.DividaTotal = movimentacoesFuturas.Select(x => x.Valor).Sum();
            var categorias = movimentacoesFuturas.Select(x => x.MovimentacaoCategoria.Descricao).Distinct();

            foreach (var item in categorias)
            {
                r.DividaPorCategoria.Add(new DashboardDividaPorCategoriaViewModel
                {
                    Categoria = item,
                    Valor = movimentacoesFuturas.Where( x => x.MovimentacaoCategoria.Descricao == item).Select(x => x.Valor).Sum()
                });
            }
            r.DividaPorCategoria = r.DividaPorCategoria.OrderBy(x => x.Valor).ToList();

            return r;
        }
    }
}
