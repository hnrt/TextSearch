package com.hideakin.textsearch.index.model;

public class FileGroupRequest {

	private String name;
	
	private String[] ownedBy;

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

}
