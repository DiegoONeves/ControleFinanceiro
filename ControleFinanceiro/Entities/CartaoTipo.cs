namespace ControleFinanceiro.Entities
{
    public class CartaoTipo
    {
        public Guid Codigo { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public bool Ativo { get; set; }

        public override string ToString()
        {
            return $"Codigo: {Codigo} - Bandeira: {Descricao} - Ativo: {(Ativo ? "Sim" : "Não")}";
        }
    }
}
