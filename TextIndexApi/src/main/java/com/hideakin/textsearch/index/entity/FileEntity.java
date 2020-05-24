package com.hideakin.textsearch.index.entity;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Table;

@Entity(name = "files")
@Table(name = "files")
public class FileEntity {

	@Id
	@Column(name="fid")
	private int fid;

	@Column(name="path")
	private String path;

	@Column(name="gid")
	private int gid;

	public int getFid() {
		return fid;
	}

	public void setFid(int fid) {
		this.fid = fid;
	}

	public String getPath() {
		return path;
	}

	public void setPath(String path) {
		this.path = path;
	}

	public int getGid() {
		return gid;
	}

	public void setGid(int gid) {
		this.gid = gid;
	}

}
