DROP TABLE transacao;
DROP TABLE cliente;

CREATE TABLE cliente(
  id SERIAL PRIMARY KEY,  
  limite INTEGER NOT NULL,
  saldo INTEGER CHECK (saldo >= -limite) NOT NULL
);

CREATE TABLE transacao(
  id SERIAL PRIMARY KEY,
  id_cliente INTEGER REFERENCES cliente(id),
  valor INTEGER NOT NULL,
  tipo VARCHAR(1) CHECK (tipo IN ('c', 'd')) NOT NULL,
  descricao VARCHAR(10) NOT NULL,
  data_transacao TIMESTAMP DEFAULT now() NOT NULL
);

DO $$
BEGIN
  INSERT INTO cliente (limite, saldo)
  VALUES
    (1000 * 100, 0),
    (800 * 100, 0),
    (10000 * 100, 0),
    (100000 * 100, 0),
    (5000 * 100, 0);
END; $$