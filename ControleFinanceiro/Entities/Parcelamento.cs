namespace ControleFinanceiro.Entities
{
    public class Parcelamento
    {
        public Guid Codigo { get; set; }
        public Guid? CodigoCartao { get; set; } = null;
        public Guid CodigoCategoria { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataPrimeiraParcela { get; set; }
        public short QuantidadeParcela { get; set; }
        public DateTime DataHora { get; set; }  
        public DateTime DataUltimaParcela { get; set; }
        public DateTime DataDaCompra { get; set; }
        public Cartao? Cartao { get; set; } = null;
        public Categoria Categoria { get; set; } = new();
    }
}
