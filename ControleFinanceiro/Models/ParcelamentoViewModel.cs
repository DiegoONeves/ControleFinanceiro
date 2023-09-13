namespace ControleFinanceiro.Models
{
    public class ParcelamentoViewModel
    {
        public Guid Codigo { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataPrimeiraParcela { get; set; }
        public short QuantidadeParcela { get; set; }
        public Guid CodigoMovimentacaoCategoria { get; set; }
        public Guid CodigoMovimentacaoTipo { get; set; }
    }
}
