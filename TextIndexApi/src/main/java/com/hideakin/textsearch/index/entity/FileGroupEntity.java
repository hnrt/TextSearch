package com.hideakin.textsearch.index.entity;

import java.time.ZonedDateTime;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Table;

@Entity(name = "file_groups")
@Table(name = "file_groups")
public class FileGroupEntity {

	@Id
	@Column(name="gid")
	private int gid;
	
	@Column(name="name")
	private String name;

	@Column(name="created_at")
	private ZonedDateTime createdAt;

	@Column(name="updated_at")
	private ZonedDateTime updatedAt;

	public FileGroupEntity() {
		this(-1, null, null);
	}

	public FileGroupEntity(int gid, String name) {
		this(gid, name, ZonedDateTime.now());
	}

	public FileGroupEntity(int gid, String name, ZonedDateTime at) {
		this.gid = gid;
		this.name = name;
		this.createdAt = at;
		this.updatedAt = at;
	}

	public int getGid() {
		return gid;
	}

	public void setGid(int gid) {
		this.gid = gid;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public ZonedDateTime getCreatedAt() {
		return createdAt;
	}

	public void setCreatedAt(ZonedDateTime createdAt) {
		this.createdAt = createdAt;
	}

	public ZonedDateTime getUpdatedAt() {
		return updatedAt;
	}

	public void setUpdatedAt(ZonedDateTime updatedAt) {
		this.updatedAt = updatedAt;
	}

}
