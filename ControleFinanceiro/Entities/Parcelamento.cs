namespace ControleFinanceiro.Entities
{
    public class Parcelamento
    {
        public Guid Codigo { get; set; }
        public Guid CodigoMovimentacaoTipo { get; set; }
        public Guid CodigoMovimentacaoCategoria { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataPrimeiraParcela { get; set; }
        public short QuantidadeParcela { get; set; }
        public DateTime DataHora { get; set; }  
        public DateTime DataUltimaParcela { get; internal set; }
    }
}
