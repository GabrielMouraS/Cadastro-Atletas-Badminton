PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS entidades (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    sigla           TEXT NOT NULL UNIQUE,
    nome_completo   TEXT NOT NULL,
    cidade          TEXT
);

CREATE TABLE IF NOT EXISTS atletas (
    id                  INTEGER PRIMARY KEY AUTOINCREMENT,
    entidade_id         INTEGER NOT NULL,
    nome_completo       TEXT NOT NULL,
    ano_nascimento      INTEGER NOT NULL,
    sexo                TEXT NOT NULL CHECK(sexo IN ('M','F')),
    codigo_federacao    TEXT UNIQUE,
    FOREIGN KEY (entidade_id) REFERENCES entidades(id)
);

CREATE TABLE IF NOT EXISTS categorias (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo          TEXT NOT NULL UNIQUE,
    chave_planilha  REAL NOT NULL,
    descricao       TEXT,
    tipo            TEXT NOT NULL CHECK(tipo IN ('SIMPLES','DUPLA','MISTA'))
);

CREATE TABLE IF NOT EXISTS torneios (
    id                  INTEGER PRIMARY KEY AUTOINCREMENT,
    nome                TEXT NOT NULL,
    tipo_ficha          TEXT NOT NULL CHECK(tipo_ficha IN ('ESTADUAL','REGIONAL','INTERESCOLAR')),
    data_inicio         TEXT,
    local               TEXT,
    responsavel_nome    TEXT,
    responsavel_tel     TEXT
);

CREATE TABLE IF NOT EXISTS inscricoes (
    id                      INTEGER PRIMARY KEY AUTOINCREMENT,
    torneio_id              INTEGER NOT NULL,
    categoria_id            INTEGER NOT NULL,
    atleta1_id              INTEGER NOT NULL,
    atleta2_id              INTEGER,
    rk_interno              INTEGER,
    rk_estadual             INTEGER,
    aceita_remanejamento    INTEGER DEFAULT 0 CHECK(aceita_remanejamento IN (0,1)),
    obs_remanejamento       TEXT,
    valor                   REAL DEFAULT 0.0,
    pago                    INTEGER DEFAULT 0 CHECK(pago IN (0,1)),
    FOREIGN KEY (torneio_id)   REFERENCES torneios(id) ON DELETE CASCADE,
    FOREIGN KEY (categoria_id) REFERENCES categorias(id),
    FOREIGN KEY (atleta1_id)   REFERENCES atletas(id),
    FOREIGN KEY (atleta2_id)   REFERENCES atletas(id)
);

-- Índices de performance
CREATE INDEX IF NOT EXISTS idx_atletas_entidade   ON atletas(entidade_id);
CREATE INDEX IF NOT EXISTS idx_atletas_nome       ON atletas(nome_completo);
CREATE INDEX IF NOT EXISTS idx_inscricoes_torneio ON inscricoes(torneio_id);
CREATE INDEX IF NOT EXISTS idx_inscricoes_cat     ON inscricoes(categoria_id);

-- RF08: impede mesmo atleta na mesma categoria/torneio (titular OU parceiro)
CREATE UNIQUE INDEX IF NOT EXISTS uq_inscricao_atleta1
    ON inscricoes(torneio_id, categoria_id, atleta1_id);

CREATE UNIQUE INDEX IF NOT EXISTS uq_inscricao_atleta2
    ON inscricoes(torneio_id, categoria_id, atleta2_id)
    WHERE atleta2_id IS NOT NULL;