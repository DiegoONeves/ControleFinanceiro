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
	CodigoMovimentacaoCategoria uniqueidentifier foreign key references MovimentacaoCategoria(Codigo),
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
	CodigoMovimentacaoRecorrenteTipo uniqueidentifier foreign key references MovimentacaoRecorrenteTipo(Codigo),
	Descricao varchar(255) null
)

INSERT INTO MovimentacaoRecorrenteTipo
VALUES
('8dca6dc6-ffd3-41b7-b78f-2af0d081a8d2','Entrada', 1),
('6f4635f2-113f-4846-9fae-eb66870af2d5','Saída',1)

INSERT INTO MovimentacaoCategoria
VALUES 
('1f9a0c6c-b823-4cac-85e1-7e974a5dce41','Uber',1),
('87e2104f-a8db-4924-8fed-4be10d61e2e7','Mercado',1),
('f925b8d5-85c4-477f-b660-22e5108acd36','Financiamento Imóvel',1),
('8e85647c-a14d-4b77-a015-aec271f0ad5d','Salário CLT',1),
('2931daa6-c1c2-4039-b87a-f77643ed0bbe','Outras Receitas',1),
('02504f44-bf7d-4c4b-be80-69320a3fbebe','Combustível',1),
('42d92351-cc01-42d9-9c9d-d7014b2b9c5f','Farmácia',1),
('50b91a25-d5c9-4c40-81e5-f854cafa70bd','Alimentação',1)

GO

--USE master

--GO

--DROP DATABASE ControleFinanceiro