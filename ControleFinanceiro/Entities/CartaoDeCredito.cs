namespace ControleFinanceiro.Entities
{
    public class CartaoDeCredito
    {
        public Guid Codigo { get; set; }
        public Guid CodigoBandeiraCartao { get; set; }
        public string FinalCartao { get; set; }
        public DateTime DataDeCorte { get; set; }
        public short Vencimento { get; set; }
        public bool Ativo { get; set; }
        public BandeiraCartao BandeiraCartao { get; set; } = new();

        public override string ToString()
        {
            return $"Codigo: {Codigo} - Bandeira: {BandeiraCartao.Descricao} Final: {FinalCartao} - Ativo: {(Ativo ? "Sim" : "Não")}";
        }
    }
}
