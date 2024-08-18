using ControleFinanceiro.Entities;
using ControleFinanceiro.Models;
using ControleFinanceiro.ValueObjects;
using Dapper;
using System.Data.SqlClient;
using System.Text;
using System.Transactions;

namespace ControleFinanceiro.Services
{
    public class ParcelamentoService : BaseService
    {
        private readonly MovimentacaoService _movimentacaoService;
        private readonly TipoService _TipoService;
        private readonly CartaoService _cartaoService;
        public ParcelamentoService(IConfiguration config,
            MovimentacaoService movimentacaoService,
            TipoService TipoService,
            CartaoService cartaoService)
            : base(config)
        {
            _movimentacaoService = movimentacaoService;
            _TipoService = TipoService;
            _cartaoService = cartaoService;
        }

        public List<ParcelamentoViewModel> ObterParaListar()
        {
            List<ParcelamentoViewModel> parcelamentosTemp = new();
            List<ParcelamentoViewModel> parcelamentosFinal = new();
            int contadorAtivos = 1;
            foreach (var item in SelectSQL().OrderByDescending(x => x.DataDaCompra))
            {
                bool finalizado = item.DataUltimaParcela <= DateTime.Now;

                parcelamentosTemp.Add(new ParcelamentoViewModel
                {
                    NumeroParcelamentoAtivo = !finalizado ? (contadorAtivos++).ToString().PadLeft(2, '0') : "-",
                    Codigo = item.Codigo,
                    Categoria = item.Categoria.Descricao,
                    CodigoCartao = item.CodigoCartao,
                    CodigoCategoria = item.CodigoCategoria,
                    Descricao = item.Descricao,
                    DataDaCompra = item.DataDaCompra,
                    DataPrimeiraParcela = item.DataPrimeiraParcela,
                    DataUltimaParcela = item.DataUltimaParcela,
                    QuantidadeParcela = item.QuantidadeParcela,
                    Valor = item.Valor,
                    Finalizado = finalizado,
                    MeioDeParcelamento = CommonHelper.FormatarDescricaoCartao(item.Cartao)
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
            r.DataDaCompra = DateOnly.FromDateTime(resultado.DataDaCompra);
            r.CodigoCartao = resultado.CodigoCartao;
            r.Codigo = resultado.Codigo;
            r.Valor = resultado.Valor;
            r.CodigoCategoria = resultado.CodigoCategoria;
            r.Descricao = resultado.Descricao;
            r.QuantidadeParcela = resultado.QuantidadeParcela;
            return r;
        }

        public void AbrirTransacaoParaInserirNovoParcelamento(ParcelamentoNovoViewModel model)
        {
            using (TransactionScope scope = new())
            {
                DateTime dataDaCompra = CommonHelper.ConverterDateOnlyParaDateTime(model.DataDaCompra);
                DateOnly dataDaParcela = model.CodigoCartao is null ? model.DataDaCompra : _cartaoService.ObterDataDaParcela(model.CodigoCartao.Value, model.DataDaCompra);
                var dataPrimeiraParcela = CommonHelper.ConverterDateOnlyParaDateTime(dataDaParcela);
                Parcelamento p = new()
                {
                    Codigo = Guid.NewGuid(),
                    CodigoCartao = model.CodigoCartao,
                    DataDaCompra = dataDaCompra,
                    Descricao = model.Descricao,
                    QuantidadeParcela = model.QuantidadeParcela,
                    CodigoCategoria = model.CodigoCategoria,
                    DataPrimeiraParcela = dataPrimeiraParcela,
                    DataUltimaParcela = dataPrimeiraParcela.AddMonths(model.QuantidadeParcela - 1),
                    Valor = model.Valor,
                    DataHora = DateTime.Now
                };
                InsertSQL(p);
                InserirMovimentacoesDeParcelamento(model, dataDaParcela, p);

                scope.Complete();
            }
        }

        private void InserirMovimentacoesDeParcelamento(dynamic model, DateOnly dataDaParcela, Parcelamento p)
        {
            Guid codigoTipo = _TipoService.Obter().First(x => x.Descricao == TipoDeMovimentacao.Saida).Codigo;
            for (int i = 0; i < model.QuantidadeParcela; i++)
            {
                dataDaParcela = dataDaParcela.AddMonths(i == 0 ? 0 : 1);
                MovimentacaoNovaViewModel m = new()
                {
                    DataMovimentacao = dataDaParcela,
                    DataDaCompra = model.DataDaCompra,
                    CodigoTipo = codigoTipo,
                    CodigoParcelamento = p.Codigo,
                    Valor = CommonHelper.TransformarDecimalNegativoOuPositivo(model.Valor),
                    Descricao = $"Parcela {i + 1} de {model.QuantidadeParcela} - {model.Descricao}",
                    CodigoCategoria = model.CodigoCategoria,
                    CodigoCartao = model.CodigoCartao
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
            DateOnly dataDaParcela = model.CodigoCartao is null ? model.DataDaCompra : _cartaoService.ObterDataDaParcela(model.CodigoCartao.Value, model.DataDaCompra);
            var dataPrimeiraParcela = CommonHelper.ConverterDateOnlyParaDateTime(dataDaParcela);

            parcelamentoEdicao.CodigoCategoria = model.CodigoCategoria;
            parcelamentoEdicao.CodigoCartao = model.CodigoCartao;
            parcelamentoEdicao.DataPrimeiraParcela = dataPrimeiraParcela;
            parcelamentoEdicao.DataUltimaParcela = dataPrimeiraParcela.AddMonths(model.QuantidadeParcela - 1);
            parcelamentoEdicao.QuantidadeParcela = model.QuantidadeParcela;
            parcelamentoEdicao.DataDaCompra = dataDaCompra;

            //edito o parcelamento
            UpdateSQL(parcelamentoEdicao);

            //vou reinserir as movimentações do parcelamento
            InserirMovimentacoesDeParcelamento(model, dataDaParcela, parcelamentoEdicao);

            //fecho a transação
            scope.Complete();
        }

        public void AbrirTransacaoParaExcluirParcelamento(Guid codigoParcelamento)
        {
            //abro a transação no banco de dados
            using TransactionScope scope = new();
            //exclusão de todas as movimentações desse parcelamento
            _movimentacaoService.DeleteSQL(codigoParcelamento: codigoParcelamento);
            //efetuando exclusão do parcelamento depois de excluir as movimentações
            DeleteSQL(codigoParcelamento);
            scope.Complete();
        }


        #region Crud

        public void UpdateSQL(Parcelamento parcelamento)
        {
            StringBuilder sql = new();
            sql.AppendLine(@"UPDATE Parcelamento");
            sql.AppendLine("SET CodigoCategoria = @CodigoCategoria,");
            sql.AppendLine("Descricao = @Descricao,");
            sql.AppendLine("QuantidadeParcela = @QuantidadeParcela,");
            sql.AppendLine("DataPrimeiraParcela = @DataPrimeiraParcela,");
            sql.AppendLine("DataUltimaParcela = @DataUltimaParcela,");
            sql.AppendLine("Valor = @Valor,");
            sql.AppendLine("CodigoCartao = @CodigoCartao,");
            sql.AppendLine("DataDaCompra = @DataDaCompra");
            sql.AppendLine("WHERE Codigo = @Codigo");
            using var conn = new SqlConnection(ConnectionString);
            conn.Execute(sql.ToString(), parcelamento);
        }

        public List<Parcelamento> SelectSQL(Guid? codigo = null)
        {
            StringBuilder sb = new();
            sb.AppendLine("SELECT A.*,B.*,C.*,D.*,E.* FROM Parcelamento A (NOLOCK) ");
            sb.AppendLine("INNER JOIN Categoria B (NOLOCK) ON A.CodigoCategoria = B.Codigo ");
            sb.AppendLine("LEFT JOIN Cartao C (NOLOCK) ON A.CodigoCartao = C.Codigo ");
            sb.AppendLine("LEFT JOIN CartaoBandeira D (NOLOCK) ON C.CodigoCartaoBandeira = D.Codigo ");
            sb.AppendLine("LEFT JOIN CartaoTipo E (NOLOCK) ON C.CodigoCartaoTipo = E.Codigo ");
            sb.AppendLine("WHERE A.Codigo = ISNULL(@Codigo,A.Codigo) ORDER BY A.DataDaCompra DESC ");

            using (var conn = new SqlConnection(ConnectionString))
                return conn.Query<Parcelamento, Categoria, Cartao, CartaoBandeira, CartaoTipo, Parcelamento>(sb.ToString(), (parcelamento, categoria, cartao, cartaoBandeira, cartaoTipo) =>
                {
                    parcelamento.Categoria = categoria;
                    if (cartao is not null)
                    {
                        cartao.CartaoTipo = cartaoTipo;
                        cartao.CartaoBandeira = cartaoBandeira;
                        parcelamento.Cartao = cartao;

                    }

                    return parcelamento;
                }, new { @Codigo = codigo },
                splitOn: "Codigo").ToList();
        }

        public void InsertSQL(Parcelamento p)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Execute("insert into Parcelamento (CodigoCartao,Codigo,DataDaCompra,QuantidadeParcela,DataPrimeiraParcela,DataHora,Valor,CodigoCategoria,Descricao,DataUltimaParcela) values (@CodigoCartao,@Codigo,@DataDaCompra,@QuantidadeParcela,@DataPrimeiraParcela,@DataHora,@Valor,@CodigoCategoria,@Descricao,@DataUltimaParcela)", p);
        }

        public void DeleteSQL(Guid codigo)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Execute("delete from Parcelamento where Codigo = @Codigo", new { @Codigo = codigo });
        }

        #endregion


    }
}
