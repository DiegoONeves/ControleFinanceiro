namespace ControleFinanceiro.Entities
{
    public class Categoria
    {
        public Guid Codigo { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public bool Ativo { get; set; } = false;
        public Categoria? CategoriaPai { get; set; } = null;
        public Guid? CodigoCategoriaPai { get; set; } = null;

        public override string ToString()
        {
            return $"Código: {Codigo} - Descrição: {Descricao} - Ativo: {(Ativo ? "Sim" : "Não")}";
        }
    }
}
