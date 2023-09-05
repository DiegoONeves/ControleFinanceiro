namespace ControleFinanceiro.Models
{
    public class MovimentacaoViewModel
    {
        public DateOnly DataInicial { get; set; }
        public DateOnly DataFinal { get; set; }

        public List<MovimentacaoItemViewModel> Movimentacoes { get; set; } = new();
    }
}
