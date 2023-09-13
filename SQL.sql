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

CREATE TABLE MovimentacaoTipo --Entrada/Saída
(
	Codigo uniqueidentifier primary key,
	Descricao varchar(255) not null,
	Ativo bit not null default 0,
)

CREATE TABLE Parcelamento
(
	Codigo uniqueidentifier primary key,
	CodigoMovimentacaoTipo uniqueidentifier foreign key references MovimentacaoTipo(Codigo),
	CodigoMovimentacaoCategoria uniqueidentifier foreign key references MovimentacaoCategoria(Codigo),
	Descricao varchar(255) not null,
	QuantidadeParcela smallint, 
	Valor money not null,
	DataPrimeiraParcela Date not null,
	DataHora Datetime not null
)


CREATE TABLE Movimentacao
(
	Codigo uniqueidentifier primary key,
	CodigoParcelamento uniqueidentifier foreign key references Parcelamento(Codigo) default null,
	DataMovimentacao Date not null,
	DataHora datetime not null,
	Valor money,
	CodigoMovimentacaoCategoria uniqueidentifier foreign key references MovimentacaoCategoria(Codigo) not null,
	CodigoMovimentacaoTipo uniqueidentifier foreign key references MovimentacaoTipo(Codigo) not null,
	Descricao varchar(255) null
)

INSERT INTO MovimentacaoTipo
VALUES
('9EDF4D03-BFB8-4B59-B456-6C12E4031B40','Entrada', 1),
('B7BA8BF6-A1E9-4E00-9DE4-5BFF52F5D56A','Saída',1)

INSERT INTO MovimentacaoCategoria
VALUES 
--('F35BD2FA-B05B-42E4-AA12-665CA02D27DE','Transporte',1),
--('A3D7E0BE-F4BD-4E5D-9A02-13BCDA7C18DD','Mercado',1),
--('88D23073-CA4A-41B5-B576-5924FD614EB1','Moradia',1),
--('DD39D1D1-29DF-44E7-A5CA-193B39A4A4D7','Carro',1),
--('AB4C85B4-21A1-4E33-B485-B8519F105A42','Receita',1),
--('1DB7844F-253A-4AE1-B5FC-C60E6868C871','Saúde',1),
--('13C73BDE-0E03-4E53-98EF-323875BDC125','Lazer/Hobbie',1),
('7B6390C0-BBFB-48E3-836A-7907E5FB5996','Estudo', 1),
('CB26AA01-EC1C-421B-AC0D-78EEEC80B959','Outros', 1)

--GO

--USE master

--GO

--DROP DATABASE ControleFinanceiro
