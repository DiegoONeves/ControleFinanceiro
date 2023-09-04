CREATE DATABASE ControleFinanceiro
GO

USE ControleFinanceiro

GO


CREATE TABLE MovimentacaoCategoria
(
	Codigo uniqueidentifier primary key,
	Descricao varchar(255) not null,
	Ativo bit not null default 0,
)

CREATE TABLE MovimentacaoRecorrenteTipo --Entrada/Saída
(
	Codigo uniqueidentifier primary key,
	Descricao varchar(255) not null,
	Ativo bit not null default 0,
)

CREATE TABLE MovimentacaoRecorrente
(
	Codigo uniqueidentifier primary key,
	CodigoMovimentacaoRecorrenteTipo uniqueidentifier foreign key references MovimentacaoRecorrenteTipo(Codigo),
	Descricao varchar(255) not null,
	QuantidadeParcela smallint, 
	Valor money not null,
	DataPrimeiraParcela Date not null,
	DataUltimaParcela Date not null,
	DataHora Datetime not null,
	Continua bit default 0
)


CREATE TABLE Movimentacao
(
	Codigo uniqueidentifier primary key,
	DataHora datetime not null,
	Valor money,
	CodigoMovimetacaoCategoria uniqueidentifier foreign key references MovimentacaoCategoria(Codigo),
	Descricao varchar(255) null
)

USE master

GO

DROP DATABASE ControleFinanceiro