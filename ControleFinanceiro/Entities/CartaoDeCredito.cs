namespace ControleFinanceiro.Entities
{
    public class CartaoDeCredito
    {
        public Guid Codigo { get; set; }
        public Guid CodigoBandeiraCartao { get; set; }
        public string NumeroCartao { get; set; } = string.Empty;
        public string DataDeCorte { get; set; } = string.Empty;
        public string Vencimento { get; set; } = string.Empty;  
        public bool Ativo { get; set; }
        public bool Virtual { get; set; }
        public BandeiraCartao BandeiraCartao { get; set; } = new();

        public override string ToString()
        {
            return $"Codigo: {Codigo} - Bandeira: {BandeiraCartao.Descricao} - Número: {NumeroCartao} - Ativo: {(Ativo ? "Sim" : "Não")}";
        }
    }
}
