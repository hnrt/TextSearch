package com.hideakin.textsearch.index.entity;

import java.time.ZonedDateTime;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Table;

@Entity
@Table(name = "files")
public class FileEntity {

	@Id
	@Column(name="fid")
	private int fid;

	@Column(name="path")
	private String path;

	@Column(name="size")
	private long size;

	@Column(name="updated_at")
	private ZonedDateTime updatedAt;

	@Column(name="gid")
	private int gid;
	
	public FileEntity() {
		this.fid = -1;
		this.path = null;
		this.size = -1;
		this.updatedAt = ZonedDateTime.now();
		this.gid = -1;
	}

	public FileEntity(int fid, String path, int gid) {
		this.fid = fid;
		this.path = path;
		this.size = -1;
		this.updatedAt = ZonedDateTime.now();
		this.gid = gid;
	}

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

	public long getSize() {
		return size;
	}

	public void setSize(long size) {
		this.size = size;
	}

	public ZonedDateTime getUpdatedAt() {
		return updatedAt;
	}

	public void setUpdatedAt(ZonedDateTime updatedAt) {
		this.updatedAt = updatedAt;
	}

}
