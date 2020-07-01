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

	@Column(name="owned_by")
	private String ownedBy;
	
	@Column(name="created_at")
	private ZonedDateTime createdAt;

	@Column(name="updated_at")
	private ZonedDateTime updatedAt;

	public FileGroupEntity() {
		this(-1, null, null, null);
	}

	public FileGroupEntity(int gid, String name, String[] ownedBy) {
		this(gid, name, csv(ownedBy), ZonedDateTime.now());
	}

	public FileGroupEntity(int gid, String name, String ownedBy) {
		this(gid, name, ownedBy, ZonedDateTime.now());
	}

	public FileGroupEntity(int gid, String name, String ownedBy, ZonedDateTime at) {
		this.gid = gid;
		this.name = name;
		this.ownedBy = ownedBy;
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

	public String getOwnedBy() {
		return ownedBy;
	}

	public void setOwnedBy(String ownedBy) {
		this.ownedBy = ownedBy;
	}

	public void setOwnedBy(String[] ownedBy) {
		this.ownedBy = csv(ownedBy);
	}

	public boolean isOwner(String username) {
		for (String owner : ownedBy.split(",")) {
			if (owner.equals(username)) {
				return true;
			}
		}
		return false;
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

	private static String csv(String[] array) {
		if (array == null || array.length == 0) {
			return null;
		}
		String value = array[0];
		for (int index = 1; index < array.length; index++) {
			value += ",";
			value += array[index];
		}
		return value;
	}

}
