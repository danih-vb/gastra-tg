CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `item_cardapio` (
    `id` int NOT NULL AUTO_INCREMENT,
    `nome` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `categoria` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `preco` decimal(10,2) NOT NULL,
    `descricao` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `disponivel` tinyint(1) NOT NULL,
    `imagem` varchar(255) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_item_cardapio` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `item_cardapio_flag` (
    `flag` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `item_cardapio_id` int NOT NULL,
    CONSTRAINT `PK_item_cardapio_flag` PRIMARY KEY (`item_cardapio_id`, `flag`),
    CONSTRAINT `FK_item_cardapio_flag_item_cardapio_item_cardapio_id` FOREIGN KEY (`item_cardapio_id`) REFERENCES `item_cardapio` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_item_cardapio_nome` ON `item_cardapio` (`nome`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260915041810_CriaItemCardapio', '9.0.20');

CREATE TABLE `usuario` (
    `id` int NOT NULL AUTO_INCREMENT,
    `nome` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `email` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `senha_hash` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `papel` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `ativo` tinyint(1) NOT NULL,
    `segredo_totp` varchar(500) CHARACTER SET utf8mb4 NULL,
    `chave_sessao` char(36) COLLATE ascii_general_ci NOT NULL,
    CONSTRAINT `PK_usuario` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_usuario_email` ON `usuario` (`email`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260915045821_CriaUsuario', '9.0.20');

CREATE TABLE `praca` (
    `id` int NOT NULL AUTO_INCREMENT,
    `codigo` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `quantidade_garcons` int NOT NULL,
    CONSTRAINT `PK_praca` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `registro_auditoria` (
    `id` int NOT NULL AUTO_INCREMENT,
    `data_hora_utc` datetime(6) NOT NULL,
    `evento` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `resultado` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `usuario_id` int NULL,
    `papel` varchar(20) CHARACTER SET utf8mb4 NULL,
    `entidade` varchar(50) CHARACTER SET utf8mb4 NULL,
    `id_entidade` int NULL,
    `detalhes` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `ip` varchar(45) CHARACTER SET utf8mb4 NULL,
    `id_correlacao` varchar(50) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_registro_auditoria` PRIMARY KEY (`id`),
    CONSTRAINT `FK_registro_auditoria_usuario_usuario_id` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `alocacao` (
    `id` int NOT NULL AUTO_INCREMENT,
    `data` date NOT NULL,
    `periodo` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `garcom_id` int NOT NULL,
    `praca_id` int NOT NULL,
    `confirmada` tinyint(1) NOT NULL,
    CONSTRAINT `PK_alocacao` PRIMARY KEY (`id`),
    CONSTRAINT `FK_alocacao_praca_praca_id` FOREIGN KEY (`praca_id`) REFERENCES `praca` (`id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_alocacao_usuario_garcom_id` FOREIGN KEY (`garcom_id`) REFERENCES `usuario` (`id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `mesa` (
    `id` int NOT NULL AUTO_INCREMENT,
    `numero` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `capacidade` int NOT NULL,
    `praca_id` int NOT NULL,
    CONSTRAINT `PK_mesa` PRIMARY KEY (`id`),
    CONSTRAINT `FK_mesa_praca_praca_id` FOREIGN KEY (`praca_id`) REFERENCES `praca` (`id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `comanda` (
    `id` int NOT NULL AUTO_INCREMENT,
    `mesa_id` int NOT NULL,
    `garcom_id` int NOT NULL,
    `data_hora_abertura` datetime(6) NOT NULL,
    `data_hora_fechamento` datetime(6) NULL,
    `status` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `quantidade_pessoas` int NOT NULL,
    `composicao` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `taxa_servico_removida` tinyint(1) NOT NULL,
    `codigo_acesso_cliente` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_comanda` PRIMARY KEY (`id`),
    CONSTRAINT `FK_comanda_mesa_mesa_id` FOREIGN KEY (`mesa_id`) REFERENCES `mesa` (`id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_comanda_usuario_garcom_id` FOREIGN KEY (`garcom_id`) REFERENCES `usuario` (`id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `item_pedido` (
    `id` int NOT NULL AUTO_INCREMENT,
    `comanda_id` int NOT NULL,
    `item_cardapio_id` int NOT NULL,
    `quantidade` int NOT NULL,
    `preco_unitario_no_momento` decimal(10,2) NOT NULL,
    `data_hora_registro` datetime(6) NOT NULL,
    `status` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `motivo_cancelamento` varchar(20) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_item_pedido` PRIMARY KEY (`id`),
    CONSTRAINT `FK_item_pedido_comanda_comanda_id` FOREIGN KEY (`comanda_id`) REFERENCES `comanda` (`id`) ON DELETE CASCADE,
    CONSTRAINT `FK_item_pedido_item_cardapio_item_cardapio_id` FOREIGN KEY (`item_cardapio_id`) REFERENCES `item_cardapio` (`id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `restricao_alimentar` (
    `id` int NOT NULL AUTO_INCREMENT,
    `comanda_id` int NOT NULL,
    `categoria` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `observacao_livre` varchar(200) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_restricao_alimentar` PRIMARY KEY (`id`),
    CONSTRAINT `FK_restricao_alimentar_comanda_comanda_id` FOREIGN KEY (`comanda_id`) REFERENCES `comanda` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_alocacao_data_periodo_garcom_id` ON `alocacao` (`data`, `periodo`, `garcom_id`);

CREATE INDEX `IX_alocacao_garcom_id` ON `alocacao` (`garcom_id`);

CREATE INDEX `IX_alocacao_praca_id` ON `alocacao` (`praca_id`);

CREATE UNIQUE INDEX `IX_comanda_codigo_acesso_cliente` ON `comanda` (`codigo_acesso_cliente`);

CREATE INDEX `IX_comanda_garcom_id` ON `comanda` (`garcom_id`);

CREATE INDEX `IX_comanda_mesa_id_status` ON `comanda` (`mesa_id`, `status`);

CREATE INDEX `IX_item_pedido_comanda_id` ON `item_pedido` (`comanda_id`);

CREATE INDEX `IX_item_pedido_item_cardapio_id` ON `item_pedido` (`item_cardapio_id`);

CREATE UNIQUE INDEX `IX_mesa_numero` ON `mesa` (`numero`);

CREATE INDEX `IX_mesa_praca_id` ON `mesa` (`praca_id`);

CREATE UNIQUE INDEX `IX_praca_codigo` ON `praca` (`codigo`);

CREATE INDEX `IX_registro_auditoria_data_hora_utc` ON `registro_auditoria` (`data_hora_utc`);

CREATE INDEX `IX_registro_auditoria_usuario_id_data_hora_utc` ON `registro_auditoria` (`usuario_id`, `data_hora_utc`);

CREATE INDEX `IX_restricao_alimentar_comanda_id` ON `restricao_alimentar` (`comanda_id`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260916043617_CriaNucleoComandas', '9.0.20');

COMMIT;

