using ControleFinanceiro.Models;

namespace ControleFinanceiro.Services
{
    public class DashboardService : BaseService
    {
        private readonly MovimentacaoService _movimentacaoService;
        private readonly TipoService _TipoService;

        public DashboardService(IConfiguration config, MovimentacaoService movimentacaoService, TipoService TipoService) : base(config)
        {
            _movimentacaoService = movimentacaoService;
            _TipoService = TipoService;

        }

        public DashBoardViewModel GerarDashboard()
        {
            DashBoardViewModel r = new();

            var movimentacoesFuturas = _movimentacaoService.SelectSQL(codigoTipo: _TipoService.ObterSaida().Codigo, dataMaiorOuIgualA: DateTime.Now, baixado: false, somenteParcelamentos: true);
            r.DividaTotal = movimentacoesFuturas.Select(x => x.Valor).Sum();
            var categorias = movimentacoesFuturas.Select(x => x.Categoria.Descricao).Distinct();

            foreach (var item in categorias)
            {
                r.DividaPorCategoria.Add(new DashboardDividaPorCategoriaViewModel
                {
                    Categoria = item,
                    Valor = movimentacoesFuturas.Where( x => x.Categoria.Descricao == item).Select(x => x.Valor).Sum()
                });
            }
            r.DividaPorCategoria = r.DividaPorCategoria.OrderBy(x => x.Valor).Take(5).ToList();

            return r;
        }
    }
}
