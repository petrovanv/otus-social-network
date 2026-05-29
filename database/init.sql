-- Создание базы данных и таблицы пользователей
CREATE DATABASE IF NOT EXISTS social_network
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE social_network;

CREATE TABLE IF NOT EXISTS users (
    id           VARCHAR(36)   NOT NULL PRIMARY KEY,
    email        VARCHAR(255)  NOT NULL UNIQUE,
    first_name   VARCHAR(100)  NOT NULL,
    second_name  VARCHAR(100)  NOT NULL,
    birthdate    DATE          NOT NULL,
    gender       VARCHAR(10),
    biography    TEXT,
    city         VARCHAR(100)  NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at   TIMESTAMP     DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
