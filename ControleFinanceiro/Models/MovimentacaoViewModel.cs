using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Models
{
    public class MovimentacaoViewModel
    {
        [Display(Name = "Período")]
        public string Periodo { get; set; } = string.Empty;
        public string Direcao { get; set; } = string.Empty;
        public decimal Valor { get; set; } = 0;
        public decimal ValorTotalSaida { get; set; } = 0;
        public decimal ValorTotalEntrada { get; set; } = 0;
        public decimal ValorTotalDeParcelamento { get; set; } = 0;
        public decimal ValorAmortizadoNoMes { get; set; } = 0;
        public decimal ValorContasNaoEssenciais { get; set; } = 0;
        public decimal ValorContasEssenciais { get; set; } = 0;
        public IEnumerable<MovimentacaoItemViewModel> Movimentacoes { get; set; }

        public List<DashboardDividaPorCategoriaViewModel> TotaisPorCategoria { get; set; } = new();
        public List<DashboardDividaPorCategoriaViewModel> TotaisPorCategoriaParcelamentos { get; set; } = new();
    }
}
