CREATE TABLE IF NOT EXISTS users (
	uid int PRIMARY KEY,
	username varchar(260) UNIQUE NOT NULL,
	password varchar(260),
	roles varchar(260),
	created_at timestamp with time zone,
	updated_at timestamp with time zone,
	expiry timestamp with time zone,
	apikey varchar(260)
);

INSERT INTO users (uid,username,password,roles,created_at,updated_at) VALUES(0,'root','Kanagawa2020!','administrator',now(),now());

CREATE TABLE IF NOT EXISTS file_groups (
	gid int PRIMARY KEY,
	name varchar(260) UNIQUE NOT NULL,
	owned_by varchar(260) NOT NULL,
	created_at timestamp with time zone,
	updated_at timestamp with time zone
);

INSERT INTO file_groups (gid,name,owned_by,created_at,updated_at) VALUES(0,'default','root',now(),now());

CREATE TABLE IF NOT EXISTS files (
	fid int PRIMARY KEY,
	path varchar(260) NOT NULL,
	size bigint,
	updated_at timestamp with time zone,
	gid int REFERENCES file_groups (gid),
	UNIQUE (path,gid)
);

CREATE TABLE IF NOT EXISTS file_contents (
	fid int PRIMARY KEY,
	data bytea
);

CREATE TABLE IF NOT EXISTS texts (
	txt varchar(260) PRIMARY KEY,
	dst bytea
);

CREATE TABLE IF NOT EXISTS preferences (
	name varchar(260) PRIMARY KEY,
	value varchar(8192)
);
