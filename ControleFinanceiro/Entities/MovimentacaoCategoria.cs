namespace ControleFinanceiro.Entities
{
    public class MovimentacaoCategoria
    {
        public Guid Codigo { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public bool Ativo { get; set; } = false;

        public override string ToString()
        {
            return $"Código: {Codigo} - Descrição: {Descricao} - Ativo: {(Ativo ? "Sim" : "Não")}";
        }
    }
}
