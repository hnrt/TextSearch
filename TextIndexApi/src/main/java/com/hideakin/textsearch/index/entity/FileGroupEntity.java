package com.hideakin.textsearch.index.entity;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Table;

@Entity(name = "filegroups")
@Table(name = "filegroups")
public class FileGroupEntity {

	@Id
	@Column(name="gid")
	private int gid;
	
	@Column(name="name")
	private String name;

	public FileGroupEntity() {
		this.gid = -1;
		this.name = null;
	}

	public FileGroupEntity(int gid, String name) {
		this.gid = gid;
		this.name = name;
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

}
