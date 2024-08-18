using ControleFinanceiro.Entities;
using ControleFinanceiro.ValueObjects;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ControleFinanceiro.Services
{
    public class TipoService : BaseService
    {
        public TipoService(IConfiguration config)
         : base(config)
        {

        }

        public Tipo ObterEntrada() => Obter().First(x => x.Descricao == TipoDeMovimentacao.Entrada);
        public Tipo ObterSaida() => Obter().First(x => x.Descricao == TipoDeMovimentacao.Saida);

        public List<Tipo> Obter(Guid? codigo = null)
        {
            using var conn = new SqlConnection(ConnectionString);
            return conn.Query<Tipo>("SELECT * FROM Tipo (NOLOCK) WHERE Codigo = ISNULL(@Codigo,Codigo)", new { @Codigo = codigo }).ToList();
        }

        public decimal ObterFormatoValorConformeTipo(Guid codigoTipo, decimal valor) 
            => Obter(codigo: codigoTipo).First().Descricao != TipoDeMovimentacao.Entrada ? CommonHelper.TransformarDecimalNegativoOuPositivo(valor) : valor;

    }
}
