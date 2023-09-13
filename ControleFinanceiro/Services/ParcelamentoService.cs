using ControleFinanceiro.Entities;
using ControleFinanceiro.Models;
using Dapper;
using System.Data.SqlClient;
using System.Transactions;

namespace ControleFinanceiro.Services
{
    public class ParcelamentoService : BaseService
    {
        private readonly MovimentacaoService _movimentacaoService;
        private readonly MovimentacaoTipoService _movimentacaoTipoService;
        public ParcelamentoService(IConfiguration config, MovimentacaoService movimentacaoService, MovimentacaoTipoService movimentacaoTipoService)
            : base(config)
        {
            _movimentacaoService = movimentacaoService;
            _movimentacaoTipoService = movimentacaoTipoService;
        }

        public List<ParcelamentoViewModel> Obter(Guid? codigo = null)
        {
            List<ParcelamentoViewModel> r = new();

            using (var conn = new SqlConnection(ConnectionString))
                r = conn.Query<ParcelamentoViewModel>("SELECT A.*,B.Descricao Categoria FROM Parcelamento A INNER JOIN MovimentacaoCategoria B ON A.CodigoMovimentacaoCategoria = B.Codigo WHERE A.Codigo = ISNULL(@Codigo,A.Codigo) ORDER BY A.DataHora DESC", new { @Codigo = codigo }).ToList();

            return r;
        }

        public ParcelamentoEdicaoViewModel ObterParaEditar(Guid codigo)
        {
            ParcelamentoEdicaoViewModel r = new();
            var resultado = Obter(codigo).First();
            r.DataPrimeiraParcela = DateOnly.FromDateTime(resultado.DataPrimeiraParcela);
            r.Codigo = resultado.Codigo;
            r.Valor = resultado.Valor;
            r.CodigoMovimentacaoCategoria = resultado.CodigoMovimentacaoCategoria;
            r.Descricao = resultado.Descricao;
            r.QuantidadeParcela = resultado.QuantidadeParcela;
            return r;
        }

        public void AbrirTransacaoParaInserirNovoParcelamento(ParcelamentoNovoViewModel model)
        {
            DateOnly dataDaParcela = model.DataPrimeiraParcela;

            using (TransactionScope scope = new())
            {
                var dataPrimeiraParcela = model.DataPrimeiraParcela.ToDateTime(TimeOnly.Parse("10:00 PM"));
                Parcelamento p = new()
                {
                    Codigo = Guid.NewGuid(),
                    Descricao = model.Descricao,
                    QuantidadeParcela = model.QuantidadeParcela,
                    CodigoMovimentacaoCategoria = model.CodigoMovimentacaoCategoria,
                    DataPrimeiraParcela = dataPrimeiraParcela,
                    DataUltimaParcela = dataPrimeiraParcela.AddMonths(model.QuantidadeParcela - 1),
                    Valor = model.Valor,
                    DataHora = DateTime.Now
                };
                using var conn = new SqlConnection(ConnectionString);
                conn.Execute("insert into Parcelamento (Codigo,QuantidadeParcela,DataPrimeiraParcela,DataHora,Valor,CodigoMovimentacaoCategoria,Descricao,DataUltimaParcela) values (@Codigo,@QuantidadeParcela,@DataPrimeiraParcela,@DataHora,@Valor,@CodigoMovimentacaoCategoria,@Descricao,@DataUltimaParcela)", p);

                for (int i = 0; i < model.QuantidadeParcela; i++)
                {
                    dataDaParcela = dataDaParcela.AddMonths(i == 0 ? 0 : 1);
                    MovimentacaoNovaViewModel m = new()
                    {
                        DataMovimentacao = dataDaParcela,
                        CodigoMovimentacaoTipo = _movimentacaoTipoService.Obter().First(x => x.Descricao.ToLower() == "saída").Codigo,
                        CodigoParcelamento = p.Codigo,
                        Valor = model.Valor * -1,
                        Descricao = $"Parcela {i + 1} de {model.QuantidadeParcela} - {model.Descricao}",
                        CodigoMovimentacaoCategoria = model.CodigoMovimentacaoCategoria,
                    };
                    _movimentacaoService.InserirMovimentacao(m);
                }

                scope.Complete();
            }
        }

        public void AbrirTransacaoParaAtualizarParcelamento(ParcelamentoEdicaoViewModel model)
        {
            var parcelamentoEdicao = ObterParcelamentos(model.Codigo).First();


        }

        public List<Parcelamento> ObterParcelamentos(Guid? codigo = null)
        {
            using var conn = new SqlConnection(ConnectionString);
            return conn.Query<Parcelamento>("SELECT * FROM Parcelamento WHERE Codigo = ISNULL(@Codigo,Codigo) ORDER BY DataHora DESC", new { @Codigo = codigo }).ToList();

        }

        public void EditarParcelamento(Parcelamento parcelamento)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Execute("UPDATE Parcelamento SET CodigoCategoria = @CodigoCategoria, Descricao = @Descricao, QuantidadeParcela = @QuantidadeParcela, Valor = @Valor, DataPrimeiraParcela = @DataPrimeiraParcela", parcelamento);

        }


    }
}
