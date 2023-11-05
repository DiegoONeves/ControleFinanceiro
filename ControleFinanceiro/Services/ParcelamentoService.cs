using ControleFinanceiro.Entities;
using ControleFinanceiro.Models;
using Dapper;
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

        public List<ParcelamentoViewModel> ObterParaListar()
        {
            List<ParcelamentoViewModel> parcelamentosTemp = new();
            List<ParcelamentoViewModel> parcelamentosFinal = new();
            foreach (var item in SelectSQL().OrderByDescending(x => x.DataDaCompra))
            {
                parcelamentosTemp.Add(new ParcelamentoViewModel
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
                    Finalizado = item.DataUltimaParcela <= DateTime.Now,
                    MeioDeParcelamento = item.CodigoCartaoDeCredito is null ? "Outro" : $"Crédito/{item?.CartaoDeCredito?.BandeiraCartao.Descricao}/{item?.CartaoDeCredito?.NumeroCartao.Substring(12, 4)}/{((item?.CartaoDeCredito?.Virtual ?? false) ? "Virtual" : "Físico")}"
                });
            }

            parcelamentosFinal.AddRange(parcelamentosTemp.Where(x => !x.Finalizado));
            parcelamentosFinal.AddRange(parcelamentosTemp.Where(x => x.Finalizado));

            return parcelamentosFinal;
        }

        public ParcelamentoEdicaoViewModel ObterParaEditar(Guid codigo)
        {
            ParcelamentoEdicaoViewModel r = new();
            var resultado = SelectSQL(codigo).First();
            r.DataPrimeiraParcela = DateOnly.FromDateTime(resultado.DataPrimeiraParcela);
            r.DataDaCompra = DateOnly.FromDateTime(resultado.DataPrimeiraParcela);
            r.CodigoCartaoDeCredito = resultado.CodigoCartaoDeCredito;
            r.Codigo = resultado.Codigo;
            r.Valor = resultado.Valor;
            r.Prioritaria = resultado.ContaPrioritaria;
            r.CodigoMovimentacaoCategoria = resultado.CodigoMovimentacaoCategoria;
            r.Descricao = resultado.Descricao;
            r.QuantidadeParcela = resultado.QuantidadeParcela;
            return r;
        }

        public void AbrirTransacaoParaInserirNovoParcelamento(ParcelamentoNovoViewModel model)
        {
            using (TransactionScope scope = new())
            {
                DateTime dataDaCompra = CommonHelper.ConverterDateOnlyParaDateTime(model.DataDaCompra);
                DateOnly dataDaParcela = model.CodigoCartaoDeCredito is null ? model.DataDaCompra : _cartaoDeCreditoService.ObterDataDaParcela(model.CodigoCartaoDeCredito.Value, model.DataDaCompra);
                var dataPrimeiraParcela = CommonHelper.ConverterDateOnlyParaDateTime(dataDaParcela);
                Parcelamento p = new()
                {
                    Codigo = Guid.NewGuid(),
                    DataDaCompra = dataDaCompra,
                    Descricao = model.Descricao,
                    QuantidadeParcela = model.QuantidadeParcela,
                    CodigoMovimentacaoCategoria = model.CodigoMovimentacaoCategoria,
                    DataPrimeiraParcela = dataPrimeiraParcela,
                    DataUltimaParcela = dataPrimeiraParcela.AddMonths(model.QuantidadeParcela - 1),
                    Valor = model.Valor,
                    ContaPrioritaria = model.Prioritaria,
                    DataHora = DateTime.Now
                };
                InsertSQL(p);
                InserirMovimentacoesDeParcelamento(model, dataDaParcela, p);

                scope.Complete();
            }
        }

        private void InserirMovimentacoesDeParcelamento(dynamic model, DateOnly dataDaParcela, Parcelamento p)
        {
            for (int i = 0; i < model.QuantidadeParcela; i++)
            {
                dataDaParcela = dataDaParcela.AddMonths(i == 0 ? 0 : 1);
                MovimentacaoNovaViewModel m = new()
                {
                    DataMovimentacao = dataDaParcela,
                    DataDaCompra = model.DataDaCompra,
                    CodigoMovimentacaoTipo = _movimentacaoTipoService.Obter().First(x => x.Descricao.ToLower() == "saída").Codigo,
                    CodigoParcelamento = p.Codigo,
                    Valor = model.Valor * -1,
                    Descricao = $"Parcela {i + 1} de {model.QuantidadeParcela} - {model.Descricao}",
                    CodigoMovimentacaoCategoria = model.CodigoMovimentacaoCategoria,
                    CodigoCartaoDeCredito = model.CodigoCartaoDeCredito
                };
                _movimentacaoService.InserirMovimentacao(m);
            }
        }

        public void AbrirTransacaoParaAtualizarParcelamento(ParcelamentoEdicaoViewModel model)
        {
            //abro a trasação no banco de dados
            using TransactionScope scope = new();
            //vou obter parcelamento do banco
            var parcelamentoEdicao = SelectSQL(model.Codigo).First();

            //exclusão de todas as movimentações desse parcelamento
            _movimentacaoService.DeleteSQL(codigoParcelamento: model.Codigo);

            DateTime dataDaCompra = CommonHelper.ConverterDateOnlyParaDateTime(model.DataDaCompra);
            DateOnly dataDaParcela = model.CodigoCartaoDeCredito is null ? model.DataDaCompra : _cartaoDeCreditoService.ObterDataDaParcela(model.CodigoCartaoDeCredito.Value, model.DataDaCompra);
            var dataPrimeiraParcela = CommonHelper.ConverterDateOnlyParaDateTime(dataDaParcela);

            parcelamentoEdicao.CodigoMovimentacaoCategoria = model.CodigoMovimentacaoCategoria;
            parcelamentoEdicao.CodigoCartaoDeCredito = model.CodigoCartaoDeCredito;
            parcelamentoEdicao.DataPrimeiraParcela = dataPrimeiraParcela;
            parcelamentoEdicao.DataUltimaParcela = dataPrimeiraParcela.AddMonths(model.QuantidadeParcela - 1);
            parcelamentoEdicao.QuantidadeParcela = model.QuantidadeParcela;
            parcelamentoEdicao.DataDaCompra = dataDaCompra;
            parcelamentoEdicao.ContaPrioritaria = model.Prioritaria;

            //edito o parcelamento
            UpdateSQL(parcelamentoEdicao);

            //vou reinserir as movimentações do parcelamento
            InserirMovimentacoesDeParcelamento(model, dataDaParcela, parcelamentoEdicao);

            //fecho a transação
            scope.Complete();
        }



        #region Crud

        public void UpdateSQL(Parcelamento parcelamento)
        {
            StringBuilder sql = new();
            sql.AppendLine(@"UPDATE Parcelamento");
            sql.AppendLine("SET CodigoMovimentacaoCategoria = @CodigoMovimentacaoCategoria,");
            sql.AppendLine("ContaPrioritaria = @ContaPrioritaria,");
            sql.AppendLine("Descricao = @Descricao,");
            sql.AppendLine("QuantidadeParcela = @QuantidadeParcela,");
            sql.AppendLine("DataPrimeiraParcela = @DataPrimeiraParcela,");
            sql.AppendLine("DataUltimaParcela = @DataUltimaParcela,");
            sql.AppendLine("Valor = @Valor,");
            sql.AppendLine("CodigoCartaoDeCredito = @CodigoCartaoDeCredito,");
            sql.AppendLine("DataDaCompra = @DataDaCompra");
            sql.AppendLine("WHERE Codigo = @Codigo");
            using var conn = new SqlConnection(ConnectionString);
            conn.Execute(sql.ToString(), parcelamento);
        }

        public List<Parcelamento> SelectSQL(Guid? codigo = null)
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

        public void InsertSQL(Parcelamento p)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Execute("insert into Parcelamento (ContaPrioritaria,Codigo,DataDaCompra,QuantidadeParcela,DataPrimeiraParcela,DataHora,Valor,CodigoMovimentacaoCategoria,Descricao,DataUltimaParcela) values (@ContaPrioritaria,@Codigo,@DataDaCompra,@QuantidadeParcela,@DataPrimeiraParcela,@DataHora,@Valor,@CodigoMovimentacaoCategoria,@Descricao,@DataUltimaParcela)", p);
        }

        public void DeleteSQL(Guid codigo)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Execute("delete from Parcelamento where Codigo = @Codigo", new { @Codigo = codigo });
        }

        #endregion


    }
}
