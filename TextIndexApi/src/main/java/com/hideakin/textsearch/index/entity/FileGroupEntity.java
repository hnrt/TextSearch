package com.hideakin.textsearch.index.entity;

import java.time.ZonedDateTime;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Table;

@Entity
@Table(name = "file_groups")
public class FileGroupEntity {

	@Id
	@Column(name="gid")
	private int gid;
	
	@Column(name="name")
	private String name;

	@Column(name="owned_by")
	private String ownedBy;
	
	@Column(name="created_at")
	private ZonedDateTime createdAt;

	@Column(name="updated_at")
	private ZonedDateTime updatedAt;

	public FileGroupEntity() {
		this.gid = -1;
		this.name = null;
		this.ownedBy = null;
		this.createdAt = ZonedDateTime.now();
		this.updatedAt = this.createdAt;
	}

	public FileGroupEntity(int gid, String name, String ownedBy) {
		this.gid = gid;
		this.name = name;
		this.ownedBy = ownedBy;
		this.createdAt = ZonedDateTime.now();
		this.updatedAt = this.createdAt;
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

	public String getOwnedBy() {
		return ownedBy;
	}

	public void setOwnedBy(String ownedBy) {
		this.ownedBy = ownedBy;
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
