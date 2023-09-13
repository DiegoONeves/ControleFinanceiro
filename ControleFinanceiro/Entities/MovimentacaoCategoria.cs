namespace ControleFinanceiro.Entities
{
    public class MovimentacaoCategoria
    {
        public Guid Codigo { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public bool Ativo { get; set; } = false;
    }
}
