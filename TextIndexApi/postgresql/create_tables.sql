CREATE TABLE IF NOT EXISTS users (
	uid int PRIMARY KEY,
	username varchar(256) UNIQUE NOT NULL,
	password varchar(256),
	roles varchar(256),
	created_at timestamp with time zone,
	updated_at timestamp with time zone,
	access_token varchar(256),
	expires_at timestamp with time zone
);

-- password:Kanagawa2020!
INSERT INTO users (uid,username,password,roles,created_at,updated_at) VALUES(0,'root','B4D116153B59A21EF9BC8D8756D016F38A2D0EBD3CB6780B143231CD0805321A','administrator',now(),now());

CREATE TABLE IF NOT EXISTS file_groups (
	gid int PRIMARY KEY,
	name varchar(256) UNIQUE NOT NULL,
	created_at timestamp with time zone,
	updated_at timestamp with time zone
);

INSERT INTO file_groups (gid,name,created_at,updated_at) VALUES(0,'default',now(),now());

CREATE TABLE IF NOT EXISTS files (
	fid int PRIMARY KEY,
	path varchar(260) NOT NULL,
	size int,
	updated_at timestamp with time zone,
	gid int REFERENCES file_groups (gid),
	stale boolean
);

CREATE TABLE IF NOT EXISTS file_contents (
	fid int PRIMARY KEY,
	data bytea
);

CREATE TABLE IF NOT EXISTS texts (
	txt varchar(256) PRIMARY KEY,
	dst bytea
);

CREATE TABLE IF NOT EXISTS preferences (
	name varchar(256) PRIMARY KEY,
	value varchar(8192)
);
