namespace ControleFinanceiro.Entities
{
    public class Parcelamento
    {
        public Guid Codigo { get; set; }
        public Guid? CodigoCartaoDeCredito { get; set; } = null;
        public Guid CodigoMovimentacaoCategoria { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataPrimeiraParcela { get; set; }
        public short QuantidadeParcela { get; set; }
        public DateTime DataHora { get; set; }  
        public DateTime DataUltimaParcela { get; set; }
        public DateTime DataDaCompra { get; set; }
        public CartaoDeCredito? CartaoDeCredito { get; set; } = null;
        public MovimentacaoCategoria MovimentacaoCategoria { get; set; } = new();
    }
}
