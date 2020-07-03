package com.hideakin.textsearch.index.entity;

import java.time.ZonedDateTime;

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

	@Column(name="size")
	private int size;

	@Column(name="updated_at")
	private ZonedDateTime updatedAt;

	@Column(name="gid")
	private int gid;

	@Column(name="stale")
	private boolean stale;
	
	public FileEntity() {
		this(-1, null, -1, -1);
	}

	public FileEntity(int fid, String path, int size, int gid) {
		this.fid = fid;
		this.path = path;
		this.size = size;
		this.updatedAt = ZonedDateTime.now();
		this.gid = gid;
		this.stale = false;
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

	public int getSize() {
		return size;
	}

	public void setSize(int size) {
		this.size = size;
	}

	public ZonedDateTime getUpdatedAt() {
		return updatedAt;
	}

	public void setUpdatedAt(ZonedDateTime updatedAt) {
		this.updatedAt = updatedAt;
	}

	public int getGid() {
		return gid;
	}

	public void setGid(int gid) {
		this.gid = gid;
	}

	public boolean isStale() {
		return stale;
	}

	public void setStale(boolean stale) {
		this.stale = stale;
	}

}
