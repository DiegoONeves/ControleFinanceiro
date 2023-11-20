namespace ControleFinanceiro.Entities
{
    public class Cartao
    {
        public Guid Codigo { get; set; }
        public Guid CodigoCartaoBandeira { get; set; }
        public Guid CodigoCartaoTipo { get; set; }
        public string NumeroCartao { get; set; } = string.Empty;
        public string DataDeCorte { get; set; } = string.Empty;
        public string Vencimento { get; set; } = string.Empty;  
        public bool Ativo { get; set; }
        public bool Virtual { get; set; }
        public CartaoBandeira CartaoBandeira { get; set; } = new();
        public CartaoTipo CartaoTipo { get; set; } = new();

        public override string ToString()
        {
            return $"Codigo: {Codigo} - Bandeira: {CartaoBandeira.Descricao} - Tipo: {CartaoTipo.Descricao} - Número: {NumeroCartao} - Ativo: {(Ativo ? "Sim" : "Não")}";
        }
    }
}
