package com.hideakin.textsearch.index.model;

import com.hideakin.textsearch.index.entity.FileGroupEntity;

public class FileGroupInfo {

	private int gid;
	
	private String name;
	
	private String[] ownedBy;
	
	public FileGroupInfo() {
		this(-1, null, (String[])null);
	}

	public FileGroupInfo(FileGroupEntity entity) {
		this(entity.getGid(), entity.getName(), entity.getOwnedBy());
	}


	public FileGroupInfo(int gid, String name, String ownedBy) {
		this(gid, name, ownedBy.split(","));
	}

	public FileGroupInfo(int gid, String name, String[] ownedBy) {
		this.gid = gid;
		this.name = name;
		this.ownedBy = ownedBy;
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

	public String[] getOwnedBy() {
		return ownedBy;
	}

	public void setOwnedBy(String[] ownedBy) {
		this.ownedBy = ownedBy;
	}

	public void setOwnedBy(String ownedBy) {
		this.ownedBy = ownedBy.split(",");
	}

}
