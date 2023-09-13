namespace ControleFinanceiro.Models
{
    public class MovimentacaoItemViewModel
    {
        public Guid Codigo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataMovimentacao { get; set; }
        public decimal Valor { get; set; }
        public string Categoria { get; set; }
        public string Tipo { get; set; }
        public string CorDoTextDeValor { get; set; }
    }
}
