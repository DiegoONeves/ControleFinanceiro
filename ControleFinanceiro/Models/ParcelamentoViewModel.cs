namespace ControleFinanceiro.Models
{
    public class ParcelamentoViewModel
    {
        public string MeioDeParcelamento { get; set; }
        public Guid Codigo { get; set; }
        public Guid? CodigoCartao { get; set; } = null;
        public string Categoria { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataDaCompra { get; set; }
        public DateTime DataPrimeiraParcela { get; set; }
        public DateTime DataUltimaParcela { get; set; }
        public short QuantidadeParcela { get; set; }
        public Guid CodigoMovimentacaoCategoria { get; set; }
        public bool Finalizado { get; set; }
    }
}
