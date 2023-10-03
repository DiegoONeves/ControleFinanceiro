namespace ControleFinanceiro.Models
{
    public class MovimentacaoItemViewModel
    {
        public Guid Codigo { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataMovimentacao { get; set; }
        public decimal Valor { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string CorDoTextDeValor { get; set; } = string.Empty;
        public string MeioDeParcelamento { get; set; } = string.Empty;
        public DateTime DataDaCompra { get; set; }
        public bool Baixado { get; set; }
        public bool UltimaParcela { get; set; } = false;
    }
}
