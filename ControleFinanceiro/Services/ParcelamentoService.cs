using ControleFinanceiro.Entities;
using ControleFinanceiro.Models;
using Dapper;
using Microsoft.VisualBasic;
using System.Data.SqlClient;
using System.Text;
using System.Transactions;

namespace ControleFinanceiro.Services
{
    public class ParcelamentoService : BaseService
    {
        private readonly MovimentacaoService _movimentacaoService;
        private readonly MovimentacaoTipoService _movimentacaoTipoService;
        private readonly CartaoDeCreditoService _cartaoDeCreditoService;
        public ParcelamentoService(IConfiguration config,
            MovimentacaoService movimentacaoService,
            MovimentacaoTipoService movimentacaoTipoService,
            CartaoDeCreditoService cartaoDeCreditoService)
            : base(config)
        {
            _movimentacaoService = movimentacaoService;
            _movimentacaoTipoService = movimentacaoTipoService;
            _cartaoDeCreditoService = cartaoDeCreditoService;
        }

        public List<Parcelamento> Obter(Guid? codigo = null)
        {
            StringBuilder sb = new();
            sb.AppendLine("SELECT A.*,B.*,C.*,D.* FROM Parcelamento A (NOLOCK) ");
            sb.AppendLine("INNER JOIN MovimentacaoCategoria B (NOLOCK) ON A.CodigoMovimentacaoCategoria = B.Codigo ");
            sb.AppendLine("LEFT JOIN CartaoDeCredito C (NOLOCK) ON A.CodigoCartaoDeCredito = C.Codigo ");
            sb.AppendLine("LEFT JOIN BandeiraCartao D (NOLOCK) ON C.CodigoBandeiraCartao = D.Codigo ");
            sb.AppendLine("WHERE A.Codigo = ISNULL(@Codigo,A.Codigo) ORDER BY A.DataDaCompra DESC ");

            using (var conn = new SqlConnection(ConnectionString))
                return conn.Query<Parcelamento, MovimentacaoCategoria, CartaoDeCredito, BandeiraCartao, Parcelamento>(sb.ToString(), (parcelamento, categoria, cartaoDeCredito, bandeiraCartao) =>
                {
                    parcelamento.MovimentacaoCategoria = categoria;
                    if (cartaoDeCredito is not null)
                    {
                        cartaoDeCredito.BandeiraCartao = bandeiraCartao;
                        parcelamento.CartaoDeCredito = cartaoDeCredito;
                    }

                    return parcelamento;
                }, new { @Codigo = codigo },
                splitOn: "Codigo").ToList();
        }

        public IEnumerable<ParcelamentoViewModel> ObterParaListar()
        {
            foreach (var item in Obter())
            {
                yield return new ParcelamentoViewModel
                {
                    Codigo = item.Codigo,
                    Categoria = item.MovimentacaoCategoria.Descricao,
                    CodigoCartaoDeCredito = item.CodigoCartaoDeCredito,
                    CodigoMovimentacaoCategoria = item.CodigoMovimentacaoCategoria,
                    Descricao = item.Descricao,
                    DataDaCompra = item.DataDaCompra,
                    DataPrimeiraParcela = item.DataPrimeiraParcela,
                    DataUltimaParcela = item.DataUltimaParcela,
                    QuantidadeParcela = item.QuantidadeParcela,
                    Valor = item.Valor,
                    MeioDeParcelamento = item.CodigoCartaoDeCredito is null ? "Outro" : $"Crédito/{item?.CartaoDeCredito?.BandeiraCartao.Descricao}/{item?.CartaoDeCredito?.NumeroCartao.Substring(12, 4)}/{((item?.CartaoDeCredito?.Virtual ?? false) ? "Virtual" : "Físico")}"
                };
            }
        }

        public ParcelamentoEdicaoViewModel ObterParaEditar(Guid codigo)
        {
            ParcelamentoEdicaoViewModel r = new();
            var resultado = Obter(codigo).First();
            r.DataPrimeiraParcela = DateOnly.FromDateTime(resultado.DataPrimeiraParcela);
            r.DataDaCompra = DateOnly.FromDateTime(resultado.DataPrimeiraParcela);
            r.CodigoCartaoDeCredito = resultado.CodigoCartaoDeCredito;
            r.Codigo = resultado.Codigo;
            r.Valor = resultado.Valor;
            r.CodigoMovimentacaoCategoria = resultado.CodigoMovimentacaoCategoria;
            r.Descricao = resultado.Descricao;
            r.QuantidadeParcela = resultado.QuantidadeParcela;
            return r;
        }

        public void AbrirTransacaoParaInserirNovoParcelamento(ParcelamentoNovoViewModel model)
        {
            using (TransactionScope scope = new())
            {
                DateOnly dataDaParcela = model.CodigoCartaoDeCredito is null ? model.DataDaCompra : _cartaoDeCreditoService.ObterDataDaParcela(model.CodigoCartaoDeCredito.Value, model.DataDaCompra);
                var dataPrimeiraParcela = dataDaParcela.ToDateTime(TimeOnly.Parse("10:00 PM"));
                Parcelamento p = new()
                {
                    Codigo = Guid.NewGuid(),
                    DataDaCompra = model.DataDaCompra.ToDateTime(TimeOnly.Parse("10:00 PM")),
                    Descricao = model.Descricao,
                    QuantidadeParcela = model.QuantidadeParcela,
                    CodigoMovimentacaoCategoria = model.CodigoMovimentacaoCategoria,
                    DataPrimeiraParcela = dataPrimeiraParcela,
                    DataUltimaParcela = dataPrimeiraParcela.AddMonths(model.QuantidadeParcela - 1),
                    Valor = model.Valor,
                    DataHora = DateTime.Now
                };
                using var conn = new SqlConnection(ConnectionString);
                conn.Execute("insert into Parcelamento (Codigo,DataDaCompra,QuantidadeParcela,DataPrimeiraParcela,DataHora,Valor,CodigoMovimentacaoCategoria,Descricao,DataUltimaParcela) values (@Codigo,@DataDaCompra,@QuantidadeParcela,@DataPrimeiraParcela,@DataHora,@Valor,@CodigoMovimentacaoCategoria,@Descricao,@DataUltimaParcela)", p);

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
            using (TransactionScope scope = new())
            {
                var parcelamentoEdicao = Obter(model.Codigo).First();
                DateOnly dataDaParcela = model.CodigoCartaoDeCredito is null ? model.DataDaCompra : _cartaoDeCreditoService.ObterDataDaParcela(model.CodigoCartaoDeCredito.Value, model.DataDaCompra);

                scope.Complete();
            }
        }

        public void EditarParcelamento(Parcelamento parcelamento)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Execute("UPDATE Parcelamento SET CodigoCategoria = @CodigoCategoria, Descricao = @Descricao, QuantidadeParcela = @QuantidadeParcela, Valor = @Valor, DataPrimeiraParcela = @DataPrimeiraParcela", parcelamento);

        }


    }
}
