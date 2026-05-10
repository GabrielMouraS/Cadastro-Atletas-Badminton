-- ============================================================
-- SEED — dados base idempotentes (INSERT OR IGNORE)
-- chave_planilha: PLACEHOLDER — confirmar com stakeholder (RN05)
-- ============================================================

-- Entidades
INSERT OR IGNORE INTO entidades (sigla, nome_completo, cidade) VALUES
    ('ITAPÊ',  'Associação Badminton Itapê',                  'Itapeva'),
    ('SBB',    'Santo André Badminton',                        'Santo André'),
    ('ATB',    'Associação Taubateana de Badminton',           'Taubaté'),
    ('SNEC',   'Sociedade Nipônica Esportiva e Cultural',      'São Paulo'),
    ('SESI',   'SESI Badminton',                               'São Paulo'),
    ('ECP',    'Esporte Clube Pinheiros',                      'São Paulo'),
    ('MOCOCA', 'Associação Badminton Mococa',                  'Mococa');

-- ============================================================
-- Categorias — SIMPLES MASCULINO (tipo = 'SIMPLES')
-- ============================================================
INSERT OR IGNORE INTO categorias (codigo, chave_planilha, descricao, tipo) VALUES
    ('SM09',  1.0,  'Simples Masculino Sub-9',   'SIMPLES'),
    ('SM11',  2.0,  'Simples Masculino Sub-11',  'SIMPLES'),
    ('SM13',  3.0,  'Simples Masculino Sub-13',  'SIMPLES'),
    ('SM15',  4.0,  'Simples Masculino Sub-15',  'SIMPLES'),
    ('SM17',  5.0,  'Simples Masculino Sub-17',  'SIMPLES'),
    ('SM19',  6.0,  'Simples Masculino Sub-19',  'SIMPLES'),
    ('SMA',   7.0,  'Simples Masculino Adulto',  'SIMPLES'),
    ('SM35',  8.0,  'Simples Masculino +35',     'SIMPLES');

-- ============================================================
-- Categorias — SIMPLES FEMININO (tipo = 'SIMPLES')
-- ============================================================
INSERT OR IGNORE INTO categorias (codigo, chave_planilha, descricao, tipo) VALUES
    ('SF09',  11.0, 'Simples Feminino Sub-9',    'SIMPLES'),
    ('SF11',  12.0, 'Simples Feminino Sub-11',   'SIMPLES'),
    ('SF13',  13.0, 'Simples Feminino Sub-13',   'SIMPLES'),
    ('SF15',  14.0, 'Simples Feminino Sub-15',   'SIMPLES'),
    ('SF17',  15.0, 'Simples Feminino Sub-17',   'SIMPLES'),
    ('SF19',  16.0, 'Simples Feminino Sub-19',   'SIMPLES'),
    ('SFA',   17.0, 'Simples Feminino Adulto',   'SIMPLES'),
    ('SF35',  18.0, 'Simples Feminino +35',      'SIMPLES');

-- ============================================================
-- Categorias — DUPLA MASCULINA (tipo = 'DUPLA')
-- ============================================================
INSERT OR IGNORE INTO categorias (codigo, chave_planilha, descricao, tipo) VALUES
    ('DM09',  21.0, 'Dupla Masculina Sub-9',     'DUPLA'),
    ('DM11',  22.0, 'Dupla Masculina Sub-11',    'DUPLA'),
    ('DM13',  23.0, 'Dupla Masculina Sub-13',    'DUPLA'),
    ('DM15',  24.0, 'Dupla Masculina Sub-15',    'DUPLA'),
    ('DM17',  25.0, 'Dupla Masculina Sub-17',    'DUPLA'),
    ('DM19',  26.0, 'Dupla Masculina Sub-19',    'DUPLA'),
    ('DMA',   27.0, 'Dupla Masculina Adulto',    'DUPLA'),
    ('DM35',  28.0, 'Dupla Masculina +35',       'DUPLA');

-- ============================================================
-- Categorias — DUPLA FEMININA (tipo = 'DUPLA')
-- ============================================================
INSERT OR IGNORE INTO categorias (codigo, chave_planilha, descricao, tipo) VALUES
    ('DF09',  31.0, 'Dupla Feminina Sub-9',      'DUPLA'),
    ('DF11',  32.0, 'Dupla Feminina Sub-11',     'DUPLA'),
    ('DF13',  33.0, 'Dupla Feminina Sub-13',     'DUPLA'),
    ('DF15',  34.0, 'Dupla Feminina Sub-15',     'DUPLA'),
    ('DF17',  35.0, 'Dupla Feminina Sub-17',     'DUPLA'),
    ('DF19',  36.0, 'Dupla Feminina Sub-19',     'DUPLA'),
    ('DFA',   37.0, 'Dupla Feminina Adulto',     'DUPLA'),
    ('DF35',  38.0, 'Dupla Feminina +35',        'DUPLA');

-- ============================================================
-- Categorias — DUPLA MISTA (tipo = 'MISTA')
-- ============================================================
INSERT OR IGNORE INTO categorias (codigo, chave_planilha, descricao, tipo) VALUES
    ('DX09',  41.0, 'Dupla Mista Sub-9',         'MISTA'),
    ('DX11',  42.0, 'Dupla Mista Sub-11',        'MISTA'),
    ('DX13',  43.0, 'Dupla Mista Sub-13',        'MISTA'),
    ('DX15',  44.0, 'Dupla Mista Sub-15',        'MISTA'),
    ('DX17',  45.0, 'Dupla Mista Sub-17',        'MISTA'),
    ('DX19',  46.0, 'Dupla Mista Sub-19',        'MISTA'),
    ('DXA',   47.0, 'Dupla Mista Adulto',        'MISTA'),
    ('DX35',  48.0, 'Dupla Mista +35',           'MISTA');
