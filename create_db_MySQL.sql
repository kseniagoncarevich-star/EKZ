CREATE TABLE table_authors (
id SERIAL PRIMARY KEY,
name TEXT NOT NULL,
bio TEXT
);

CREATE TABLE table_posts (
id SERIAL PRIMARY KEY,
title TEXT NOT NULL,
content TEXT,
author_id INTEGER REFERENCES table_authors(id)
);

CREATE TABLE table_publishers (
id SERIAL PRIMARY KEY,
name TEXT NOT NULL
);

CREATE TABLE post_publishers (
post_id INTEGER REFERENCES table_posts(id) ON DELETE CASCADE,
publisher_id INTEGER REFERENCES table_publishers(id) ON DELETE CASCADE,
PRIMARY KEY (post_id, publisher_id)
);