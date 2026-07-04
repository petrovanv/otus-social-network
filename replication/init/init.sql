-- Схема на мастере. Слейвы стартуют пустыми и получают её через репликацию (GTID с нуля).
-- Индекс idx_name_search (first_name, second_name) — из ДЗ по индексам.
CREATE DATABASE IF NOT EXISTS social_network
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE social_network;

CREATE TABLE IF NOT EXISTS users (
  id            VARCHAR(36)  COLLATE utf8mb4_unicode_ci NOT NULL,
  email         VARCHAR(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  first_name    VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  second_name   VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  birthdate     DATE NOT NULL,
  gender        VARCHAR(10)  COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  biography     TEXT         COLLATE utf8mb4_unicode_ci,
  city          VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  password_hash VARCHAR(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  created_at    TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_email (email),
  KEY idx_name_search (first_name, second_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Друзья (направленно: user_id подписан на friend_id и видит его посты)
CREATE TABLE IF NOT EXISTS friends (
  user_id   VARCHAR(36) COLLATE utf8mb4_unicode_ci NOT NULL,
  friend_id VARCHAR(36) COLLATE utf8mb4_unicode_ci NOT NULL,
  created_at TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (user_id, friend_id),
  KEY idx_friend (friend_id)          -- обратный поиск подписчиков для fan-out
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Посты пользователей
CREATE TABLE IF NOT EXISTS posts (
  id             VARCHAR(36)  COLLATE utf8mb4_unicode_ci NOT NULL,
  author_user_id VARCHAR(36)  COLLATE utf8mb4_unicode_ci NOT NULL,
  text           TEXT         COLLATE utf8mb4_unicode_ci NOT NULL,
  created_at     TIMESTAMP(3) NULL DEFAULT CURRENT_TIMESTAMP(3),
  PRIMARY KEY (id),
  KEY idx_author_created (author_user_id, created_at)   -- посты автора по времени
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
