namespace ControleFinanceiro.Models
{
    public class MovimentacaoCategoriaViewModel
    {
        public Guid Codigo { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Ativo { get; set; } = "Inativo";
    }
}
