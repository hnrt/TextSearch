CREATE TABLE IF NOT EXISTS filegroups (
	gid int PRIMARY KEY,
	name varchar(260) UNIQUE NOT NULL
);

INSERT INTO filegroups (gid,name) VALUES(0,'default');

CREATE TABLE IF NOT EXISTS files (
	fid int PRIMARY KEY,
	path varchar(260) NOT NULL,
	gid int NOT NULL
);

CREATE TABLE IF NOT EXISTS texts (
	txt varchar(260) PRIMARY KEY,
	dst bytea
);

CREATE TABLE IF NOT EXISTS preferences (
	name varchar(260) PRIMARY KEY,
	value varchar(8192)
);
